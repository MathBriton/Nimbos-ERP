namespace Nimbus.Application.Common.Modelos;

/// <summary>
/// Parametros de paginacao e busca aceitos pelas listagens.
///
/// Nao e selada: as consultas de cada modulo herdam daqui para acrescentar
/// seus proprios filtros sem reimplementar a normalizacao.
/// Os valores sao normalizados para blindar a API contra
/// requisicoes com pagina zero/negativa ou tamanho abusivo.
/// </summary>
public record ConsultaPaginada
{
    public const int TamanhoMaximoDaPagina = 100;
    public const int TamanhoPadraoDaPagina = 20;

    private readonly int _pagina = 1;
    private readonly int _tamanhoDaPagina = TamanhoPadraoDaPagina;

    public int Pagina
    {
        get => _pagina;
        init => _pagina = value < 1 ? 1 : value;
    }

    public int TamanhoDaPagina
    {
        get => _tamanhoDaPagina;
        init => _tamanhoDaPagina = value switch
        {
            < 1 => TamanhoPadraoDaPagina,
            > TamanhoMaximoDaPagina => TamanhoMaximoDaPagina,
            _ => value,
        };
    }

    /// <summary>Termo de busca livre aplicado pela consulta (opcional).</summary>
    public string? Busca { get; init; }

    public int QuantidadeParaIgnorar => (Pagina - 1) * TamanhoDaPagina;
}
