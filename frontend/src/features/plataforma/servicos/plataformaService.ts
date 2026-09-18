import { clienteHttp } from '../../../shared/api/clienteHttp';
import type { IdentificacaoDaApi, RelatorioDeSaude } from '../tipos/saude';

/** Consulta os endpoints de diagnostico da API (usados na tela de status). */
export const plataformaService = {
  async obterIdentificacao(): Promise<IdentificacaoDaApi> {
    const { data } = await clienteHttp.get<IdentificacaoDaApi>('/');
    return data;
  },

  async obterSaude(): Promise<RelatorioDeSaude> {
    // O endpoint responde 503 quando degradado; validateStatus evita que o
    // axios trate isso como falha, porque o corpo continua sendo util.
    const { data } = await clienteHttp.get<RelatorioDeSaude>('/health', {
      validateStatus: (status) => status === 200 || status === 503,
    });
    return data;
  },
};
