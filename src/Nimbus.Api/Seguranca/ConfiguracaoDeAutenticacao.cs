using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Nimbus.Application.Autenticacao;
using Nimbus.Infrastructure.Seguranca;

namespace Nimbus.Api.Seguranca;

/// <summary>Configura a validacao de JWT Bearer no pipeline da API.</summary>
public static class ConfiguracaoDeAutenticacao
{
    /// <summary>
    /// Header devolvido junto do 401 quando o token expirou, para o frontend
    /// distinguir "sessao expirada" de "credenciais invalidas".
    /// </summary>
    public const string HeaderDeTokenExpirado = "X-Token-Expirado";

    public static IServiceCollection AdicionarAutenticacaoJwt(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var opcoes = configuracao.GetSection(OpcoesDeJwt.Secao).Get<OpcoesDeJwt>()
            ?? throw new InvalidOperationException(
                $"A secao '{OpcoesDeJwt.Secao}' nao foi configurada.");

        if (string.IsNullOrWhiteSpace(opcoes.Segredo)
            || opcoes.Segredo.Length < OpcoesDeJwt.TamanhoMinimoDoSegredo)
        {
            throw new InvalidOperationException(
                $"'{OpcoesDeJwt.Secao}:Segredo' precisa ter ao menos "
                + $"{OpcoesDeJwt.TamanhoMinimoDoSegredo} caracteres.");
        }

        servicos
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configurar =>
            {
                configurar.MapInboundClaims = false;

                configurar.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opcoes.Emissor,

                    ValidateAudience = true,
                    ValidAudience = opcoes.Audiencia,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(opcoes.Segredo)),

                    // Obriga o algoritmo esperado: sem isso um token "alg: none"
                    // ou assinado com outro algoritmo poderia ser aceito.
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],

                    ValidateLifetime = true,
                    ClockSkew = opcoes.ToleranciaDeRelogio,

                    NameClaimType = ClaimsDoNimbus.Nome,
                    RoleClaimType = ClaimsDoNimbus.Papel,
                };

                configurar.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = contexto =>
                    {
                        if (contexto.Exception is SecurityTokenExpiredException)
                        {
                            contexto.Response.Headers[HeaderDeTokenExpirado] = "true";
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        servicos.AddAuthorization();

        return servicos;
    }
}
