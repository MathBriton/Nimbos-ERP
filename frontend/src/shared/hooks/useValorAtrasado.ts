import { useEffect, useState } from 'react';

/**
 * Devolve o valor apenas depois de ele ficar estavel por `atrasoEmMs`.
 *
 * Usado no campo de busca: sem isso, cada tecla dispararia uma requisicao.
 * O temporizador e reiniciado a cada mudanca, entao so a ultima vale.
 */
export function useValorAtrasado<T>(valor: T, atrasoEmMs = 400): T {
  const [valorAtrasado, definirValorAtrasado] = useState(valor);

  useEffect(() => {
    const temporizador = window.setTimeout(() => definirValorAtrasado(valor), atrasoEmMs);

    return () => window.clearTimeout(temporizador);
  }, [valor, atrasoEmMs]);

  return valorAtrasado;
}
