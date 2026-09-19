import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { PrimeReactProvider } from 'primereact/api';

// O CSS do tema NAO e importado aqui: ele entra por um <link> trocavel em
// tempo de execucao (ver src/app/tema/). O script inline do index.html ja o
// aplicou antes desta linha rodar.
import 'primereact/resources/primereact.min.css';
import 'primeicons/primeicons.css';
import './styles/global.css';

import { registrarLocalePtBr } from './shared/config/localePtBr';
import { App } from './app/App';

registrarLocalePtBr();

const elementoRaiz = document.getElementById('root');

if (!elementoRaiz) {
  throw new Error('Elemento #root nao encontrado em index.html.');
}

createRoot(elementoRaiz).render(
  <StrictMode>
    <PrimeReactProvider value={{ ripple: true, locale: 'pt-BR' }}>
      <App />
    </PrimeReactProvider>
  </StrictMode>,
);
