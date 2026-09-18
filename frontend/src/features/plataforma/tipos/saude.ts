/** Estados possiveis de um health check do ASP.NET Core. */
export type StatusDeSaude = 'Healthy' | 'Degraded' | 'Unhealthy';

export interface VerificacaoDeSaude {
  nome: string;
  status: StatusDeSaude;
  duracaoMs: number;
  descricao?: string;
}

export interface RelatorioDeSaude {
  status: StatusDeSaude;
  duracaoMs: number;
  verificacoes: VerificacaoDeSaude[];
}

export interface IdentificacaoDaApi {
  aplicacao: string;
  versao: string;
  ambiente: string;
  documentacao: string;
  saude: string;
}
