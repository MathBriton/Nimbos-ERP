import { useEffect, useRef, useState } from 'react';
import { Button } from 'primereact/button';
import { Column } from 'primereact/column';
import { ConfirmDialog, confirmDialog } from 'primereact/confirmdialog';
import { DataTable, type DataTablePageEvent, type DataTableSortEvent } from 'primereact/datatable';
import { Dropdown } from 'primereact/dropdown';
import { IconField } from 'primereact/iconfield';
import { InputIcon } from 'primereact/inputicon';
import { InputText } from 'primereact/inputtext';
import { Message } from 'primereact/message';
import { Tag } from 'primereact/tag';
import { Toast } from 'primereact/toast';

import { ErroDaApi } from '../../../shared/api/erros';
import { useValorAtrasado } from '../../../shared/hooks/useValorAtrasado';
import { useListaDeClientes } from '../hooks/useListaDeClientes';
import { clientesService } from '../servicos/clientesService';
import type { Cliente, ConsultaDeClientes, TipoDePessoa } from '../tipos/cliente';
import { FormularioDeCliente } from './FormularioDeCliente';

const OPCOES_DE_SITUACAO = [
  { label: 'Todos', value: null },
  { label: 'Ativos', value: true },
  { label: 'Inativos', value: false },
];

const OPCOES_DE_TIPO = [
  { label: 'Todos', value: null },
  { label: 'Pessoa fisica', value: 'Fisica' as TipoDePessoa },
  { label: 'Pessoa juridica', value: 'Juridica' as TipoDePessoa },
];

/** Traduz o campo ordenado do DataTable para o enum que a API aceita. */
const ORDENACAO_POR_COLUNA: Record<string, ConsultaDeClientes['ordenacao']> = {
  nome: 'Nome',
  documentoFormatado: 'Documento',
  criadoEm: 'CriadoEm',
};

const COLUNA_POR_ORDENACAO: Record<string, string> = {
  Nome: 'nome',
  Documento: 'documentoFormatado',
  CriadoEm: 'criadoEm',
};

export function PaginaDeClientes() {
  const { consulta, resultado, carregando, erro, filtrar, irParaPagina, ordenarPor, recarregar } =
    useListaDeClientes();

  const [busca, definirBusca] = useState('');
  const buscaAtrasada = useValorAtrasado(busca);

  const [formularioVisivel, definirFormularioVisivel] = useState(false);
  const [clienteEmEdicao, definirClienteEmEdicao] = useState<Cliente | null>(null);

  const toast = useRef<Toast>(null);

  // A busca so vai para a API depois de o usuario parar de digitar. Na
  // montagem, filtrar() percebe que nada mudou e nao dispara consulta extra.
  useEffect(() => {
    filtrar({ busca: buscaAtrasada || undefined });
  }, [buscaAtrasada, filtrar]);

  function abrirCriacao() {
    definirClienteEmEdicao(null);
    definirFormularioVisivel(true);
  }

  function abrirEdicao(cliente: Cliente) {
    definirClienteEmEdicao(cliente);
    definirFormularioVisivel(true);
  }

  function aoSalvar(mensagem: string) {
    definirFormularioVisivel(false);
    toast.current?.show({ severity: 'success', summary: mensagem, life: 3000 });
    void recarregar();
  }

  function confirmarExclusao(cliente: Cliente) {
    confirmDialog({
      header: 'Excluir cliente',
      message: `Excluir "${cliente.nome}"? O registro sai das listagens, mas o historico e preservado.`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Excluir',
      rejectLabel: 'Cancelar',
      acceptClassName: 'p-button-danger',
      accept: () => void excluir(cliente),
    });
  }

  async function excluir(cliente: Cliente) {
    try {
      await clientesService.excluir(cliente.id);

      toast.current?.show({
        severity: 'success',
        summary: 'Cliente excluido.',
        life: 3000,
      });

      void recarregar();
    } catch (falha) {
      toast.current?.show({
        severity: 'error',
        summary: 'Nao foi possivel excluir',
        detail: falha instanceof ErroDaApi ? falha.message : undefined,
        life: 5000,
      });
    }
  }

  function aoPaginar(evento: DataTablePageEvent) {
    // O DataTable conta paginas a partir de zero; a API, a partir de um.
    irParaPagina((evento.page ?? 0) + 1, evento.rows);
  }

  function aoOrdenar(evento: DataTableSortEvent) {
    const campo = ORDENACAO_POR_COLUNA[evento.sortField];

    if (campo) {
      ordenarPor(campo, evento.sortOrder === -1);
    }
  }

  return (
    <div className="listagem">
      <Toast ref={toast} position="top-right" />
      <ConfirmDialog />

      <FormularioDeCliente
        visivel={formularioVisivel}
        cliente={clienteEmEdicao}
        aoFechar={() => definirFormularioVisivel(false)}
        aoSalvar={aoSalvar}
      />

      <header className="listagem__barra">
        <div className="listagem__filtros">
          <IconField iconPosition="left" className="listagem__busca">
            <InputIcon className="pi pi-search" />
            <InputText
              value={busca}
              onChange={(evento) => definirBusca(evento.target.value)}
              placeholder="Buscar por nome, documento ou e-mail"
              aria-label="Buscar clientes"
            />
          </IconField>

          <Dropdown
            value={consulta.ativo ?? null}
            options={OPCOES_DE_SITUACAO}
            onChange={(evento) => filtrar({ ativo: evento.value })}
            aria-label="Filtrar por situacao"
          />

          <Dropdown
            value={consulta.tipoDePessoa ?? null}
            options={OPCOES_DE_TIPO}
            onChange={(evento) => filtrar({ tipoDePessoa: evento.value })}
            aria-label="Filtrar por tipo de pessoa"
          />
        </div>

        <Button label="Novo cliente" icon="pi pi-plus" onClick={abrirCriacao} />
      </header>

      {erro && <Message severity="error" text={erro} className="listagem__aviso" />}

      <DataTable
        value={resultado.itens}
        dataKey="id"
        loading={carregando}
        // lazy: a paginacao e a ordenacao acontecem no servidor. Sem isso o
        // DataTable tentaria paginar apenas o que ja esta na tela.
        lazy
        paginator
        rows={resultado.tamanhoDaPagina}
        first={(resultado.paginaAtual - 1) * resultado.tamanhoDaPagina}
        totalRecords={resultado.totalDeItens}
        rowsPerPageOptions={[10, 20, 50]}
        onPage={aoPaginar}
        onSort={aoOrdenar}
        sortField={COLUNA_POR_ORDENACAO[consulta.ordenacao ?? 'Nome']}
        sortOrder={consulta.descendente ? -1 : 1}
        emptyMessage="Nenhum cliente encontrado."
        paginatorTemplate="FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink RowsPerPageDropdown"
        currentPageReportTemplate="{first} a {last} de {totalRecords}"
        stripedRows
        removableSort
      >
        <Column field="nome" header="Nome" sortable body={colunaNome} />
        <Column field="documentoFormatado" header="Documento" sortable style={{ width: '11rem' }} />
        <Column header="Contato" body={colunaContato} style={{ width: '16rem' }} />
        <Column header="Situacao" body={colunaSituacao} style={{ width: '7rem' }} />
        <Column
          header="Acoes"
          body={(cliente: Cliente) => colunaAcoes(cliente, abrirEdicao, confirmarExclusao)}
          style={{ width: '7rem' }}
          align="right"
        />
      </DataTable>
    </div>
  );
}

function colunaNome(cliente: Cliente) {
  return (
    <div className="listagem__celula-principal">
      <strong>{cliente.nome}</strong>
      {cliente.nomeFantasia && <small>{cliente.nomeFantasia}</small>}
    </div>
  );
}

function colunaContato(cliente: Cliente) {
  if (!cliente.email && !cliente.telefoneFormatado) {
    return <span className="listagem__vazio">&mdash;</span>;
  }

  return (
    <div className="listagem__celula-principal">
      {cliente.email && <span>{cliente.email}</span>}
      {cliente.telefoneFormatado && <small>{cliente.telefoneFormatado}</small>}
    </div>
  );
}

function colunaSituacao(cliente: Cliente) {
  return (
    <Tag
      severity={cliente.ativo ? 'success' : 'danger'}
      value={cliente.ativo ? 'Ativo' : 'Inativo'}
    />
  );
}

function colunaAcoes(
  cliente: Cliente,
  aoEditar: (cliente: Cliente) => void,
  aoExcluir: (cliente: Cliente) => void,
) {
  return (
    <div className="listagem__acoes">
      <Button
        icon="pi pi-pencil"
        onClick={() => aoEditar(cliente)}
        aria-label={`Editar ${cliente.nome}`}
        severity="secondary"
        text
        rounded
      />
      <Button
        icon="pi pi-trash"
        onClick={() => aoExcluir(cliente)}
        aria-label={`Excluir ${cliente.nome}`}
        severity="danger"
        text
        rounded
      />
    </div>
  );
}
