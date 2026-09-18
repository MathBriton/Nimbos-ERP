using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Persistencia.Configuracoes;

/// <summary>Mapeamento relacional da entidade <see cref="Usuario"/>.</summary>
public sealed class UsuarioConfiguracao : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(usuario => usuario.Email)
            .IsRequired()
            .HasMaxLength(256);

        // O hash BCrypt tem 60 caracteres; a folga cobre uma futura troca de algoritmo.
        builder.Property(usuario => usuario.SenhaHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(usuario => usuario.Ativo)
            .IsRequired();

        // Indice unico apenas entre os nao excluidos: permite recadastrar um
        // e-mail cujo usuario anterior foi excluido logicamente.
        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("IX_Usuarios_Email");

        builder.HasQueryFilter(usuario => !usuario.Excluido);
    }
}
