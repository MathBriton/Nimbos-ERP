import { PainelDeStatus } from '../features/plataforma/componentes/PainelDeStatus';

/**
 * Raiz da aplicacao.
 * Na Sprint 0 renderiza apenas o painel de status; o roteamento completo
 * com sidebar e rotas protegidas chega nas Sprints 1 e 2.
 */
export function App() {
  return <PainelDeStatus />;
}
