import { createContext } from 'react';

import type { PreferenciaDeTema, TemaEfetivo } from './tipos';

export interface EstadoDoTema {
  /** O que o usuario escolheu: claro, escuro ou acompanhar o sistema. */
  preferencia: PreferenciaDeTema;
  /** O que esta de fato na tela, com "sistema" ja resolvido. */
  temaEfetivo: TemaEfetivo;
  definirPreferencia: (preferencia: PreferenciaDeTema) => void;
  /** Alterna direto entre claro e escuro, saindo do modo "sistema". */
  alternar: () => void;
}

/**
 * Contexto separado do provider de proposito: assim o arquivo do provider
 * exporta apenas componentes, o que mantem o lint de Fast Refresh feliz.
 */
export const TemaContexto = createContext<EstadoDoTema | null>(null);
