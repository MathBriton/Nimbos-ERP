/** Espelha TipoDePessoa do backend (enum serializado como texto). */
export type TipoDePessoa = 'Fisica' | 'Juridica';

/** Campos pelos quais a listagem pode ser ordenada. */
export type OrdenacaoDeClientes = 'Nome' | 'Documento' | 'CriadoEm';

export interface Endereco {
  cep?: string | null;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  uf?: string | null;
}

export interface Cliente {
  id: string;
  nome: string;
  nomeFantasia?: string | null;
  tipoDePessoa: TipoDePessoa;
  /** Somente digitos, como esta persistido. */
  documento: string;
  /** Com mascara, pronto para exibir. */
  documentoFormatado: string;
  email?: string | null;
  telefone?: string | null;
  telefoneFormatado?: string | null;
  endereco?: Endereco | null;
  observacoes?: string | null;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm?: string | null;
}

/** Corpo de criacao e de edicao (POST e PUT usam o mesmo formato). */
export interface SalvarCliente {
  nome: string;
  nomeFantasia?: string | null;
  tipoDePessoa: TipoDePessoa;
  documento: string;
  email?: string | null;
  telefone?: string | null;
  endereco?: Endereco | null;
  observacoes?: string | null;
  ativo: boolean;
}

/** Parametros de GET /api/clientes. */
export interface ConsultaDeClientes {
  pagina?: number;
  tamanhoDaPagina?: number;
  busca?: string;
  ativo?: boolean | null;
  tipoDePessoa?: TipoDePessoa | null;
  ordenacao?: OrdenacaoDeClientes;
  descendente?: boolean;
}
