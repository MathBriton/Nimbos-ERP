using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Persistencia;

/// <summary>
/// Contexto unico do ERP. Os DbSets sao adicionados modulo a modulo
/// conforme as sprints avancam; o mapeamento de cada entidade fica em uma
/// classe <c>IEntityTypeConfiguration</c> dedicada em <c>Persistencia/Configuracoes</c>.
/// </summary>
public class NimbusDbContext : DbContext
{
    public NimbusDbContext(DbContextOptions<NimbusDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Carrega automaticamente todas as IEntityTypeConfiguration deste assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        base.ConfigureConventions(configurationBuilder);

        // Valores monetarios do ERP: precisao fixa para evitar erro de arredondamento.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 4);

        // Evita nvarchar(max) acidental em colunas de texto sem configuracao explicita.
        configurationBuilder.Properties<string>().HaveMaxLength(256);
    }
}
