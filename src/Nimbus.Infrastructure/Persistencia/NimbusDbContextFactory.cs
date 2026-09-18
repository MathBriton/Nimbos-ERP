using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nimbus.Infrastructure.Persistencia;

/// <summary>
/// Fabrica usada apenas em tempo de design pelo <c>dotnet ef</c>.
/// Permite gerar migrations sem precisar subir a API nem o SQL Server:
/// <c>dotnet ef migrations add Nome --project src/Nimbus.Infrastructure --startup-project src/Nimbus.Api</c>.
/// </summary>
public sealed class NimbusDbContextFactory : IDesignTimeDbContextFactory<NimbusDbContext>
{
    public NimbusDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("NIMBUS_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=NimbusErp;User Id=sa;Password=Nimbus@Local123;TrustServerCertificate=True;Encrypt=False";

        var opcoes = new DbContextOptionsBuilder<NimbusDbContext>()
            .UseSqlServer(conexao, sql => sql.MigrationsAssembly(typeof(NimbusDbContext).Assembly.FullName))
            .Options;

        return new NimbusDbContext(opcoes);
    }
}
