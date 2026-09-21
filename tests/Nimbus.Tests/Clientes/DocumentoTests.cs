using Nimbus.Domain.Common;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Tests.Clientes;

public class DocumentoTests
{
    // CPFs e CNPJs com digitos verificadores corretos, gerados para teste.
    private const string CpfValido = "52998224725";
    private const string CpfValidoFormatado = "529.982.247-25";
    private const string CnpjValido = "11222333000181";
    private const string CnpjValidoFormatado = "11.222.333/0001-81";

    [Theory]
    [InlineData(CpfValido)]
    [InlineData(CpfValidoFormatado)]
    [InlineData("  529.982.247-25  ")]
    public void CPF_valido_e_aceito_com_ou_sem_mascara(string entrada)
    {
        var documento = Documento.Criar(entrada, TipoDePessoa.Fisica);

        // Sempre normalizado: a mascara nao chega ao banco.
        Assert.Equal(CpfValido, documento.Numero);
        Assert.Equal(TipoDePessoa.Fisica, documento.TipoDePessoa);
    }

    [Theory]
    [InlineData("52998224724")] // ultimo digito verificador errado
    [InlineData("52998224715")] // penultimo digito verificador errado
    [InlineData("12345678901")]
    public void CPF_com_digito_verificador_errado_e_rejeitado(string cpf)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar(cpf, TipoDePessoa.Fisica));

        Assert.Contains("CPF invalido", excecao.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void CPF_com_todos_os_digitos_iguais_e_rejeitado(string cpf)
    {
        // Passam no calculo do DV, mas a Receita Federal nao os emite.
        Assert.Throws<ExcecaoDeDominio>(() => Documento.Criar(cpf, TipoDePessoa.Fisica));
    }

    [Theory]
    [InlineData("5299822472")]
    [InlineData("529982247250")]
    public void CPF_com_tamanho_errado_e_rejeitado(string cpf)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar(cpf, TipoDePessoa.Fisica));

        Assert.Contains("11 digitos", excecao.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CnpjValido)]
    [InlineData(CnpjValidoFormatado)]
    public void CNPJ_valido_e_aceito_com_ou_sem_mascara(string entrada)
    {
        var documento = Documento.Criar(entrada, TipoDePessoa.Juridica);

        Assert.Equal(CnpjValido, documento.Numero);
        Assert.Equal(TipoDePessoa.Juridica, documento.TipoDePessoa);
    }

    [Theory]
    [InlineData("11222333000182")] // ultimo digito errado
    [InlineData("11222333000191")] // penultimo digito errado
    [InlineData("12345678000100")]
    public void CNPJ_com_digito_verificador_errado_e_rejeitado(string cnpj)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar(cnpj, TipoDePessoa.Juridica));

        Assert.Contains("CNPJ invalido", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CNPJ_com_todos_os_digitos_iguais_e_rejeitado()
    {
        Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar("11111111111111", TipoDePessoa.Juridica));
    }

    [Fact]
    public void CPF_informado_como_CNPJ_e_rejeitado_pelo_tamanho()
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar(CpfValido, TipoDePessoa.Juridica));

        Assert.Contains("14 digitos", excecao.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public void Documento_vazio_ou_sem_digitos_e_rejeitado(string? valor)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Documento.Criar(valor, TipoDePessoa.Fisica));

        Assert.Contains("Informe o CPF", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Tipo_de_pessoa_e_derivado_do_tamanho()
    {
        Assert.Equal(TipoDePessoa.Fisica, Documento.DoBanco(CpfValido).TipoDePessoa);
        Assert.Equal(TipoDePessoa.Juridica, Documento.DoBanco(CnpjValido).TipoDePessoa);
    }

    [Fact]
    public void Formatacao_aplica_a_mascara_certa_para_cada_tipo()
    {
        Assert.Equal(CpfValidoFormatado, Documento.Formatar(CpfValido));
        Assert.Equal(CnpjValidoFormatado, Documento.Formatar(CnpjValido));
    }

    [Fact]
    public void EhValido_responde_sem_lancar_excecao()
    {
        Assert.True(Documento.EhValido(CpfValido, TipoDePessoa.Fisica));
        Assert.False(Documento.EhValido("12345678901", TipoDePessoa.Fisica));
        Assert.True(Documento.EhValido(CnpjValidoFormatado, TipoDePessoa.Juridica));
        Assert.False(Documento.EhValido(null, TipoDePessoa.Juridica));
    }

    [Fact]
    public void Dois_documentos_com_o_mesmo_numero_sao_iguais()
    {
        // Comportamento de objeto de valor: igualdade por valor, nao por referencia.
        var primeiro = Documento.Criar(CpfValidoFormatado, TipoDePessoa.Fisica);
        var segundo = Documento.Criar(CpfValido, TipoDePessoa.Fisica);

        Assert.Equal(primeiro, segundo);
    }
}
