using Nimbus.Application.Common.Modelos;

namespace Nimbus.Tests.Common;

public class ConsultaPaginadaTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Pagina_invalida_e_normalizada_para_a_primeira(int paginaInformada)
    {
        var consulta = new ConsultaPaginada { Pagina = paginaInformada };

        Assert.Equal(1, consulta.Pagina);
    }

    [Fact]
    public void Tamanho_de_pagina_acima_do_maximo_e_limitado()
    {
        var consulta = new ConsultaPaginada { TamanhoDaPagina = 5_000 };

        Assert.Equal(ConsultaPaginada.TamanhoMaximoDaPagina, consulta.TamanhoDaPagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Tamanho_de_pagina_invalido_volta_para_o_padrao(int tamanhoInformado)
    {
        var consulta = new ConsultaPaginada { TamanhoDaPagina = tamanhoInformado };

        Assert.Equal(ConsultaPaginada.TamanhoPadraoDaPagina, consulta.TamanhoDaPagina);
    }

    [Fact]
    public void Quantidade_para_ignorar_reflete_a_pagina_solicitada()
    {
        var consulta = new ConsultaPaginada { Pagina = 4, TamanhoDaPagina = 25 };

        Assert.Equal(75, consulta.QuantidadeParaIgnorar);
    }

    [Fact]
    public void Consulta_padrao_comeca_na_primeira_pagina()
    {
        var consulta = new ConsultaPaginada();

        Assert.Equal(1, consulta.Pagina);
        Assert.Equal(ConsultaPaginada.TamanhoPadraoDaPagina, consulta.TamanhoDaPagina);
        Assert.Equal(0, consulta.QuantidadeParaIgnorar);
    }
}
