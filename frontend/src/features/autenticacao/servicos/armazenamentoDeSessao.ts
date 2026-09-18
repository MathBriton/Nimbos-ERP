import type { SessaoArmazenada } from '../tipos/autenticacao';

const CHAVE = 'nimbus.sessao';

/**
 * Persistencia da sessao no localStorage.
 *
 * Decisao consciente: o token fica acessivel ao JavaScript, o que o expoe a XSS.
 * A alternativa mais segura seria cookie httpOnly + SameSite, mas ela exige que
 * API e frontend compartilhem site (ou CORS com credenciais e CSRF token), o que
 * nao vale a complexidade neste estagio. O mitigador aqui e a validade curta do
 * token. Migrar para cookie httpOnly seria o proximo passo em um cenario real.
 *
 * Todo acesso e protegido: em aba privada ou com armazenamento bloqueado,
 * localStorage pode lancar excecao.
 */
export const armazenamentoDeSessao = {
  ler(): SessaoArmazenada | null {
    try {
      const bruto = window.localStorage.getItem(CHAVE);
      if (!bruto) {
        return null;
      }

      const sessao = JSON.parse(bruto) as SessaoArmazenada;

      // Descarta formato inesperado (versao antiga do app, dado corrompido).
      if (!sessao?.token || !sessao?.expiraEm || !sessao?.usuario?.id) {
        return null;
      }

      return sessao;
    } catch {
      return null;
    }
  },

  gravar(sessao: SessaoArmazenada): void {
    try {
      window.localStorage.setItem(CHAVE, JSON.stringify(sessao));
    } catch {
      // Sem persistencia a sessao continua valendo em memoria, so nao
      // sobrevive ao reload. Nao e motivo para quebrar o login.
    }
  },

  limpar(): void {
    try {
      window.localStorage.removeItem(CHAVE);
    } catch {
      // Ignora: nada a limpar se o armazenamento esta inacessivel.
    }
  },
};

/** Indica se o token ja expirou, com uma margem de seguranca. */
export function sessaoExpirou(
  expiraEm: string,
  margemEmSegundos = 30,
): boolean {
  const expiracao = new Date(expiraEm).getTime();

  if (Number.isNaN(expiracao)) {
    return true;
  }

  return expiracao - margemEmSegundos * 1000 <= Date.now();
}
