namespace Nimbus.Application.Autenticacao;

/// <summary>
/// Nomes das claims usadas nos tokens do Nimbus.
///
/// Sao os nomes curtos e padronizados do JWT (RFC 7519) em vez das URIs longas
/// de <c>System.Security.Claims.ClaimTypes</c>: o token fica bem menor e
/// legivel em qualquer decodificador. Por isso a API roda com
/// <c>MapInboundClaims = false</c>, desligando a traducao automatica para URIs.
/// </summary>
public static class ClaimsDoNimbus
{
    /// <summary>Identificador do usuario (claim padrao "subject").</summary>
    public const string Id = "sub";

    public const string Email = "email";

    public const string Nome = "name";

    /// <summary>Identificador unico do token, base da futura lista de revogacao.</summary>
    public const string IdDoToken = "jti";

    /// <summary>Papel do usuario. Passa a ser preenchida na Sprint 15 (RBAC).</summary>
    public const string Papel = "role";
}
