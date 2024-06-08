using System;
using System.Numerics;
using System.Security.Cryptography;

// Gera p e q de 1024 bits
BigInteger p = GerarNumeroPrimo(1024);
BigInteger q = GerarNumeroPrimo(1024);
Console.WriteLine($"p: {p}\n");
Console.WriteLine($"q: {q}\n");

static BigInteger GerarNumeroPrimo(int bits)
{
    RandomNumberGenerator rng = RandomNumberGenerator.Create();
    byte[] bytes = new byte[bits / 8];
    BigInteger p;

    do
    {
        rng.GetBytes(bytes);
        p = new BigInteger(bytes);
        p = BigInteger.Abs(p);
        p |= BigInteger.One; // Garantir que p seja ímpar
    } while (!EhPrimo(p, 10));

    return p;
}

// Utiliza Miller-Rabin para verificar se um número é primo (https://www.youtube.com/watch?v=zmhUlVck3J0)
static bool EhPrimo(BigInteger source, int certainty)
{
    // Se o número for 2 ou 3, ele é primo.
    if (source == 2 || source == 3) return true;
    // Se o número for menor que 2 ou par, não é primo.
    if (source < 2 || source % 2 == 0) return false;

    // Inicialmente, d é source - 1.
    BigInteger d = source - 1;
    int s = 0;

    // Divide d por 2 até que d se torne ímpar, contando quantas vezes isso é feito.
    while (d % 2 == 0)
    {
        d /= 2;
        s += 1;
    }

    // Criar um gerador de números aleatórios criptograficamente seguro.
    RandomNumberGenerator rng = RandomNumberGenerator.Create();
    // Array de bytes para armazenar os números gerados aleatoriamente.
    byte[] bytes = new byte[source.ToByteArray().LongLength];

    // Executar o teste de Miller-Rabin 'certainty' vezes para aumentar a confiabilidade.
    for (int i = 0; i < certainty; i++)
    {
        BigInteger a;

        // Gerar um número aleatório a, tal que 2 <= a <= source - 2.
        do
        {
            rng.GetBytes(bytes);
            a = new BigInteger(bytes);
            a = BigInteger.Abs(a);
        } while (a < 2 || a >= source - 2);

        // Calcular x = a^d mod source.
        BigInteger x = BigInteger.ModPow(a, d, source);
        // Se x for 1 ou source - 1, pode ser primo.
        if (x == 1 || x == source - 1) continue;

        // Repetir r vezes: x = x^2 mod source.
        for (int r = 1; r < s; r++)
        {
            x = BigInteger.ModPow(x, 2, source);
            // Se x for 1, source não é primo.
            if (x == 1) return false;
            // Se x for source - 1, pode ser primo, sair do loop.
            if (x == source - 1) break;
        }

        // Se x não for source - 1 após todos os testes, não é primo.
        if (x != source - 1) return false;
    }

    // Se passar em todos os testes, é considerado primo.
    return true;
}
