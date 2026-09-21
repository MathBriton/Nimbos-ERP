using Nimbus.Application.Common.Modelos;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Application.Clientes.Modelos;

/// <summary>
/// Parametros aceitos por <c>GET /api/clientes</c>.
/// Herda paginacao e busca de <see cref="ConsultaPaginada"/> e acrescenta os
/// filtros proprios do modulo.
/// </summary>
public sealed record ConsultaDeClientes : ConsultaPaginada
{
    /// <summary>Quando nulo, traz ativos e inativos.</summary>
    public bool? Ativo { get; init; }

    /// <summary>Quando nulo, traz pessoas fisicas e juridicas.</summary>
    public TipoDePessoa? TipoDePessoa { get; init; }

    public OrdenacaoDeClientes Ordenacao { get; init; } = OrdenacaoDeClientes.Nome;

    public bool Descendente { get; init; }

    /// <summary>Traduz para o filtro que o repositorio entende.</summary>
    public FiltroDeClientes ParaFiltro() => new()
    {
        Busca = string.IsNullOrWhiteSpace(Busca) ? null : Busca.Trim(),
        Ativo = Ativo,
        TipoDePessoa = TipoDePessoa,
        Ordenacao = Ordenacao,
        Descendente = Descendente,
        QuantidadeParaIgnorar = QuantidadeParaIgnorar,
        QuantidadeParaTrazer = TamanhoDaPagina,
    };
}
