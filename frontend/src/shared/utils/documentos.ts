/**
 * Formatacao e validacao de documentos brasileiros.
 *
 * O algoritmo de digito verificador esta duplicado aqui de proposito: o
 * backend continua sendo a autoridade (ver Documento.cs), mas repetir a
 * checagem no navegador da retorno imediato ao usuario, sem uma ida ao
 * servidor para descobrir que errou um digito. Se as duas versoes divergirem,
 * a do backend prevalece - ela e a que protege o banco.
 */

export const TAMANHO_DO_CPF = 11;
export const TAMANHO_DO_CNPJ = 14;

export function somenteDigitos(valor: string | null | undefined): string {
  return (valor ?? '').replace(/\D/g, '');
}

function todosOsDigitosIguais(digitos: string): boolean {
  return digitos.split('').every((digito) => digito === digitos[0]);
}

/** Digito verificador do CPF: pesos decrescentes a partir de `pesoInicial`. */
function digitoDoCpf(digitos: string, quantidade: number, pesoInicial: number): string {
  let soma = 0;

  for (let indice = 0; indice < quantidade; indice++) {
    soma += Number(digitos[indice]) * (pesoInicial - indice);
  }

  const resto = soma % 11;
  return resto < 2 ? '0' : String(11 - resto);
}

/** Digito verificador do CNPJ: pesos ciclicos de 2 a 9, da direita para a esquerda. */
function digitoDoCnpj(digitos: string, quantidade: number): string {
  let soma = 0;
  let peso = 2;

  for (let indice = quantidade - 1; indice >= 0; indice--) {
    soma += Number(digitos[indice]) * peso;
    peso = peso === 9 ? 2 : peso + 1;
  }

  const resto = soma % 11;
  return resto < 2 ? '0' : String(11 - resto);
}

export function cpfEhValido(valor: string | null | undefined): boolean {
  const digitos = somenteDigitos(valor);

  if (digitos.length !== TAMANHO_DO_CPF || todosOsDigitosIguais(digitos)) {
    return false;
  }

  return (
    digitos[9] === digitoDoCpf(digitos, 9, 10) && digitos[10] === digitoDoCpf(digitos, 10, 11)
  );
}

export function cnpjEhValido(valor: string | null | undefined): boolean {
  const digitos = somenteDigitos(valor);

  if (digitos.length !== TAMANHO_DO_CNPJ || todosOsDigitosIguais(digitos)) {
    return false;
  }

  return digitos[12] === digitoDoCnpj(digitos, 12) && digitos[13] === digitoDoCnpj(digitos, 13);
}

/** Aplica a mascara conforme o tamanho: 11 digitos = CPF, 14 = CNPJ. */
export function formatarDocumento(valor: string | null | undefined): string {
  const digitos = somenteDigitos(valor);

  if (digitos.length === TAMANHO_DO_CPF) {
    return `${digitos.slice(0, 3)}.${digitos.slice(3, 6)}.${digitos.slice(6, 9)}-${digitos.slice(9)}`;
  }

  if (digitos.length === TAMANHO_DO_CNPJ) {
    return `${digitos.slice(0, 2)}.${digitos.slice(2, 5)}.${digitos.slice(5, 8)}/${digitos.slice(8, 12)}-${digitos.slice(12)}`;
  }

  // Tamanho intermediario (usuario ainda digitando): devolve sem mascara.
  return digitos;
}

/** Mascara progressiva, aplicada enquanto o usuario digita. */
export function mascararDocumento(
  valor: string,
  tipoDePessoa: 'Fisica' | 'Juridica',
): string {
  const limite = tipoDePessoa === 'Fisica' ? TAMANHO_DO_CPF : TAMANHO_DO_CNPJ;
  const digitos = somenteDigitos(valor).slice(0, limite);

  if (tipoDePessoa === 'Fisica') {
    return digitos
      .replace(/^(\d{3})(\d)/, '$1.$2')
      .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
      .replace(/\.(\d{3})(\d{1,2})$/, '.$1-$2');
  }

  return digitos
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d{1,2})$/, '$1-$2');
}

export function mascararTelefone(valor: string): string {
  const digitos = somenteDigitos(valor).slice(0, 11);

  if (digitos.length <= 10) {
    return digitos.replace(/^(\d{2})(\d)/, '($1) $2').replace(/(\d{4})(\d{1,4})$/, '$1-$2');
  }

  return digitos.replace(/^(\d{2})(\d)/, '($1) $2').replace(/(\d{5})(\d{1,4})$/, '$1-$2');
}

export function mascararCep(valor: string): string {
  return somenteDigitos(valor)
    .slice(0, 8)
    .replace(/^(\d{5})(\d{1,3})$/, '$1-$2');
}

/** As 27 unidades federativas, para o seletor de UF. */
export const UNIDADES_FEDERATIVAS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS',
  'MG', 'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC',
  'SP', 'SE', 'TO',
] as const;
