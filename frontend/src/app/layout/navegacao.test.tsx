import { afterEach, describe, expect, it } from 'vitest';
import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import { itensDoMenu, menu } from './menu';
import { renderizarAutenticado, simularLarguraDaTela } from '../../testes/utilitarios';

/**
 * A sidebar, pelo id.
 *
 * Nao usa getByRole de proposito: com a gaveta fechada em tela estreita ela
 * carrega aria-hidden, e o Testing Library - corretamente - nao a encontra por
 * papel. Justamente esse estado e o que varios testes daqui precisam inspecionar.
 */
function barraLateral(): HTMLElement {
  const nav = document.getElementById('barra-lateral');

  if (!nav) {
    throw new Error('A barra lateral nao foi renderizada.');
  }

  return nav;
}

describe('navegacao pela sidebar', () => {
  afterEach(() => {
    // Remove o matchMedia instalado pelos testes de responsividade.
    Reflect.deleteProperty(window, 'matchMedia');
  });

  it('a sidebar expoe um link para cada item do menu', async () => {
    renderizarAutenticado();

    const nav = await screen.findByRole('navigation', { name: /menu principal/i });

    for (const item of itensDoMenu) {
      const link = within(nav).getByRole('link', { name: item.rotulo });
      expect(link).toHaveAttribute('href', item.caminho);
    }
  });

  it('agrupa os itens sob os titulos de secao', async () => {
    renderizarAutenticado();
    await layoutPronto();

    const titulos = menu
      .map((grupo) => grupo.titulo)
      .filter((titulo): titulo is string => titulo !== null);

    for (const titulo of titulos) {
      expect(within(barraLateral()).getByRole('heading', { name: titulo })).toBeInTheDocument();
    }
  });

  // Critério de aceite da Sprint 2: dá para navegar por todas as páginas.
  it.each(itensDoMenu.map((item) => [item.rotulo, item.caminho] as const))(
    'navega para %s (%s) e atualiza o titulo do cabecalho',
    async (rotulo, caminho) => {
      const usuario = userEvent.setup();
      renderizarAutenticado();
      await layoutPronto();

      await usuario.click(within(barraLateral()).getByRole('link', { name: rotulo }));

      // O h1 do cabecalho reflete a rota atual.
      expect(
        await screen.findByRole('heading', { level: 1, name: rotulo }),
      ).toBeInTheDocument();

      // O link da rota atual fica marcado como ativo.
      expect(within(barraLateral()).getByRole('link', { name: rotulo })).toHaveClass(
        'barra-lateral__link--ativo',
      );

      expect(caminho).toBeTruthy();
    },
  );

  it('mostra a sprint planejada nos modulos ainda nao implementados', async () => {
    const usuario = userEvent.setup();
    renderizarAutenticado();
    await layoutPronto();

    await usuario.click(within(barraLateral()).getByRole('link', { name: 'Clientes' }));

    expect(await screen.findByText(/planejado para a sprint 4/i)).toBeInTheDocument();
    expect(screen.getByText(/cadastro de clientes/i)).toBeInTheDocument();
  });

  it('um caminho desconhecido cai na pagina inicial', async () => {
    renderizarAutenticado('/rota-que-nao-existe');

    expect(await screen.findByText(/ola, administrador/i)).toBeInTheDocument();
  });

  it('a rota /status abre o painel de status da plataforma', async () => {
    renderizarAutenticado('/status');

    expect(
      await screen.findByRole('heading', { name: /status da plataforma/i }),
    ).toBeInTheDocument();
  });
});

describe('sidebar em tela larga', () => {
  afterEach(() => {
    Reflect.deleteProperty(window, 'matchMedia');
  });

  it('recolhe e expande, guardando a preferencia', async () => {
    simularLarguraDaTela(false);

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await layoutPronto();

    expect(barraLateral()).not.toHaveClass('barra-lateral--recolhida');

    await usuario.click(screen.getByRole('button', { name: /recolher menu/i }));

    expect(barraLateral()).toHaveClass('barra-lateral--recolhida');
    expect(window.localStorage.getItem('nimbus.sidebar.recolhida')).toBe('true');

    await usuario.click(screen.getByRole('button', { name: /expandir menu/i }));

    expect(barraLateral()).not.toHaveClass('barra-lateral--recolhida');
    expect(window.localStorage.getItem('nimbus.sidebar.recolhida')).toBe('false');
  });

  it('respeita a preferencia guardada na montagem', async () => {
    simularLarguraDaTela(false);
    window.localStorage.setItem('nimbus.sidebar.recolhida', 'true');

    renderizarAutenticado();
    await layoutPronto();

    expect(barraLateral()).toHaveClass('barra-lateral--recolhida');
    // Recolhida, o botao oferece a acao inversa.
    expect(screen.getByRole('button', { name: /expandir menu/i })).toBeInTheDocument();
  });

  it('nao mostra o botao de abrir o menu, porque a sidebar esta sempre visivel', async () => {
    simularLarguraDaTela(false);

    renderizarAutenticado();
    await layoutPronto();

    expect(
      screen.queryByRole('button', { name: /menu de navegacao/i }),
    ).not.toBeInTheDocument();
  });
});

describe('sidebar em tela estreita', () => {
  afterEach(() => {
    Reflect.deleteProperty(window, 'matchMedia');
  });

  it('comeca fechada e fora do alcance do teclado', async () => {
    simularLarguraDaTela(true);

    renderizarAutenticado();
    await layoutPronto();

    const nav = barraLateral();
    expect(nav).toHaveClass('barra-lateral--sobreposta');
    expect(nav).not.toHaveClass('barra-lateral--aberta');
    // Fechada, a gaveta sai da arvore de acessibilidade.
    expect(nav).toHaveAttribute('aria-hidden', 'true');
  });

  it('abre pelo botao do cabecalho e fecha ao navegar', async () => {
    simularLarguraDaTela(true);

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await layoutPronto();

    const botaoDoMenu = screen.getByRole('button', { name: /menu de navegacao/i });
    expect(botaoDoMenu).toHaveAttribute('aria-expanded', 'false');

    await usuario.click(botaoDoMenu);

    expect(barraLateral()).toHaveClass('barra-lateral--aberta');
    expect(barraLateral()).toHaveAttribute('aria-hidden', 'false');
    expect(botaoDoMenu).toHaveAttribute('aria-expanded', 'true');

    // Navegar fecha a gaveta sozinha.
    await usuario.click(within(barraLateral()).getByRole('link', { name: 'Produtos' }));

    expect(await screen.findByRole('heading', { level: 1, name: 'Produtos' })).toBeInTheDocument();
    expect(barraLateral()).not.toHaveClass('barra-lateral--aberta');
  });

  it('fecha com a tecla Esc', async () => {
    simularLarguraDaTela(true);

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await layoutPronto();

    await usuario.click(screen.getByRole('button', { name: /menu de navegacao/i }));
    expect(barraLateral()).toHaveClass('barra-lateral--aberta');

    await usuario.keyboard('{Escape}');

    expect(barraLateral()).not.toHaveClass('barra-lateral--aberta');
  });

  it('fecha ao clicar no botao de fechar da propria gaveta', async () => {
    simularLarguraDaTela(true);

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await layoutPronto();

    await usuario.click(screen.getByRole('button', { name: /menu de navegacao/i }));
    await usuario.click(screen.getByRole('button', { name: /fechar menu/i }));

    expect(barraLateral()).not.toHaveClass('barra-lateral--aberta');
  });

  it('nao oferece recolher a sidebar, que ali e uma gaveta', async () => {
    simularLarguraDaTela(true);

    renderizarAutenticado();
    await layoutPronto();

    expect(screen.queryByRole('button', { name: /recolher menu/i })).not.toBeInTheDocument();
  });
});

/**
 * Espera a sessao ser reidratada e o layout montar.
 *
 * Aguarda o h1 do cabecalho, que existe em qualquer largura de tela - ao
 * contrario da sidebar, invisivel a11y quando a gaveta esta fechada.
 */
async function layoutPronto() {
  await screen.findByRole('heading', { level: 1 });
}
