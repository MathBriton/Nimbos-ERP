/**
 * Preferencia escolhida pelo usuario.
 * "sistema" acompanha a configuracao do sistema operacional.
 */
export type PreferenciaDeTema = 'claro' | 'escuro' | 'sistema';

/** Tema realmente aplicado na tela, ja com "sistema" resolvido. */
export type TemaEfetivo = 'claro' | 'escuro';

/** Chave da preferencia no localStorage. Usada tambem pelo script do index.html. */
export const CHAVE_DO_TEMA = 'nimbus.tema';

/** Id do <link> que carrega o CSS do tema. Tambem definido no index.html. */
export const ID_DO_LINK_DO_TEMA = 'tema-do-app';

export const PREFERENCIAS_DE_TEMA: readonly PreferenciaDeTema[] = [
  'claro',
  'escuro',
  'sistema',
];

export function ehPreferenciaValida(valor: unknown): valor is PreferenciaDeTema {
  return (
    valor === 'claro' || valor === 'escuro' || valor === 'sistema'
  );
}
