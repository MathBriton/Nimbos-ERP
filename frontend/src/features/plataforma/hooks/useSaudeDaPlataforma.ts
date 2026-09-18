import { useCallback, useEffect, useState } from 'react';

import { ErroDaApi } from '../../../shared/api/erros';
import { plataformaService } from '../servicos/plataformaService';
import type { IdentificacaoDaApi, RelatorioDeSaude } from '../tipos/saude';

interface EstadoDaPlataforma {
  identificacao: IdentificacaoDaApi | null;
  saude: RelatorioDeSaude | null;
  carregando: boolean;
  erro: string | null;
}

const estadoInicial: EstadoDaPlataforma = {
  identificacao: null,
  saude: null,
  // Comeca carregando: a primeira consulta dispara junto com a montagem.
  carregando: true,
  erro: null,
};

/**
 * Consulta identificacao + health checks da API e revalida periodicamente.
 * E a fonte de dados da tela de status da Sprint 0 e a base da tela de
 * monitoramento da Sprint 22.
 */
export function useSaudeDaPlataforma(intervaloEmMs = 15_000) {
  const [estado, setEstado] = useState<EstadoDaPlataforma>(estadoInicial);

  const consultar = useCallback(async () => {
    try {
      const [identificacao, saude] = await Promise.all([
        plataformaService.obterIdentificacao(),
        plataformaService.obterSaude(),
      ]);

      setEstado({ identificacao, saude, carregando: false, erro: null });
    } catch (erro) {
      setEstado({
        identificacao: null,
        saude: null,
        carregando: false,
        erro: erro instanceof ErroDaApi ? erro.message : 'Falha ao consultar a API.',
      });
    }
  }, []);

  /** Recarga manual: mostra o estado de carregamento antes de consultar. */
  const reconsultar = useCallback(async () => {
    setEstado((anterior) => ({ ...anterior, carregando: true }));
    await consultar();
  }, [consultar]);

  useEffect(() => {
    // Falso positivo de react/set-state-in-effect: consultar() so chama
    // setState depois de aguardar a rede, nunca de forma sincrona - e o efeito
    // existe justamente para sincronizar com um sistema externo (a API).
    // oxlint-disable-next-line react/set-state-in-effect
    void consultar();

    const temporizador = window.setInterval(() => void consultar(), intervaloEmMs);
    return () => window.clearInterval(temporizador);
  }, [consultar, intervaloEmMs]);

  return { ...estado, reconsultar };
}
