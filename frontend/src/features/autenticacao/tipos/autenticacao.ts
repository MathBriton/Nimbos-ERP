/** Credenciais enviadas para POST /api/auth/login. */
export interface LoginRequisicao {
  email: string;
  senha: string;
}

/** Usuario autenticado, como a API o devolve. */
export interface UsuarioAutenticado {
  id: string;
  nome: string;
  email: string;
}

/** Resposta de um login bem-sucedido. */
export interface LoginResposta {
  token: string;
  /** Instante de expiracao do token, em ISO 8601. */
  expiraEm: string;
  usuario: UsuarioAutenticado;
}

/** Sessao persistida entre recarregamentos da pagina. */
export interface SessaoArmazenada {
  token: string;
  expiraEm: string;
  usuario: UsuarioAutenticado;
}
