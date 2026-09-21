using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Persistencia.Configuracoes;

/// <summary>Mapeamento relacional da entidade <see cref="Cliente"/>.</summary>
public sealed class ClienteConfiguracao : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cliente => cliente.NomeFantasia)
            .HasMaxLength(200);

        builder.Property(cliente => cliente.Email)
            .HasMaxLength(256);

        builder.Property(cliente => cliente.Telefone)
            .HasMaxLength(11);

        builder.Property(cliente => cliente.Observacoes)
            .HasMaxLength(2000);

        builder.Property(cliente => cliente.Ativo)
            .IsRequired();

        // Somente digitos: a validacao e a normalizacao acontecem no dominio,
        // antes de chegar aqui.
        builder.Property(cliente => cliente.Documento)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(cliente => cliente.TipoDePessoa)
            .HasConversion<int>()
            .IsRequired();

        // Endereco e opcional, entao precisa ser um tipo owned (e nao complex):
        // complex properties do EF Core ainda nao aceitam valor nulo.
        builder.OwnsOne(cliente => cliente.Endereco, endereco =>
        {
            endereco.Property(valor => valor.Cep)
                .HasColumnName("EnderecoCep")
                .HasMaxLength(8);

            endereco.Property(valor => valor.Logradouro)
                .HasColumnName("EnderecoLogradouro")
                .HasMaxLength(200);

            endereco.Property(valor => valor.Numero)
                .HasColumnName("EnderecoNumero")
                .HasMaxLength(20);

            endereco.Property(valor => valor.Complemento)
                .HasColumnName("EnderecoComplemento")
                .HasMaxLength(100);

            endereco.Property(valor => valor.Bairro)
                .HasColumnName("EnderecoBairro")
                .HasMaxLength(100);

            endereco.Property(valor => valor.Cidade)
                .HasColumnName("EnderecoCidade")
                .HasMaxLength(100);

            endereco.Property(valor => valor.Uf)
                .HasColumnName("EnderecoUf")
                .HasMaxLength(2);
        });

        // Unicidade do documento apenas entre os nao excluidos, para permitir
        // recadastrar o CPF/CNPJ de um cliente que foi excluido logicamente.
        builder.HasIndex(cliente => cliente.Documento)
            .IsUnique()
            .HasFilter("[Excluido] = 0")
            .HasDatabaseName("IX_Clientes_Documento");

        // Cobre a ordenacao padrao da listagem.
        builder.HasIndex(cliente => cliente.Nome)
            .HasDatabaseName("IX_Clientes_Nome");

        builder.HasQueryFilter(cliente => !cliente.Excluido);
    }
}
