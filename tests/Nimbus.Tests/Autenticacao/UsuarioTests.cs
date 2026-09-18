using Nimbus.Domain.Common;
using Nimbus.Domain.Entidades;

namespace Nimbus.Tests.Autenticacao;

public class UsuarioTests
{
    private static readonly DateTimeOffset Agora =
        new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);

    private static Usuario CriarValido() =>
        Usuario.Criar("Administrador", "admin@erp.com", "hash-qualquer");

    [Theory]
    [InlineData("ADMIN@ERP.COM", "admin@erp.com")]
    [InlineData("  Admin@Erp.Com  ", "admin@erp.com")]
    public void Email_e_normalizado_para_minusculas_e_sem_espacos(string entrada, string esperado)
    {
        var usuario = Usuario.Criar("Teste", entrada, "hash");

        Assert.Equal(esperado, usuario.Email);
    }

    [Theory]
    [InlineData("sem-arroba.com")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("usuario@sem-ponto")]
    [InlineData("dois@@arrobas.com")]
    [InlineData("com espaco@dominio.com")]
    [InlineData("")]
    [InlineData(null)]
    public void Email_invalido_e_rejeitado(string? email)
    {
        Assert.Throws<ExcecaoDeDominio>(() => Usuario.Criar("Teste", email!, "hash"));
    }

    [Fact]
    public void Usuario_novo_nasce_ativo_e_apto_a_autenticar()
    {
        var usuario = CriarValido();

        Assert.True(usuario.Ativo);
        Assert.True(usuario.PodeAutenticar(Agora));
        Assert.Null(usuario.UltimoAcessoEm);
        Assert.Equal(0, usuario.TentativasDeLoginFalhas);
    }

    [Fact]
    public void Usuario_inativo_nao_pode_autenticar()
    {
        var usuario = CriarValido();

        usuario.Inativar();

        Assert.False(usuario.PodeAutenticar(Agora));
    }

    [Fact]
    public void Falhas_abaixo_do_limite_nao_bloqueiam_a_conta()
    {
        var usuario = CriarValido();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio - 1; tentativa++)
        {
            usuario.RegistrarFalhaDeLogin(Agora);
        }

        Assert.False(usuario.EstaBloqueado(Agora));
        Assert.Equal(Usuario.TentativasAntesDoBloqueio - 1, usuario.TentativasDeLoginFalhas);
    }

    [Fact]
    public void Atingir_o_limite_de_falhas_bloqueia_a_conta()
    {
        var usuario = CriarValido();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            usuario.RegistrarFalhaDeLogin(Agora);
        }

        Assert.True(usuario.EstaBloqueado(Agora));
        Assert.False(usuario.PodeAutenticar(Agora));
        Assert.Equal(Agora.Add(Usuario.DuracaoDoBloqueio), usuario.BloqueadoAte);
    }

    [Fact]
    public void Bloqueio_expira_depois_da_duracao_configurada()
    {
        var usuario = CriarValido();

        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            usuario.RegistrarFalhaDeLogin(Agora);
        }

        var depoisDoBloqueio = Agora.Add(Usuario.DuracaoDoBloqueio).AddSeconds(1);

        Assert.False(usuario.EstaBloqueado(depoisDoBloqueio));
        Assert.True(usuario.PodeAutenticar(depoisDoBloqueio));
    }

    [Fact]
    public void Acesso_bem_sucedido_zera_falhas_e_registra_o_horario()
    {
        var usuario = CriarValido();
        usuario.RegistrarFalhaDeLogin(Agora);
        usuario.RegistrarFalhaDeLogin(Agora);

        usuario.RegistrarAcessoBemSucedido(Agora);

        Assert.Equal(0, usuario.TentativasDeLoginFalhas);
        Assert.Null(usuario.BloqueadoAte);
        Assert.Equal(Agora, usuario.UltimoAcessoEm);
    }

    [Fact]
    public void Trocar_a_senha_libera_a_conta_bloqueada()
    {
        var usuario = CriarValido();
        for (var tentativa = 0; tentativa < Usuario.TentativasAntesDoBloqueio; tentativa++)
        {
            usuario.RegistrarFalhaDeLogin(Agora);
        }

        usuario.DefinirSenhaHash("novo-hash");

        Assert.False(usuario.EstaBloqueado(Agora));
        Assert.Equal("novo-hash", usuario.SenhaHash);
    }

    [Fact]
    public void Nome_em_branco_e_rejeitado()
    {
        Assert.Throws<ExcecaoDeDominio>(() => Usuario.Criar("   ", "admin@erp.com", "hash"));
    }

    [Fact]
    public void Hash_de_senha_em_branco_e_rejeitado()
    {
        Assert.Throws<ExcecaoDeDominio>(() => Usuario.Criar("Teste", "admin@erp.com", ""));
    }
}
