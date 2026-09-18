using Nimbus.Application.Autenticacao.Abstracoes;

namespace Nimbus.Tests.Dobras;

/// <summary>
/// Hash trivial e reversivel ("hash:" + senha). Os testes de caso de uso querem
/// verificar o fluxo de autenticacao, nao a forca do BCrypt - e rodar BCrypt de
/// verdade (12 rodadas) deixaria a suite lenta sem ganho nenhum.
/// </summary>
public sealed class ServicoDeHashDeSenhaFalso : IServicoDeHashDeSenha
{
    private const string Prefixo = "hash:";

    /// <summary>Quantas vezes <see cref="Conferir"/> foi chamado.</summary>
    public int QuantidadeDeConferencias { get; private set; }

    public string HashDeIsca => $"{Prefixo}__isca__";

    public string GerarHash(string senhaEmTextoPuro) => Prefixo + senhaEmTextoPuro;

    public bool Conferir(string senhaEmTextoPuro, string hashArmazenado)
    {
        QuantidadeDeConferencias++;

        return string.Equals(
            Prefixo + senhaEmTextoPuro,
            hashArmazenado,
            StringComparison.Ordinal);
    }
}
