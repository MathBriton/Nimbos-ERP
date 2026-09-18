namespace Nimbus.Application.Common.Modelos;

/// <summary>
/// Envelope padrao de respostas paginadas. Todas as listagens do ERP
/// (Clientes, Produtos, Pedidos...) retornam neste formato para que o
/// frontend possa usar um unico componente de DataTable paginada.
/// </summary>
/// <typeparam name="T">Tipo do item retornado (normalmente um DTO).</typeparam>
public sealed record ResultadoPaginado<T>
{
    public required IReadOnlyList<T> Itens { get; init; }

    public required int PaginaAtual { get; init; }

    public required int TamanhoDaPagina { get; init; }

    public required int TotalDeItens { get; init; }

    public int TotalDePaginas =>
        TamanhoDaPagina <= 0 ? 0 : (int)Math.Ceiling(TotalDeItens / (double)TamanhoDaPagina);

    public bool TemPaginaAnterior => PaginaAtual > 1;

    public bool TemProximaPagina => PaginaAtual < TotalDePaginas;

    public static ResultadoPaginado<T> Criar(
        IReadOnlyList<T> itens,
        int paginaAtual,
        int tamanhoDaPagina,
        int totalDeItens) => new()
        {
            Itens = itens,
            PaginaAtual = paginaAtual,
            TamanhoDaPagina = tamanhoDaPagina,
            TotalDeItens = totalDeItens,
        };

    public static ResultadoPaginado<T> Vazio(int tamanhoDaPagina) =>
        Criar([], paginaAtual: 1, tamanhoDaPagina, totalDeItens: 0);
}
