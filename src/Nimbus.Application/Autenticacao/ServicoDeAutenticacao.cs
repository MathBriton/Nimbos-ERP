using Microsoft.Extensions.Logging;
using Nimbus.Application.Autenticacao.Abstracoes;
using Nimbus.Application.Autenticacao.Modelos;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Application.Common.Excecoes;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Common;
using Nimbus.Domain.Entidades;

namespace Nimbus.Application.Autenticacao;

/// <summary>Caso de uso de autenticacao por e-mail e senha.</summary>
public sealed class ServicoDeAutenticacao
{
    private readonly IRepositorioDeUsuarios _repositorioDeUsuarios;
    private readonly IServicoDeHashDeSenha _servicoDeHashDeSenha;
    private readonly IGeradorDeTokenDeAcesso _geradorDeToken;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly ILogger<ServicoDeAutenticacao> _logger;

    public ServicoDeAutenticacao(
        IRepositorioDeUsuarios repositorioDeUsuarios,
        IServicoDeHashDeSenha servicoDeHashDeSenha,
        IGeradorDeTokenDeAcesso geradorDeToken,
        IProvedorDeDataHora provedorDeDataHora,
        ILogger<ServicoDeAutenticacao> logger)
    {
        _repositorioDeUsuarios = repositorioDeUsuarios;
        _servicoDeHashDeSenha = servicoDeHashDeSenha;
        _geradorDeToken = geradorDeToken;
        _provedorDeDataHora = provedorDeDataHora;
        _logger = logger;
    }

    /// <summary>
    /// Valida as credenciais e emite um token de acesso.
    /// </summary>
    /// <exception cref="ExcecaoDeAutenticacao">
    /// Credenciais invalidas, conta bloqueada por tentativas excessivas ou
    /// usuario inativo.
    /// </exception>
    public async Task<LoginResposta> AutenticarAsync(
        LoginRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var agora = _provedorDeDataHora.Agora;
        var email = NormalizarEmailOuFalhar(requisicao.Email);

        var usuario = await _repositorioDeUsuarios
            .ObterPorEmailAsync(email, cancelamento)
            .ConfigureAwait(false);

        if (usuario is null)
        {
            // Gasta o mesmo tempo de um BCrypt real antes de recusar, para que
            // e-mail inexistente e senha errada sejam indistinguiveis.
            _servicoDeHashDeSenha.Conferir(requisicao.Senha, _servicoDeHashDeSenha.HashDeIsca);

            _logger.LogWarning("Tentativa de login para e-mail nao cadastrado.");
            throw new ExcecaoDeAutenticacao(ExcecaoDeAutenticacao.CredenciaisInvalidas);
        }

        // O bloqueio e checado ANTES da senha: e justamente o que trava a forca bruta.
        if (usuario.EstaBloqueado(agora))
        {
            var minutosRestantes = Math.Max(
                1,
                (int)Math.Ceiling((usuario.BloqueadoAte!.Value - agora).TotalMinutes));

            _logger.LogWarning(
                "Login recusado para o usuario {UsuarioId}: conta bloqueada.", usuario.Id);

            throw new ExcecaoDeAutenticacao(
                $"Conta temporariamente bloqueada por excesso de tentativas. " +
                $"Tente novamente em {minutosRestantes} minuto(s).");
        }

        if (!_servicoDeHashDeSenha.Conferir(requisicao.Senha, usuario.SenhaHash))
        {
            usuario.RegistrarFalhaDeLogin(agora);
            await _repositorioDeUsuarios.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

            _logger.LogWarning("Senha incorreta para o usuario {UsuarioId}.", usuario.Id);
            throw new ExcecaoDeAutenticacao(ExcecaoDeAutenticacao.CredenciaisInvalidas);
        }

        // Verificado somente depois da senha correta: assim a resposta nao revela
        // a existencia de contas inativas para quem nao sabe a senha.
        if (!usuario.Ativo)
        {
            _logger.LogWarning("Login recusado para o usuario inativo {UsuarioId}.", usuario.Id);
            throw new ExcecaoDeAutenticacao("Usuario inativo. Procure o administrador do sistema.");
        }

        usuario.RegistrarAcessoBemSucedido(agora);
        await _repositorioDeUsuarios.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

        var token = _geradorDeToken.Gerar(usuario);

        _logger.LogInformation("Usuario {UsuarioId} autenticado com sucesso.", usuario.Id);

        return new LoginResposta
        {
            Token = token.Token,
            ExpiraEm = token.ExpiraEm,
            Usuario = ParaDto(usuario),
        };
    }

    /// <summary>Recupera os dados do usuario autenticado (endpoint <c>GET /api/auth/eu</c>).</summary>
    public async Task<UsuarioAutenticadoDto> ObterPorIdAsync(
        Guid id,
        CancellationToken cancelamento = default)
    {
        var usuario = await _repositorioDeUsuarios
            .ObterPorIdAsync(id, cancelamento)
            .ConfigureAwait(false);

        if (usuario is null || !usuario.Ativo)
        {
            throw new ExcecaoDeAutenticacao("Sessao invalida. Faca login novamente.");
        }

        return ParaDto(usuario);
    }

    private static UsuarioAutenticadoDto ParaDto(Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Nome,
        Email = usuario.Email,
    };

    /// <summary>
    /// Normaliza o e-mail informado. Um formato invalido nao revela nada sobre a
    /// base, entao vira a mesma mensagem generica de credenciais invalidas.
    /// </summary>
    private static string NormalizarEmailOuFalhar(string email)
    {
        try
        {
            return Usuario.NormalizarEmail(email);
        }
        catch (ExcecaoDeDominio)
        {
            throw new ExcecaoDeAutenticacao(ExcecaoDeAutenticacao.CredenciaisInvalidas);
        }
    }
}
