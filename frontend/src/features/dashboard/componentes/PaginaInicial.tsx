import { Card } from 'primereact/card';

import { useAutenticacao } from '../../autenticacao/hooks/useAutenticacao';
import { PainelDeStatus } from '../../plataforma/componentes/PainelDeStatus';

/**
 * Pagina inicial da area logada.
 * O dashboard com graficos e agregacoes reais chega na Sprint 12; por ora
 * confirma quem esta logado e reaproveita o painel de status da plataforma.
 */
export function PaginaInicial() {
  const { usuario } = useAutenticacao();

  return (
    <div className="pagina-inicial">
      <Card title={`Ola, ${usuario?.nome ?? ''}`}>
        <p className="pagina-inicial__texto">
          Voce esta autenticado no Nimbus ERP. Os modulos de negocio serao
          liberados nas proximas sprints.
        </p>
      </Card>

      <PainelDeStatus />
    </div>
  );
}
