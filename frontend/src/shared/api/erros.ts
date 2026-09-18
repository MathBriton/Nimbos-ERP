import { AxiosError } from 'axios';

import type { ProblemDetails } from '../tipos/api';

/** Erro de API normalizado, pronto para exibir em toast/mensagem de formulario. */
export class ErroDaApi extends Error {
  readonly status: number;
  readonly errosDeValidacao: Record<string, string[]>;

  constructor(mensagem: string, status: number, errosDeValidacao: Record<string, string[]> = {}) {
    super(mensagem);
    this.name = 'ErroDaApi';
    this.status = status;
    this.errosDeValidacao = errosDeValidacao;
  }

  get ehNaoAutenticado(): boolean {
    return this.status === 401;
  }

  get ehSemPermissao(): boolean {
    return this.status === 403;
  }
}

/** Converte qualquer falha do axios em um {@link ErroDaApi} com mensagem legivel. */
export function normalizarErro(erro: unknown): ErroDaApi {
  if (erro instanceof ErroDaApi) {
    return erro;
  }

  if (erro instanceof AxiosError) {
    if (!erro.response) {
      return new ErroDaApi(
        'Nao foi possivel falar com o servidor. Verifique se a API esta no ar.',
        0,
      );
    }

    const problema = erro.response.data as ProblemDetails | undefined;

    return new ErroDaApi(
      problema?.detail ?? problema?.title ?? erro.message,
      erro.response.status,
      problema?.errors ?? {},
    );
  }

  return new ErroDaApi(
    erro instanceof Error ? erro.message : 'Erro inesperado.',
    0,
  );
}
