using Nimbus.Domain.Common;

namespace Nimbus.Domain.Entidades;

/// <summary>
/// Usuario interno do ERP. Na Sprint 1 carrega apenas o necessario para
/// autenticar; a associacao com papeis/permissoes chega na Sprint 15 (RBAC).
/// </summary>
public class Usuario : EntidadeBase
{
    /// <summary>Numero de falhas consecutivas que dispara o bloqueio temporario.</summary>
    public const int TentativasAntesDoBloqueio = 5;

    /// <summary>Duracao do bloqueio apos estourar <see cref="TentativasAntesDoBloqueio"/>.</summary>
    public static readonly TimeSpan DuracaoDoBloqueio = TimeSpan.FromMinutes(15);

    // Construtor sem parametros exigido pelo EF Core para materializar a entidade.
    private Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
    }

    public string Nome { get; private set; }

    /// <summary>Login do usuario. Sempre normalizado para minusculas.</summary>
    public string Email { get; private set; }

    /// <summary>Hash BCrypt da senha. A senha em texto puro nunca e persistida.</summary>
    public string SenhaHash { get; private set; }

    public bool Ativo { get; private set; } = true;

    public DateTimeOffset? UltimoAcessoEm { get; private set; }

    public int TentativasDeLoginFalhas { get; private set; }

    public DateTimeOffset? BloqueadoAte { get; private set; }

    /// <summary>
    /// Cria um usuario ja validado. O hash da senha e calculado fora do dominio
    /// (porta <c>IServicoDeHashDeSenha</c>), para o Domain nao depender de BCrypt.
    /// </summary>
    public static Usuario Criar(string nome, string email, string senhaHash)
    {
        return new Usuario
        {
            Nome = ExcecaoDeDominio.TextoObrigatorio(nome, nameof(Nome)),
            Email = NormalizarEmail(email),
            SenhaHash = ExcecaoDeDominio.TextoObrigatorio(senhaHash, nameof(SenhaHash)),
            Ativo = true,
        };
    }

    /// <summary>Normaliza e valida o formato do e-mail.</summary>
    public static string NormalizarEmail(string? email)
    {
        var normalizado = ExcecaoDeDominio.TextoObrigatorio(email, nameof(Email)).ToLowerInvariant();

        // Validacao deliberadamente simples: o objetivo e barrar erro grosseiro
        // de digitacao, nao implementar a RFC 5322 inteira.
        var posicaoDoArroba = normalizado.IndexOf('@', StringComparison.Ordinal);
        var ehFormatoValido =
            posicaoDoArroba > 0
            && posicaoDoArroba < normalizado.Length - 1
            && normalizado.IndexOf('@', posicaoDoArroba + 1) < 0
            && normalizado.Contains('.', StringComparison.Ordinal)
            && !normalizado.Contains(' ', StringComparison.Ordinal);

        ExcecaoDeDominio.SeVerdadeiro(!ehFormatoValido, $"O e-mail '{email}' nao e valido.");

        return normalizado;
    }

    public void AlterarNome(string nome) =>
        Nome = ExcecaoDeDominio.TextoObrigatorio(nome, nameof(Nome));

    public void AlterarEmail(string email) => Email = NormalizarEmail(email);

    public void DefinirSenhaHash(string senhaHash)
    {
        SenhaHash = ExcecaoDeDominio.TextoObrigatorio(senhaHash, nameof(SenhaHash));

        // Trocar a senha encerra qualquer bloqueio pendente.
        TentativasDeLoginFalhas = 0;
        BloqueadoAte = null;
    }

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;

    /// <summary>Indica se o usuario esta cumprindo bloqueio temporario em <paramref name="agora"/>.</summary>
    public bool EstaBloqueado(DateTimeOffset agora) => BloqueadoAte is { } ate && ate > agora;

    /// <summary>
    /// Verifica se o usuario pode autenticar. Nao checa a senha: isso e
    /// responsabilidade do servico de hash, na camada de aplicacao.
    /// </summary>
    public bool PodeAutenticar(DateTimeOffset agora) => Ativo && !EstaBloqueado(agora);

    /// <summary>Registra um login bem-sucedido, zerando o contador de falhas.</summary>
    public void RegistrarAcessoBemSucedido(DateTimeOffset agora)
    {
        UltimoAcessoEm = agora;
        TentativasDeLoginFalhas = 0;
        BloqueadoAte = null;
    }

    /// <summary>
    /// Registra uma tentativa de senha incorreta e aplica bloqueio temporario
    /// ao atingir <see cref="TentativasAntesDoBloqueio"/> falhas consecutivas.
    /// </summary>
    public void RegistrarFalhaDeLogin(DateTimeOffset agora)
    {
        TentativasDeLoginFalhas++;

        if (TentativasDeLoginFalhas >= TentativasAntesDoBloqueio)
        {
            BloqueadoAte = agora.Add(DuracaoDoBloqueio);
            TentativasDeLoginFalhas = 0;
        }
    }
}
