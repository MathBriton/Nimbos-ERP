using Microsoft.Extensions.Logging;
using Nimbus.Application.Autenticacao.Abstracoes;

namespace Nimbus.Infrastructure.Seguranca;

/// <summary>
/// Hash de senha com BCrypt. O work factor define o custo de CPU por
/// verificacao: quanto maior, mais caro fica um ataque de forca bruta offline.
/// </summary>
public sealed class ServicoDeHashDeSenhaBCrypt : IServicoDeHashDeSenha
{
    /// <summary>
    /// 12 rodadas: ~250ms por hash em hardware atual. Equilibra resistencia a
    /// forca bruta com tempo de resposta aceitavel no login.
    /// </summary>
    public const int FatorDeTrabalho = 12;

    /// <summary>
    /// Gerado uma unica vez, na inicializacao do tipo, a partir de um valor
    /// aleatorio: e um hash real, com o mesmo custo dos demais, e a senha que
    /// o originou nao existe em lugar nenhum.
    /// </summary>
    private static readonly string HashDeIscaCompartilhado =
        BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), FatorDeTrabalho);

    private readonly ILogger<ServicoDeHashDeSenhaBCrypt> _logger;

    public ServicoDeHashDeSenhaBCrypt(ILogger<ServicoDeHashDeSenhaBCrypt> logger) =>
        _logger = logger;

    public string HashDeIsca => HashDeIscaCompartilhado;

    public string GerarHash(string senhaEmTextoPuro)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaEmTextoPuro);

        return BCrypt.Net.BCrypt.HashPassword(senhaEmTextoPuro, FatorDeTrabalho);
    }

    public bool Conferir(string senhaEmTextoPuro, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senhaEmTextoPuro) || string.IsNullOrWhiteSpace(hashArmazenado))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(senhaEmTextoPuro, hashArmazenado);
        }
        catch (BCrypt.Net.SaltParseException excecao)
        {
            // Hash corrompido ou gerado por outro algoritmo: trata como senha
            // incorreta, nunca como erro 500 exposto ao cliente.
            _logger.LogError(excecao, "Hash de senha armazenado em formato invalido.");
            return false;
        }
    }
}
