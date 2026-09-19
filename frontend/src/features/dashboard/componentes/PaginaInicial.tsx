import { Link } from 'react-router';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';

import { itensDoMenu } from '../../../app/layout/menu';
import { useAutenticacao } from '../../autenticacao/hooks/useAutenticacao';

/** Modulos destacados como atalho na pagina inicial. */
const CAMINHOS_DE_ATALHO = ['/clientes', '/produtos', '/estoque', '/pedidos-de-venda'];

/**
 * Pagina inicial da area logada.
 *
 * O dashboard com graficos e agregacoes reais chega na Sprint 12. Por ora
 * confirma quem esta logado e serve de ponto de partida para a navegacao,
 * que e justamente o que a Sprint 2 entrega.
 */
export function PaginaInicial() {
  const { usuario } = useAutenticacao();

  const atalhos = CAMINHOS_DE_ATALHO.map((caminho) =>
    itensDoMenu.find((item) => item.caminho === caminho),
  ).filter((item): item is NonNullable<typeof item> => item !== undefined);

  return (
    <div className="pagina-inicial">
      <Card>
        <h2 className="pagina-inicial__saudacao">Ola, {usuario?.nome}</h2>
        <p className="pagina-inicial__texto">
          Voce esta autenticado no Nimbus ERP. Use o menu a esquerda para
          navegar entre os modulos.
        </p>
        <Tag severity="info" value="Dashboard com graficos: Sprint 12" />
      </Card>

      <section className="pagina-inicial__atalhos" aria-label="Atalhos para os modulos">
        {atalhos.map((item) => (
          <Link key={item.caminho} to={item.caminho} className="atalho">
            <span className="atalho__icone" aria-hidden="true">
              <i className={`pi ${item.icone}`} />
            </span>
            <span className="atalho__rotulo">{item.rotulo}</span>
            <small className="atalho__sprint">Sprint {item.sprint}</small>
          </Link>
        ))}
      </section>
    </div>
  );
}
