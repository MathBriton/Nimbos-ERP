import { type FormEvent, useEffect, useId, useState } from 'react';
import { useLocation, useNavigate } from 'react-router';
import { Button } from 'primereact/button';
import { InputText } from 'primereact/inputtext';
import { Message } from 'primereact/message';
import { Password } from 'primereact/password';

import { SeletorDeTema } from '../../../app/tema/SeletorDeTema';
import { ErroDaApi } from '../../../shared/api/erros';
import { ambiente } from '../../../shared/config/ambiente';
import { useAutenticacao } from '../hooks/useAutenticacao';

interface EstadoDaRota {
  de?: string;
}

export function PaginaDeLogin() {
  const { entrar, estaAutenticado, avisoDeSessao, limparAvisoDeSessao } = useAutenticacao();
  const navegar = useNavigate();
  const localizacao = useLocation();

  const idDoEmail = useId();
  const idDaSenha = useId();

  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<ErroDaApi | null>(null);

  /** Rota que o usuario tentou abrir antes de ser mandado para o login. */
  const destino = (localizacao.state as EstadoDaRota | null)?.de ?? '/';

  // Se a sessao for reidratada com o login aberto, sai da tela sozinho.
  useEffect(() => {
    if (estaAutenticado) {
      void navegar(destino, { replace: true });
    }
  }, [estaAutenticado, destino, navegar]);

  async function aoEnviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();

    setErro(null);
    limparAvisoDeSessao();
    setEnviando(true);

    try {
      await entrar({ email: email.trim(), senha });
      await navegar(destino, { replace: true });
    } catch (falha) {
      setErro(
        falha instanceof ErroDaApi
          ? falha
          : new ErroDaApi('Nao foi possivel entrar. Tente novamente.', 0),
      );
      // Mantem o e-mail digitado e limpa apenas a senha.
      setSenha('');
    } finally {
      setEnviando(false);
    }
  }

  const erroDoEmail = erro?.erroDoCampo('email');
  const erroDaSenha = erro?.erroDoCampo('senha');
  // Erro geral: o que nao pertence a um campo especifico (401, servidor fora...).
  const erroGeral = erro && !erroDoEmail && !erroDaSenha ? erro.message : null;

  return (
    <div className="login">
      {/* A tela de login fica fora da area autenticada, entao precisa do seu
          proprio seletor - sem ele nao daria para trocar de tema sem entrar. */}
      <div className="login__tema">
        <SeletorDeTema />
      </div>

      <main className="login__cartao">
        <header className="login__cabecalho">
          <span className="login__marca" aria-hidden="true">
            <i className="pi pi-cloud" />
          </span>
          <h1 className="login__titulo">{ambiente.nomeDaAplicacao}</h1>
          <p className="login__subtitulo">Entre com suas credenciais para continuar.</p>
        </header>

        {avisoDeSessao && (
          <Message severity="warn" text={avisoDeSessao} className="login__mensagem" />
        )}

        {erroGeral && (
          <Message severity="error" text={erroGeral} className="login__mensagem" />
        )}

        <form className="login__formulario" onSubmit={(evento) => void aoEnviar(evento)} noValidate>
          <div className="login__campo">
            <label htmlFor={idDoEmail}>E-mail</label>
            <InputText
              id={idDoEmail}
              type="email"
              value={email}
              onChange={(evento) => setEmail(evento.target.value)}
              placeholder="voce@empresa.com"
              autoComplete="username"
              autoFocus
              disabled={enviando}
              invalid={Boolean(erroDoEmail)}
              aria-describedby={erroDoEmail ? `${idDoEmail}-erro` : undefined}
            />
            {erroDoEmail && (
              <small id={`${idDoEmail}-erro`} className="login__erro-do-campo" role="alert">
                {erroDoEmail}
              </small>
            )}
          </div>

          <div className="login__campo">
            <label htmlFor={idDaSenha}>Senha</label>
            <Password
              inputId={idDaSenha}
              value={senha}
              onChange={(evento) => setSenha(evento.target.value)}
              placeholder="Sua senha"
              autoComplete="current-password"
              // feedback={false}: o medidor de forca so faz sentido no cadastro
              // de senha, nao na tela de entrada.
              feedback={false}
              toggleMask
              disabled={enviando}
              invalid={Boolean(erroDaSenha)}
              aria-describedby={erroDaSenha ? `${idDaSenha}-erro` : undefined}
              inputClassName="login__input-de-senha"
            />
            {erroDaSenha && (
              <small id={`${idDaSenha}-erro`} className="login__erro-do-campo" role="alert">
                {erroDaSenha}
              </small>
            )}
          </div>

          <Button
            type="submit"
            label={enviando ? 'Entrando...' : 'Entrar'}
            icon="pi pi-sign-in"
            loading={enviando}
            disabled={enviando || email.trim() === '' || senha === ''}
            className="login__botao"
          />
        </form>

        {ambiente.ehDesenvolvimento && (
          <footer className="login__dica">
            <strong>Ambiente de desenvolvimento.</strong> Usuario semeado:{' '}
            <code>admin@erp.com</code> / <code>Admin123!</code>
          </footer>
        )}
      </main>
    </div>
  );
}
