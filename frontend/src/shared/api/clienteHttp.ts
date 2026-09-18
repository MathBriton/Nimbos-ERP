import axios from 'axios';

import { ambiente } from '../config/ambiente';
import { CAMINHO_DO_LOGIN } from './constantes';
import { normalizarErro } from './erros';

/**
 * Instancia unica do axios usada por todos os services de feature.
 * O interceptor de request injeta o JWT; o de response normaliza qualquer
 * falha para {@link ErroDaApi} e avisa o contexto de autenticacao quando a
 * sessao deixa de ser valida.
 */
export const clienteHttp = axios.create({
  baseURL: ambiente.urlDaApi,
  timeout: 30_000,
  headers: { 'Content-Type': 'application/json' },
});

/**
 * Token mantido em memoria, alimentado pelo contexto de autenticacao.
 * Fica fora do React de proposito: os interceptors do axios rodam fora do
 * ciclo de render e nao podem ler estado de componente.
 */
let tokenDeAcesso: string | null = null;

export function definirTokenDeAcesso(token: string | null): void {
  tokenDeAcesso = token;
}

/** Callback disparado quando a API recusa a sessao (401 em rota autenticada). */
let aoPerderSessao: ((porExpiracao: boolean) => void) | null = null;

export function definirTratadorDeSessaoPerdida(
  tratador: ((porExpiracao: boolean) => void) | null,
): void {
  aoPerderSessao = tratador;
}

clienteHttp.interceptors.request.use((configuracao) => {
  if (tokenDeAcesso) {
    configuracao.headers.Authorization = `Bearer ${tokenDeAcesso}`;
  }
  return configuracao;
});

clienteHttp.interceptors.response.use(
  (resposta) => resposta,
  (erro: unknown) => {
    const erroNormalizado = normalizarErro(erro);

    // Um 401 no proprio login e credencial errada, nao sessao perdida:
    // avisar o contexto ali provocaria um logout desnecessario.
    const ehTentativaDeLogin = erroNormalizado.caminho?.includes(CAMINHO_DO_LOGIN) ?? false;

    if (erroNormalizado.ehNaoAutenticado && !ehTentativaDeLogin) {
      aoPerderSessao?.(erroNormalizado.tokenExpirou);
    }

    return Promise.reject(erroNormalizado);
  },
);
