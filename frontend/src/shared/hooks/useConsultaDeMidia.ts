import { useCallback, useSyncExternalStore } from 'react';

/** Ponto de quebra em que a sidebar deixa de ocupar espaco fixo na tela. */
export const CONSULTA_TELA_ESTREITA = '(max-width: 960px)';

function temMatchMedia(): boolean {
  return typeof window !== 'undefined' && typeof window.matchMedia === 'function';
}

/**
 * Acompanha uma media query CSS a partir do JavaScript.
 *
 * Usa useSyncExternalStore porque matchMedia e exatamente isso: um estado que
 * vive fora do React. A alternativa com useState + useEffect precisaria de um
 * setState sincrono dentro do efeito para nao perder mudancas ocorridas entre o
 * render e a assinatura - e isso provoca um render em cascata.
 *
 * O terceiro argumento (snapshot do servidor) devolve false: em ambiente sem
 * DOM assumimos tela larga, que e o layout padrao.
 */
export function useConsultaDeMidia(consulta: string): boolean {
  const assinar = useCallback(
    (aoMudar: () => void) => {
      if (!temMatchMedia()) {
        return () => {
          // Nada a desinscrever quando nao existe matchMedia.
        };
      }

      const lista = window.matchMedia(consulta);
      lista.addEventListener('change', aoMudar);

      return () => lista.removeEventListener('change', aoMudar);
    },
    [consulta],
  );

  const obterValorAtual = useCallback(
    () => (temMatchMedia() ? window.matchMedia(consulta).matches : false),
    [consulta],
  );

  return useSyncExternalStore(assinar, obterValorAtual, () => false);
}

/** Verdadeiro em telas estreitas (celular e tablet em retrato). */
export function useEhTelaEstreita(): boolean {
  return useConsultaDeMidia(CONSULTA_TELA_ESTREITA);
}
