import { clienteHttp } from '../../../shared/api/clienteHttp';
import type { ResultadoPaginado } from '../../../shared/tipos/api';
import type { Cliente, ConsultaDeClientes, SalvarCliente } from '../tipos/cliente';

const RECURSO = '/api/clientes';

export const clientesService = {
  async listar(consulta: ConsultaDeClientes): Promise<ResultadoPaginado<Cliente>> {
    // Campos nulos/indefinidos ficam de fora da query string: mandar
    // "ativo=" vazio faria o binder do ASP.NET recusar a requisicao.
    const parametros: Record<string, string> = {};

    for (const [chave, valor] of Object.entries(consulta)) {
      if (valor !== undefined && valor !== null && valor !== '') {
        parametros[chave] = String(valor);
      }
    }

    const { data } = await clienteHttp.get<ResultadoPaginado<Cliente>>(RECURSO, {
      params: parametros,
    });

    return data;
  },

  async obterPorId(id: string): Promise<Cliente> {
    const { data } = await clienteHttp.get<Cliente>(`${RECURSO}/${id}`);
    return data;
  },

  async criar(cliente: SalvarCliente): Promise<Cliente> {
    const { data } = await clienteHttp.post<Cliente>(RECURSO, cliente);
    return data;
  },

  async atualizar(id: string, cliente: SalvarCliente): Promise<Cliente> {
    const { data } = await clienteHttp.put<Cliente>(`${RECURSO}/${id}`, cliente);
    return data;
  },

  async excluir(id: string): Promise<void> {
    await clienteHttp.delete(`${RECURSO}/${id}`);
  },
};
