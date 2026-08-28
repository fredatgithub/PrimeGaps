using System.Diagnostics;
using System.Numerics;

namespace ConsoleAppRecursiveSearch
{
  internal class Program
  {
    static void Main()
    {
      BigInteger start = BigInteger.Parse("101412319996363309069");

      const int wanted = 2;

      Console.WriteLine("Recherche des nombres premiers...");
      Console.WriteLine($"Départ : {start}");
      Console.WriteLine();

      Stopwatch sw = Stopwatch.StartNew();

      int found = 0;
      BigInteger candidate = start + 1;

      while (found < wanted)
      {
        // Élimination rapide des nombres pairs.
        if (candidate > 2 && candidate.IsEven)
        {
          candidate++;
          continue;
        }

        if (PrimeCertificate.TryCreate(candidate, out PrimeCertificate? cert))
        {
          found++;

          Console.WriteLine($"Premier #{found} : {candidate}");

          Console.WriteLine($"Écart : {candidate - start}");

          Console.WriteLine($"Certificat vérifié : {cert.Verify()}");

          Console.WriteLine();

          if (found == wanted)
          {
            Console.WriteLine("=== Certificat du 2e premier ===");
            cert.Print();
          }
        }

        candidate++;
      }

      sw.Stop();

      Console.WriteLine();
      Console.WriteLine($"Temps total : {sw.Elapsed.TotalMilliseconds:N3} ms");
    }
  }


  // ================================================================
  // Certificat de primalité Pocklington
  // ================================================================

  sealed class PrimeCertificate
  {
    public BigInteger N { get; }

    public IReadOnlyList<PocklingtonStep> Steps => _steps;

    private readonly List<PocklingtonStep> _steps;

    private PrimeCertificate(BigInteger n, List<PocklingtonStep> steps)
    {
      N = n;
      _steps = steps;
    }

    // ------------------------------------------------------------
    // Construction récursive
    // ------------------------------------------------------------

    public static bool TryCreate(BigInteger n, out PrimeCertificate? certificate)
    {
      certificate = null;

      if (n < 2)
        return false;

      // Petits nombres : preuve triviale.
      if (n <= 3)
      {
        certificate =
            new PrimeCertificate(
                n,
                new List<PocklingtonStep>());

        return true;
      }

      // On factorise n - 1.
      Dictionary<BigInteger, int> factors = Factorization.Factor(n - 1);

      if (factors.Count == 0)
        return false;

      BigInteger F = BigInteger.One;

      foreach (var pair in factors)
      {
        BigInteger p = pair.Key;
        int exponent = pair.Value;

        // On ne peut utiliser p que si p est premier.
        if (!TryCreate(p, out PrimeCertificate? child))
          return false;

        for (int i = 0; i < exponent; i++)
          F *= p;
      }

      // Condition de Pocklington :
      //
      // F > sqrt(n)
      //
      // Il est plus simple de comparer F² > n.
      if (F * F <= n)
        return false;

      var steps = new List<PocklingtonStep>();

      foreach (var pair in factors)
      {
        BigInteger q = pair.Key;

        if (!TryFindWitness(n, q, out BigInteger a))
        {
          return false;
        }

        steps.Add(new PocklingtonStep(q, a));
      }

      certificate = new PrimeCertificate(n, steps);

      return certificate.Verify();
    }

    // ------------------------------------------------------------
    // Recherche d'un témoin de Pocklington
    // ------------------------------------------------------------

    private static bool TryFindWitness(BigInteger n, BigInteger q, out BigInteger witness)
    {
      witness = 0;
      BigInteger exponent = n - 1;

      for (BigInteger a = 2; a < 10000; a++)
      {
        // Condition 1 :
        //
        // a^(n-1) ≡ 1 mod n
        //
        if (BigInteger.ModPow(a, exponent, n) != 1)
          continue;

        // Condition 2 :
        //
        // gcd(
        //     a^((n-1)/q) - 1,
        //     n
        // ) = 1
        //
        BigInteger x = BigInteger.ModPow(a, exponent / q, n);

        BigInteger gcd = BigInteger.GreatestCommonDivisor(x - 1, n);

        if (gcd == 1)
        {
          witness = a;
          return true;
        }
      }

      return false;
    }

    // ------------------------------------------------------------
    // Vérification indépendante du certificat
    // ------------------------------------------------------------

    public bool Verify()
    {
      if (N < 2)
        return false;

      if (N <= 3)
        return true;

      Dictionary<BigInteger, int> factors = Factorization.Factor(N - 1);

      if (factors.Count == 0)
        return false;

      BigInteger F = BigInteger.One;

      foreach (var pair in factors)
      {
        BigInteger q = pair.Key;
        int exponent = pair.Value;

        // Le certificat doit contenir q.
        PocklingtonStep? step = FindStep(q);

        if (step == null)
          return false;

        // Vérification récursive que q est premier.
        if (!TryCreate(q, out PrimeCertificate? child))
          return false;

        if (!child.Verify())
          return false;

        for (int i = 0; i < exponent; i++)
          F *= q;
      }

      // Condition F > sqrt(N)
      if (F * F <= N)
        return false;

      foreach (var pair in factors)
      {
        BigInteger q = pair.Key;

        PocklingtonStep? step = FindStep(q);

        if (step == null)
          return false;

        BigInteger a = step.Witness;

        // a^(N-1) mod N == 1
        if (BigInteger.ModPow(a, N - 1, N) != 1)
        {
          return false;
        }

        // gcd(a^((N-1)/q) - 1, N) == 1
        BigInteger x = BigInteger.ModPow(a, (N - 1) / q, N);

        BigInteger gcd = BigInteger.GreatestCommonDivisor(x - 1, N);

        if (gcd != 1)
          return false;
      }

      return true;
    }

    private PocklingtonStep? FindStep(BigInteger q)
    {
      foreach (PocklingtonStep step in _steps)
      {
        if (step.Q == q)
          return step;
      }

      return null;
    }

    // ------------------------------------------------------------
    // Affichage
    // ------------------------------------------------------------

    public void Print(int indent = 0)
    {
      string pad = new string(' ', indent);

      Console.WriteLine($"{pad}N = {N}");

      Console.WriteLine($"{pad}N - 1 = {N - 1}");

      Console.WriteLine($"{pad}Étapes Pocklington :");

      foreach (PocklingtonStep step in _steps)
      {
        Console.WriteLine($"{pad}  q = {step.Q}, témoin a = {step.Witness}");

        BigInteger condition1 = BigInteger.ModPow(step.Witness, N - 1, N);  

        BigInteger condition2 = BigInteger.ModPow(step.Witness, (N - 1) / step.Q, N);

        BigInteger gcd = BigInteger.GreatestCommonDivisor(condition2 - 1, N);

        Console.WriteLine($"{pad}    a^(N-1) mod N = {condition1}");

        Console.WriteLine($"{pad}    gcd(a^((N-1)/q)-1,N) = {gcd}");

        Console.WriteLine($"{pad}    => {(condition1 == 1 && gcd == 1
                ? "OK"
                : "ÉCHEC")}");
      }

      Console.WriteLine($"{pad}Résultat : " + $"{(Verify() ? "PREMIER PROUVÉ" : "ÉCHEC")}");
    }
  }

  // ================================================================
  // Une étape du certificat
  // ================================================================

  sealed class PocklingtonStep
  {
    public BigInteger Q { get; }

    public BigInteger Witness { get; }

    public PocklingtonStep(BigInteger q, BigInteger witness)
    {
      Q = q;
      Witness = witness;
    }
  }

  // ================================================================
  // Factorisation par division d'essai
  // ================================================================

  static class Factorization
  {
    public static Dictionary<BigInteger, int> Factor(BigInteger n)
    {
      var result = new Dictionary<BigInteger, int>();
      if (n < 2)
        return result;

      // Facteur 2.
      while (n.IsEven)
      {
        Add(result, 2);
        n >>= 1;
      }

      BigInteger p = 3;

      while (p * p <= n)
      {
        while (n % p == 0)
        {
          Add(result, p);
          n /= p;
        }

        p += 2;
      }

      if (n > 1)
        Add(result, n);

      return result;
    }

    private static void Add(Dictionary<BigInteger, int> factors, BigInteger p)
    {
      if (factors.TryGetValue(p, out int count))
      {
        factors[p] = count + 1;
      }
      else
      {
        factors[p] = 1;
      }
    }
  }
}
