import { ID_DO_LINK_DO_TEMA, type TemaEfetivo } from './tipos';

/** URL do CSS de um tema, servido a partir de public/temas/. */
export function urlDoTema(tema: TemaEfetivo): string {
  return `/temas/${tema}/theme.css`;
}

/**
 * Troca o CSS do tema sem piscar.
 *
 * Um <link> novo e inserido e so quando ele termina de carregar o antigo e
 * removido. Trocar o href do proprio link faria o navegador descartar o estilo
 * atual na hora e deixar a tela sem tema por alguns quadros.
 *
 * O atributo data-tema no <html> e atualizado imediatamente: ele governa as
 * cores de base do projeto (ver base.css) e nao depende do CSS do PrimeReact.
 */
export function aplicarTema(tema: TemaEfetivo): void {
  document.documentElement.dataset.tema = tema;

  // Faz o navegador pintar scrollbars, inputs nativos e a cor de fundo padrao
  // de acordo com o tema.
  document.documentElement.style.colorScheme = tema === 'escuro' ? 'dark' : 'light';

  const url = urlDoTema(tema);
  const linkAtual = document.getElementById(ID_DO_LINK_DO_TEMA) as HTMLLinkElement | null;

  // Compara pelo pathname: a propriedade href e absoluta, o atributo e relativo.
  if (linkAtual?.getAttribute('href') === url) {
    return;
  }

  const linkNovo = document.createElement('link');
  linkNovo.rel = 'stylesheet';
  linkNovo.href = url;

  // Sem tema anterior nao ha troca a suavizar: assume o id na hora. Evita
  // tambem que o link fique orfao onde o evento de load nunca chega (jsdom).
  if (!linkAtual) {
    linkNovo.id = ID_DO_LINK_DO_TEMA;
    document.head.appendChild(linkNovo);
    return;
  }

  function promover() {
    linkAtual?.remove();
    linkNovo.id = ID_DO_LINK_DO_TEMA;
  }

  linkNovo.addEventListener('load', promover, { once: true });

  // Se o CSS falhar (offline, arquivo ausente), promove assim mesmo: e melhor
  // ficar sem tema do que manter dois <link> concorrentes para sempre.
  linkNovo.addEventListener('error', promover, { once: true });

  linkAtual.insertAdjacentElement('afterend', linkNovo);
}
