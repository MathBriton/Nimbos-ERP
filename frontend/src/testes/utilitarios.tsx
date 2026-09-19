import type { ReactNode } from 'react';
import { vi } from 'vitest';
import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router';

import { Rotas } from '../app/rotas/Rotas';
import { ProvedorDeAutenticacao } from '../features/autenticacao/contexto/ProvedorDeAutenticacao';
import { autenticacaoService } from '../features/autenticacao/servicos/autenticacaoService';

export const USUARIO_DE_TESTE = {
  id: 'u-1',
  nome: 'Administrador',
  email: 'admin@erp.com',
};

/**
 * Renderiza a aplicacao com sessao ativa.
 *
 * Grava a sessao no localStorage e resolve GET /api/auth/eu, que e exatamente
 * o caminho de reidratacao usado no navegador - assim o teste exercita o fluxo
 * real em vez de injetar um contexto falso.
 */
export function renderizarAutenticado(rotaInicial = '/') {
  window.localStorage.setItem(
    'nimbus.sessao',
    JSON.stringify({
      token: 'token-de-teste',
      expiraEm: new Date(Date.now() + 60 * 60_000).toISOString(),
      usuario: USUARIO_DE_TESTE,
    }),
  );

  vi.spyOn(autenticacaoService, 'obterUsuarioAtual').mockResolvedValue(USUARIO_DE_TESTE);

  return render(
    <MemoryRouter initialEntries={[rotaInicial]}>
      <ProvedorDeAutenticacao>
        <Rotas />
      </ProvedorDeAutenticacao>
    </MemoryRouter>,
  );
}

/** Envolve qualquer arvore com roteador e sessao, para testes de componente. */
export function comProvedores(filhos: ReactNode, rotaInicial = '/') {
  return render(
    <MemoryRouter initialEntries={[rotaInicial]}>
      <ProvedorDeAutenticacao>{filhos}</ProvedorDeAutenticacao>
    </MemoryRouter>,
  );
}

/**
 * Instala um matchMedia no jsdom, que nao o implementa.
 *
 * Sem isso o hook de media query devolve false (tela larga) e nao ha como
 * testar o comportamento responsivo da sidebar.
 */
export function simularLarguraDaTela(ehTelaEstreita: boolean) {
  const ouvintes = new Set<(evento: MediaQueryListEvent) => void>();

  const matchMedia = vi.fn((consulta: string) => ({
    matches: ehTelaEstreita,
    media: consulta,
    onchange: null,
    addEventListener: (_tipo: string, ouvinte: (evento: MediaQueryListEvent) => void) => {
      ouvintes.add(ouvinte);
    },
    removeEventListener: (_tipo: string, ouvinte: (evento: MediaQueryListEvent) => void) => {
      ouvintes.delete(ouvinte);
    },
    dispatchEvent: () => false,
    addListener: () => undefined,
    removeListener: () => undefined,
  }));

  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: matchMedia,
  });

  return { matchMedia, ouvintes };
}
