using Nimbus.Domain.Common;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Tests.Clientes;

public class EnderecoTests
{
    private static Endereco? Criar(
        string? cep = "01310-100",
        string? logradouro = "Avenida Paulista",
        string? numero = "1578",
        string? complemento = null,
        string? bairro = "Bela Vista",
        string? cidade = "Sao Paulo",
        string? uf = "SP") =>
        Endereco.CriarOuNulo(cep, logradouro, numero, complemento, bairro, cidade, uf);

    [Fact]
    public void Endereco_completo_e_aceito_e_o_cep_normalizado()
    {
        var endereco = Criar();

        Assert.NotNull(endereco);
        Assert.Equal("01310100", endereco.Cep);
        Assert.Equal("01310-100", endereco.CepFormatado());
        Assert.Equal("SP", endereco.Uf);
    }

    [Fact]
    public void Todos_os_campos_vazios_significam_endereco_nao_informado()
    {
        // Endereco e opcional: nao preencher nada nao e erro.
        var endereco = Endereco.CriarOuNulo(null, null, null, null, null, null, null);

        Assert.Null(endereco);
    }

    [Fact]
    public void Preenchimento_parcial_e_rejeitado()
    {
        // Meio endereco nao serve nem para entrega nem para nota fiscal.
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => Criar(logradouro: null));

        Assert.Contains("Logradouro", excecao.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("0131010")]
    [InlineData("013101000")]
    [InlineData("abc")]
    public void CEP_com_tamanho_errado_e_rejeitado(string cep)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(() => Criar(cep: cep));

        Assert.Contains("8 digitos", excecao.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("SSP")]
    [InlineData("S")]
    public void UF_inexistente_e_rejeitada(string uf)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(() => Criar(uf: uf));

        Assert.Contains("UF invalida", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UF_em_minusculas_e_normalizada()
    {
        var endereco = Criar(uf: "sp");

        Assert.Equal("SP", endereco?.Uf);
    }

    [Fact]
    public void Todas_as_27_unidades_federativas_sao_aceitas()
    {
        Assert.Equal(27, Endereco.UnidadesFederativas.Count);

        foreach (var uf in Endereco.UnidadesFederativas)
        {
            Assert.NotNull(Criar(uf: uf));
        }
    }

    [Fact]
    public void Complemento_e_opcional_mesmo_com_o_resto_preenchido()
    {
        var endereco = Criar(complemento: "   ");

        Assert.NotNull(endereco);
        Assert.Null(endereco.Complemento);
    }

    [Fact]
    public void Endereco_em_uma_linha_inclui_o_complemento_quando_houver()
    {
        var semComplemento = Criar();
        var comComplemento = Criar(complemento: "Sala 12");

        Assert.Equal(
            "Avenida Paulista, 1578, Bela Vista, Sao Paulo/SP, 01310-100",
            semComplemento?.EmUmaLinha());

        Assert.Equal(
            "Avenida Paulista, 1578 - Sala 12, Bela Vista, Sao Paulo/SP, 01310-100",
            comComplemento?.EmUmaLinha());
    }

    [Fact]
    public void Preencher_so_o_complemento_ja_exige_o_endereco_inteiro()
    {
        // Qualquer campo preenchido tira o endereco do estado "nao informado".
        Assert.Throws<ExcecaoDeDominio>(
            () => Endereco.CriarOuNulo(null, null, null, "Fundos", null, null, null));
    }
}
