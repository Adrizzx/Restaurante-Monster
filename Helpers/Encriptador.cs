using System.Security.Cryptography;
using System.Text;

namespace RestauranteMonster.Helpers;

public static class Encriptador
{
    public static string Sha256(string texto)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
        var sb = new StringBuilder(64);
        foreach (var b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
