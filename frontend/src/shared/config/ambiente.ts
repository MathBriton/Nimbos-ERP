/**
 * Leitura centralizada das variaveis de ambiente do Vite.
 * Manter tudo aqui evita `import.meta.env` espalhado pelo codigo e
 * garante que uma variavel faltando falhe cedo, com mensagem clara.
 */
function obrigatoria(nome: string, valor: string | undefined): string {
  if (!valor) {
    throw new Error(
      `Variavel de ambiente ${nome} nao definida. Copie .env.example para .env.`,
    );
  }
  return valor;
}

export const ambiente = {
  /** URL base da API do ERP (ex.: http://localhost:5080). */
  urlDaApi: obrigatoria('VITE_API_URL', import.meta.env.VITE_API_URL),
  /** Nome exibido no header e no titulo da aba. */
  nomeDaAplicacao: import.meta.env.VITE_APP_NOME ?? 'Nimbus ERP',
  ehDesenvolvimento: import.meta.env.DEV,
} as const;
