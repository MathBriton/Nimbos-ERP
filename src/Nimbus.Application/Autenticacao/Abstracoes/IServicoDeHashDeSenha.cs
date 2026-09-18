namespace Nimbus.Application.Autenticacao.Abstracoes;

/// <summary>
/// Porta do algoritmo de hash de senha. Mantem o BCrypt confinado a
/// Infrastructure e permite trocar de algoritmo sem tocar nos casos de uso.
/// </summary>
public interface IServicoDeHashDeSenha
{
    /// <summary>
    /// Hash valido e descartavel, com o mesmo custo de CPU dos hashes reais.
    /// Serve para conferir uma senha quando o e-mail informado nao existe, de
    /// modo que o tempo de resposta nao revele quais contas estao cadastradas.
    /// </summary>
    string HashDeIsca { get; }

    string GerarHash(string senhaEmTextoPuro);

    /// <summary>
    /// Confere a senha contra o hash. Devolve <c>false</c> (sem lancar excecao)
    /// se o hash armazenado estiver corrompido ou em formato desconhecido.
    /// </summary>
    bool Conferir(string senhaEmTextoPuro, string hashArmazenado);
}
