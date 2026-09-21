using System.ComponentModel.DataAnnotations;
using Nimbus.Domain.ObjetosDeValor;

namespace Nimbus.Application.Clientes.Modelos;

/// <summary>
/// Corpo de criacao e de edicao de cliente.
///
/// Um unico tipo serve aos dois verbos porque os campos sao os mesmos: o id vem
/// da rota no PUT. Dois records identicos so divergiriam por descuido.
///
/// As DataAnnotations aqui cobrem forma (obrigatorio, tamanho) e rendem um 400
/// com erro por campo, que o formulario exibe direto no input. As regras de
/// negocio de verdade - digito verificador do documento, UF valida, coerencia
/// entre tipo de pessoa e nome fantasia - ficam no dominio, que e onde nao dao
/// para ser burladas.
/// </summary>
public sealed record SalvarClienteRequisicao
{
    [Required(ErrorMessage = "Informe o nome.")]
    [MaxLength(200, ErrorMessage = "O nome pode ter no maximo 200 caracteres.")]
    public string Nome { get; init; } = string.Empty;

    [MaxLength(200, ErrorMessage = "O nome fantasia pode ter no maximo 200 caracteres.")]
    public string? NomeFantasia { get; init; }

    [Required(ErrorMessage = "Informe o tipo de pessoa.")]
    [EnumDataType(typeof(TipoDePessoa), ErrorMessage = "Tipo de pessoa invalido.")]
    public TipoDePessoa TipoDePessoa { get; init; } = TipoDePessoa.Fisica;

    [Required(ErrorMessage = "Informe o documento.")]
    [MaxLength(18, ErrorMessage = "Documento invalido.")]
    public string Documento { get; init; } = string.Empty;

    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [MaxLength(256, ErrorMessage = "O e-mail pode ter no maximo 256 caracteres.")]
    public string? Email { get; init; }

    [MaxLength(20, ErrorMessage = "Telefone invalido.")]
    public string? Telefone { get; init; }

    public EnderecoDto? Endereco { get; init; }

    [MaxLength(2000, ErrorMessage = "As observacoes podem ter no maximo 2000 caracteres.")]
    public string? Observacoes { get; init; }

    /// <summary>Na criacao, o cliente nasce ativo se nao vier nada.</summary>
    public bool Ativo { get; init; } = true;
}
