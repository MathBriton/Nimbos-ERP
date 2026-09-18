using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nimbus.Application.Autenticacao;
using Nimbus.Application.Autenticacao.Abstracoes;
using Nimbus.Application.Common.Abstracoes;
using Nimbus.Domain.Entidades;

namespace Nimbus.Infrastructure.Seguranca;

/// <summary>
/// Emite JWT assinado com HMAC-SHA256 (chave simetrica).
/// A partir da Sprint 15 (RBAC) o token passa a carregar tambem as claims
/// de papel e permissao do usuario.
/// </summary>
public sealed class GeradorDeTokenJwt : IGeradorDeTokenDeAcesso
{
    private readonly OpcoesDeJwt _opcoes;
    private readonly IProvedorDeDataHora _provedorDeDataHora;
    private readonly SigningCredentials _credenciaisDeAssinatura;
    private readonly JsonWebTokenHandler _manipulador = new();

    public GeradorDeTokenJwt(
        IOptions<OpcoesDeJwt> opcoes,
        IProvedorDeDataHora provedorDeDataHora)
    {
        ArgumentNullException.ThrowIfNull(opcoes);

        _opcoes = opcoes.Value;
        _provedorDeDataHora = provedorDeDataHora;

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.Segredo));
        _credenciaisDeAssinatura = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
    }

    public TokenDeAcesso Gerar(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var agora = _provedorDeDataHora.Agora;
        var expiraEm = agora.Add(_opcoes.Validade);

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = _opcoes.Emissor,
            Audience = _opcoes.Audiencia,
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = _credenciaisDeAssinatura,
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimsDoNimbus.Id, usuario.Id.ToString()),
                new Claim(ClaimsDoNimbus.Email, usuario.Email),
                new Claim(ClaimsDoNimbus.Nome, usuario.Nome),

                // jti: identificador unico do token. Sera a chave da lista de
                // revogacao quando o refresh token entrar no projeto.
                new Claim(ClaimsDoNimbus.IdDoToken, Guid.CreateVersion7().ToString()),
            ]),
        };

        return new TokenDeAcesso(_manipulador.CreateToken(descritor), expiraEm);
    }
}
