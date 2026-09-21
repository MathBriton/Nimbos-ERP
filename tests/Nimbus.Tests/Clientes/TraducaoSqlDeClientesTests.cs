using Microsoft.EntityFrameworkCore;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.ObjetosDeValor;
using Nimbus.Infrastructure.Persistencia;

namespace Nimbus.Tests.Clientes;

/// <summary>
/// Verifica que a consulta da listagem traduz para SQL Server.
///
/// Os testes de caso de uso rodam sobre um repositorio em memoria, que nunca
/// revelaria uma expressao LINQ intraduzivel - esse erro so apareceria em
/// tempo de execucao, com o banco de pe. Aqui o provider do SQL Server monta a
/// consulta de verdade e falha se algo nao traduzir, sem precisar de conexao:
/// ToQueryString gera o SQL sem executar nada.
/// </summary>
public class TraducaoSqlDeClientesTests : IDisposable
{
    private readonly NimbusDbContext _contexto;

    public TraducaoSqlDeClientesTests()
    {
        var opcoes = new DbContextOptionsBuilder<NimbusDbContext>()
            // Connection string valida em formato, porem nunca aberta.
            .UseSqlServer("Server=nao-conecta;Database=Nimbus;Trusted_Connection=True;")
            .Options;

        _contexto = new NimbusDbContext(opcoes);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Consulta_base_traduz_e_aplica_o_filtro_de_exclusao_logica()
    {
        var sql = _contexto.Clientes.AsNoTracking().ToQueryString();

        Assert.Contains("Clientes", sql, StringComparison.Ordinal);
        // O filtro global de consulta precisa estar no SQL, senao registros
        // excluidos logicamente voltariam nas listagens.
        Assert.Contains("Excluido", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Busca_por_texto_traduz_para_LIKE()
    {
        var termo = "acme";

        var sql = _contexto.Clientes
            .AsNoTracking()
            .Where(cliente =>
                EF.Functions.Like(cliente.Nome, $"%{termo}%")
                || (cliente.NomeFantasia != null && EF.Functions.Like(cliente.NomeFantasia, $"%{termo}%"))
                || (cliente.Email != null && EF.Functions.Like(cliente.Email, $"%{termo}%"))
                || EF.Functions.Like(cliente.Documento, $"%{termo}%"))
            .ToQueryString();

        // Este e o ponto que motivou guardar o documento como texto simples:
        // com um objeto de valor convertido, o LIKE nao traduziria.
        Assert.Contains("LIKE", sql, StringComparison.Ordinal);
        Assert.Contains("Documento", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Filtro_por_tipo_de_pessoa_traduz_para_coluna()
    {
        var sql = _contexto.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.TipoDePessoa == TipoDePessoa.Juridica)
            .ToQueryString();

        Assert.Contains("TipoDePessoa", sql, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(OrdenacaoDeClientes.Nome, "Nome")]
    [InlineData(OrdenacaoDeClientes.Documento, "Documento")]
    [InlineData(OrdenacaoDeClientes.CriadoEm, "CriadoEm")]
    public void Ordenacao_traduz_para_ORDER_BY_na_coluna_certa(
        OrdenacaoDeClientes ordenacao,
        string colunaEsperada)
    {
        var consulta = _contexto.Clientes.AsNoTracking();

        var ordenada = ordenacao switch
        {
            OrdenacaoDeClientes.Documento => consulta.OrderBy(c => c.Documento),
            OrdenacaoDeClientes.CriadoEm => consulta.OrderBy(c => c.CriadoEm),
            _ => consulta.OrderBy(c => c.Nome).ThenBy(c => c.Id),
        };

        var sql = ordenada.ToQueryString();

        Assert.Contains("ORDER BY", sql, StringComparison.Ordinal);
        Assert.Contains(colunaEsperada, sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Paginacao_traduz_para_OFFSET_e_FETCH()
    {
        var sql = _contexto.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nome)
            .Skip(20)
            .Take(10)
            .ToQueryString();

        Assert.Contains("OFFSET", sql, StringComparison.Ordinal);
        Assert.Contains("FETCH NEXT", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Endereco_e_mapeado_em_colunas_da_propria_tabela()
    {
        var sql = _contexto.Clientes.AsNoTracking().ToQueryString();

        // Tipo owned na mesma tabela: sem JOIN.
        Assert.Contains("EnderecoCidade", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("JOIN", sql, StringComparison.Ordinal);
    }

    [Fact]
    public void Modelo_expoe_o_indice_unico_de_documento_filtrado_por_excluido()
    {
        var tipo = _contexto.Model.FindEntityType(typeof(Domain.Entidades.Cliente));
        Assert.NotNull(tipo);

        var indice = tipo.GetIndexes().Single(indice =>
            indice.Properties.Count == 1
            && indice.Properties[0].Name == nameof(Domain.Entidades.Cliente.Documento));

        Assert.True(indice.IsUnique);
        // O filtro e o que permite recadastrar o documento de um excluido.
        Assert.Equal("[Excluido] = 0", indice.GetFilter());
    }
}
