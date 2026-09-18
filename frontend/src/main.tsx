import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { PrimeReactProvider } from 'primereact/api';

// Ordem importa: tema -> core do PrimeReact -> icones -> estilos do projeto.
import 'primereact/resources/themes/lara-light-indigo/theme.css';
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
