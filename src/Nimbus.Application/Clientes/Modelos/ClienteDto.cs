using Nimbus.Domain.Entidades;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Application.Clientes.Modelos;

/// <summary>Cliente como a API o devolve.</summary>
public sealed record ClienteDto
{
    public required Guid Id { get; init; }

    public required string Nome { get; init; }

    public string? NomeFantasia { get; init; }

    public required TipoDePessoa TipoDePessoa { get; init; }

    /// <summary>Somente digitos, como esta persistido.</summary>
    public required string Documento { get; init; }

    /// <summary>Com mascara, pronto para exibir na tela.</summary>
    public required string DocumentoFormatado { get; init; }

    public string? Email { get; init; }

    public string? Telefone { get; init; }

    public string? TelefoneFormatado { get; init; }

    public EnderecoDto? Endereco { get; init; }

    public string? Observacoes { get; init; }

    public required bool Ativo { get; init; }

    public required DateTimeOffset CriadoEm { get; init; }

    public DateTimeOffset? AtualizadoEm { get; init; }

    public static ClienteDto De(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        return new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            NomeFantasia = cliente.NomeFantasia,
            TipoDePessoa = cliente.TipoDePessoa,
            Documento = cliente.Documento,
            DocumentoFormatado = cliente.DocumentoFormatado(),
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            TelefoneFormatado = cliente.TelefoneFormatado(),
            Endereco = cliente.Endereco is null
                ? null
                : new EnderecoDto
                {
                    Cep = cliente.Endereco.Cep,
                    Logradouro = cliente.Endereco.Logradouro,
                    Numero = cliente.Endereco.Numero,
                    Complemento = cliente.Endereco.Complemento,
                    Bairro = cliente.Endereco.Bairro,
                    Cidade = cliente.Endereco.Cidade,
                    Uf = cliente.Endereco.Uf,
                },
            Observacoes = cliente.Observacoes,
            Ativo = cliente.Ativo,
            CriadoEm = cliente.CriadoEm,
            AtualizadoEm = cliente.AtualizadoEm,
        };
    }
}
