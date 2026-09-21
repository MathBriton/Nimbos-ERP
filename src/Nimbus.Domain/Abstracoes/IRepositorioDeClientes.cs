using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Domain.Abstracoes;

/// <summary>
/// Campos pelos quais a listagem de clientes pode ser ordenada.
///
/// E um enum, e nao um nome de coluna em texto, de proposito: ordenacao vinda
/// de string abriria espaco para injecao e para quebra silenciosa quando uma
/// propriedade for renomeada.
/// </summary>
public enum OrdenacaoDeClientes
{
    Nome = 0,
    Documento = 1,
    CriadoEm = 2,
}

/// <summary>Criterios de busca da listagem de clientes.</summary>
public sealed record FiltroDeClientes
{
    /// <summary>Texto livre, casado contra nome, nome fantasia, documento e e-mail.</summary>
    public string? Busca { get; init; }

    /// <summary>Quando nulo, traz ativos e inativos.</summary>
    public bool? Ativo { get; init; }

    /// <summary>Quando nulo, traz pessoas fisicas e juridicas.</summary>
    public TipoDePessoa? TipoDePessoa { get; init; }

    public OrdenacaoDeClientes Ordenacao { get; init; } = OrdenacaoDeClientes.Nome;

    public bool Descendente { get; init; }

    public int QuantidadeParaIgnorar { get; init; }

    public int QuantidadeParaTrazer { get; init; } = 20;
}

/// <summary>Porta de acesso aos clientes.</summary>
public interface IRepositorioDeClientes
{
    /// <summary>
    /// Pagina os clientes que casam com o filtro.
    /// Devolve tambem o total de registros correspondentes, que a paginacao da
    /// tela precisa para saber quantas paginas existem.
    /// </summary>
    Task<(IReadOnlyList<Cliente> Itens, int Total)> ListarAsync(
        FiltroDeClientes filtro,
        CancellationToken cancelamento = default);

    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancelamento = default);

    /// <summary>
    /// Indica se ja existe outro cliente com o mesmo documento.
    /// <paramref name="idParaIgnorar"/> exclui o proprio registro da checagem
    /// durante uma edicao.
    /// </summary>
    Task<bool> ExisteComDocumentoAsync(
        string numeroDoDocumento,
        Guid? idParaIgnorar = null,
        CancellationToken cancelamento = default);

    Task AdicionarAsync(Cliente cliente, CancellationToken cancelamento = default);

    /// <summary>
    /// Marca o cliente para exclusao. O interceptor de rastreabilidade converte
    /// em exclusao logica, entao o registro permanece na tabela.
    /// </summary>
    void Remover(Cliente cliente);

    Task SalvarAlteracoesAsync(CancellationToken cancelamento = default);
}
