import { BrowserRouter } from 'react-router';

import { ProvedorDeAutenticacao } from '../features/autenticacao/contexto/ProvedorDeAutenticacao';
import { Rotas } from './rotas/Rotas';

/**
 * Raiz da aplicacao: roteador por fora, sessao por dentro.
 * O provedor de autenticacao precisa ficar dentro do BrowserRouter porque
 * consome hooks de navegacao indiretamente, via seus componentes filhos.
 */
export function App() {
  return (
    <BrowserRouter>
      <ProvedorDeAutenticacao>
        <Rotas />
      </ProvedorDeAutenticacao>
    </BrowserRouter>
  );
}
