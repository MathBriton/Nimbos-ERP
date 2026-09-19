import { BrowserRouter } from 'react-router';

import { ProvedorDeAutenticacao } from '../features/autenticacao/contexto/ProvedorDeAutenticacao';
import { ProvedorDeTema } from './tema/ProvedorDeTema';
import { Rotas } from './rotas/Rotas';

/**
 * Raiz da aplicacao: tema por fora, roteador no meio, sessao por dentro.
 *
 * O tema fica acima de tudo porque vale tambem para a tela de login, que esta
 * fora da area autenticada. O provedor de autenticacao precisa ficar dentro do
 * BrowserRouter porque consome hooks de navegacao via seus componentes filhos.
 */
export function App() {
  return (
    <ProvedorDeTema>
      <BrowserRouter>
        <ProvedorDeAutenticacao>
          <Rotas />
        </ProvedorDeAutenticacao>
      </BrowserRouter>
    </ProvedorDeTema>
  );
}
