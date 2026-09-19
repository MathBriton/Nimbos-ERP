import { useContext } from 'react';

import { TemaContexto, type EstadoDoTema } from './TemaContexto';

/** Acessa o tema atual. Falha cedo se usado fora do provider. */
export function useTema(): EstadoDoTema {
  const contexto = useContext(TemaContexto);

  if (!contexto) {
    throw new Error('useTema precisa estar dentro de <ProvedorDeTema>.');
  }

  return contexto;
}
