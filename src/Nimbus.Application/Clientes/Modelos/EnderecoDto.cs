using System.ComponentModel.DataAnnotations;

namespace Nimbus.Application.Clientes.Modelos;

/// <summary>
/// Endereco trafegado pela API.
///
/// Todos os campos sao opcionais aqui: quem decide se o conjunto esta completo
/// e o objeto de valor do dominio, que exige tudo ou nada. Repetir essa regra
/// em DataAnnotations criaria duas fontes de verdade que divergem com o tempo.
/// </summary>
public sealed record EnderecoDto
{
    [MaxLength(9)]
    public string? Cep { get; init; }

    [MaxLength(200)]
    public string? Logradouro { get; init; }

    [MaxLength(20)]
    public string? Numero { get; init; }

    [MaxLength(100)]
    public string? Complemento { get; init; }

    [MaxLength(100)]
    public string? Bairro { get; init; }

    [MaxLength(100)]
    public string? Cidade { get; init; }

    [MaxLength(2)]
    public string? Uf { get; init; }
}
