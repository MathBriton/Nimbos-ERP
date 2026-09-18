import { useCallback, useEffect, useMemo, useRef, useState } from 'react';

import {
  definirTokenDeAcesso,
  definirTratadorDeSessaoPerdida,
} from '../../../shared/api/clienteHttp';
import { armazenamentoDeSessao, sessaoExpirou } from '../servicos/armazenamentoDeSessao';
import { autenticacaoService } from '../servicos/autenticacaoService';
import type { LoginRequisicao, UsuarioAutenticado } from '../tipos/autenticacao';
import { AutenticacaoContexto, type EstadoDaAutenticacao } from './AutenticacaoContexto';

const AVISO_DE_EXPIRACAO = 'Sua sessao expirou. Entre novamente para continuar.';
const AVISO_DE_SESSAO_INVALIDA = 'Sua sessao nao e mais valida. Entre novamente.';

interface Props {
  children: React.ReactNode;
}

/**
 * Dono da sessao autenticada.
 *
 * Responsabilidades:
 * 1. Reidratar a sessao guardada no localStorage ao abrir a aplicacao,
 *    confirmando na API que o token ainda vale (GET /api/auth/eu).
 * 2. Manter o token do axios sincronizado com o estado do React.
 * 3. Derrubar a sessao quando a API responder 401 em qualquer requisicao.
 */
export function ProvedorDeAutenticacao({ children }: Props) {
  const [usuario, setUsuario] = useState<UsuarioAutenticado | null>(null);
  const [carregandoSessao, setCarregandoSessao] = useState(true);
  const [avisoDeSessao, setAvisoDeSessao] = useState<string | null>(null);

  /** Evita avisar duas vezes quando varias requisicoes falham com 401 juntas. */
  const encerrando = useRef(false);

  const aplicarSessao = useCallback(
    (token: string, expiraEm: string, usuarioAutenticado: UsuarioAutenticado) => {
      definirTokenDeAcesso(token);
      armazenamentoDeSessao.gravar({ token, expiraEm, usuario: usuarioAutenticado });
      setUsuario(usuarioAutenticado);
      encerrando.current = false;
    },
    [],
  );

  const encerrarSessao = useCallback((aviso: string | null) => {
    definirTokenDeAcesso(null);
    armazenamentoDeSessao.limpar();
    setUsuario(null);
    setAvisoDeSessao(aviso);
  }, []);

  const entrar = useCallback(
    async (credenciais: LoginRequisicao) => {
      const resposta = await autenticacaoService.login(credenciais);

      setAvisoDeSessao(null);
      aplicarSessao(resposta.token, resposta.expiraEm, resposta.usuario);
    },
    [aplicarSessao],
  );

  const sair = useCallback(() => {
    encerrarSessao(null);
  }, [encerrarSessao]);

  const limparAvisoDeSessao = useCallback(() => setAvisoDeSessao(null), []);

  // Reidratacao: roda uma vez, na montagem.
  useEffect(() => {
    let cancelado = false;

    async function reidratar() {
      const sessao = armazenamentoDeSessao.ler();

      if (!sessao || sessaoExpirou(sessao.expiraEm)) {
        // Token ausente ou ja vencido pelo relogio local: nem chega a
        // consultar a API. Sem aviso, porque nao houve sessao ativa nesta aba.
        armazenamentoDeSessao.limpar();
        if (!cancelado) {
          setCarregandoSessao(false);
        }
        return;
      }

      // O token entra no axios antes da confirmacao para que a chamada de
      // verificacao ja va autenticada.
      definirTokenDeAcesso(sessao.token);

      try {
        const usuarioAtual = await autenticacaoService.obterUsuarioAtual();

        if (!cancelado) {
          aplicarSessao(sessao.token, sessao.expiraEm, usuarioAtual);
        }
      } catch {
        // O tratador de sessao perdida cuida do 401; aqui so garantimos que
        // nenhuma sessao invalida sobreviva (ex.: API fora do ar).
        if (!cancelado) {
          definirTokenDeAcesso(null);
          armazenamentoDeSessao.limpar();
          setUsuario(null);
        }
      } finally {
        if (!cancelado) {
          setCarregandoSessao(false);
        }
      }
    }

    void reidratar();

    return () => {
      cancelado = true;
    };
  }, [aplicarSessao]);

  // Liga o interceptor de 401 do axios ao encerramento de sessao.
  useEffect(() => {
    definirTratadorDeSessaoPerdida((porExpiracao) => {
      if (encerrando.current) {
        return;
      }

      encerrando.current = true;
      encerrarSessao(porExpiracao ? AVISO_DE_EXPIRACAO : AVISO_DE_SESSAO_INVALIDA);
    });

    return () => definirTratadorDeSessaoPerdida(null);
  }, [encerrarSessao]);

  const valor = useMemo<EstadoDaAutenticacao>(
    () => ({
      usuario,
      carregandoSessao,
      estaAutenticado: usuario !== null,
      avisoDeSessao,
      entrar,
      sair,
      limparAvisoDeSessao,
    }),
    [usuario, carregandoSessao, avisoDeSessao, entrar, sair, limparAvisoDeSessao],
  );

  return <AutenticacaoContexto value={valor}>{children}</AutenticacaoContexto>;
}
