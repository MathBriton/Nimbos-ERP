import { Outlet } from 'react-router';
import { Avatar } from 'primereact/avatar';
import { Button } from 'primereact/button';

import { useAutenticacao } from '../../features/autenticacao/hooks/useAutenticacao';
import { ambiente } from '../../shared/config/ambiente';

/**
 * Casca da area logada: header com o usuario da sessao e botao de sair.
 *
 * Deliberadamente minimo nesta sprint. A Sprint 2 substitui isto pelo layout
 * definitivo, com sidebar de navegacao e comportamento responsivo.
 */
export function LayoutAutenticado() {
  const { usuario, sair } = useAutenticacao();

  const iniciais = (usuario?.nome ?? '?')
    .split(' ')
    .filter((parte) => parte.length > 0)
    .slice(0, 2)
    .map((parte) => parte[0]?.toUpperCase() ?? '')
    .join('');

  return (
    <div className="layout">
      <header className="layout__header">
        <div className="layout__marca">
          <i className="pi pi-cloud" aria-hidden="true" />
          <span>{ambiente.nomeDaAplicacao}</span>
        </div>

        <div className="layout__usuario">
          <Avatar label={iniciais} shape="circle" />
          <div className="layout__identificacao">
            <strong>{usuario?.nome}</strong>
            <small>{usuario?.email}</small>
          </div>

          <Button
            label="Sair"
            icon="pi pi-sign-out"
            onClick={sair}
            severity="secondary"
            text
          />
        </div>
      </header>

      <main className="layout__conteudo">
        <Outlet />
      </main>
    </div>
  );
}
