using Microsoft.Extensions.Logging.Abstractions;
using Nimbus.Application.Clientes;
using Nimbus.Application.Clientes.Modelos;
using Nimbus.Application.Common.Excecoes;
using Nimbus.Domain.Abstracoes;
using Nimbus.Domain.Common;
using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;
using Nimbus.Tests.Dobras;

namespace Nimbus.Tests.Clientes;

public class ServicoDeClientesTests
{
    private const string CpfValido = "529.982.247-25";
    private const string OutroCpfValido = "168.995.350-09";
    private const string CnpjValido = "11.222.333/0001-81";

    private readonly RepositorioDeClientesEmMemoria _repositorio = new();
    private readonly ServicoDeClientes _servico;

    public ServicoDeClientesTests() =>
        _servico = new ServicoDeClientes(_repositorio, NullLogger<ServicoDeClientes>.Instance);

    private static SalvarClienteRequisicao Requisicao(
        string nome = "Maria Silva",
        TipoDePessoa tipo = TipoDePessoa.Fisica,
        string documento = CpfValido,
        string? nomeFantasia = null,
        string? email = null,
        string? telefone = null,
        EnderecoDto? endereco = null,
        bool ativo = true) => new()
        {
            Nome = nome,
            TipoDePessoa = tipo,
            Documento = documento,
            NomeFantasia = nomeFantasia,
            Email = email,
            Telefone = telefone,
            Endereco = endereco,
            Ativo = ativo,
        };

    // ---------------------------------------------------------------------
    // Criacao
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Criar_persiste_o_cliente_normalizado()
    {
        var dto = await _servico.CriarAsync(Requisicao(email: "MARIA@EMPRESA.COM"));

        Assert.Equal("Maria Silva", dto.Nome);
        Assert.Equal("52998224725", dto.Documento);
        Assert.Equal("529.982.247-25", dto.DocumentoFormatado);
        Assert.Equal("maria@empresa.com", dto.Email);
        Assert.True(dto.Ativo);
        Assert.Equal(1, _repositorio.QuantidadeDeSalvamentos);
    }

    [Fact]
    public async Task Criar_com_documento_repetido_e_recusado_com_conflito()
    {
        await _servico.CriarAsync(Requisicao());

        var excecao = await Assert.ThrowsAsync<ExcecaoDeConflito>(
            () => _servico.CriarAsync(Requisicao(nome: "Outra pessoa")));

        Assert.Contains("529.982.247-25", excecao.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Criar_detecta_documento_repetido_mesmo_com_mascara_diferente()
    {
        // Sem normalizar antes de consultar, "529.982.247-25" e "52998224725"
        // passariam como documentos distintos.
        await _servico.CriarAsync(Requisicao(documento: "52998224725"));

        await Assert.ThrowsAsync<ExcecaoDeConflito>(
            () => _servico.CriarAsync(Requisicao(documento: "529.982.247-25")));
    }

    [Fact]
    public async Task Criar_com_documento_invalido_falha_antes_de_tocar_o_repositorio()
    {
        await Assert.ThrowsAsync<ExcecaoDeDominio>(
            () => _servico.CriarAsync(Requisicao(documento: "12345678901")));

        Assert.Empty(_repositorio.Todos);
        Assert.Equal(0, _repositorio.QuantidadeDeSalvamentos);
    }

    [Fact]
    public async Task Criar_inativo_respeita_a_escolha()
    {
        var dto = await _servico.CriarAsync(Requisicao(ativo: false));

        Assert.False(dto.Ativo);
    }

    [Fact]
    public async Task Criar_com_endereco_completo_guarda_o_endereco()
    {
        var dto = await _servico.CriarAsync(Requisicao(endereco: new EnderecoDto
        {
            Cep = "01310-100",
            Logradouro = "Avenida Paulista",
            Numero = "1578",
            Bairro = "Bela Vista",
            Cidade = "Sao Paulo",
            Uf = "sp",
        }));

        Assert.NotNull(dto.Endereco);
        Assert.Equal("01310100", dto.Endereco.Cep);
        Assert.Equal("SP", dto.Endereco.Uf);
    }

    [Fact]
    public async Task Criar_com_endereco_todo_vazio_nao_guarda_endereco()
    {
        var dto = await _servico.CriarAsync(Requisicao(endereco: new EnderecoDto()));

        Assert.Null(dto.Endereco);
    }

    // ---------------------------------------------------------------------
    // Consulta
    // ---------------------------------------------------------------------

    [Fact]
    public async Task ObterPorId_devolve_o_cliente()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        var encontrado = await _servico.ObterPorIdAsync(criado.Id);

        Assert.Equal(criado.Id, encontrado.Id);
    }

    [Fact]
    public async Task ObterPorId_inexistente_resulta_em_nao_encontrado()
    {
        await Assert.ThrowsAsync<ExcecaoDeRecursoNaoEncontrado>(
            () => _servico.ObterPorIdAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Listar_pagina_os_resultados_e_informa_o_total()
    {
        _repositorio.Semear(
            Cliente.Criar("Ana", TipoDePessoa.Fisica, CpfValido),
            Cliente.Criar("Bruno", TipoDePessoa.Fisica, OutroCpfValido),
            Cliente.Criar("Acme Ltda", TipoDePessoa.Juridica, CnpjValido));

        var pagina = await _servico.ListarAsync(new ConsultaDeClientes
        {
            Pagina = 1,
            TamanhoDaPagina = 2,
        });

        Assert.Equal(2, pagina.Itens.Count);
        // O total e de registros que casam com o filtro, nao o tamanho da pagina.
        Assert.Equal(3, pagina.TotalDeItens);
        Assert.Equal(2, pagina.TotalDePaginas);
        Assert.True(pagina.TemProximaPagina);
        Assert.False(pagina.TemPaginaAnterior);
    }

    [Fact]
    public async Task Listar_segunda_pagina_traz_o_restante()
    {
        _repositorio.Semear(
            Cliente.Criar("Ana", TipoDePessoa.Fisica, CpfValido),
            Cliente.Criar("Bruno", TipoDePessoa.Fisica, OutroCpfValido),
            Cliente.Criar("Carlos Ltda", TipoDePessoa.Juridica, CnpjValido));

        var pagina = await _servico.ListarAsync(new ConsultaDeClientes
        {
            Pagina = 2,
            TamanhoDaPagina = 2,
        });

        Assert.Single(pagina.Itens);
        Assert.True(pagina.TemPaginaAnterior);
        Assert.False(pagina.TemProximaPagina);
    }

    [Fact]
    public async Task Listar_filtra_por_tipo_de_pessoa()
    {
        _repositorio.Semear(
            Cliente.Criar("Ana", TipoDePessoa.Fisica, CpfValido),
            Cliente.Criar("Acme Ltda", TipoDePessoa.Juridica, CnpjValido));

        var pagina = await _servico.ListarAsync(new ConsultaDeClientes
        {
            TipoDePessoa = TipoDePessoa.Juridica,
        });

        Assert.Single(pagina.Itens);
        Assert.Equal("Acme Ltda", pagina.Itens[0].Nome);
    }

    [Fact]
    public async Task Listar_filtra_por_ativo()
    {
        var inativo = Cliente.Criar("Bruno", TipoDePessoa.Fisica, OutroCpfValido);
        inativo.Inativar();

        _repositorio.Semear(Cliente.Criar("Ana", TipoDePessoa.Fisica, CpfValido), inativo);

        var ativos = await _servico.ListarAsync(new ConsultaDeClientes { Ativo = true });
        var inativos = await _servico.ListarAsync(new ConsultaDeClientes { Ativo = false });

        Assert.Single(ativos.Itens);
        Assert.Equal("Ana", ativos.Itens[0].Nome);
        Assert.Single(inativos.Itens);
        Assert.Equal("Bruno", inativos.Itens[0].Nome);
    }

    [Fact]
    public async Task Listar_busca_por_nome_e_por_documento()
    {
        _repositorio.Semear(
            Cliente.Criar("Ana Paula", TipoDePessoa.Fisica, CpfValido),
            Cliente.Criar("Bruno", TipoDePessoa.Fisica, OutroCpfValido));

        var porNome = await _servico.ListarAsync(new ConsultaDeClientes { Busca = "paula" });
        var porDocumento = await _servico.ListarAsync(new ConsultaDeClientes { Busca = "52998224725" });

        Assert.Single(porNome.Itens);
        Assert.Single(porDocumento.Itens);
        Assert.Equal("Ana Paula", porDocumento.Itens[0].Nome);
    }

    [Fact]
    public async Task Listar_normaliza_pagina_e_tamanho_fora_do_intervalo()
    {
        _repositorio.Semear(Cliente.Criar("Ana", TipoDePessoa.Fisica, CpfValido));

        var pagina = await _servico.ListarAsync(new ConsultaDeClientes
        {
            Pagina = -5,
            TamanhoDaPagina = 10_000,
        });

        Assert.Equal(1, pagina.PaginaAtual);
        Assert.Equal(100, pagina.TamanhoDaPagina);
    }

    // ---------------------------------------------------------------------
    // Atualizacao
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Atualizar_altera_os_campos()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        var atualizado = await _servico.AtualizarAsync(
            criado.Id,
            Requisicao(nome: "Maria Souza", email: "maria.souza@empresa.com"));

        Assert.Equal("Maria Souza", atualizado.Nome);
        Assert.Equal("maria.souza@empresa.com", atualizado.Email);
    }

    [Fact]
    public async Task Atualizar_permite_manter_o_proprio_documento()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        // A checagem de unicidade precisa ignorar o proprio registro.
        var atualizado = await _servico.AtualizarAsync(criado.Id, Requisicao(nome: "Novo nome"));

        Assert.Equal("52998224725", atualizado.Documento);
    }

    [Fact]
    public async Task Atualizar_para_documento_de_outro_cliente_e_recusado()
    {
        var primeiro = await _servico.CriarAsync(Requisicao());
        await _servico.CriarAsync(Requisicao(nome: "Bruno", documento: OutroCpfValido));

        await Assert.ThrowsAsync<ExcecaoDeConflito>(
            () => _servico.AtualizarAsync(primeiro.Id, Requisicao(documento: OutroCpfValido)));
    }

    [Fact]
    public async Task Atualizar_troca_pessoa_fisica_por_juridica_com_nome_fantasia()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        var atualizado = await _servico.AtualizarAsync(criado.Id, Requisicao(
            nome: "Acme Ltda",
            tipo: TipoDePessoa.Juridica,
            documento: CnpjValido,
            nomeFantasia: "Acme"));

        Assert.Equal(TipoDePessoa.Juridica, atualizado.TipoDePessoa);
        Assert.Equal("Acme", atualizado.NomeFantasia);
    }

    [Fact]
    public async Task Atualizar_inexistente_resulta_em_nao_encontrado()
    {
        await Assert.ThrowsAsync<ExcecaoDeRecursoNaoEncontrado>(
            () => _servico.AtualizarAsync(Guid.CreateVersion7(), Requisicao()));
    }

    [Fact]
    public async Task Atualizar_pode_inativar_o_cliente()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        var atualizado = await _servico.AtualizarAsync(criado.Id, Requisicao(ativo: false));

        Assert.False(atualizado.Ativo);
    }

    // ---------------------------------------------------------------------
    // Exclusao
    // ---------------------------------------------------------------------

    [Fact]
    public async Task Excluir_tira_o_cliente_das_consultas()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        await _servico.ExcluirAsync(criado.Id);

        await Assert.ThrowsAsync<ExcecaoDeRecursoNaoEncontrado>(
            () => _servico.ObterPorIdAsync(criado.Id));

        var pagina = await _servico.ListarAsync(new ConsultaDeClientes());
        Assert.Empty(pagina.Itens);
    }

    [Fact]
    public async Task Excluir_e_logico_o_registro_permanece_no_banco()
    {
        var criado = await _servico.CriarAsync(Requisicao());

        await _servico.ExcluirAsync(criado.Id);

        // Preserva historico e auditoria: some das listagens, nao da tabela.
        var registro = Assert.Single(_repositorio.Todos);
        Assert.True(registro.Excluido);
    }

    [Fact]
    public async Task Excluir_inexistente_resulta_em_nao_encontrado()
    {
        await Assert.ThrowsAsync<ExcecaoDeRecursoNaoEncontrado>(
            () => _servico.ExcluirAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task Documento_de_cliente_excluido_pode_ser_reaproveitado()
    {
        var criado = await _servico.CriarAsync(Requisicao());
        await _servico.ExcluirAsync(criado.Id);

        var novo = await _servico.CriarAsync(Requisicao(nome: "Outro cliente"));

        Assert.Equal("52998224725", novo.Documento);
    }
}
