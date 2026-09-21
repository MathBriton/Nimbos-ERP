import type { ReactNode } from 'react';
import { Navigate, Route, Routes } from 'react-router';

import { PaginaDeLogin } from '../../features/autenticacao/componentes/PaginaDeLogin';
import { RotaProtegida } from '../../features/autenticacao/componentes/RotaProtegida';
import { PaginaDeClientes } from '../../features/clientes/componentes/PaginaDeClientes';
import { PaginaInicial } from '../../features/dashboard/componentes/PaginaInicial';
import { PainelDeStatus } from '../../features/plataforma/componentes/PainelDeStatus';
import { PaginaEmConstrucao } from '../../shared/componentes/PaginaEmConstrucao';
import { LayoutAutenticado } from '../layout/LayoutAutenticado';
import { itensDoMenu } from '../layout/menu';

/**
 * Telas ja implementadas. Todo caminho do menu que nao estiver aqui recebe
 * automaticamente a pagina em construcao, montada com os dados do proprio
 * menu - por isso adicionar um modulo novo nao exige mexer neste arquivo.
 */
const telasImplementadas: Record<string, ReactNode> = {
  '/': <PaginaInicial />,
  '/clientes': <PaginaDeClientes />,
  '/status': <PainelDeStatus />,
};

/**
 * Mapa de rotas da aplicacao.
 *
 * /login e publica. Todo o resto fica sob <RotaProtegida>, que exige sessao
 * autenticada, e dentro de <LayoutAutenticado>, que fornece sidebar e cabecalho.
 */
export function Rotas() {
  return (
    <Routes>
      <Route path="/login" element={<PaginaDeLogin />} />

      <Route element={<RotaProtegida />}>
        <Route element={<LayoutAutenticado />}>
          {itensDoMenu.map((item) => (
            <Route
              key={item.caminho}
              path={item.caminho}
              element={
                telasImplementadas[item.caminho] ?? (
                  <PaginaEmConstrucao
                    titulo={item.rotulo}
                    icone={item.icone}
                    descricao={item.descricao}
                    sprint={item.sprint}
                  />
                )
              }
            />
          ))}
        </Route>
      </Route>

      {/* Qualquer caminho desconhecido volta para a raiz. */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
