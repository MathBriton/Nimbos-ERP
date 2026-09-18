import { Navigate, Outlet, useLocation } from 'react-router';
import { ProgressSpinner } from 'primereact/progressspinner';

import { useAutenticacao } from '../hooks/useAutenticacao';

/**
 * Guarda de rota: libera as rotas filhas apenas para sessao autenticada.
 *
 * Enquanto a sessao guardada esta sendo revalidada, mostra um indicador de
 * carregamento. Sem isso, um reload dentro da area logada redirecionaria para
 * o login por uma fracao de segundo antes de reidratar a sessao.
 */
export function RotaProtegida() {
  const { estaAutenticado, carregandoSessao } = useAutenticacao();
  const localizacao = useLocation();

  if (carregandoSessao) {
    return (
      <div className="carregando-sessao" role="status" aria-live="polite">
        <ProgressSpinner style={{ width: '48px', height: '48px' }} strokeWidth="4" />
        <span>Verificando sua sessao...</span>
      </div>
    );
  }

  if (!estaAutenticado) {
    // Guarda o destino original para voltar a ele depois do login.
    return <Navigate to="/login" replace state={{ de: localizacao.pathname }} />;
  }

  return <Outlet />;
}
