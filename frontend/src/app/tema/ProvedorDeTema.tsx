import { useCallback, useEffect, useMemo } from 'react';

import { useArmazenamentoLocal } from '../../shared/hooks/useArmazenamentoLocal';
import { useConsultaDeMidia } from '../../shared/hooks/useConsultaDeMidia';
import { aplicarTema } from './aplicarTema';
import { TemaContexto, type EstadoDoTema } from './TemaContexto';
import {
  CHAVE_DO_TEMA,
  ehPreferenciaValida,
  type PreferenciaDeTema,
  type TemaEfetivo,
} from './tipos';

/** Media query do modo escuro configurado no sistema operacional. */
const PREFERE_ESCURO = '(prefers-color-scheme: dark)';

interface Props {
  children: React.ReactNode;
}

/**
 * Dono da preferencia de tema.
 *
 * O tema inicial ja foi aplicado pelo script inline do index.html, antes da
 * primeira pintura. Este provider assume dali em diante: reage a troca feita
 * pelo usuario e, no modo "sistema", a mudanca do tema do sistema operacional.
 */
export function ProvedorDeTema({ children }: Props) {
  const [preferenciaGuardada, definirPreferenciaGuardada] =
    useArmazenamentoLocal<PreferenciaDeTema>(CHAVE_DO_TEMA, 'sistema');

  // Valor corrompido no localStorage (ou de uma versao anterior) nao pode
  // deixar a aplicacao em um estado de tema invalido.
  const preferencia: PreferenciaDeTema = ehPreferenciaValida(preferenciaGuardada)
    ? preferenciaGuardada
    : 'sistema';

  const sistemaPrefereEscuro = useConsultaDeMidia(PREFERE_ESCURO);

  const temaEfetivo: TemaEfetivo =
    preferencia === 'sistema' ? (sistemaPrefereEscuro ? 'escuro' : 'claro') : preferencia;

  // Sincroniza o DOM com o tema calculado. aplicarTema sai cedo quando o CSS
  // ja e o correto, entao a montagem inicial nao recarrega nada.
  useEffect(() => {
    aplicarTema(temaEfetivo);
  }, [temaEfetivo]);

  const definirPreferencia = useCallback(
    (nova: PreferenciaDeTema) => definirPreferenciaGuardada(nova),
    [definirPreferenciaGuardada],
  );

  /** Alterna claro/escuro. Escolher explicitamente encerra o modo "sistema". */
  const alternar = useCallback(() => {
    definirPreferenciaGuardada(temaEfetivo === 'escuro' ? 'claro' : 'escuro');
  }, [definirPreferenciaGuardada, temaEfetivo]);

  const valor = useMemo<EstadoDoTema>(
    () => ({ preferencia, temaEfetivo, definirPreferencia, alternar }),
    [preferencia, temaEfetivo, definirPreferencia, alternar],
  );

  return <TemaContexto value={valor}>{children}</TemaContexto>;
}
