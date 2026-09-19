import { useCallback, useState } from 'react';

/**
 * Estado de React espelhado no localStorage.
 *
 * Todo acesso ao armazenamento e protegido: em aba privada, ou com dados de
 * site bloqueados, tanto a leitura quanto a escrita podem lancar excecao. Nesse
 * caso o estado continua valendo em memoria e apenas nao sobrevive ao reload.
 *
 * Usado pela sidebar (estado recolhido) e, na Sprint 3, pela preferencia de tema.
 */
export function useArmazenamentoLocal<T>(
  chave: string,
  valorInicial: T,
): [T, (novoValor: T | ((anterior: T) => T)) => void] {
  // A leitura fica no inicializador do useState para rodar uma unica vez, na
  // montagem, em vez de a cada render.
  const [valor, setValor] = useState<T>(() => {
    try {
      const bruto = window.localStorage.getItem(chave);
      return bruto === null ? valorInicial : (JSON.parse(bruto) as T);
    } catch {
      return valorInicial;
    }
  });

  const definir = useCallback(
    (novoValor: T | ((anterior: T) => T)) => {
      setValor((anterior) => {
        const resolvido =
          typeof novoValor === 'function'
            ? (novoValor as (anterior: T) => T)(anterior)
            : novoValor;

        try {
          window.localStorage.setItem(chave, JSON.stringify(resolvido));
        } catch {
          // Sem persistencia, o valor vale so nesta sessao da aba.
        }

        return resolvido;
      });
    },
    [chave],
  );

  return [valor, definir];
}
