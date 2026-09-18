using System.ComponentModel.DataAnnotations;

namespace Nimbus.Application.Autenticacao.Modelos;

/// <summary>
/// Credenciais enviadas para <c>POST /api/auth/login</c>.
///
/// Usa DataAnnotations em vez da palavra-chave <c>required</c> de proposito: o
/// [ApiController] valida os atributos e responde 400 com os erros por campo,
/// que o frontend exibe direto no formulario. Com <c>required</c>, a falha
/// aconteceria antes, na desserializacao, sem essa granularidade.
/// </summary>
public sealed record LoginRequisicao
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [MaxLength(256, ErrorMessage = "O e-mail e longo demais.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [MaxLength(128, ErrorMessage = "A senha e longa demais.")]
    public string Senha { get; init; } = string.Empty;
}
