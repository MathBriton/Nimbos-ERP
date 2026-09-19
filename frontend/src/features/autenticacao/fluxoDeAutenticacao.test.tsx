import { describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router';

import { App } from '../../app/App';
import { Rotas } from '../../app/rotas/Rotas';
import { ErroDaApi } from '../../shared/api/erros';
import { ProvedorDeAutenticacao } from './contexto/ProvedorDeAutenticacao';
import { autenticacaoService } from './servicos/autenticacaoService';
import type { LoginResposta } from './tipos/autenticacao';

const USUARIO = { id: 'u-1', nome: 'Administrador', email: 'admin@erp.com' };

function respostaDeLogin(minutosDeValidade = 60): LoginResposta {
  return {
    token: 'token-de-teste',
    expiraEm: new Date(Date.now() + minutosDeValidade * 60_000).toISOString(),
    usuario: USUARIO,
  };
}

/** Monta a aplicacao em uma rota especifica, sem depender do BrowserRouter. */
function renderizarEm(rotaInicial: string) {
  return render(
    <MemoryRouter initialEntries={[rotaInicial]}>
      <ProvedorDeAutenticacao>
        <Rotas />
      </ProvedorDeAutenticacao>
    </MemoryRouter>,
  );
}

describe('fluxo de autenticacao', () => {
  it('manda para o login quem tenta abrir rota protegida sem sessao', async () => {
    renderizarEm('/');

    expect(
      await screen.findByRole('heading', { name: /nimbus erp/i }),
    ).toBeInTheDocument();
    expect(screen.getByLabelText(/e-mail/i)).toBeInTheDocument();
  });

  it('autentica com as credenciais do seed e entra na area logada', async () => {
    const login = vi
      .spyOn(autenticacaoService, 'login')
      .mockResolvedValue(respostaDeLogin());

    const usuario = userEvent.setup();
    renderizarEm('/');

    await usuario.type(await screen.findByLabelText(/e-mail/i), 'admin@erp.com');
    await usuario.type(screen.getByLabelText(/senha/i), 'Admin123!');
    await usuario.click(screen.getByRole('button', { name: /entrar/i }));

    // Chegou na area logada: saudacao na pagina inicial e menu de navegacao.
    expect(await screen.findByText(/ola, administrador/i)).toBeInTheDocument();
    expect(screen.getByRole('navigation', { name: /menu principal/i })).toBeInTheDocument();

    expect(login).toHaveBeenCalledWith({ email: 'admin@erp.com', senha: 'Admin123!' });
  });

  it('exibe a mensagem da API quando as credenciais estao erradas', async () => {
    vi.spyOn(autenticacaoService, 'login').mockRejectedValue(
      new ErroDaApi('E-mail ou senha invalidos.', 401),
    );

    const usuario = userEvent.setup();
    renderizarEm('/login');

    await usuario.type(screen.getByLabelText(/e-mail/i), 'admin@erp.com');
    await usuario.type(screen.getByLabelText(/senha/i), 'errada');
    await usuario.click(screen.getByRole('button', { name: /entrar/i }));

    expect(await screen.findByText(/e-mail ou senha invalidos/i)).toBeInTheDocument();
    // Continua no login: a area logada nunca chegou a montar.
    expect(
      screen.queryByRole('navigation', { name: /menu principal/i }),
    ).not.toBeInTheDocument();
  });

  it('mostra os erros de validacao por campo devolvidos pela API', async () => {
    vi.spyOn(autenticacaoService, 'login').mockRejectedValue(
      new ErroDaApi('Erro de validacao', 400, {
        errosDeValidacao: { Email: ['Informe um e-mail valido.'] },
      }),
    );

    const usuario = userEvent.setup();
    renderizarEm('/login');

    await usuario.type(screen.getByLabelText(/e-mail/i), 'nao-e-email');
    await usuario.type(screen.getByLabelText(/senha/i), 'qualquer');
    await usuario.click(screen.getByRole('button', { name: /entrar/i }));

    expect(await screen.findByText(/informe um e-mail valido/i)).toBeInTheDocument();
  });

  it('volta para o login ao sair e nao deixa a sessao guardada', async () => {
    vi.spyOn(autenticacaoService, 'login').mockResolvedValue(respostaDeLogin());

    const usuario = userEvent.setup();
    renderizarEm('/');

    await usuario.type(await screen.findByLabelText(/e-mail/i), 'admin@erp.com');
    await usuario.type(screen.getByLabelText(/senha/i), 'Admin123!');
    await usuario.click(screen.getByRole('button', { name: /entrar/i }));

    await screen.findByText(/ola, administrador/i);
    expect(window.localStorage.getItem('nimbus.sessao')).not.toBeNull();

    // "Sair" vive no menu suspenso do avatar, no cabecalho.
    await usuario.click(screen.getByRole('button', { name: /conta de administrador/i }));
    await usuario.click(await screen.findByText('Sair'));

    expect(await screen.findByLabelText(/senha/i)).toBeInTheDocument();
    expect(window.localStorage.getItem('nimbus.sessao')).toBeNull();
  });

  it('reidrata a sessao guardada confirmando o token na API', async () => {
    window.localStorage.setItem(
      'nimbus.sessao',
      JSON.stringify(respostaDeLogin()),
    );
    const obterUsuarioAtual = vi
      .spyOn(autenticacaoService, 'obterUsuarioAtual')
      .mockResolvedValue(USUARIO);

    renderizarEm('/');

    expect(await screen.findByText(/ola, administrador/i)).toBeInTheDocument();
    expect(obterUsuarioAtual).toHaveBeenCalledOnce();
  });

  it('nao chama a API quando o token guardado ja venceu', async () => {
    // Token vencido: vai direto para o login, sem gastar uma requisicao.
    window.localStorage.setItem(
      'nimbus.sessao',
      JSON.stringify(respostaDeLogin(-10)),
    );
    const obterUsuarioAtual = vi.spyOn(autenticacaoService, 'obterUsuarioAtual');

    renderizarEm('/');

    expect(await screen.findByLabelText(/senha/i)).toBeInTheDocument();
    expect(obterUsuarioAtual).not.toHaveBeenCalled();
    expect(window.localStorage.getItem('nimbus.sessao')).toBeNull();
  });

  it('descarta a sessao guardada quando a API recusa o token', async () => {
    window.localStorage.setItem(
      'nimbus.sessao',
      JSON.stringify(respostaDeLogin()),
    );
    vi.spyOn(autenticacaoService, 'obterUsuarioAtual').mockRejectedValue(
      new ErroDaApi('Sessao expirada.', 401, { tokenExpirou: true }),
    );

    renderizarEm('/');

    expect(await screen.findByLabelText(/senha/i)).toBeInTheDocument();
    await waitFor(() =>
      expect(window.localStorage.getItem('nimbus.sessao')).toBeNull(),
    );
  });

  it('a raiz da aplicacao monta sem erro com o BrowserRouter', async () => {
    render(<App />);

    expect(await screen.findByLabelText(/e-mail/i)).toBeInTheDocument();
  });
});
