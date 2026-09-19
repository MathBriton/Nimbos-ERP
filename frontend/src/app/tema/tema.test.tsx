import { beforeEach, describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

import {
  renderizarAutenticado,
  renderizarSemSessao,
  simularMidia,
} from '../../testes/utilitarios';
import { aplicarTema, urlDoTema } from './aplicarTema';
import { CHAVE_DO_TEMA, ID_DO_LINK_DO_TEMA } from './tipos';

/** O <link> que esta valendo como tema (o que carrega o id). */
function linkDoTema(): HTMLLinkElement | null {
  return document.getElementById(ID_DO_LINK_DO_TEMA) as HTMLLinkElement | null;
}

/** Todos os <link> de estilo, inclusive um recem-inserido ainda sem id. */
function linksDeEstilo(): HTMLLinkElement[] {
  return [...document.querySelectorAll<HTMLLinkElement>('link[rel="stylesheet"]')];
}

function temaNoHtml(): string | undefined {
  return document.documentElement.dataset.tema;
}

/**
 * O <head> e do documento inteiro e nao e limpo entre testes pelo cleanup do
 * Testing Library, entao um link deixado para tras contaminaria o proximo.
 */
function limparEstadoDoTema() {
  for (const link of linksDeEstilo()) {
    link.remove();
  }

  Reflect.deleteProperty(document.documentElement.dataset, 'tema');
  document.documentElement.style.colorScheme = '';
  Reflect.deleteProperty(window, 'matchMedia');
}

/** Abre o menu do seletor e escolhe uma das opcoes. */
async function escolherTema(usuario: ReturnType<typeof userEvent.setup>, rotulo: string) {
  await usuario.click(screen.getByRole('button', { name: /alterar tema/i }));
  await usuario.click(await screen.findByText(rotulo));
}

describe('aplicarTema', () => {
  beforeEach(limparEstadoDoTema);

  it('cria o link do tema quando ainda nao existe', () => {
    aplicarTema('escuro');

    expect(linkDoTema()).not.toBeNull();
    expect(linkDoTema()?.getAttribute('href')).toBe('/temas/escuro/theme.css');
    expect(temaNoHtml()).toBe('escuro');
  });

  it('marca o color-scheme, para o navegador pintar scrollbar e inputs nativos', () => {
    aplicarTema('escuro');
    expect(document.documentElement.style.colorScheme).toBe('dark');

    // Sem link anterior no primeiro caso; aqui ja existe um, entao e troca.
    aplicarTema('claro');
    expect(document.documentElement.style.colorScheme).toBe('light');
  });

  it('nao recria o link quando o tema pedido ja e o vigente', () => {
    aplicarTema('claro');
    const primeiro = linkDoTema();

    aplicarTema('claro');

    expect(linkDoTema()).toBe(primeiro);
    expect(linksDeEstilo()).toHaveLength(1);
  });

  it('so descarta o CSS antigo depois que o novo carrega', () => {
    aplicarTema('claro');
    const linkAntigo = linkDoTema();

    aplicarTema('escuro');

    // Momento da troca: os dois <link> coexistem, para nao piscar sem estilo.
    const links = linksDeEstilo();
    expect(links).toHaveLength(2);
    expect(linkAntigo?.isConnected).toBe(true);

    // O novo ainda nao assumiu o id: assume quando disparar o load.
    const linkNovo = links[1];
    expect(linkNovo.getAttribute('href')).toBe('/temas/escuro/theme.css');
    expect(linkNovo.id).toBe('');

    linkNovo.dispatchEvent(new Event('load'));

    expect(linkAntigo?.isConnected).toBe(false);
    expect(linkDoTema()).toBe(linkNovo);
  });

  it('promove o novo CSS mesmo se ele falhar ao carregar', () => {
    aplicarTema('claro');
    const linkAntigo = linkDoTema();

    aplicarTema('escuro');
    linksDeEstilo()[1].dispatchEvent(new Event('error'));

    // Sem isso os dois links ficariam para sempre na pagina.
    expect(linkAntigo?.isConnected).toBe(false);
    expect(linksDeEstilo()).toHaveLength(1);
  });

  it('monta a URL a partir do nome do tema', () => {
    expect(urlDoTema('claro')).toBe('/temas/claro/theme.css');
    expect(urlDoTema('escuro')).toBe('/temas/escuro/theme.css');
  });
});

describe('seletor de tema', () => {
  beforeEach(limparEstadoDoTema);

  // Critério de aceite da Sprint 3: troca instantânea.
  it('troca para escuro e aplica na hora', async () => {
    simularMidia({ sistemaPrefereEscuro: false });

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(temaNoHtml()).toBe('claro');

    await escolherTema(usuario, 'Escuro');

    // data-tema muda no mesmo quadro: e ele que governa as cores de base.
    expect(temaNoHtml()).toBe('escuro');

    // O CSS do PrimeReact entra por um <link> novo, que so substitui o antigo
    // quando terminar de carregar - por isso a busca e por href, nao pelo id.
    expect(
      linksDeEstilo().some((link) => link.getAttribute('href') === '/temas/escuro/theme.css'),
    ).toBe(true);
  });

  // Critério de aceite da Sprint 3: preferência mantida após reload.
  it('guarda a preferencia no localStorage', async () => {
    simularMidia({});

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    await escolherTema(usuario, 'Escuro');

    expect(window.localStorage.getItem(CHAVE_DO_TEMA)).toBe('"escuro"');
  });

  it('respeita a preferencia guardada ao montar', async () => {
    simularMidia({ sistemaPrefereEscuro: false });
    window.localStorage.setItem(CHAVE_DO_TEMA, '"escuro"');

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    // Mesmo com o sistema em claro, a escolha explicita prevalece.
    expect(temaNoHtml()).toBe('escuro');
  });

  it('volta de escuro para claro', async () => {
    simularMidia({});
    window.localStorage.setItem(CHAVE_DO_TEMA, '"escuro"');

    const usuario = userEvent.setup();
    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    await escolherTema(usuario, 'Claro');

    expect(temaNoHtml()).toBe('claro');
    expect(window.localStorage.getItem(CHAVE_DO_TEMA)).toBe('"claro"');
  });

  it('no modo sistema, segue o sistema operacional em escuro', async () => {
    simularMidia({ sistemaPrefereEscuro: true });
    window.localStorage.setItem(CHAVE_DO_TEMA, '"sistema"');

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(temaNoHtml()).toBe('escuro');
  });

  it('no modo sistema, segue o sistema operacional em claro', async () => {
    simularMidia({ sistemaPrefereEscuro: false });
    window.localStorage.setItem(CHAVE_DO_TEMA, '"sistema"');

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(temaNoHtml()).toBe('claro');
  });

  it('sem preferencia guardada, o padrao e acompanhar o sistema', async () => {
    simularMidia({ sistemaPrefereEscuro: true });

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(temaNoHtml()).toBe('escuro');
    // O botao anuncia que esta no modo automatico, nao no tema resolvido.
    expect(screen.getByRole('button', { name: /tema: sistema/i })).toBeInTheDocument();
  });

  it('preferencia corrompida no localStorage cai para o padrao', async () => {
    simularMidia({ sistemaPrefereEscuro: true });
    window.localStorage.setItem(CHAVE_DO_TEMA, '"roxo-neon"');

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(temaNoHtml()).toBe('escuro');
    expect(screen.getByRole('button', { name: /tema: sistema/i })).toBeInTheDocument();
  });

  it('o botao anuncia o tema escolhido no nome acessivel', async () => {
    simularMidia({});
    window.localStorage.setItem(CHAVE_DO_TEMA, '"escuro"');

    renderizarAutenticado();
    await screen.findByRole('heading', { level: 1 });

    expect(screen.getByRole('button', { name: /tema: escuro/i })).toBeInTheDocument();
  });

  it('a tela de login tambem permite trocar de tema', async () => {
    simularMidia({});

    const usuario = userEvent.setup();
    renderizarSemSessao('/login');
    await screen.findByLabelText(/e-mail/i);

    // Sem isso nao daria para escolher o tema antes de entrar no sistema.
    await escolherTema(usuario, 'Escuro');

    expect(temaNoHtml()).toBe('escuro');
    expect(window.localStorage.getItem(CHAVE_DO_TEMA)).toBe('"escuro"');
  });
});
