using System.ComponentModel.DataAnnotations;

namespace Nimbus.Infrastructure.Seguranca;

/// <summary>
/// Configuracao da emissao e validacao de tokens (secao <c>Jwt</c> do appsettings).
/// Validada na subida da aplicacao: um segredo curto ou ausente derruba o processo
/// em vez de gerar tokens fracos silenciosamente.
/// </summary>
public sealed class OpcoesDeJwt
{
    public const string Secao = "Jwt";

    /// <summary>
    /// Tamanho minimo do segredo. HMAC-SHA256 usa chave de 256 bits, entao
    /// menos de 32 caracteres enfraquece a assinatura.
    /// </summary>
    public const int TamanhoMinimoDoSegredo = 32;

    [Required(ErrorMessage = "Jwt:Emissor e obrigatorio.")]
    public string Emissor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Audiencia e obrigatorio.")]
    public string Audiencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jwt:Segredo e obrigatorio.")]
    [MinLength(
        TamanhoMinimoDoSegredo,
        ErrorMessage = "Jwt:Segredo precisa ter ao menos 32 caracteres.")]
    public string Segredo { get; set; } = string.Empty;

    [Range(1, 24 * 60, ErrorMessage = "Jwt:MinutosDeValidade precisa estar entre 1 e 1440.")]
    public int MinutosDeValidade { get; set; } = 60;

    /// <summary>
    /// Tolerancia de relogio entre quem emite e quem valida o token.
    /// Zero e o correto quando emissor e validador sao o mesmo processo.
    /// </summary>
    [Range(0, 300, ErrorMessage = "Jwt:SegundosDeToleranciaDeRelogio precisa estar entre 0 e 300.")]
    public int SegundosDeToleranciaDeRelogio { get; set; }

    public TimeSpan Validade => TimeSpan.FromMinutes(MinutosDeValidade);

    public TimeSpan ToleranciaDeRelogio => TimeSpan.FromSeconds(SegundosDeToleranciaDeRelogio);
}
