using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

// Gera p e q de 1024 bits
BigInteger p = GerarNumeroPrimo(1024);
Console.WriteLine($"p: {p}\n");
BigInteger q = GerarNumeroPrimo(1024);
Console.WriteLine($"q: {q}\n");

// Calcular n e φ(n)
BigInteger n = p * q;
Console.WriteLine($"n: {n}\n");
BigInteger phi = (p - 1) * (q - 1);
Console.WriteLine($"φ(n): {phi}\n");
BigInteger e = GerarNumeroE(phi);
Console.WriteLine($"e: {e}\n");

// Calcular d
BigInteger d = ModInverse(e, phi);
Console.WriteLine($"d: {d}\n");

// Mostra chaves
Console.WriteLine($"Chave Pública: (n={n}, e={e})\n");
Console.WriteLine($"Chave Privada: (n={n}, d={d})\n");


string mensagem = "Criada em 1965, a Universidade do Estado de Santa Catarina (Udesc), que tem excelência no ensino superior atuando nas áreas de ensino, pesquisa e extensão e está entre as melhores universidades do Brasil e do mundo, conta com estrutura multicampi, com 13 unidades distribuídas em dez cidades de Santa Catarina, na Região Sul do Brasil, além de cerca de 30 polos de apoio presencial para o ensino a distância, em parceria com a Universidade Aberta do Brasil (UAB), do Ministério da Educação (MEC). Atualmente, são cerca de 14 mil alunos distribuídos em mais de 60 cursos de graduação e em mais de 50 mestrados e doutorados, além de cursos lato sensu e residências, que são oferecidos gratuitamente, e cerca de 90% dos professores efetivos são doutores. O ingresso na universidade pode ser feito via vestibular (verão e inverno), Sistema de Seleção Unificada (Sisu) e editais de transferência. Ao todo, são mais de três mil vagas todos os anos, sendo 20% para estudantes de escolas públicas e 10% para candidatos negros.";
Console.WriteLine($"Mensagem Original: {mensagem}\n");

List<BigInteger> mensagemCifrada = CrifrarMensagemEmBlocos(mensagem, e, n);
Console.WriteLine("Mensagem Cifrada:");
foreach (var parteCifrada in mensagemCifrada)
{
    Console.WriteLine(parteCifrada);
}
Console.WriteLine();

string mensagemDecriptada = DecriptarMensagemEmBlocos(mensagemCifrada, d, n);
Console.WriteLine($"Mensagem Decriptada: {mensagemDecriptada}");

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

static BigInteger GerarNumeroE(BigInteger phi)
{
    RandomNumberGenerator rng = RandomNumberGenerator.Create();
    byte[] bytes = new byte[phi.ToByteArray().LongLength];
    BigInteger e;

    do
    {
        rng.GetBytes(bytes);
        e = new BigInteger(bytes);
        e = BigInteger.Abs(e);
        e = e % (phi - 1) + 1; // Garantir que 1 < e < phi
    } while (BigInteger.GreatestCommonDivisor(e, phi) != 1);

    return e;
}

// Função para calcular o inverso modular usando o algoritmo estendido de Euclides
static BigInteger ModInverse(BigInteger a, BigInteger m)
{
    BigInteger m0 = m, t, q;
    BigInteger x0 = 0, x1 = 1;

    if (m == 1)
        return 0;

    while (a > 1)
    {
        // q é o quociente
        q = a / m;
        t = m;

        // m é o resto agora, processa a mesma coisa que o algoritmo de Euclides
        m = a % m;
        a = t;
        t = x0;

        x0 = x1 - q * x0;
        x1 = t;
    }

    // Faz x1 positivo
    if (x1 < 0)
        x1 += m0;

    return x1;
}

static List<BigInteger> CrifrarMensagemEmBlocos(string mensagem, BigInteger e, BigInteger n)
{
    byte[] bytes = Encoding.UTF8.GetBytes(mensagem);
    int maxBlockSize = (n.GetByteCount() - 1); // Tamanho máximo do bloco em bytes

    List<BigInteger> blocosCifrados = new List<BigInteger>();
    for (int i = 0; i < bytes.Length; i += maxBlockSize)
    {
        byte[] bloco = new byte[Math.Min(maxBlockSize, bytes.Length - i)];
        Array.Copy(bytes, i, bloco, 0, bloco.Length);
        BigInteger m = new BigInteger(bloco);
        BigInteger c = BigInteger.ModPow(m, e, n);
        blocosCifrados.Add(c);
    }

    return blocosCifrados;
}

static string DecriptarMensagemEmBlocos(List<BigInteger> mensagemCifrada, BigInteger d, BigInteger n)
{
    List<byte> bytesDecriptados = new List<byte>();

    foreach (var blocoCifrado in mensagemCifrada)
    {
        BigInteger m = BigInteger.ModPow(blocoCifrado, d, n);
        byte[] blocoDecriptado = m.ToByteArray();
        bytesDecriptados.AddRange(blocoDecriptado);
    }

    return Encoding.UTF8.GetString(bytesDecriptados.ToArray());
}