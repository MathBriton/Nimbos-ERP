import { Navigate, Route, Routes } from 'react-router';

import { PaginaDeLogin } from '../../features/autenticacao/componentes/PaginaDeLogin';
import { RotaProtegida } from '../../features/autenticacao/componentes/RotaProtegida';
import { PaginaInicial } from '../../features/dashboard/componentes/PaginaInicial';
import { LayoutAutenticado } from '../layout/LayoutAutenticado';

/**
 * Mapa de rotas da aplicacao.
 * Tudo que fica sob <RotaProtegida> exige sessao autenticada; a Sprint 2
 * acrescenta aqui uma rota por modulo do ERP.
 */
export function Rotas() {
  return (
    <Routes>
      <Route path="/login" element={<PaginaDeLogin />} />

      <Route element={<RotaProtegida />}>
        <Route element={<LayoutAutenticado />}>
          <Route path="/" element={<PaginaInicial />} />
        </Route>
      </Route>

      {/* Qualquer caminho desconhecido volta para a raiz. */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
