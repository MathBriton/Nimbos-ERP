using System.Reflection;
using Nimbus.Application.Common.Modelos;
using Nimbus.Domain.Common;
using Nimbus.Infrastructure.Persistencia;

namespace Nimbus.Tests.Arquitetura;

/// <summary>
/// Guarda-corpos da Clean Architecture: se alguem referenciar EF Core ou
/// ASP.NET Core dentro do Dominio, o build de testes quebra.
/// </summary>
public class RegrasDeDependenciaTests
{
    private static readonly string[] DependenciasProibidasNoDominio =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Microsoft.Extensions",
    ];

    [Fact]
    public void Dominio_nao_depende_de_frameworks_de_infraestrutura()
    {
        var referencias = typeof(EntidadeBase).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        var violacoes = referencias
            .Where(nome => DependenciasProibidasNoDominio.Any(
                proibida => nome.StartsWith(proibida, StringComparison.Ordinal)))
            .ToArray();

        Assert.Empty(violacoes);
    }

    [Fact]
    public void Aplicacao_nao_depende_de_EntityFrameworkCore()
    {
        var referencias = typeof(ConsultaPaginada).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", referencias, StringComparer.Ordinal);
    }

    [Fact]
    public void Infraestrutura_referencia_a_camada_de_aplicacao()
    {
        var referencias = typeof(NimbusDbContext).Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name ?? string.Empty)
            .ToArray();

        Assert.Contains("Nimbus.Application", referencias, StringComparer.Ordinal);
    }

    [Fact]
    public void Toda_entidade_do_dominio_herda_de_EntidadeBase()
    {
        var entidades = typeof(EntidadeBase).Assembly
            .GetTypes()
            .Where(tipo => tipo.Namespace == "Nimbus.Domain.Entidades")
            .Where(tipo => tipo is { IsClass: true, IsAbstract: false })
            .ToArray();

        var foraDoPadrao = entidades
            .Where(tipo => !typeof(EntidadeBase).IsAssignableFrom(tipo))
            .Select(tipo => tipo.Name)
            .ToArray();

        Assert.Empty(foraDoPadrao);
    }
}
