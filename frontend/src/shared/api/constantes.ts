/**
 * Constantes compartilhadas entre o cliente HTTP e a normalizacao de erros.
 * Vivem em modulo separado para evitar import circular entre os dois.
 */

/** Header que a API envia junto do 401 quando o token expirou. */
export const HEADER_DE_TOKEN_EXPIRADO = 'x-token-expirado';

/** Caminho do login: um 401 aqui e credencial errada, nao sessao perdida. */
export const CAMINHO_DO_LOGIN = '/api/auth/login';
