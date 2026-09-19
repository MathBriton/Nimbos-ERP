import { useCallback, useEffect, useState } from 'react';
import { Outlet, useLocation } from 'react-router';

import { useArmazenamentoLocal } from '../../shared/hooks/useArmazenamentoLocal';
import { useEhTelaEstreita } from '../../shared/hooks/useConsultaDeMidia';
import { BarraLateral } from './BarraLateral';
import { CabecalhoDaAplicacao } from './CabecalhoDaAplicacao';
import { itemPorCaminho } from './menu';

const CHAVE_DA_SIDEBAR = 'nimbus.sidebar.recolhida';

/** Estado da gaveta, com a rota em que ela foi aberta. */
interface EstadoDaGaveta {
  aberta: boolean;
  /** Caminho vigente quando a gaveta foi aberta. */
  caminho: string;
}

/**
 * Casca da area logada: sidebar a esquerda, cabecalho no topo e conteudo da
 * rota no meio.
 *
 * Comportamento responsivo:
 * - Tela larga: a sidebar ocupa espaco fixo e pode ser recolhida a so icones.
 *   A preferencia fica no localStorage.
 * - Tela estreita: a sidebar vira gaveta sobreposta, aberta pelo botao do
 *   cabecalho e fechada ao navegar, ao clicar fora ou com Esc.
 */
export function LayoutAutenticado() {
  const ehTelaEstreita = useEhTelaEstreita();
  const localizacao = useLocation();

  const [recolhida, definirRecolhida] = useArmazenamentoLocal(CHAVE_DA_SIDEBAR, false);
  const [gaveta, definirGaveta] = useState<EstadoDaGaveta>({
    aberta: false,
    caminho: localizacao.pathname,
  });

  /**
   * A gaveta so conta como aberta na tela estreita e na rota em que foi aberta.
   *
   * Derivar isso no render (em vez de fechar a gaveta dentro de um useEffect)
   * resolve de uma vez tres situacoes: navegacao pela sidebar, navegacao que
   * nao passa por ela (botao voltar do navegador, redirect programatico) e
   * volta para tela larga com a gaveta aberta - que de outro modo deixaria a
   * cortina presa sobre o conteudo.
   */
  const gavetaAberta =
    gaveta.aberta && ehTelaEstreita && gaveta.caminho === localizacao.pathname;

  const fecharGaveta = useCallback(() => {
    definirGaveta((anterior) => ({ ...anterior, aberta: false }));
  }, []);

  const alternarGaveta = useCallback(() => {
    definirGaveta((anterior) => ({
      aberta: !anterior.aberta,
      caminho: localizacao.pathname,
    }));
  }, [localizacao.pathname]);

  // Esc fecha a gaveta, como se espera de qualquer overlay.
  useEffect(() => {
    if (!gavetaAberta) {
      return;
    }

    function aoTeclar(evento: KeyboardEvent) {
      if (evento.key === 'Escape') {
        fecharGaveta();
      }
    }

    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [gavetaAberta, fecharGaveta]);

  const item = itemPorCaminho(localizacao.pathname);
  const titulo = item?.rotulo ?? 'Nimbus ERP';

  const classes = [
    'layout',
    recolhida && !ehTelaEstreita ? 'layout--sidebar-recolhida' : '',
    ehTelaEstreita ? 'layout--estreito' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={classes}>
      <BarraLateral
        recolhida={recolhida}
        ehTelaEstreita={ehTelaEstreita}
        abertaNoCelular={gavetaAberta}
        aoAlternarRecolhida={() => definirRecolhida((anterior) => !anterior)}
        aoFechar={fecharGaveta}
      />

      {/* Cortina do overlay: clicar fora fecha a gaveta. */}
      {gavetaAberta && (
        <div className="layout__cortina" onClick={fecharGaveta} aria-hidden="true" />
      )}

      <div className="layout__painel">
        <CabecalhoDaAplicacao
          titulo={titulo}
          ehTelaEstreita={ehTelaEstreita}
          menuAberto={gavetaAberta}
          aoAlternarMenu={alternarGaveta}
        />

        <main className="layout__conteudo">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
