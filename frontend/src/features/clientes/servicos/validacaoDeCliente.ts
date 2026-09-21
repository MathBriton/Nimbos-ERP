import {
  cnpjEhValido,
  cpfEhValido,
  somenteDigitos,
  UNIDADES_FEDERATIVAS,
} from '../../../shared/utils/documentos';
import type { SalvarCliente } from '../tipos/cliente';

/** Erros por campo. A chave e o nome do campo no formulario. */
export type ErrosDoFormulario = Partial<Record<string, string>>;

/**
 * Valida o formulario de cliente no navegador.
 *
 * Espelha as regras do dominio para dar retorno imediato; o backend revalida
 * tudo e continua sendo a autoridade. Funcao pura de proposito: da para testar
 * sem montar componente nenhum.
 */
export function validarCliente(cliente: SalvarCliente): ErrosDoFormulario {
  const erros: ErrosDoFormulario = {};

  if (!cliente.nome?.trim()) {
    erros.nome = 'Informe o nome.';
  } else if (cliente.nome.trim().length > 200) {
    erros.nome = 'O nome pode ter no maximo 200 caracteres.';
  }

  if (cliente.tipoDePessoa === 'Fisica' && cliente.nomeFantasia?.trim()) {
    erros.nomeFantasia = 'Nome fantasia se aplica apenas a pessoa juridica.';
  }

  const documento = somenteDigitos(cliente.documento);

  if (!documento) {
    erros.documento = cliente.tipoDePessoa === 'Fisica' ? 'Informe o CPF.' : 'Informe o CNPJ.';
  } else if (cliente.tipoDePessoa === 'Fisica' && !cpfEhValido(documento)) {
    erros.documento = 'CPF invalido.';
  } else if (cliente.tipoDePessoa === 'Juridica' && !cnpjEhValido(documento)) {
    erros.documento = 'CNPJ invalido.';
  }

  if (cliente.email?.trim() && !emailEhValido(cliente.email.trim())) {
    erros.email = 'Informe um e-mail valido.';
  }

  const telefone = somenteDigitos(cliente.telefone);

  if (telefone && telefone.length !== 10 && telefone.length !== 11) {
    erros.telefone = 'O telefone precisa ter 10 ou 11 digitos, incluindo o DDD.';
  }

  return { ...erros, ...validarEndereco(cliente) };
}

/**
 * Endereco e tudo ou nada: ou nenhum campo preenchido, ou o conjunto minimo
 * que o torna utilizavel. Mesma regra do objeto de valor no dominio.
 */
function validarEndereco(cliente: SalvarCliente): ErrosDoFormulario {
  const endereco = cliente.endereco;

  if (!endereco) {
    return {};
  }

  const campos = [
    endereco.cep,
    endereco.logradouro,
    endereco.numero,
    endereco.complemento,
    endereco.bairro,
    endereco.cidade,
    endereco.uf,
  ];

  if (campos.every((campo) => !campo?.trim())) {
    return {};
  }

  const erros: ErrosDoFormulario = {};

  if (somenteDigitos(endereco.cep).length !== 8) {
    erros['endereco.cep'] = 'O CEP precisa ter 8 digitos.';
  }

  if (!endereco.logradouro?.trim()) {
    erros['endereco.logradouro'] = 'Informe o logradouro.';
  }

  if (!endereco.numero?.trim()) {
    erros['endereco.numero'] = 'Informe o numero.';
  }

  if (!endereco.bairro?.trim()) {
    erros['endereco.bairro'] = 'Informe o bairro.';
  }

  if (!endereco.cidade?.trim()) {
    erros['endereco.cidade'] = 'Informe a cidade.';
  }

  const uf = (endereco.uf ?? '').trim().toUpperCase();

  if (!UNIDADES_FEDERATIVAS.includes(uf as (typeof UNIDADES_FEDERATIVAS)[number])) {
    erros['endereco.uf'] = 'Selecione a UF.';
  }

  return erros;
}

/** Mesma checagem simples usada no backend: barra erro grosseiro de digitacao. */
function emailEhValido(email: string): boolean {
  const posicaoDoArroba = email.indexOf('@');

  return (
    posicaoDoArroba > 0 &&
    posicaoDoArroba < email.length - 1 &&
    email.indexOf('@', posicaoDoArroba + 1) < 0 &&
    email.includes('.') &&
    !email.includes(' ')
  );
}
