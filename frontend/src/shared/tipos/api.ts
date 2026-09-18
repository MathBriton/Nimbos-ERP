/** Envelope de listagens paginadas devolvido pela API (ResultadoPaginado<T>). */
export interface ResultadoPaginado<T> {
  itens: T[];
  paginaAtual: number;
  tamanhoDaPagina: number;
  totalDeItens: number;
  totalDePaginas: number;
  temPaginaAnterior: boolean;
  temProximaPagina: boolean;
}

/** Parametros de paginacao/busca aceitos pelas listagens. */
export interface ConsultaPaginada {
  pagina?: number;
  tamanhoDaPagina?: number;
  busca?: string;
}

/** Corpo de erro padronizado da API (ProblemDetails, RFC 9457). */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}
