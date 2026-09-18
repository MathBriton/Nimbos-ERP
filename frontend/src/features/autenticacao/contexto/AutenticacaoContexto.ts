import { createContext } from 'react';

import type { LoginRequisicao, UsuarioAutenticado } from '../tipos/autenticacao';

export interface EstadoDaAutenticacao {
  usuario: UsuarioAutenticado | null;
  /** Verdadeiro enquanto a sessao guardada esta sendo revalidada na API. */
  carregandoSessao: boolean;
  estaAutenticado: boolean;
  /** Mensagem do motivo do ultimo logout automatico (ex.: sessao expirada). */
  avisoDeSessao: string | null;
  entrar: (credenciais: LoginRequisicao) => Promise<void>;
  sair: () => void;
  limparAvisoDeSessao: () => void;
}

/**
 * Contexto separado do provider de proposito: assim o arquivo do provider
 * exporta apenas componentes, o que mantem o lint de Fast Refresh feliz.
 */
export const AutenticacaoContexto = createContext<EstadoDaAutenticacao | null>(null);
