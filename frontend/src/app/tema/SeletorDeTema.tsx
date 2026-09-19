import { useRef } from 'react';
import { Button } from 'primereact/button';
import { Menu } from 'primereact/menu';
import type { MenuItem } from 'primereact/menuitem';

import { useTema } from './useTema';
import type { PreferenciaDeTema } from './tipos';

interface OpcaoDeTema {
  valor: PreferenciaDeTema;
  rotulo: string;
  icone: string;
}

const OPCOES: OpcaoDeTema[] = [
  { valor: 'claro', rotulo: 'Claro', icone: 'pi pi-sun' },
  { valor: 'escuro', rotulo: 'Escuro', icone: 'pi pi-moon' },
  { valor: 'sistema', rotulo: 'Sistema', icone: 'pi pi-desktop' },
];

/**
 * Seletor de tema do cabecalho.
 *
 * O icone do botao mostra a preferencia escolhida - inclusive "sistema", com o
 * icone de monitor. Mostrar o tema resolvido no lugar esconderia do usuario que
 * ele esta no modo automatico.
 */
export function SeletorDeTema() {
  const { preferencia, definirPreferencia } = useTema();
  const menu = useRef<Menu>(null);

  const opcaoAtual = OPCOES.find((opcao) => opcao.valor === preferencia) ?? OPCOES[2];

  const itens: MenuItem[] = OPCOES.map((opcao) => ({
    label: opcao.rotulo,
    icon: opcao.icone,
    // O PrimeReact aplica p-focus ao item ativo; a classe propria marca o
    // selecionado de forma visivel e serve de ancora para os testes.
    className: opcao.valor === preferencia ? 'seletor-de-tema__item--ativo' : undefined,
    command: () => definirPreferencia(opcao.valor),
  }));

  return (
    <>
      <Menu model={itens} popup ref={menu} id="menu-de-tema" />

      <Button
        icon={opcaoAtual.icone}
        onClick={(evento) => menu.current?.toggle(evento)}
        aria-haspopup
        aria-controls="menu-de-tema"
        aria-label={`Tema: ${opcaoAtual.rotulo}. Alterar tema`}
        severity="secondary"
        text
        rounded
      />
    </>
  );
}
