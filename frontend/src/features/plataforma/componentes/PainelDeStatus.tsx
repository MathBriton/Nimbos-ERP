import { Button } from 'primereact/button';
import { Card } from 'primereact/card';
import { Message } from 'primereact/message';
import { ProgressSpinner } from 'primereact/progressspinner';
import { Tag } from 'primereact/tag';

import { useSaudeDaPlataforma } from '../hooks/useSaudeDaPlataforma';
import type { StatusDeSaude } from '../tipos/saude';

const severidadePorStatus: Record<StatusDeSaude, 'success' | 'warning' | 'danger'> = {
  Healthy: 'success',
  Degraded: 'warning',
  Unhealthy: 'danger',
};

const rotuloPorStatus: Record<StatusDeSaude, string> = {
  Healthy: 'Operacional',
  Degraded: 'Degradado',
  Unhealthy: 'Indisponivel',
};

/**
 * Tela de status da plataforma: prova, de ponta a ponta, que o frontend fala
 * com a API e que a API fala com o SQL Server.
 *
 * Nasceu como criterio de aceite da Sprint 0 e evolui para o dashboard de
 * monitoramento da Sprint 22.
 */
export function PainelDeStatus() {
  const { identificacao, saude, carregando, erro, reconsultar } = useSaudeDaPlataforma();

  return (
    <div className="painel-status">
      <header className="painel-status__cabecalho">
        <div>
          {/* h2 e nao h1: o h1 da pagina e o titulo do cabecalho do layout. */}
          <h2>Status da plataforma</h2>
          <p className="painel-status__subtitulo">
            Estado da API e das dependencias, revalidado a cada 15 segundos.
          </p>
        </div>

        <Button
          label="Atualizar"
          icon="pi pi-refresh"
          onClick={() => void reconsultar()}
          loading={carregando}
          outlined
        />
      </header>

      {erro && (
        <Message severity="error" text={erro} className="painel-status__mensagem" />
      )}

      {carregando && !saude && (
        <div className="painel-status__carregando">
          <ProgressSpinner style={{ width: '48px', height: '48px' }} strokeWidth="4" />
          <span>Consultando a API...</span>
        </div>
      )}

      <div className="painel-status__grade">
        <Card title="Frontend">
          <div className="painel-status__linha">
            <span>React + TypeScript (Vite)</span>
            <Tag severity="success" value="Operacional" />
          </div>
        </Card>

        <Card title="API">
          <div className="painel-status__linha">
            <span>{identificacao ? `${identificacao.aplicacao} v${identificacao.versao}` : 'ASP.NET Core'}</span>
            <Tag
              severity={identificacao ? 'success' : 'danger'}
              value={identificacao ? 'Operacional' : 'Indisponivel'}
            />
          </div>
          {identificacao && (
            <small className="painel-status__detalhe">
              Ambiente: {identificacao.ambiente}
            </small>
          )}
        </Card>

        {saude?.verificacoes.map((verificacao) => (
          <Card key={verificacao.nome} title={`Dependencia: ${verificacao.nome}`}>
            <div className="painel-status__linha">
              <span>{verificacao.descricao ?? 'SQL Server'}</span>
              <Tag
                severity={severidadePorStatus[verificacao.status]}
                value={rotuloPorStatus[verificacao.status]}
              />
            </div>
            <small className="painel-status__detalhe">
              Latencia: {verificacao.duracaoMs.toFixed(0)} ms
            </small>
          </Card>
        ))}
      </div>

      {saude && (
        <footer className="painel-status__rodape">
          Estado agregado:{' '}
          <Tag severity={severidadePorStatus[saude.status]} value={rotuloPorStatus[saude.status]} />
          <span className="painel-status__detalhe">
            verificado em {saude.duracaoMs.toFixed(0)} ms
          </span>
        </footer>
      )}
    </div>
  );
}
