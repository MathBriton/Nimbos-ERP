import { useContext } from 'react';

import { AutenticacaoContexto, type EstadoDaAutenticacao } from '../contexto/AutenticacaoContexto';

/** Acessa a sessao autenticada. Falha cedo se usado fora do provider. */
export function useAutenticacao(): EstadoDaAutenticacao {
  const contexto = useContext(AutenticacaoContexto);

  if (!contexto) {
    throw new Error('useAutenticacao precisa estar dentro de <ProvedorDeAutenticacao>.');
  }

  return contexto;
}
