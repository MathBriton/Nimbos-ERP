import type { ReactNode } from 'react';
import { vi } from 'vitest';
import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router';

import { Rotas } from '../app/rotas/Rotas';
import { ProvedorDeTema } from '../app/tema/ProvedorDeTema';
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
    <ProvedorDeTema>
      <MemoryRouter initialEntries={[rotaInicial]}>
        <ProvedorDeAutenticacao>
          <Rotas />
        </ProvedorDeAutenticacao>
      </MemoryRouter>
    </ProvedorDeTema>,
  );
}

/** Renderiza a aplicacao sem sessao: cai na tela de login. */
export function renderizarSemSessao(rotaInicial = '/login') {
  return render(
    <ProvedorDeTema>
      <MemoryRouter initialEntries={[rotaInicial]}>
        <ProvedorDeAutenticacao>
          <Rotas />
        </ProvedorDeAutenticacao>
      </MemoryRouter>
    </ProvedorDeTema>,
  );
}

/** Envolve qualquer arvore com roteador e sessao, para testes de componente. */
export function comProvedores(filhos: ReactNode, rotaInicial = '/') {
  return render(
    <ProvedorDeTema>
      <MemoryRouter initialEntries={[rotaInicial]}>
        <ProvedorDeAutenticacao>{filhos}</ProvedorDeAutenticacao>
      </MemoryRouter>
    </ProvedorDeTema>,
  );
}

/**
 * Instala um matchMedia no jsdom, que nao o implementa.
 *
 * Responde por consulta, e nao com um valor unico: a aplicacao pergunta tanto
 * pela largura da tela quanto por prefers-color-scheme, e um stub que
 * respondesse a mesma coisa para as duas faria a sidebar e o tema andarem
 * sempre juntos - mascarando bugs em vez de revelar.
 */
export function simularMidia(opcoes: {
  telaEstreita?: boolean;
  sistemaPrefereEscuro?: boolean;
}) {
  const { telaEstreita = false, sistemaPrefereEscuro = false } = opcoes;

  const ouvintesPorConsulta = new Map<string, Set<() => void>>();

  function responde(consulta: string): boolean {
    if (consulta.includes('prefers-color-scheme: dark')) {
      return sistemaPrefereEscuro;
    }

    if (consulta.includes('max-width')) {
      return telaEstreita;
    }

    return false;
  }

  const matchMedia = vi.fn((consulta: string) => ({
    matches: responde(consulta),
    media: consulta,
    onchange: null,
    addEventListener: (_tipo: string, ouvinte: () => void) => {
      const ouvintes = ouvintesPorConsulta.get(consulta) ?? new Set();
      ouvintes.add(ouvinte);
      ouvintesPorConsulta.set(consulta, ouvintes);
    },
    removeEventListener: (_tipo: string, ouvinte: () => void) => {
      ouvintesPorConsulta.get(consulta)?.delete(ouvinte);
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

  return { matchMedia, ouvintesPorConsulta };
}

/** Atalho para os testes que so se importam com a largura da tela. */
export function simularLarguraDaTela(ehTelaEstreita: boolean) {
  return simularMidia({ telaEstreita: ehTelaEstreita });
}
