import { AxiosError } from 'axios';

import type { ProblemDetails } from '../tipos/api';
import { HEADER_DE_TOKEN_EXPIRADO } from './constantes';

/** Erro de API normalizado, pronto para exibir em toast/mensagem de formulario. */
export class ErroDaApi extends Error {
  readonly status: number;
  readonly errosDeValidacao: Record<string, string[]>;
  /** Caminho da requisicao que falhou, quando conhecido. */
  readonly caminho?: string;
  /** Verdadeiro quando a API sinalizou que o 401 veio de token expirado. */
  readonly tokenExpirou: boolean;

  constructor(
    mensagem: string,
    status: number,
    opcoes: {
      errosDeValidacao?: Record<string, string[]>;
      caminho?: string;
      tokenExpirou?: boolean;
    } = {},
  ) {
    super(mensagem);
    this.name = 'ErroDaApi';
    this.status = status;
    this.errosDeValidacao = opcoes.errosDeValidacao ?? {};
    this.caminho = opcoes.caminho;
    this.tokenExpirou = opcoes.tokenExpirou ?? false;
  }

  get ehNaoAutenticado(): boolean {
    return this.status === 401;
  }

  get ehSemPermissao(): boolean {
    return this.status === 403;
  }

  /** Primeira mensagem de validacao de um campo, para exibir sob o input. */
  erroDoCampo(campo: string): string | undefined {
    // A API responde com as chaves capitalizadas ("Email"); aceita as duas formas.
    const comInicialMaiuscula = campo.charAt(0).toUpperCase() + campo.slice(1);

    return (
      this.errosDeValidacao[campo]?.[0] ?? this.errosDeValidacao[comInicialMaiuscula]?.[0]
    );
  }
}

/** Converte qualquer falha do axios em um {@link ErroDaApi} com mensagem legivel. */
export function normalizarErro(erro: unknown): ErroDaApi {
  if (erro instanceof ErroDaApi) {
    return erro;
  }

  if (erro instanceof AxiosError) {
    const caminho = erro.config?.url;

    if (!erro.response) {
      return new ErroDaApi(
        'Nao foi possivel falar com o servidor. Verifique se a API esta no ar.',
        0,
        { caminho },
      );
    }

    const problema = erro.response.data as ProblemDetails | undefined;
    const tokenExpirou =
      String(erro.response.headers?.[HEADER_DE_TOKEN_EXPIRADO] ?? '') === 'true';

    return new ErroDaApi(
      problema?.detail ?? problema?.title ?? mensagemPadraoPara(erro.response.status),
      erro.response.status,
      { errosDeValidacao: problema?.errors ?? {}, caminho, tokenExpirou },
    );
  }

  return new ErroDaApi(erro instanceof Error ? erro.message : 'Erro inesperado.', 0);
}

function mensagemPadraoPara(status: number): string {
  switch (status) {
    case 401:
      return 'Sessao expirada. Faca login novamente.';
    case 403:
      return 'Voce nao tem permissao para esta acao.';
    case 404:
      return 'Registro nao encontrado.';
    default:
      return 'Erro ao processar a requisicao.';
  }
}
