import { afterEach, describe, expect, it, vi } from 'vitest';

import { armazenamentoDeSessao, sessaoExpirou } from './armazenamentoDeSessao';
import type { SessaoArmazenada } from '../tipos/autenticacao';

function sessaoValida(expiraEm: string): SessaoArmazenada {
  return {
    token: 'token-qualquer',
    expiraEm,
    usuario: { id: 'u-1', nome: 'Administrador', email: 'admin@erp.com' },
  };
}

describe('sessaoExpirou', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('considera expirada uma data no passado', () => {
    const passado = new Date(Date.now() - 60_000).toISOString();

    expect(sessaoExpirou(passado)).toBe(true);
  });

  it('considera valida uma data suficientemente no futuro', () => {
    const futuro = new Date(Date.now() + 10 * 60_000).toISOString();

    expect(sessaoExpirou(futuro)).toBe(false);
  });

  it('expira antecipadamente dentro da margem de seguranca', () => {
    // Faltando 10s para expirar, com margem de 30s: ja deve ser tratada como
    // expirada, para nao disparar uma requisicao que vai falhar com 401.
    const quaseExpirando = new Date(Date.now() + 10_000).toISOString();

    expect(sessaoExpirou(quaseExpirando, 30)).toBe(true);
    expect(sessaoExpirou(quaseExpirando, 0)).toBe(false);
  });

  it('trata data invalida como expirada', () => {
    expect(sessaoExpirou('nao-e-data')).toBe(true);
    expect(sessaoExpirou('')).toBe(true);
  });
});

describe('armazenamentoDeSessao', () => {
  it('grava e le a sessao', () => {
    const sessao = sessaoValida(new Date(Date.now() + 60_000).toISOString());

    armazenamentoDeSessao.gravar(sessao);

    expect(armazenamentoDeSessao.ler()).toEqual(sessao);
  });

  it('devolve null quando nao ha nada guardado', () => {
    expect(armazenamentoDeSessao.ler()).toBeNull();
  });

  it('limpa a sessao guardada', () => {
    armazenamentoDeSessao.gravar(sessaoValida(new Date().toISOString()));

    armazenamentoDeSessao.limpar();

    expect(armazenamentoDeSessao.ler()).toBeNull();
  });

  it('descarta JSON corrompido em vez de estourar', () => {
    window.localStorage.setItem('nimbus.sessao', '{isso nao e json');

    expect(armazenamentoDeSessao.ler()).toBeNull();
  });

  it('descarta sessao com formato inesperado', () => {
    // Simula dado de uma versao anterior do app, sem os campos esperados.
    window.localStorage.setItem('nimbus.sessao', JSON.stringify({ token: 'abc' }));

    expect(armazenamentoDeSessao.ler()).toBeNull();
  });

  it('nao quebra quando o localStorage esta bloqueado', () => {
    // Aba privada / cookies bloqueados: o acesso lanca excecao.
    const erro = new Error('acesso negado');
    vi.spyOn(window.localStorage, 'getItem').mockImplementation(() => {
      throw erro;
    });
    vi.spyOn(window.localStorage, 'setItem').mockImplementation(() => {
      throw erro;
    });
    vi.spyOn(window.localStorage, 'removeItem').mockImplementation(() => {
      throw erro;
    });

    expect(armazenamentoDeSessao.ler()).toBeNull();
    expect(() => armazenamentoDeSessao.gravar(sessaoValida(new Date().toISOString()))).not.toThrow();
    expect(() => armazenamentoDeSessao.limpar()).not.toThrow();
  });
});
