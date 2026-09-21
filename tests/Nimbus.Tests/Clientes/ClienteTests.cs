using Nimbus.Domain.Common;
using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Tests.Clientes;

public class ClienteTests
{
    private const string CpfValido = "529.982.247-25";
    private const string CnpjValido = "11.222.333/0001-81";

    private static Cliente PessoaFisica() =>
        Cliente.Criar("Maria Silva", TipoDePessoa.Fisica, CpfValido);

    private static Cliente PessoaJuridica() =>
        Cliente.Criar("Acme Ltda", TipoDePessoa.Juridica, CnpjValido, nomeFantasia: "Acme");

    [Fact]
    public void Cliente_novo_nasce_ativo_com_documento_normalizado()
    {
        var cliente = PessoaFisica();

        Assert.True(cliente.Ativo);
        Assert.Equal("52998224725", cliente.Documento);
        Assert.Equal("529.982.247-25", cliente.DocumentoFormatado());
        Assert.Equal(TipoDePessoa.Fisica, cliente.TipoDePessoa);
        Assert.False(cliente.Excluido);
    }

    [Fact]
    public void Pessoa_juridica_aceita_nome_fantasia()
    {
        var cliente = PessoaJuridica();

        Assert.Equal("Acme", cliente.NomeFantasia);
        Assert.Equal(TipoDePessoa.Juridica, cliente.TipoDePessoa);
    }

    [Fact]
    public void Pessoa_fisica_nao_aceita_nome_fantasia()
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Cliente.Criar("Maria", TipoDePessoa.Fisica, CpfValido, nomeFantasia: "Lojinha"));

        Assert.Contains("pessoa juridica", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Nome_em_branco_e_rejeitado()
    {
        Assert.Throws<ExcecaoDeDominio>(
            () => Cliente.Criar("   ", TipoDePessoa.Fisica, CpfValido));
    }

    [Fact]
    public void Nome_longo_demais_e_rejeitado()
    {
        var nomeGigante = new string('a', 201);

        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Cliente.Criar(nomeGigante, TipoDePessoa.Fisica, CpfValido));

        Assert.Contains("200 caracteres", excecao.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------
    // Contato
    // ---------------------------------------------------------------------

    [Fact]
    public void Email_e_normalizado_para_minusculas()
    {
        var cliente = PessoaFisica();

        cliente.DefinirContato("  Maria@Empresa.COM  ", null);

        Assert.Equal("maria@empresa.com", cliente.Email);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("a@b")]
    [InlineData("dois@@arrobas.com")]
    public void Email_invalido_e_rejeitado(string email)
    {
        var cliente = PessoaFisica();

        Assert.Throws<ExcecaoDeDominio>(() => cliente.DefinirContato(email, null));
    }

    [Fact]
    public void Contato_vazio_limpa_os_campos()
    {
        var cliente = PessoaFisica();
        cliente.DefinirContato("maria@empresa.com", "11987654321");

        cliente.DefinirContato(null, null);

        Assert.Null(cliente.Email);
        Assert.Null(cliente.Telefone);
    }

    [Theory]
    [InlineData("(11) 98765-4321", "11987654321", "(11) 98765-4321")]
    [InlineData("1133334444", "1133334444", "(11) 3333-4444")]
    public void Telefone_e_normalizado_e_formatado(
        string entrada,
        string esperadoArmazenado,
        string esperadoFormatado)
    {
        var cliente = PessoaFisica();

        cliente.DefinirContato(null, entrada);

        Assert.Equal(esperadoArmazenado, cliente.Telefone);
        Assert.Equal(esperadoFormatado, cliente.TelefoneFormatado());
    }

    [Theory]
    [InlineData("987654321")] // sem DDD
    [InlineData("119876543210")] // digitos demais
    public void Telefone_com_quantidade_invalida_de_digitos_e_rejeitado(string telefone)
    {
        var cliente = PessoaFisica();

        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => cliente.DefinirContato(null, telefone));

        Assert.Contains("10 ou 11 digitos", excecao.Message, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------------
    // Troca de documento
    // ---------------------------------------------------------------------

    [Fact]
    public void Trocar_o_documento_atualiza_o_tipo_de_pessoa_junto()
    {
        var cliente = PessoaFisica();

        cliente.AlterarDocumento(CnpjValido, TipoDePessoa.Juridica);

        Assert.Equal("11222333000181", cliente.Documento);
        Assert.Equal(TipoDePessoa.Juridica, cliente.TipoDePessoa);
    }

    [Fact]
    public void Virar_pessoa_fisica_com_nome_fantasia_preenchido_e_bloqueado()
    {
        var cliente = PessoaJuridica();

        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => cliente.AlterarDocumento(CpfValido, TipoDePessoa.Fisica));

        Assert.Contains("nome fantasia", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Virar_pessoa_fisica_funciona_depois_de_limpar_o_nome_fantasia()
    {
        var cliente = PessoaJuridica();

        cliente.DefinirDadosCadastrais("Acme Ltda", nomeFantasia: null, observacoes: null);
        cliente.AlterarDocumento(CpfValido, TipoDePessoa.Fisica);

        Assert.Equal(TipoDePessoa.Fisica, cliente.TipoDePessoa);
        Assert.Null(cliente.NomeFantasia);
    }

    [Fact]
    public void Documento_invalido_na_troca_nao_altera_o_cliente()
    {
        var cliente = PessoaFisica();

        Assert.Throws<ExcecaoDeDominio>(
            () => cliente.AlterarDocumento("12345678901", TipoDePessoa.Fisica));

        // O estado anterior permanece intacto.
        Assert.Equal("52998224725", cliente.Documento);
    }

    // ---------------------------------------------------------------------
    // Ativacao
    // ---------------------------------------------------------------------

    [Fact]
    public void Inativar_e_reativar_alternam_o_estado()
    {
        var cliente = PessoaFisica();

        cliente.Inativar();
        Assert.False(cliente.Ativo);

        cliente.Ativar();
        Assert.True(cliente.Ativo);
    }
}
