/**
 * Definicao do menu de navegacao do ERP.
 *
 * Esta e a fonte unica de verdade: a sidebar renderiza a partir daqui e as
 * rotas sao geradas a partir da mesma estrutura. Acrescentar um modulo em uma
 * sprint futura significa mexer em um lugar so, sem risco de a sidebar e o
 * roteador ficarem fora de sincronia.
 *
 * A partir da Sprint 15 (RBAC) cada item ganha a permissao exigida, e a sidebar
 * passa a esconder o que o usuario logado nao pode acessar.
 */

export interface ItemDeMenu {
  /** Caminho da rota. Precisa ser unico em todo o menu. */
  caminho: string;
  rotulo: string;
  /** Classe do PrimeIcons, ex.: "pi-users". */
  icone: string;
  /** Sprint que entrega a tela de verdade; exibida na pagina em construcao. */
  sprint: number;
  /** Descricao do que o modulo vai fazer, mostrada na pagina em construcao. */
  descricao: string;
}

export interface GrupoDeMenu {
  /** Titulo da secao na sidebar. Nulo para itens soltos, como o Dashboard. */
  titulo: string | null;
  itens: ItemDeMenu[];
}

export const CAMINHO_DA_PAGINA_INICIAL = '/';

export const menu: GrupoDeMenu[] = [
  {
    titulo: null,
    itens: [
      {
        caminho: CAMINHO_DA_PAGINA_INICIAL,
        rotulo: 'Dashboard',
        icone: 'pi-home',
        sprint: 12,
        descricao:
          'Visao consolidada do negocio: vendas do mes, produtos mais vendidos e contas a vencer.',
      },
    ],
  },
  {
    titulo: 'Cadastros',
    itens: [
      {
        caminho: '/clientes',
        rotulo: 'Clientes',
        icone: 'pi-users',
        sprint: 4,
        descricao:
          'Cadastro de clientes com dados de contato e endereco, listagem paginada e busca.',
      },
      {
        caminho: '/fornecedores',
        rotulo: 'Fornecedores',
        icone: 'pi-truck',
        sprint: 5,
        descricao: 'Cadastro de fornecedores, base dos pedidos de compra.',
      },
      {
        caminho: '/produtos',
        rotulo: 'Produtos',
        icone: 'pi-box',
        sprint: 7,
        descricao:
          'Cadastro de produtos com SKU, preco, unidade e categoria. Base do estoque e dos pedidos.',
      },
      {
        caminho: '/usuarios',
        rotulo: 'Usuarios',
        icone: 'pi-id-card',
        sprint: 6,
        descricao:
          'Gestao dos usuarios internos do sistema, incluindo ativacao e inativacao de contas.',
      },
    ],
  },
  {
    titulo: 'Operacao',
    itens: [
      {
        caminho: '/estoque',
        rotulo: 'Estoque',
        icone: 'pi-warehouse',
        sprint: 8,
        descricao:
          'Movimentacoes de entrada, saida e ajuste, com saldo por produto e bloqueio de saldo negativo.',
      },
      {
        caminho: '/pedidos-de-compra',
        rotulo: 'Pedidos de compra',
        icone: 'pi-shopping-cart',
        sprint: 9,
        descricao:
          'Pedidos ao fornecedor. Confirmar o recebimento gera entrada no estoque.',
      },
      {
        caminho: '/pedidos-de-venda',
        rotulo: 'Pedidos de venda',
        icone: 'pi-shopping-bag',
        sprint: 10,
        descricao:
          'Pedidos do cliente. Confirmar a venda gera saida no estoque, respeitando o saldo.',
      },
    ],
  },
  {
    titulo: 'Financeiro',
    itens: [
      {
        caminho: '/financeiro',
        rotulo: 'Contas',
        icone: 'pi-wallet',
        sprint: 11,
        descricao:
          'Contas a pagar e a receber, geradas automaticamente a partir dos pedidos confirmados.',
      },
    ],
  },
  {
    titulo: 'Analise',
    itens: [
      {
        caminho: '/relatorios',
        rotulo: 'Relatorios',
        icone: 'pi-chart-bar',
        sprint: 16,
        descricao:
          'Relatorio de vendas por periodo, com exportacao em PDF e CSV e drill-down por regiao.',
      },
      {
        caminho: '/auditoria',
        rotulo: 'Auditoria',
        icone: 'pi-history',
        sprint: 13,
        descricao:
          'Consulta de todas as alteracoes feitas no sistema, com filtro por usuario, entidade e periodo.',
      },
    ],
  },
  {
    titulo: 'Sistema',
    itens: [
      {
        caminho: '/configuracoes',
        rotulo: 'Configuracoes',
        icone: 'pi-cog',
        sprint: 15,
        descricao:
          'Papeis e permissoes por modulo, alem das regras de alerta configuraveis.',
      },
      {
        caminho: '/status',
        rotulo: 'Status',
        icone: 'pi-server',
        sprint: 0,
        descricao: 'Estado da API e das dependencias da plataforma.',
      },
    ],
  },
];

/** Todos os itens do menu em uma lista plana, na ordem em que aparecem. */
export const itensDoMenu: ItemDeMenu[] = menu.flatMap((grupo) => grupo.itens);

/** Encontra o item de menu correspondente a um caminho de rota. */
export function itemPorCaminho(caminho: string): ItemDeMenu | undefined {
  return itensDoMenu.find((item) => item.caminho === caminho);
}
