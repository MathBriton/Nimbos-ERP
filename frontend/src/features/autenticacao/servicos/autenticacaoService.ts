import { clienteHttp } from '../../../shared/api/clienteHttp';
import type {
  LoginRequisicao,
  LoginResposta,
  UsuarioAutenticado,
} from '../tipos/autenticacao';

export const autenticacaoService = {
  async login(credenciais: LoginRequisicao): Promise<LoginResposta> {
    const { data } = await clienteHttp.post<LoginResposta>('/api/auth/login', credenciais);
    return data;
  },

  /** Reidrata a sessao ao recarregar a pagina; 401 significa token invalido. */
  async obterUsuarioAtual(): Promise<UsuarioAutenticado> {
    const { data } = await clienteHttp.get<UsuarioAutenticado>('/api/auth/eu');
    return data;
  },
};
