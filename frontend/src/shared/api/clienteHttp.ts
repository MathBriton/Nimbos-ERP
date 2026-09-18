import axios from 'axios';

import { ambiente } from '../config/ambiente';
import { normalizarErro } from './erros';

/**
 * Instancia unica do axios usada por todos os services de feature.
 * O interceptor de request injeta o JWT a partir da Sprint 1; o de response
 * normaliza qualquer falha para {@link ErroDaApi}.
 */
export const clienteHttp = axios.create({
  baseURL: ambiente.urlDaApi,
  timeout: 30_000,
  headers: { 'Content-Type': 'application/json' },
});

/** Token em memoria (o fluxo completo de sessao chega na Sprint 1). */
let tokenDeAcesso: string | null = null;

export function definirTokenDeAcesso(token: string | null): void {
  tokenDeAcesso = token;
}

clienteHttp.interceptors.request.use((configuracao) => {
  if (tokenDeAcesso) {
    configuracao.headers.Authorization = `Bearer ${tokenDeAcesso}`;
  }
  return configuracao;
});

clienteHttp.interceptors.response.use(
  (resposta) => resposta,
  (erro: unknown) => Promise.reject(normalizarErro(erro)),
);
