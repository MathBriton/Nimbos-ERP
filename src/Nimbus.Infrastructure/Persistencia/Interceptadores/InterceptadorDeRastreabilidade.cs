using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Domain.Common;

namespace Nimbus.Infrastructure.Persistencia.Interceptadores;

/// <summary>
/// Preenche <c>CriadoEm/CriadoPor</c> e <c>AtualizadoEm/AtualizadoPor</c> em toda
/// escrita, e converte exclusao fisica em exclusao logica.
///
/// A Sprint 13 estende esta mesma ideia para gerar os registros da tabela de
/// auditoria com os valores antes/depois de cada alteracao.
/// </summary>
public sealed class InterceptadorDeRastreabilidade : SaveChangesInterceptor
{
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly IUsuarioAtual _usuarioAtual;

    public InterceptadorDeRastreabilidade(
        IProvedorDeDataHora provedorDeDataHora,
        IUsuarioAtual usuarioAtual)
    {
        _provedorDeDataHora = provedorDeDataHora;
        _usuarioAtual = usuarioAtual;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Marcar(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);

        Marcar(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Marcar(DbContext? contexto)
    {
        if (contexto is null)
        {
            return;
        }

        var agora = _provedorDeDataHora.Agora;
        var autor = _usuarioAtual.Email ?? _usuarioAtual.Id?.ToString() ?? "sistema";

        foreach (var entrada in contexto.ChangeTracker.Entries<EntidadeBase>())
        {
            switch (entrada.State)
            {
                case EntityState.Added:
                    entrada.Entity.CriadoEm = agora;
                    entrada.Entity.CriadoPor = autor;
                    break;

                case EntityState.Modified:
                    entrada.Entity.AtualizadoEm = agora;
                    entrada.Entity.AtualizadoPor = autor;

                    // CriadoEm/CriadoPor sao imutaveis depois da insercao.
                    entrada.Property(entidade => entidade.CriadoEm).IsModified = false;
                    entrada.Property(entidade => entidade.CriadoPor).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Exclusao logica: o registro permanece na tabela e o filtro
                    // global de consulta passa a esconde-lo.
                    entrada.State = EntityState.Modified;
                    entrada.Entity.Excluido = true;
                    entrada.Entity.AtualizadoEm = agora;
                    entrada.Entity.AtualizadoPor = autor;
                    break;

                default:
                    break;
            }
        }
    }
}
