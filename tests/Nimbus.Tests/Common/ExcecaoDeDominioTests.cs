using Nimbus.Domain.Common;

namespace Nimbus.Tests.Common;

public class ExcecaoDeDominioTests
{
    [Fact]
    public void SeVerdadeiro_lanca_quando_a_condicao_e_violada()
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => ExcecaoDeDominio.SeVerdadeiro(true, "Saldo insuficiente."));

        Assert.Equal("Saldo insuficiente.", excecao.Message);
    }

    [Fact]
    public void SeVerdadeiro_nao_lanca_quando_a_condicao_e_falsa()
    {
        ExcecaoDeDominio.SeVerdadeiro(false, "Nao deveria lancar.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TextoObrigatorio_rejeita_valor_vazio(string? valor)
    {
        var excecao = Assert.Throws<ExcecaoDeDominio>(
            () => ExcecaoDeDominio.TextoObrigatorio(valor, "Nome"));

        Assert.Contains("Nome", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TextoObrigatorio_devolve_o_valor_sem_espacos_nas_bordas()
    {
        var resultado = ExcecaoDeDominio.TextoObrigatorio("  Teclado Mecanico  ", "Descricao");

        Assert.Equal("Teclado Mecanico", resultado);
    }
}
