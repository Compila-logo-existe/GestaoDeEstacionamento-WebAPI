using System.Security.Cryptography;

namespace TokenGenerator;

internal class Program
{
    static void Main(string[] args)
    {
        string chave = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        Console.Write("Chave Gerada 32 Bytes: " + chave);

        Console.ReadKey();
    }
}
