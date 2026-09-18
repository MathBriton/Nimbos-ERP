using Microsoft.Extensions.Logging.Abstractions;
using Nimbus.Application.Autenticacao;
using Nimbus.Application.Autenticacao.Modelos;
using Nimbus.Application.Common.Excecoes;
using Nimbus.Domain.Entidades;
using Nimbus.Tests.Dobras;

namespace Nimbus.Tests.Autenticacao;

public class ServicoDeAutenticacaoTests
{
    private const string EmailDoAdmin = "admin@erp.com";
    private const string SenhaCorreta = "Admin123!";

    private readonly ProvedorDeDataHoraFalso _relogio = new();
    private readonly RepositorioDeUsuariosEmMemoria _repositorio = new();
    private readonly ServicoDeHashDeSenhaFalso _hash = new();
    private readonly ServicoDeAutenticacao _servico;

    public ServicoDeAutenticacaoTests()
    {
        _servico = new ServicoDeAutenticacao(
            _repositorio,
            _hash,
            new GeradorDeTokenFalso(_relogio),
            _relogio,
            NullLogger<ServicoDeAutenticacao>.Instance);
    }

    private Usuario SemearAdmin(string senha = SenhaCorreta)
    {
        var usuario = Usuario.Criar("Administrador", EmailDoAdmin, _hash.GerarHash(senha));
        _repositorio.Semear(usuario);
        return usuario;
    }

    private static LoginRequisicao Credenciais(string email, string senha) =>
        new() { Email = email, Senha = senha };

    // ---------------------------------------------------------------------
    // Caminhos felizes
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Credenciais_corretas_devolvem_token_e_dados_do_usuario()
    {
        var usuario = SemearAdmin();

        var resposta = await _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta));

        Assert.Equal($"token-de-{EmailDoAdmin}", resposta.Token);
        Assert.Equal(_relogio.Agora.Add(GeradorDeTokenFalso.Validade), resposta.ExpiraEm);
        Assert.Equal(usuario.Id, resposta.Usuario.Id);
        Assert.Equal(EmailDoAdmin, resposta.Usuario.Email);
        Assert.Equal("Administrador", resposta.Usuario.Nome);
    }

    [Fact]
    public async Task Email_com_caixa_e_espacos_diferentes_ainda_autentica()
    {
        SemearAdmin();

        var resposta = await _servico.AutenticarAsync(
            Credenciais("  ADMIN@ERP.COM  ", SenhaCorreta));

        Assert.Equal(EmailDoAdmin, resposta.Usuario.Email);
    }

    [Fact]
    public async Task Login_bem_sucedido_registra_o_ultimo_acesso()
    {
        var usuario = SemearAdmin();

        await _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta));

        Assert.Equal(_relogio.Agora, usuario.UltimoAcessoEm);
        Assert.Equal(1, _repositorio.QuantidadeDeSalvamentos);
    }

    // ---------------------------------------------------------------------
    // Credenciais invalidas
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Senha_errada_falha_com_mensagem_generica()
    {
        SemearAdmin();

        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "senha-errada")));

        Assert.Equal(ExcecaoDeAutenticacao.CredenciaisInvalidas, excecao.Message);
    }

    [Fact]
    public async Task Email_inexistente_falha_com_a_mesma_mensagem_da_senha_errada()
    {
        SemearAdmin();

        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais("ninguem@erp.com", SenhaCorreta)));

        // Mensagem identica: nao da para descobrir quais e-mails existem.
        Assert.Equal(ExcecaoDeAutenticacao.CredenciaisInvalidas, excecao.Message);
    }

    [Fact]
    public async Task Email_inexistente_ainda_gasta_uma_conferencia_de_hash()
    {
        // Protecao contra timing attack: sem a conferencia de isca, a resposta
        // para e-mail inexistente voltaria visivelmente mais rapido.
        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais("ninguem@erp.com", SenhaCorreta)));

        Assert.Equal(ExcecaoDeAutenticacao.CredenciaisInvalidas, excecao.Message);
        Assert.Equal(1, _hash.QuantidadeDeConferencias);
    }

    [Theory]
    [InlineData("nao-e-email")]
    [InlineData("")]
    public async Task Email_em_formato_invalido_nao_revela_erro_de_validacao(string email)
    {
        SemearAdmin();

        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(email, SenhaCorreta)));

        Assert.Equal(ExcecaoDeAutenticacao.CredenciaisInvalidas, excecao.Message);
    }

    // ---------------------------------------------------------------------
    // Bloqueio por tentativas
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Cada_senha_errada_e_persistida_no_contador_de_falhas()
    {
        var usuario = SemearAdmin();

        await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "errada")));

        Assert.Equal(1, usuario.TentativasDeLoginFalhas);
        Assert.Equal(1, _repositorio.QuantidadeDeSalvamentos);
    }

    [Fact]
    public async Task Excesso_de_tentativas_bloqueia_a_conta_e_avisa_o_tempo_restante()
    {
        SemearAdmin();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
                () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "errada")));
        }

        // Agora nem a senha correta passa.
        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta)));

        Assert.Contains("bloqueada", excecao.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("15 minuto", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Depois_do_bloqueio_expirar_a_senha_correta_volta_a_funcionar()
    {
        SemearAdmin();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
                () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "errada")));
        }

        _relogio.Avancar(Usuario.DuracaoDoBloqueio + TimeSpan.FromSeconds(1));

        var resposta = await _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta));

        Assert.Equal(EmailDoAdmin, resposta.Usuario.Email);
    }

    [Fact]
    public async Task Conta_bloqueada_nao_chega_a_conferir_a_senha()
    {
        SemearAdmin();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
                () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "errada")));
        }

        var conferenciasAntes = _hash.QuantidadeDeConferencias;

        await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta)));

        // O bloqueio e verificado antes do BCrypt: e isso que torna a forca
        // bruta barata de recusar.
        Assert.Equal(conferenciasAntes, _hash.QuantidadeDeConferencias);
    }

    // ---------------------------------------------------------------------
    // Usuario inativo
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Usuario_inativo_com_senha_correta_recebe_mensagem_especifica()
    {
        var usuario = SemearAdmin();
        usuario.Inativar();

        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, SenhaCorreta)));

        Assert.Contains("inativo", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Usuario_inativo_com_senha_errada_nao_revela_que_a_conta_existe()
    {
        var usuario = SemearAdmin();
        usuario.Inativar();

        var excecao = await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.AutenticarAsync(Credenciais(EmailDoAdmin, "errada")));

        Assert.Equal(ExcecaoDeAutenticacao.CredenciaisInvalidas, excecao.Message);
    }

    // ---------------------------------------------------------------------
    // Reidratacao de sessao (GET /api/auth/eu)
    // ---------------------------------------------------------------------

    [Fact]
    public async Task ObterPorId_devolve_o_usuario_ativo()
    {
        var usuario = SemearAdmin();

        var dto = await _servico.ObterPorIdAsync(usuario.Id);

        Assert.Equal(usuario.Id, dto.Id);
        Assert.Equal(EmailDoAdmin, dto.Email);
    }

    [Fact]
    public async Task ObterPorId_recusa_usuario_inexistente()
    {
        await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.ObterPorIdAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task ObterPorId_recusa_usuario_que_foi_inativado_depois_do_login()
    {
        var usuario = SemearAdmin();
        usuario.Inativar();

        await Assert.ThrowsAsync<ExcecaoDeAutenticacao>(
            () => _servico.ObterPorIdAsync(usuario.Id));
    }
}
