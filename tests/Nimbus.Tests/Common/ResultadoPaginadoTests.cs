using Nimbus.Application.Common.Modelos;

namespace Nimbus.Tests.Common;

public class ResultadoPaginadoTests
{
    [Theory]
    [InlineData(100, 20, 5)]
    [InlineData(101, 20, 6)]
    [InlineData(1, 20, 1)]
    [InlineData(0, 20, 0)]
    public void Total_de_paginas_arredonda_para_cima(int totalDeItens, int tamanho, int esperado)
    {
        var resultado = ResultadoPaginado<string>.Criar([], paginaAtual: 1, tamanho, totalDeItens);

        Assert.Equal(esperado, resultado.TotalDePaginas);
    }

    [Fact]
    public void Primeira_pagina_nao_tem_anterior_mas_tem_proxima()
    {
        var resultado = ResultadoPaginado<string>.Criar(
            ["a"], paginaAtual: 1, tamanhoDaPagina: 1, totalDeItens: 3);

        Assert.False(resultado.TemPaginaAnterior);
        Assert.True(resultado.TemProximaPagina);
    }

    [Fact]
    public void Ultima_pagina_nao_tem_proxima()
    {
        var resultado = ResultadoPaginado<string>.Criar(
            ["c"], paginaAtual: 3, tamanhoDaPagina: 1, totalDeItens: 3);

        Assert.True(resultado.TemPaginaAnterior);
        Assert.False(resultado.TemProximaPagina);
    }

    [Fact]
    public void Resultado_vazio_nao_tem_itens_nem_paginas()
    {
        var resultado = ResultadoPaginado<string>.Vazio(tamanhoDaPagina: 20);

        Assert.Empty(resultado.Itens);
        Assert.Equal(0, resultado.TotalDeItens);
        Assert.Equal(0, resultado.TotalDePaginas);
        Assert.False(resultado.TemPaginaAnterior);
        Assert.False(resultado.TemProximaPagina);
    }
}
