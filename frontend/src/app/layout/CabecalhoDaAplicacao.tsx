import { useRef } from 'react';
import { Avatar } from 'primereact/avatar';
import { Button } from 'primereact/button';
import { Menu } from 'primereact/menu';

import { useAutenticacao } from '../../features/autenticacao/hooks/useAutenticacao';

interface Props {
  titulo: string;
  ehTelaEstreita: boolean;
  menuAberto: boolean;
  aoAlternarMenu: () => void;
}

/** Extrai as iniciais do nome para o avatar (no maximo duas letras). */
function iniciaisDe(nome: string | undefined): string {
  return (nome ?? '?')
    .split(' ')
    .filter((parte) => parte.length > 0)
    .slice(0, 2)
    .map((parte) => parte[0]?.toUpperCase() ?? '')
    .join('');
}

export function CabecalhoDaAplicacao({
  titulo,
  ehTelaEstreita,
  menuAberto,
  aoAlternarMenu,
}: Props) {
  const { usuario, sair } = useAutenticacao();
  const menuDoUsuario = useRef<Menu>(null);

  return (
    <header className="cabecalho">
      <div className="cabecalho__esquerda">
        {/* Nome acessivel estavel + aria-expanded: e a pratica recomendada para
            um botao de alternancia, e evita que este botao e o de fechar da
            propria gaveta acabem com o mesmo nome acessivel. */}
        {ehTelaEstreita && (
          <Button
            icon="pi pi-bars"
            onClick={aoAlternarMenu}
            aria-label="Menu de navegacao"
            aria-expanded={menuAberto}
            aria-controls="barra-lateral"
            severity="secondary"
            text
            rounded
          />
        )}

        <h1 className="cabecalho__titulo">{titulo}</h1>
      </div>

      <div className="cabecalho__direita">
        <div className="cabecalho__identificacao">
          <strong>{usuario?.nome}</strong>
          <small>{usuario?.email}</small>
        </div>

        <Menu
          model={[
            {
              label: usuario?.email ?? '',
              disabled: true,
            },
            { separator: true },
            {
              label: 'Sair',
              icon: 'pi pi-sign-out',
              command: sair,
            },
          ]}
          popup
          ref={menuDoUsuario}
          id="menu-do-usuario"
        />

        <button
          type="button"
          className="cabecalho__botao-do-avatar"
          onClick={(evento) => menuDoUsuario.current?.toggle(evento)}
          aria-haspopup
          aria-controls="menu-do-usuario"
          aria-label={`Conta de ${usuario?.nome ?? 'usuario'}`}
        >
          <Avatar label={iniciaisDe(usuario?.nome)} shape="circle" />
        </button>
      </div>
    </header>
  );
}
