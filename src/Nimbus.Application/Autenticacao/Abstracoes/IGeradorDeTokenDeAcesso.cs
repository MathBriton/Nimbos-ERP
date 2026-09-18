using Nimbus.Domain.Entidades;

namespace Nimbus.Application.Autenticacao.Abstracoes;

/// <summary>Token de acesso emitido para um usuario autenticado.</summary>
public sealed record TokenDeAcesso(string Token, DateTimeOffset ExpiraEm);

/// <summary>
/// Porta de emissao de tokens. A implementacao atual gera JWT assinado com
/// HMAC-SHA256; a troca para chaves assimetricas nao afeta os casos de uso.
/// </summary>
public interface IGeradorDeTokenDeAcesso
{
    TokenDeAcesso Gerar(Usuario usuario);
}
