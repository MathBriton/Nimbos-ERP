import { Link } from 'react-router';
import { Card } from 'primereact/card';
import { Tag } from 'primereact/tag';

interface Props {
  titulo: string;
  icone: string;
  descricao: string;
  /** Sprint do roadmap que entrega esta tela. */
  sprint: number;
}

/**
 * Placeholder dos modulos ainda nao implementados.
 *
 * Em vez de uma pagina vazia, mostra o que o modulo vai fazer e em qual sprint
 * ele chega: quem navega pelo sistema entende que a ausencia e planejada, e a
 * navegacao da Sprint 2 fica demonstravel de verdade.
 */
export function PaginaEmConstrucao({ titulo, icone, descricao, sprint }: Props) {
  return (
    <div className="em-construcao">
      <Card>
        <div className="em-construcao__cabecalho">
          <span className="em-construcao__icone" aria-hidden="true">
            <i className={`pi ${icone}`} />
          </span>

          <div>
            <h2 className="em-construcao__titulo">{titulo}</h2>
            <Tag severity="info" value={`Planejado para a Sprint ${sprint}`} />
          </div>
        </div>

        <p className="em-construcao__descricao">{descricao}</p>

        <p className="em-construcao__rodape">
          O roadmap completo esta no <code>SDD.md</code> do repositorio.{' '}
          <Link to="/">Voltar ao dashboard</Link>
        </p>
      </Card>
    </div>
  );
}
