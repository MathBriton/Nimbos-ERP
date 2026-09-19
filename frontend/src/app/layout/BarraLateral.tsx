import { NavLink } from 'react-router';
import { Button } from 'primereact/button';
import { Tooltip } from 'primereact/tooltip';

import { ambiente } from '../../shared/config/ambiente';
import { menu } from './menu';

interface Props {
  /** Em telas largas: sidebar reduzida a apenas icones. */
  recolhida: boolean;
  /** Em telas estreitas: sidebar sobreposta ao conteudo. */
  ehTelaEstreita: boolean;
  /** Visivel apenas quando a tela e estreita e o usuario abriu o menu. */
  abertaNoCelular: boolean;
  aoAlternarRecolhida: () => void;
  aoFechar: () => void;
}

export function BarraLateral({
  recolhida,
  ehTelaEstreita,
  abertaNoCelular,
  aoAlternarRecolhida,
  aoFechar,
}: Props) {
  // Em tela estreita a sidebar nunca fica no modo "so icones": ali ela e uma
  // gaveta sobreposta, e esconder os rotulos so atrapalharia.
  const mostrarApenasIcones = recolhida && !ehTelaEstreita;

  const classes = [
    'barra-lateral',
    mostrarApenasIcones ? 'barra-lateral--recolhida' : '',
    ehTelaEstreita ? 'barra-lateral--sobreposta' : '',
    ehTelaEstreita && abertaNoCelular ? 'barra-lateral--aberta' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <nav
      id="barra-lateral"
      className={classes}
      aria-label="Menu principal"
      // Em tela estreita fechada, a gaveta sai de cena: sem isso o leitor de
      // tela e o Tab continuariam alcancando links invisiveis.
      aria-hidden={ehTelaEstreita && !abertaNoCelular}
      inert={ehTelaEstreita && !abertaNoCelular}
    >
      {/* Tooltip unico, delegado por seletor: evita instanciar um por item. */}
      {mostrarApenasIcones && <Tooltip target=".barra-lateral__link" position="right" />}

      <div className="barra-lateral__topo">
        <span className="barra-lateral__marca">
          <i className="pi pi-cloud" aria-hidden="true" />
          {!mostrarApenasIcones && <span>{ambiente.nomeDaAplicacao}</span>}
        </span>

        {ehTelaEstreita ? (
          <Button
            icon="pi pi-times"
            onClick={aoFechar}
            aria-label="Fechar menu"
            severity="secondary"
            text
            rounded
          />
        ) : (
          <Button
            icon={recolhida ? 'pi pi-chevron-right' : 'pi pi-chevron-left'}
            onClick={aoAlternarRecolhida}
            aria-label={recolhida ? 'Expandir menu' : 'Recolher menu'}
            severity="secondary"
            text
            rounded
          />
        )}
      </div>

      <div className="barra-lateral__grupos">
        {menu.map((grupo, indice) => (
          <section
            key={grupo.titulo ?? `grupo-${indice}`}
            className="barra-lateral__grupo"
          >
            {grupo.titulo && !mostrarApenasIcones && (
              <h2 className="barra-lateral__titulo-do-grupo">{grupo.titulo}</h2>
            )}

            <ul className="barra-lateral__lista">
              {grupo.itens.map((item) => (
                <li key={item.caminho}>
                  <NavLink
                    to={item.caminho}
                    className={({ isActive }) =>
                      `barra-lateral__link${isActive ? ' barra-lateral__link--ativo' : ''}`
                    }
                    // end: sem isso a rota "/" ficaria marcada como ativa em
                    // qualquer pagina, porque todo caminho comeca com barra.
                    end={item.caminho === '/'}
                    onClick={ehTelaEstreita ? aoFechar : undefined}
                    data-pr-tooltip={mostrarApenasIcones ? item.rotulo : undefined}
                  >
                    <i className={`pi ${item.icone}`} aria-hidden="true" />
                    <span className="barra-lateral__rotulo">{item.rotulo}</span>
                  </NavLink>
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </nav>
  );
}
