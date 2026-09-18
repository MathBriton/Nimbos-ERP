import '@testing-library/jest-dom/vitest';

import { afterEach, beforeEach, vi } from 'vitest';
import { cleanup } from '@testing-library/react';

// O modulo de ambiente exige VITE_API_URL e falha cedo se faltar.
vi.stubEnv('VITE_API_URL', 'http://api.teste');
vi.stubEnv('VITE_APP_NOME', 'Nimbus ERP');

beforeEach(() => {
  // Cada teste comeca com o armazenamento limpo, para nao herdar sessao.
  window.localStorage.clear();
});

afterEach(() => {
  cleanup();
});
