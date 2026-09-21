import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { ErroDaApi } from '../../shared/api/erros';
import type { ResultadoPaginado } from '../../shared/tipos/api';
import { renderizarAutenticado, simularMidia } from '../../testes/utilitarios';
import { clientesService } from './servicos/clientesService';
import type { Cliente } from './tipos/cliente';

const CPF_VALIDO = '529.982.247-25';
const CNPJ_VALIDO = '11.222.333/0001-81';

function clienteFalso(mudanca: Partial<Cliente> = {}): Cliente {
  return {
    id: 'c-1',
    nome: 'Maria Silva',
    tipoDePessoa: 'Fisica',
    documento: '52998224725',
    documentoFormatado: CPF_VALIDO,
    email: 'maria@empresa.com',
    telefone: '11987654321',
    telefoneFormatado: '(11) 98765-4321',
    ativo: true,
    criadoEm: '2026-09-01T10:00:00Z',
    ...mudanca,
  };
}

function pagina(itens: Cliente[], total = itens.length): ResultadoPaginado<Cliente> {
  return {
    itens,
    paginaAtual: 1,
    tamanhoDaPagina: 10,
    totalDeItens: total,
    totalDePaginas: Math.ceil(total / 10),
    temPaginaAnterior: false,
    temProximaPagina: total > 10,
  };
}

/**
 * Abre um Dropdown do PrimeReact.
 *
 * O aria-label fica em um input escondido com pointer-events: none; quem
 * recebe o clique e o container visivel. Sem este desvio, o userEvent recusa
 * a interacao.
 */
async function abrirSeletor(
  usuario: ReturnType<typeof userEvent.setup>,
  rotulo: RegExp,
) {
  const campoOculto = screen.getByLabelText(rotulo);
  const container = campoOculto.closest('.p-dropdown');

  if (!container) {
    throw new Error(`Dropdown de "${rotulo.source}" nao encontrado.`);
  }

  await usuario.click(container);
}

/** Espera a listagem terminar de carregar e mostrar o cliente semeado. */
async function abrirListagem(itens: Cliente[] = [clienteFalso()]) {
  const listar = vi.spyOn(clientesService, 'listar').mockResolvedValue(pagina(itens));

  renderizarAutenticado('/clientes');

  await screen.findByRole('heading', { level: 1, name: 'Clientes' });

  return listar;
}

describe('CRUD de clientes', () => {
  beforeEach(() => {
    simularMidia({});
  });

  // -------------------------------------------------------------------
  // Listagem
  // -------------------------------------------------------------------

  it('mostra os clientes retornados pela API', async () => {
    await abrirListagem([
      clienteFalso(),
      clienteFalso({
        id: 'c-2',
        nome: 'Acme Ltda',
        nomeFantasia: 'Acme',
        tipoDePessoa: 'Juridica',
        documento: '11222333000181',
        documentoFormatado: CNPJ_VALIDO,
        ativo: false,
      }),
    ]);

    expect(await screen.findByText('Maria Silva')).toBeInTheDocument();
    expect(screen.getByText(CPF_VALIDO)).toBeInTheDocument();
    expect(screen.getByText('Acme Ltda')).toBeInTheDocument();
    // O nome fantasia aparece como complemento do nome.
    expect(screen.getByText('Acme')).toBeInTheDocument();
    expect(screen.getByText('Inativo')).toBeInTheDocument();
  });

  it('avisa quando nao ha clientes', async () => {
    await abrirListagem([]);

    expect(await screen.findByText(/nenhum cliente encontrado/i)).toBeInTheDocument();
  });

  it('mostra o erro quando a API falha', async () => {
    vi.spyOn(clientesService, 'listar').mockRejectedValue(
      new ErroDaApi('Servidor indisponivel.', 500),
    );

    renderizarAutenticado('/clientes');

    expect(await screen.findByText('Servidor indisponivel.')).toBeInTheDocument();
  });

  it('busca no servidor depois que o usuario para de digitar', async () => {
    const usuario = userEvent.setup();
    const listar = await abrirListagem();

    await screen.findByText('Maria Silva');
    const chamadasIniciais = listar.mock.calls.length;

    await usuario.type(screen.getByLabelText(/buscar clientes/i), 'acme');

    // A busca e atrasada: cada tecla nao dispara uma requisicao.
    await waitFor(() =>
      expect(listar).toHaveBeenCalledWith(expect.objectContaining({ busca: 'acme', pagina: 1 })),
    );

    // Uma unica chamada extra para as quatro teclas digitadas.
    expect(listar.mock.calls.length).toBe(chamadasIniciais + 1);
  });

  it('nao dispara consulta redundante na montagem', async () => {
    const listar = await abrirListagem();

    await screen.findByText('Maria Silva');

    // A sincronizacao do campo de busca nao pode gerar uma segunda requisicao.
    expect(listar).toHaveBeenCalledTimes(1);
  });

  it('filtra por situacao', async () => {
    const usuario = userEvent.setup();
    const listar = await abrirListagem();
    await screen.findByText('Maria Silva');

    await abrirSeletor(usuario, /filtrar por situacao/i);
    await usuario.click(await screen.findByText('Inativos'));

    await waitFor(() =>
      expect(listar).toHaveBeenCalledWith(expect.objectContaining({ ativo: false, pagina: 1 })),
    );
  });

  // -------------------------------------------------------------------
  // Criacao
  // -------------------------------------------------------------------

  it('cadastra um cliente novo', async () => {
    const usuario = userEvent.setup();
    const listar = await abrirListagem([]);
    const criar = vi.spyOn(clientesService, 'criar').mockResolvedValue(clienteFalso());

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));

    const dialogo = await screen.findByRole('dialog');
    await usuario.type(within(dialogo).getByLabelText(/^nome/i), 'Maria Silva');
    await usuario.type(within(dialogo).getByLabelText(/^cpf/i), '52998224725');
    await usuario.click(within(dialogo).getByRole('button', { name: /cadastrar/i }));

    await waitFor(() =>
      expect(criar).toHaveBeenCalledWith(
        expect.objectContaining({
          nome: 'Maria Silva',
          // Sai sem mascara: o dominio guarda somente digitos.
          documento: '52998224725',
          tipoDePessoa: 'Fisica',
        }),
      ),
    );

    // A listagem e recarregada apos salvar.
    await waitFor(() => expect(listar.mock.calls.length).toBeGreaterThan(1));
  });

  it('bloqueia o envio quando o CPF tem digito verificador errado', async () => {
    const usuario = userEvent.setup();
    await abrirListagem([]);
    const criar = vi.spyOn(clientesService, 'criar');

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));

    const dialogo = await screen.findByRole('dialog');
    await usuario.type(within(dialogo).getByLabelText(/^nome/i), 'Maria');
    await usuario.type(within(dialogo).getByLabelText(/^cpf/i), '12345678901');
    await usuario.click(within(dialogo).getByRole('button', { name: /cadastrar/i }));

    expect(await within(dialogo).findByText('CPF invalido.')).toBeInTheDocument();
    // Nem chega a bater na API.
    expect(criar).not.toHaveBeenCalled();
  });

  it('troca os campos ao escolher pessoa juridica', async () => {
    const usuario = userEvent.setup();
    await abrirListagem([]);

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));
    const dialogo = await screen.findByRole('dialog');

    // Pessoa fisica nao tem nome fantasia.
    expect(within(dialogo).queryByLabelText(/nome fantasia/i)).not.toBeInTheDocument();

    await usuario.click(within(dialogo).getByText('Pessoa juridica'));

    expect(await within(dialogo).findByLabelText(/razao social/i)).toBeInTheDocument();
    expect(within(dialogo).getByLabelText(/^cnpj/i)).toBeInTheDocument();
    expect(within(dialogo).getByLabelText(/nome fantasia/i)).toBeInTheDocument();
  });

  it('mostra o conflito de documento duplicado vindo da API', async () => {
    const usuario = userEvent.setup();
    await abrirListagem([]);

    vi.spyOn(clientesService, 'criar').mockRejectedValue(
      new ErroDaApi('Ja existe um cliente cadastrado com o documento 529.982.247-25.', 409),
    );

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));

    const dialogo = await screen.findByRole('dialog');
    await usuario.type(within(dialogo).getByLabelText(/^nome/i), 'Maria');
    await usuario.type(within(dialogo).getByLabelText(/^cpf/i), '52998224725');
    await usuario.click(within(dialogo).getByRole('button', { name: /cadastrar/i }));

    expect(await within(dialogo).findByText(/ja existe um cliente/i)).toBeInTheDocument();
  });

  it('exibe erro por campo devolvido pelo backend', async () => {
    const usuario = userEvent.setup();
    await abrirListagem([]);

    vi.spyOn(clientesService, 'criar').mockRejectedValue(
      new ErroDaApi('Erro de validacao', 400, {
        errosDeValidacao: { Nome: ['O nome pode ter no maximo 200 caracteres.'] },
      }),
    );

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));

    const dialogo = await screen.findByRole('dialog');
    await usuario.type(within(dialogo).getByLabelText(/^nome/i), 'Maria');
    await usuario.type(within(dialogo).getByLabelText(/^cpf/i), '52998224725');
    await usuario.click(within(dialogo).getByRole('button', { name: /cadastrar/i }));

    expect(await within(dialogo).findByText(/no maximo 200 caracteres/i)).toBeInTheDocument();
  });

  // -------------------------------------------------------------------
  // Edicao
  // -------------------------------------------------------------------

  it('abre a edicao com os dados preenchidos e salva', async () => {
    const usuario = userEvent.setup();
    await abrirListagem();
    const atualizar = vi.spyOn(clientesService, 'atualizar').mockResolvedValue(clienteFalso());

    await screen.findByText('Maria Silva');
    await usuario.click(screen.getByRole('button', { name: /editar maria silva/i }));

    const dialogo = await screen.findByRole('dialog');
    const campoNome = within(dialogo).getByLabelText(/^nome/i);

    expect(campoNome).toHaveValue('Maria Silva');
    expect(within(dialogo).getByLabelText(/^cpf/i)).toHaveValue(CPF_VALIDO);

    await usuario.clear(campoNome);
    await usuario.type(campoNome, 'Maria Souza');
    await usuario.click(within(dialogo).getByRole('button', { name: /salvar/i }));

    await waitFor(() =>
      expect(atualizar).toHaveBeenCalledWith(
        'c-1',
        expect.objectContaining({ nome: 'Maria Souza' }),
      ),
    );
  });

  it('nao herda os dados do cliente anterior ao abrir outro formulario', async () => {
    const usuario = userEvent.setup();
    await abrirListagem([
      clienteFalso(),
      clienteFalso({ id: 'c-2', nome: 'Bruno Lima', documentoFormatado: '168.995.350-09' }),
    ]);

    await screen.findByText('Maria Silva');

    await usuario.click(screen.getByRole('button', { name: /editar maria silva/i }));
    let dialogo = await screen.findByRole('dialog');
    expect(within(dialogo).getByLabelText(/^nome/i)).toHaveValue('Maria Silva');
    await usuario.click(within(dialogo).getByRole('button', { name: /cancelar/i }));

    await usuario.click(screen.getByRole('button', { name: /editar bruno lima/i }));
    dialogo = await screen.findByRole('dialog');

    // A key do formulario garante o estado reiniciado por cliente.
    expect(within(dialogo).getByLabelText(/^nome/i)).toHaveValue('Bruno Lima');
  });

  it('abrir o cadastro depois de uma edicao comeca em branco', async () => {
    const usuario = userEvent.setup();
    await abrirListagem();
    await screen.findByText('Maria Silva');

    await usuario.click(screen.getByRole('button', { name: /editar maria silva/i }));
    let dialogo = await screen.findByRole('dialog');
    await usuario.click(within(dialogo).getByRole('button', { name: /cancelar/i }));

    await usuario.click(screen.getByRole('button', { name: /novo cliente/i }));
    dialogo = await screen.findByRole('dialog');

    expect(within(dialogo).getByLabelText(/^nome/i)).toHaveValue('');
  });

  // -------------------------------------------------------------------
  // Exclusao
  // -------------------------------------------------------------------

  it('pede confirmacao antes de excluir', async () => {
    const usuario = userEvent.setup();
    await abrirListagem();
    const excluir = vi.spyOn(clientesService, 'excluir').mockResolvedValue();

    await screen.findByText('Maria Silva');
    await usuario.click(screen.getByRole('button', { name: /excluir maria silva/i }));

    expect(await screen.findByText(/excluir "maria silva"/i)).toBeInTheDocument();

    // Desistir nao exclui nada.
    await usuario.click(screen.getByRole('button', { name: /cancelar/i }));
    expect(excluir).not.toHaveBeenCalled();
  });

  it('exclui apos confirmar e recarrega a listagem', async () => {
    const usuario = userEvent.setup();
    const listar = await abrirListagem();
    const excluir = vi.spyOn(clientesService, 'excluir').mockResolvedValue();

    await screen.findByText('Maria Silva');
    await usuario.click(screen.getByRole('button', { name: /excluir maria silva/i }));
    await usuario.click(await screen.findByRole('button', { name: /^excluir$/i }));

    await waitFor(() => expect(excluir).toHaveBeenCalledWith('c-1'));
    await waitFor(() => expect(listar.mock.calls.length).toBeGreaterThan(1));
  });

  it('avisa quando a exclusao falha', async () => {
    const usuario = userEvent.setup();
    await abrirListagem();

    vi.spyOn(clientesService, 'excluir').mockRejectedValue(
      new ErroDaApi('Cliente nao encontrado.', 404),
    );

    await screen.findByText('Maria Silva');
    await usuario.click(screen.getByRole('button', { name: /excluir maria silva/i }));
    await usuario.click(await screen.findByRole('button', { name: /^excluir$/i }));

    expect(await screen.findByText(/nao foi possivel excluir/i)).toBeInTheDocument();
  });
});
