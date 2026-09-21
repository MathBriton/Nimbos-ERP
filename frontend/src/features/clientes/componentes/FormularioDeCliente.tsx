import { type FormEvent, useState } from 'react';
import { Button } from 'primereact/button';
import { Dialog } from 'primereact/dialog';
import { Dropdown } from 'primereact/dropdown';
import { InputText } from 'primereact/inputtext';
import { InputTextarea } from 'primereact/inputtextarea';
import { Message } from 'primereact/message';
import { SelectButton } from 'primereact/selectbutton';
import { ToggleButton } from 'primereact/togglebutton';

import { ErroDaApi } from '../../../shared/api/erros';
import {
  mascararCep,
  mascararDocumento,
  mascararTelefone,
  somenteDigitos,
  UNIDADES_FEDERATIVAS,
} from '../../../shared/utils/documentos';
import { clientesService } from '../servicos/clientesService';
import { validarCliente, type ErrosDoFormulario } from '../servicos/validacaoDeCliente';
import type { Cliente, SalvarCliente, TipoDePessoa } from '../tipos/cliente';

const OPCOES_DE_TIPO = [
  { label: 'Pessoa fisica', value: 'Fisica' satisfies TipoDePessoa },
  { label: 'Pessoa juridica', value: 'Juridica' satisfies TipoDePessoa },
];

const FORMULARIO_VAZIO: SalvarCliente = {
  nome: '',
  nomeFantasia: '',
  tipoDePessoa: 'Fisica',
  documento: '',
  email: '',
  telefone: '',
  endereco: {
    cep: '',
    logradouro: '',
    numero: '',
    complemento: '',
    bairro: '',
    cidade: '',
    uf: '',
  },
  observacoes: '',
  ativo: true,
};

interface Props {
  visivel: boolean;
  /** Cliente em edicao; ausente significa criacao. */
  cliente: Cliente | null;
  aoFechar: () => void;
  aoSalvar: (mensagem: string) => void;
}

/** Converte o cliente vindo da API para o formato do formulario. */
function paraFormulario(cliente: Cliente): SalvarCliente {
  return {
    nome: cliente.nome,
    nomeFantasia: cliente.nomeFantasia ?? '',
    tipoDePessoa: cliente.tipoDePessoa,
    documento: cliente.documentoFormatado,
    email: cliente.email ?? '',
    telefone: cliente.telefoneFormatado ?? '',
    endereco: {
      cep: cliente.endereco?.cep ? mascararCep(cliente.endereco.cep) : '',
      logradouro: cliente.endereco?.logradouro ?? '',
      numero: cliente.endereco?.numero ?? '',
      complemento: cliente.endereco?.complemento ?? '',
      bairro: cliente.endereco?.bairro ?? '',
      cidade: cliente.endereco?.cidade ?? '',
      uf: cliente.endereco?.uf ?? '',
    },
    observacoes: cliente.observacoes ?? '',
    ativo: cliente.ativo,
  };
}

export function FormularioDeCliente({ visivel, cliente, aoFechar, aoSalvar }: Props) {
  return (
    <Dialog
      header={cliente ? 'Editar cliente' : 'Novo cliente'}
      visible={visivel}
      onHide={aoFechar}
      style={{ width: 'min(46rem, 95vw)' }}
      breakpoints={{ '640px': '95vw' }}
      draggable={false}
      blockScroll
    >
      {/*
        O corpo so existe enquanto o dialogo esta aberto, e a key muda a cada
        cliente. E assim que o React reinicia estado: um useEffect limpando os
        campos na abertura faria o mesmo, mas com um render extra a toa.
      */}
      {visivel && (
        <CorpoDoFormulario
          key={cliente?.id ?? 'novo'}
          cliente={cliente}
          aoFechar={aoFechar}
          aoSalvar={aoSalvar}
        />
      )}
    </Dialog>
  );
}

function CorpoDoFormulario({
  cliente,
  aoFechar,
  aoSalvar,
}: Omit<Props, 'visivel'>) {
  const [formulario, definirFormulario] = useState<SalvarCliente>(() =>
    cliente ? paraFormulario(cliente) : FORMULARIO_VAZIO,
  );
  const [erros, definirErros] = useState<ErrosDoFormulario>({});
  const [erroGeral, definirErroGeral] = useState<string | null>(null);
  const [salvando, definirSalvando] = useState(false);

  const ehEdicao = cliente !== null;

  function alterar<C extends keyof SalvarCliente>(campo: C, valor: SalvarCliente[C]) {
    definirFormulario((anterior) => ({ ...anterior, [campo]: valor }));
  }

  function alterarEndereco(campo: string, valor: string) {
    definirFormulario((anterior) => ({
      ...anterior,
      endereco: { ...anterior.endereco, [campo]: valor },
    }));
  }

  /** Trocar o tipo de pessoa invalida o documento e o nome fantasia digitados. */
  function alterarTipoDePessoa(tipo: TipoDePessoa) {
    definirFormulario((anterior) => ({
      ...anterior,
      tipoDePessoa: tipo,
      documento: '',
      nomeFantasia: tipo === 'Fisica' ? '' : anterior.nomeFantasia,
    }));
  }

  async function aoEnviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();

    const errosLocais = validarCliente(formulario);
    definirErros(errosLocais);
    definirErroGeral(null);

    if (Object.keys(errosLocais).length > 0) {
      return;
    }

    definirSalvando(true);

    try {
      // O que vai para a API sai sem mascara: o dominio guarda so digitos.
      const payload: SalvarCliente = {
        ...formulario,
        nome: formulario.nome.trim(),
        nomeFantasia: formulario.nomeFantasia?.trim() || null,
        documento: somenteDigitos(formulario.documento),
        email: formulario.email?.trim() || null,
        telefone: somenteDigitos(formulario.telefone) || null,
        observacoes: formulario.observacoes?.trim() || null,
        endereco: enderecoParaEnvio(formulario),
      };

      if (ehEdicao) {
        await clientesService.atualizar(cliente.id, payload);
        aoSalvar('Cliente atualizado.');
      } else {
        await clientesService.criar(payload);
        aoSalvar('Cliente cadastrado.');
      }
    } catch (falha) {
      if (falha instanceof ErroDaApi) {
        // Erros por campo vindos do backend entram nos inputs; o resto (409 de
        // documento duplicado, por exemplo) vira mensagem no topo.
        const errosDaApi = mapearErrosDaApi(falha);

        definirErros(errosDaApi);
        definirErroGeral(Object.keys(errosDaApi).length > 0 ? null : falha.message);
      } else {
        definirErroGeral('Nao foi possivel salvar o cliente.');
      }
    } finally {
      definirSalvando(false);
    }
  }

  return (
    <form
      className="formulario"
      onSubmit={(evento) => void aoEnviar(evento)}
      noValidate
    >
      {erroGeral && <Message severity="error" text={erroGeral} className="formulario__aviso" />}

      <SelectButton
        value={formulario.tipoDePessoa}
        options={OPCOES_DE_TIPO}
        onChange={(evento) => {
          // O SelectButton emite null ao clicar na opcao ja selecionada.
          if (evento.value) {
            alterarTipoDePessoa(evento.value as TipoDePessoa);
          }
        }}
        allowEmpty={false}
        aria-label="Tipo de pessoa"
      />

      <div className="formulario__grade">
        <Campo
          rotulo={formulario.tipoDePessoa === 'Fisica' ? 'Nome' : 'Razao social'}
          erro={erros.nome}
          obrigatorio
          largura="dobro"
        >
          {(id) => (
            <InputText
              id={id}
              value={formulario.nome}
              onChange={(evento) => alterar('nome', evento.target.value)}
              invalid={Boolean(erros.nome)}
              maxLength={200}
              autoFocus
            />
          )}
        </Campo>

        <Campo
          rotulo={formulario.tipoDePessoa === 'Fisica' ? 'CPF' : 'CNPJ'}
          erro={erros.documento}
          obrigatorio
        >
          {(id) => (
            <InputText
              id={id}
              value={formulario.documento}
              onChange={(evento) =>
                alterar('documento', mascararDocumento(evento.target.value, formulario.tipoDePessoa))
              }
              invalid={Boolean(erros.documento)}
              inputMode="numeric"
              placeholder={formulario.tipoDePessoa === 'Fisica' ? '000.000.000-00' : '00.000.000/0000-00'}
            />
          )}
        </Campo>

        {formulario.tipoDePessoa === 'Juridica' && (
          <Campo rotulo="Nome fantasia" erro={erros.nomeFantasia}>
            {(id) => (
              <InputText
                id={id}
                value={formulario.nomeFantasia ?? ''}
                onChange={(evento) => alterar('nomeFantasia', evento.target.value)}
                invalid={Boolean(erros.nomeFantasia)}
                maxLength={200}
              />
            )}
          </Campo>
        )}

        <Campo rotulo="E-mail" erro={erros.email}>
          {(id) => (
            <InputText
              id={id}
              type="email"
              value={formulario.email ?? ''}
              onChange={(evento) => alterar('email', evento.target.value)}
              invalid={Boolean(erros.email)}
              maxLength={256}
            />
          )}
        </Campo>

        <Campo rotulo="Telefone" erro={erros.telefone}>
          {(id) => (
            <InputText
              id={id}
              value={formulario.telefone ?? ''}
              onChange={(evento) => alterar('telefone', mascararTelefone(evento.target.value))}
              invalid={Boolean(erros.telefone)}
              inputMode="numeric"
              placeholder="(11) 98765-4321"
            />
          )}
        </Campo>
      </div>

      <fieldset className="formulario__secao">
        <legend>Endereco</legend>
        <p className="formulario__ajuda">
          Opcional. Se preencher algum campo, informe o endereco completo.
        </p>

        <div className="formulario__grade">
          <Campo rotulo="CEP" erro={erros['endereco.cep']}>
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.cep ?? ''}
                onChange={(evento) => alterarEndereco('cep', mascararCep(evento.target.value))}
                invalid={Boolean(erros['endereco.cep'])}
                inputMode="numeric"
                placeholder="00000-000"
              />
            )}
          </Campo>

          <Campo rotulo="Logradouro" erro={erros['endereco.logradouro']} largura="dobro">
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.logradouro ?? ''}
                onChange={(evento) => alterarEndereco('logradouro', evento.target.value)}
                invalid={Boolean(erros['endereco.logradouro'])}
                maxLength={200}
              />
            )}
          </Campo>

          <Campo rotulo="Numero" erro={erros['endereco.numero']}>
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.numero ?? ''}
                onChange={(evento) => alterarEndereco('numero', evento.target.value)}
                invalid={Boolean(erros['endereco.numero'])}
                maxLength={20}
              />
            )}
          </Campo>

          <Campo rotulo="Complemento">
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.complemento ?? ''}
                onChange={(evento) => alterarEndereco('complemento', evento.target.value)}
                maxLength={100}
              />
            )}
          </Campo>

          <Campo rotulo="Bairro" erro={erros['endereco.bairro']}>
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.bairro ?? ''}
                onChange={(evento) => alterarEndereco('bairro', evento.target.value)}
                invalid={Boolean(erros['endereco.bairro'])}
                maxLength={100}
              />
            )}
          </Campo>

          <Campo rotulo="Cidade" erro={erros['endereco.cidade']}>
            {(id) => (
              <InputText
                id={id}
                value={formulario.endereco?.cidade ?? ''}
                onChange={(evento) => alterarEndereco('cidade', evento.target.value)}
                invalid={Boolean(erros['endereco.cidade'])}
                maxLength={100}
              />
            )}
          </Campo>

          <Campo rotulo="UF" erro={erros['endereco.uf']}>
            {(id) => (
              <Dropdown
                inputId={id}
                value={formulario.endereco?.uf || null}
                options={[...UNIDADES_FEDERATIVAS]}
                onChange={(evento) => alterarEndereco('uf', String(evento.value ?? ''))}
                placeholder="Selecione"
                showClear
                invalid={Boolean(erros['endereco.uf'])}
              />
            )}
          </Campo>
        </div>
      </fieldset>

      <Campo rotulo="Observacoes">
        {(id) => (
          <InputTextarea
            id={id}
            value={formulario.observacoes ?? ''}
            onChange={(evento) => alterar('observacoes', evento.target.value)}
            rows={3}
            maxLength={2000}
            autoResize
          />
        )}
      </Campo>

      <ToggleButton
        checked={formulario.ativo}
        onChange={(evento) => alterar('ativo', evento.value)}
        onLabel="Cliente ativo"
        offLabel="Cliente inativo"
        onIcon="pi pi-check"
        offIcon="pi pi-ban"
        className="formulario__ativo"
      />

      {/* Os botoes vivem dentro do <form>, e nao no rodape do Dialog: assim o
          submit nativo funciona, inclusive com Enter nos campos de texto. */}
      <footer className="formulario__rodape">
        <Button
          type="button"
          label="Cancelar"
          icon="pi pi-times"
          onClick={aoFechar}
          severity="secondary"
          text
          disabled={salvando}
        />
        <Button
          type="submit"
          label={ehEdicao ? 'Salvar' : 'Cadastrar'}
          icon="pi pi-check"
          loading={salvando}
        />
      </footer>
    </form>
  );
}

/** Rotulo + controle + mensagem de erro, com id amarrando os tres. */
function Campo({
  rotulo,
  erro,
  obrigatorio = false,
  largura = 'simples',
  children,
}: {
  rotulo: string;
  erro?: string;
  obrigatorio?: boolean;
  largura?: 'simples' | 'dobro';
  children: (id: string) => React.ReactNode;
}) {
  const id = `campo-${rotulo.toLowerCase().replace(/[^a-z]+/g, '-')}`;

  return (
    <div className={`formulario__campo formulario__campo--${largura}`}>
      <label htmlFor={id}>
        {rotulo}
        {obrigatorio && (
          <span className="formulario__obrigatorio" aria-hidden="true">
            {' '}
            *
          </span>
        )}
      </label>

      {children(id)}

      {erro && (
        <small className="formulario__erro" role="alert">
          {erro}
        </small>
      )}
    </div>
  );
}

/** Endereco totalmente vazio vira null: o backend entende como nao informado. */
function enderecoParaEnvio(formulario: SalvarCliente): SalvarCliente['endereco'] {
  const endereco = formulario.endereco;

  if (!endereco) {
    return null;
  }

  const preenchido = Object.values(endereco).some((valor) => valor?.trim());

  if (!preenchido) {
    return null;
  }

  return {
    ...endereco,
    cep: somenteDigitos(endereco.cep),
  };
}

/** Traduz as chaves de erro do backend para os nomes dos campos do formulario. */
function mapearErrosDaApi(erro: ErroDaApi): ErrosDoFormulario {
  const mapeados: ErrosDoFormulario = {};

  for (const [chave, mensagens] of Object.entries(erro.errosDeValidacao)) {
    // O ASP.NET responde "Nome", "Endereco.Uf"; o formulario usa minusculas.
    const campo = chave
      .split('.')
      .map((parte) => parte.charAt(0).toLowerCase() + parte.slice(1))
      .join('.');

    mapeados[campo] = mensagens[0];
  }

  return mapeados;
}
