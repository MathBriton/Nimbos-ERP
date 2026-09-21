using Microsoft.Extensions.Logging;
using Nimbus.Application.Clientes.Modelos;
using Nimbus.Application.Common.Excecoes;
using Nimbus.Application.Common.Modelos;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Application.Clientes;

/// <summary>
/// Casos de uso do cadastro de clientes.
///
/// Este servico e o molde que Fornecedores (Sprint 5), Usuarios (Sprint 6) e
/// Produtos (Sprint 7) replicam: listar paginado, obter por id, criar, atualizar
/// e excluir logicamente.
/// </summary>
public sealed class ServicoDeClientes
{
    private const string NomeDoRecurso = "Cliente";

    private readonly IRepositorioDeClientes _repositorio;
    private readonly ILogger<ServicoDeClientes> _logger;

    public ServicoDeClientes(
        IRepositorioDeClientes repositorio,
        ILogger<ServicoDeClientes> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<ResultadoPaginado<ClienteDto>> ListarAsync(
        ConsultaDeClientes consulta,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        var (itens, total) = await _repositorio
            .ListarAsync(consulta.ParaFiltro(), cancelamento)
            .ConfigureAwait(false);

        return ResultadoPaginado<ClienteDto>.Criar(
            [.. itens.Select(ClienteDto.De)],
            consulta.Pagina,
            consulta.TamanhoDaPagina,
            total);
    }

    public async Task<ClienteDto> ObterPorIdAsync(
        Guid id,
        CancellationToken cancelamento = default)
    {
        var cliente = await BuscarOuFalharAsync(id, cancelamento).ConfigureAwait(false);

        return ClienteDto.De(cliente);
    }

    public async Task<ClienteDto> CriarAsync(
        SalvarClienteRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        await GarantirDocumentoIneditoAsync(
            requisicao.Documento,
            requisicao.TipoDePessoa,
            idParaIgnorar: null,
            cancelamento).ConfigureAwait(false);

        var cliente = Cliente.Criar(
            requisicao.Nome,
            requisicao.TipoDePessoa,
            requisicao.Documento,
            requisicao.NomeFantasia,
            requisicao.Email,
            requisicao.Telefone,
            MontarEndereco(requisicao.Endereco),
            requisicao.Observacoes);

        if (!requisicao.Ativo)
        {
            cliente.Inativar();
        }

        await _repositorio.AdicionarAsync(cliente, cancelamento).ConfigureAwait(false);
        await _repositorio.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

        _logger.LogInformation("Cliente {ClienteId} criado.", cliente.Id);

        return ClienteDto.De(cliente);
    }

    public async Task<ClienteDto> AtualizarAsync(
        Guid id,
        SalvarClienteRequisicao requisicao,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var cliente = await BuscarOuFalharAsync(id, cancelamento).ConfigureAwait(false);

        await GarantirDocumentoIneditoAsync(
            requisicao.Documento,
            requisicao.TipoDePessoa,
            idParaIgnorar: id,
            cancelamento).ConfigureAwait(false);

        // O documento vem antes dos dados cadastrais: e ele que define o tipo
        // de pessoa, do qual depende a validacao do nome fantasia.
        cliente.AlterarDocumento(requisicao.Documento, requisicao.TipoDePessoa);
        cliente.DefinirDadosCadastrais(
            requisicao.Nome,
            requisicao.NomeFantasia,
            requisicao.Observacoes);
        cliente.DefinirContato(requisicao.Email, requisicao.Telefone);
        cliente.DefinirEndereco(MontarEndereco(requisicao.Endereco));

        if (requisicao.Ativo)
        {
            cliente.Ativar();
        }
        else
        {
            cliente.Inativar();
        }

        await _repositorio.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

        _logger.LogInformation("Cliente {ClienteId} atualizado.", cliente.Id);

        return ClienteDto.De(cliente);
    }

    /// <summary>
    /// Exclui o cliente. A exclusao e logica: o interceptor de rastreabilidade
    /// converte o delete em atualizacao do campo Excluido.
    /// </summary>
    public async Task ExcluirAsync(Guid id, CancellationToken cancelamento = default)
    {
        var cliente = await BuscarOuFalharAsync(id, cancelamento).ConfigureAwait(false);

        _repositorio.Remover(cliente);
        await _repositorio.SalvarAlteracoesAsync(cancelamento).ConfigureAwait(false);

        _logger.LogInformation("Cliente {ClienteId} excluido.", id);
    }

    private async Task<Cliente> BuscarOuFalharAsync(Guid id, CancellationToken cancelamento)
    {
        var cliente = await _repositorio
            .ObterPorIdAsync(id, cancelamento)
            .ConfigureAwait(false);

        return cliente
            ?? throw ExcecaoDeRecursoNaoEncontrado.Para(NomeDoRecurso, id);
    }

    /// <summary>
    /// Impede dois clientes com o mesmo CPF/CNPJ.
    ///
    /// Valida o documento antes de consultar o banco para comparar sempre o
    /// numero ja normalizado - sem isso, "123.456.789-09" e "12345678909"
    /// passariam como documentos diferentes.
    /// </summary>
    private async Task GarantirDocumentoIneditoAsync(
        string documento,
        TipoDePessoa tipoDePessoa,
        Guid? idParaIgnorar,
        CancellationToken cancelamento)
    {
        var normalizado = Documento.Criar(documento, tipoDePessoa);

        var jaExiste = await _repositorio
            .ExisteComDocumentoAsync(normalizado.Numero, idParaIgnorar, cancelamento)
            .ConfigureAwait(false);

        if (jaExiste)
        {
            throw new ExcecaoDeConflito(
                $"Ja existe um cliente cadastrado com o documento {normalizado.Formatado()}.");
        }
    }

    private static Endereco? MontarEndereco(EnderecoDto? dto) =>
        dto is null
            ? null
            : Endereco.CriarOuNulo(
                dto.Cep,
                dto.Logradouro,
                dto.Numero,
                dto.Complemento,
                dto.Bairro,
                dto.Cidade,
                dto.Uf);
}
