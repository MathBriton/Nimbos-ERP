import { useCallback, useEffect, useRef, useState } from 'react';

import { ErroDaApi } from '../../../shared/api/erros';
import type { ResultadoPaginado } from '../../../shared/tipos/api';
import { clientesService } from '../servicos/clientesService';
import type { Cliente, ConsultaDeClientes } from '../tipos/cliente';

const TAMANHO_PADRAO_DA_PAGINA = 10;

const RESULTADO_VAZIO: ResultadoPaginado<Cliente> = {
  itens: [],
  paginaAtual: 1,
  tamanhoDaPagina: TAMANHO_PADRAO_DA_PAGINA,
  totalDeItens: 0,
  totalDePaginas: 0,
  temPaginaAnterior: false,
  temProximaPagina: false,
};

/**
 * Estado da listagem de clientes: paginacao no servidor, busca e filtros.
 *
 * A paginacao e "lazy" (feita no backend) e nao no DataTable: a tabela recebe
 * apenas a pagina atual. Carregar tudo e paginar no navegador funcionaria com
 * dezenas de registros e quebraria com milhares.
 */
export function useListaDeClientes() {
  const [consulta, definirConsulta] = useState<ConsultaDeClientes>({
    pagina: 1,
    tamanhoDaPagina: TAMANHO_PADRAO_DA_PAGINA,
    ordenacao: 'Nome',
    descendente: false,
  });

  const [resultado, definirResultado] = useState<ResultadoPaginado<Cliente>>(RESULTADO_VAZIO);
  const [carregando, definirCarregando] = useState(true);
  const [erro, definirErro] = useState<string | null>(null);

  /**
   * Identifica a consulta mais recente. Respostas de buscas antigas, que
   * chegarem fora de ordem, sao descartadas - sem isso, digitar rapido no campo
   * de busca poderia deixar na tela o resultado de um termo ja abandonado.
   */
  const consultaAtual = useRef(0);

  const buscar = useCallback(async (parametros: ConsultaDeClientes) => {
    const identificador = ++consultaAtual.current;

    definirCarregando(true);

    try {
      const pagina = await clientesService.listar(parametros);

      if (identificador === consultaAtual.current) {
        definirResultado(pagina);
        definirErro(null);
      }
    } catch (falha) {
      if (identificador === consultaAtual.current) {
        definirResultado(RESULTADO_VAZIO);
        definirErro(
          falha instanceof ErroDaApi ? falha.message : 'Falha ao carregar os clientes.',
        );
      }
    } finally {
      if (identificador === consultaAtual.current) {
        definirCarregando(false);
      }
    }
  }, []);

  useEffect(() => {
    // O aviso react/set-state-in-effect nao se aplica: este efeito existe
    // exatamente para sincronizar com um sistema externo (a API), que e a
    // excecao documentada pela propria regra. Marcar "carregando" no mesmo
    // quadro em que a consulta muda e o comportamento desejado - adiar isso
    // deixaria a tabela exibindo o resultado antigo como se fosse o novo.
    // oxlint-disable-next-line react/set-state-in-effect
    void buscar(consulta);
  }, [buscar, consulta]);

  /** Altera filtros e volta para a primeira pagina. */
  const filtrar = useCallback((mudanca: Partial<ConsultaDeClientes>) => {
    definirConsulta((anterior) => {
      // Devolver o mesmo objeto quando nada mudou evita uma busca redundante.
      // Sem isso, a sincronizacao do campo de busca dispararia uma segunda
      // requisicao logo na montagem da tela.
      const jaEstaAssim =
        anterior.pagina === 1 &&
        Object.entries(mudanca).every(
          ([chave, valor]) => anterior[chave as keyof ConsultaDeClientes] === valor,
        );

      // Voltar para a pagina 1 e essencial: filtrar estando na pagina 5 poderia
      // cair em um resultado vazio mesmo havendo registros.
      return jaEstaAssim ? anterior : { ...anterior, ...mudanca, pagina: 1 };
    });
  }, []);

  const irParaPagina = useCallback((pagina: number, tamanhoDaPagina: number) => {
    definirConsulta((anterior) => ({ ...anterior, pagina, tamanhoDaPagina }));
  }, []);

  const ordenarPor = useCallback(
    (ordenacao: ConsultaDeClientes['ordenacao'], descendente: boolean) => {
      definirConsulta((anterior) => ({ ...anterior, ordenacao, descendente, pagina: 1 }));
    },
    [],
  );

  /** Recarrega a pagina atual, apos criar, editar ou excluir. */
  const recarregar = useCallback(() => buscar(consulta), [buscar, consulta]);

  return {
    consulta,
    resultado,
    carregando,
    erro,
    filtrar,
    irParaPagina,
    ordenarPor,
    recarregar,
  };
}
