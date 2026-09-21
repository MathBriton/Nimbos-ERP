import { describe, expect, it } from 'vitest';

import {
  cnpjEhValido,
  cpfEhValido,
  formatarDocumento,
  mascararCep,
  mascararDocumento,
  mascararTelefone,
  somenteDigitos,
} from '../../../shared/utils/documentos';
import { validarCliente } from './validacaoDeCliente';
import type { SalvarCliente } from '../tipos/cliente';

const CPF_VALIDO = '52998224725';
const CNPJ_VALIDO = '11222333000181';

function cliente(mudanca: Partial<SalvarCliente> = {}): SalvarCliente {
  return {
    nome: 'Maria Silva',
    tipoDePessoa: 'Fisica',
    documento: CPF_VALIDO,
    ativo: true,
    ...mudanca,
  };
}

describe('validacao de CPF e CNPJ no navegador', () => {
  it.each([CPF_VALIDO, '529.982.247-25'])('aceita CPF valido: %s', (valor) => {
    expect(cpfEhValido(valor)).toBe(true);
  });

  it.each(['52998224724', '12345678901', '11111111111', '5299822472', ''])(
    'rejeita CPF invalido: %s',
    (valor) => {
      expect(cpfEhValido(valor)).toBe(false);
    },
  );

  it.each([CNPJ_VALIDO, '11.222.333/0001-81'])('aceita CNPJ valido: %s', (valor) => {
    expect(cnpjEhValido(valor)).toBe(true);
  });

  it.each(['11222333000182', '12345678000100', '11111111111111', ''])(
    'rejeita CNPJ invalido: %s',
    (valor) => {
      expect(cnpjEhValido(valor)).toBe(false);
    },
  );

  it('concorda com o backend: mesma regra, mesmo resultado', () => {
    // Os mesmos documentos usados em DocumentoTests.cs. Se as duas validacoes
    // divergirem, o usuario ve um erro so depois de enviar o formulario.
    expect(cpfEhValido('529.982.247-25')).toBe(true);
    expect(cnpjEhValido('11.222.333/0001-81')).toBe(true);
    expect(cpfEhValido('52998224724')).toBe(false);
    expect(cnpjEhValido('11222333000182')).toBe(false);
  });
});

describe('mascaras', () => {
  it('formata documento conforme o tamanho', () => {
    expect(formatarDocumento(CPF_VALIDO)).toBe('529.982.247-25');
    expect(formatarDocumento(CNPJ_VALIDO)).toBe('11.222.333/0001-81');
  });

  it('mascara o CPF progressivamente e limita o tamanho', () => {
    expect(mascararDocumento('529', 'Fisica')).toBe('529');
    expect(mascararDocumento('529982', 'Fisica')).toBe('529.982');
    expect(mascararDocumento('52998224725', 'Fisica')).toBe('529.982.247-25');
    // Digitos alem do limite sao descartados.
    expect(mascararDocumento('529982247259999', 'Fisica')).toBe('529.982.247-25');
  });

  it('mascara o CNPJ progressivamente', () => {
    expect(mascararDocumento('11222333000181', 'Juridica')).toBe('11.222.333/0001-81');
  });

  it('mascara telefone fixo e celular', () => {
    expect(mascararTelefone('1133334444')).toBe('(11) 3333-4444');
    expect(mascararTelefone('11987654321')).toBe('(11) 98765-4321');
  });

  it('mascara CEP', () => {
    expect(mascararCep('01310100')).toBe('01310-100');
  });

  it('somenteDigitos remove qualquer pontuacao', () => {
    expect(somenteDigitos('529.982.247-25')).toBe('52998224725');
    expect(somenteDigitos(null)).toBe('');
  });
});

describe('validarCliente', () => {
  it('nao aponta erro em um cliente valido', () => {
    expect(validarCliente(cliente())).toEqual({});
  });

  it('exige o nome', () => {
    expect(validarCliente(cliente({ nome: '   ' })).nome).toBe('Informe o nome.');
  });

  it('limita o tamanho do nome', () => {
    const erros = validarCliente(cliente({ nome: 'a'.repeat(201) }));

    expect(erros.nome).toContain('200 caracteres');
  });

  it('exige documento', () => {
    expect(validarCliente(cliente({ documento: '' })).documento).toBe('Informe o CPF.');
    expect(
      validarCliente(cliente({ tipoDePessoa: 'Juridica', documento: '' })).documento,
    ).toBe('Informe o CNPJ.');
  });

  it('aponta CPF e CNPJ invalidos', () => {
    expect(validarCliente(cliente({ documento: '12345678901' })).documento).toBe('CPF invalido.');
    expect(
      validarCliente(cliente({ tipoDePessoa: 'Juridica', documento: '12345678000100' })).documento,
    ).toBe('CNPJ invalido.');
  });

  it('rejeita nome fantasia em pessoa fisica', () => {
    const erros = validarCliente(cliente({ nomeFantasia: 'Lojinha' }));

    expect(erros.nomeFantasia).toContain('pessoa juridica');
  });

  it('aceita nome fantasia em pessoa juridica', () => {
    const erros = validarCliente(
      cliente({ tipoDePessoa: 'Juridica', documento: CNPJ_VALIDO, nomeFantasia: 'Acme' }),
    );

    expect(erros.nomeFantasia).toBeUndefined();
  });

  it('valida o e-mail quando informado', () => {
    expect(validarCliente(cliente({ email: 'sem-arroba' })).email).toBe('Informe um e-mail valido.');
    expect(validarCliente(cliente({ email: 'maria@empresa.com' })).email).toBeUndefined();
    // E-mail e opcional.
    expect(validarCliente(cliente({ email: '' })).email).toBeUndefined();
  });

  it('valida a quantidade de digitos do telefone', () => {
    expect(validarCliente(cliente({ telefone: '987654321' })).telefone).toContain('10 ou 11');
    expect(validarCliente(cliente({ telefone: '(11) 98765-4321' })).telefone).toBeUndefined();
    expect(validarCliente(cliente({ telefone: '' })).telefone).toBeUndefined();
  });

  describe('endereco e tudo ou nada', () => {
    it('endereco totalmente vazio nao gera erro', () => {
      const erros = validarCliente(
        cliente({ endereco: { cep: '', logradouro: '', numero: '', bairro: '', cidade: '', uf: '' } }),
      );

      expect(erros).toEqual({});
    });

    it('preencher so um campo exige o endereco inteiro', () => {
      const erros = validarCliente(cliente({ endereco: { cidade: 'Sao Paulo' } }));

      expect(erros['endereco.cep']).toBeDefined();
      expect(erros['endereco.logradouro']).toBeDefined();
      expect(erros['endereco.numero']).toBeDefined();
      expect(erros['endereco.bairro']).toBeDefined();
      expect(erros['endereco.uf']).toBeDefined();
      // A cidade foi a unica preenchida, entao nao aparece como erro.
      expect(erros['endereco.cidade']).toBeUndefined();
    });

    it('endereco completo nao gera erro', () => {
      const erros = validarCliente(
        cliente({
          endereco: {
            cep: '01310-100',
            logradouro: 'Avenida Paulista',
            numero: '1578',
            bairro: 'Bela Vista',
            cidade: 'Sao Paulo',
            uf: 'SP',
          },
        }),
      );

      expect(erros).toEqual({});
    });

    it('rejeita CEP de tamanho errado e UF inexistente', () => {
      const erros = validarCliente(
        cliente({
          endereco: {
            cep: '0131',
            logradouro: 'Avenida Paulista',
            numero: '1578',
            bairro: 'Bela Vista',
            cidade: 'Sao Paulo',
            uf: 'XX',
          },
        }),
      );

      expect(erros['endereco.cep']).toContain('8 digitos');
      expect(erros['endereco.uf']).toBe('Selecione a UF.');
    });

    it('aceita UF em minusculas', () => {
      const erros = validarCliente(
        cliente({
          endereco: {
            cep: '01310-100',
            logradouro: 'Avenida Paulista',
            numero: '1578',
            bairro: 'Bela Vista',
            cidade: 'Sao Paulo',
            uf: 'sp',
          },
        }),
      );

      expect(erros['endereco.uf']).toBeUndefined();
    });
  });
});
