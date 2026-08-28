using System.Diagnostics;
using System.Numerics;

namespace ConsoleAppPollard
{
  internal class Program
  {
    private static readonly Dictionary<BigInteger, PrimeCertificate> Cache = new();

    static void Main()
    {
      BigInteger start =
          BigInteger.Parse("101412319996363309069");

      const int wanted = 2;

      Console.WriteLine("Recherche de nombres premiers");
      Console.WriteLine($"Départ : {start:N0}");
      Console.WriteLine();

      Stopwatch sw = Stopwatch.StartNew();

      int found = 0;
      long candidates = 0;

      BigInteger n = start + 1;

      while (found < wanted)
      {
        candidates++;

        // Tous les nombres pairs > 2 sont composés.
        if (n > 2 && n.IsEven)
        {
          n++;
          continue;
        }

        // Petit filtre.
        if (PrimeMath.IsDivisibleBySmallPrime(n))
        {
          n++;
          continue;
        }

        if (PrimeCertificate.TryCreate(n, out PrimeCertificate? cert))
        {
          found++;

          Console.WriteLine(
              $"Premier #{found} : {n}");

          Console.WriteLine(
              $"Écart : {n - start}");

          Console.WriteLine(
              $"Certificat : {cert.Verify()}");

          Console.WriteLine();

          if (found == wanted)
          {
            Console.WriteLine(
                "=== Certificat du deuxième nombre ===");
            Console.WriteLine();

            cert.Print();
          }
        }

        n++;
      }

      sw.Stop();

      Console.WriteLine();
      Console.WriteLine("------------------------------------------");
      Console.WriteLine($"Candidats examinés : {candidates:N0}");
      Console.WriteLine(
          $"Certificats en cache : {Cache.Count:N0}");
      Console.WriteLine(
          $"Temps : {sw.Elapsed.TotalSeconds:F3} s");
    }


    // ============================================================
    // Certificat de Pocklington
    // ============================================================

    private sealed class PrimeCertificate
    {
      public BigInteger N { get; }

      public BigInteger F { get; }

      public IReadOnlyList<FactorProof> Factors =>
          _factors;

      private readonly List<FactorProof> _factors;

      private PrimeCertificate(
          BigInteger n,
          BigInteger f,
          List<FactorProof> factors)
      {
        N = n;
        F = f;
        _factors = factors;
      }


      // --------------------------------------------------------
      // Construction
      // --------------------------------------------------------

      public static bool TryCreate(
          BigInteger n,
          out PrimeCertificate? certificate)
      {
        certificate = null;

        if (n < 2)
          return false;

        if (Cache.TryGetValue(n, out certificate))
          return true;

        // Petits nombres : preuve immédiate.
        if (n <= 3)
        {
          certificate =
              new PrimeCertificate(
                  n,
                  n - 1,
                  new List<FactorProof>());

          Cache[n] = certificate;
          return true;
        }

        // Filtre probabiliste uniquement destiné à éviter
        // de factoriser n-1 pour un nombre manifestement composé.
        if (!PrimeMath.MillerRabin(n))
          return false;

        BigInteger nm1 = n - 1;

        /*
         * On factorise n-1 avec Pollard Rho.
         *
         * On peut ensuite sélectionner suffisamment de
         * facteurs pour avoir F > sqrt(n).
         */
        Dictionary<BigInteger, int> factorization =
            PrimeMath.Factor(nm1);

        if (factorization.Count == 0)
          return false;

        BigInteger F = BigInteger.One;

        var selected =
            new List<(BigInteger Q, int Exponent)>();

        /*
         * On privilégie les facteurs les plus grands.
         * Cela permet généralement d'atteindre F > sqrt(n)
         * avec peu de facteurs.
         */
        var ordered =
            new List<KeyValuePair<BigInteger, int>>(
                factorization);

        ordered.Sort(
            (x, y) => y.Key.CompareTo(x.Key));

        foreach (var pair in ordered)
        {
          BigInteger q = pair.Key;
          int exponent = pair.Value;

          if (!TryCreate(
                  q,
                  out PrimeCertificate? child))
          {
            continue;
          }

          for (int i = 0; i < exponent; i++)
          {
            F *= q;

            selected.Add(
                (q, exponent));

            if (F * F > n)
              break;
          }

          if (F * F > n)
            break;
        }

        // Condition fondamentale de Pocklington :
        //
        // F > sqrt(n)
        //
        if (F * F <= n)
          return false;

        var proofs = new List<FactorProof>();

        var alreadyAdded =
            new HashSet<BigInteger>();

        foreach (var item in selected)
        {
          if (!alreadyAdded.Add(item.Q))
            continue;

          if (!TryFindWitness(
                  n,
                  item.Q,
                  out BigInteger witness))
          {
            return false;
          }

          proofs.Add(
              new FactorProof(
                  item.Q,
                  witness));
        }

        certificate =
            new PrimeCertificate(
                n,
                F,
                proofs);

        if (!certificate.Verify())
        {
          certificate = null;
          return false;
        }

        Cache[n] = certificate;

        return true;
      }


      // --------------------------------------------------------
      // Recherche du témoin de Pocklington
      // --------------------------------------------------------

      private static bool TryFindWitness(
          BigInteger n,
          BigInteger q,
          out BigInteger witness)
      {
        witness = 0;

        BigInteger exponent = n - 1;
        BigInteger reducedExponent = exponent / q;

        for (BigInteger a = 2; a < 10000; a++)
        {
          /*
           * a^(n-1) ≡ 1 mod n
           */
          if (BigInteger.ModPow(
                  a,
                  exponent,
                  n) != 1)
          {
            continue;
          }

          /*
           * gcd(
           *     a^((n-1)/q) - 1,
           *     n
           * ) = 1
           */
          BigInteger x =
              BigInteger.ModPow(
                  a,
                  reducedExponent,
                  n);

          BigInteger gcd =
              BigInteger.GreatestCommonDivisor(
                  x - 1,
                  n);

          if (gcd == 1)
          {
            witness = a;
            return true;
          }
        }

        return false;
      }


      // --------------------------------------------------------
      // Vérification indépendante
      // --------------------------------------------------------

      public bool Verify()
      {
        if (N < 2)
          return false;

        if (N <= 3)
          return true;

        /*
         * Vérifie F > sqrt(N).
         */
        if (F * F <= N)
          return false;

        foreach (FactorProof proof in _factors)
        {
          BigInteger q = proof.Q;
          BigInteger a = proof.Witness;

          /*
           * Le facteur q doit lui-même être prouvé premier.
           */
          if (!TryCreate(
                  q,
                  out PrimeCertificate? child))
          {
            return false;
          }

          if (!child.Verify())
            return false;

          /*
           * a^(N-1) ≡ 1 mod N
           */
          if (BigInteger.ModPow(
                  a,
                  N - 1,
                  N) != 1)
          {
            return false;
          }

          /*
           * gcd(a^((N-1)/q) - 1, N) = 1
           */
          BigInteger x =
              BigInteger.ModPow(
                  a,
                  (N - 1) / q,
                  N);

          BigInteger gcd =
              BigInteger.GreatestCommonDivisor(
                  x - 1,
                  N);

          if (gcd != 1)
            return false;
        }

        return true;
      }


      // --------------------------------------------------------
      // Affichage
      // --------------------------------------------------------

      public void Print(int level = 0)
      {
        string indent =
            new string(' ', level * 2);

        Console.WriteLine(
            $"{indent}N = {N}");

        Console.WriteLine(
            $"{indent}F = {F}");

        Console.WriteLine(
            $"{indent}F² > N : {F * F > N}");

        foreach (FactorProof proof in _factors)
        {
          Console.WriteLine();

          Console.WriteLine(
              $"{indent}q = {proof.Q}");

          Console.WriteLine(
              $"{indent}a = {proof.Witness}");

          BigInteger c1 =
              BigInteger.ModPow(
                  proof.Witness,
                  N - 1,
                  N);

          BigInteger c2 =
              BigInteger.ModPow(
                  proof.Witness,
                  (N - 1) / proof.Q,
                  N);

          BigInteger gcd =
              BigInteger.GreatestCommonDivisor(
                  c2 - 1,
                  N);

          Console.WriteLine(
              $"{indent}a^(N-1) mod N = {c1}");

          Console.WriteLine(
              $"{indent}gcd(...) = {gcd}");

          Console.WriteLine(
              $"{indent}Pocklington : " +
              $"{(c1 == 1 && gcd == 1 ? "OK" : "ÉCHEC")}");

          if (Cache.TryGetValue(
                  proof.Q,
                  out PrimeCertificate? child))
          {
            Console.WriteLine();

            Console.WriteLine(
                $"{indent}Preuve récursive de q :");

            child.Print(level + 1);
          }
        }

        Console.WriteLine();

        Console.WriteLine(
            $"{indent}=> " +
            $"{(Verify()
                ? "PREMIER PROUVÉ"
                : "ÉCHEC")}");
      }
    }


    // ============================================================
    // Preuve d'un facteur q
    // ============================================================

    private sealed class FactorProof
    {
      public BigInteger Q { get; }

      public BigInteger Witness { get; }

      public FactorProof(
          BigInteger q,
          BigInteger witness)
      {
        Q = q;
        Witness = witness;
      }
    }


    // ============================================================
    // Mathématiques
    // ============================================================

    private static class PrimeMath
    {
      private static readonly int[] SmallPrimes =
      {
            2, 3, 5, 7, 11, 13, 17, 19,
            23, 29, 31, 37, 41, 43, 47,
            53, 59, 61, 67, 71, 73, 79,
            83, 89, 97
        };


      // --------------------------------------------------------
      // Petit filtre
      // --------------------------------------------------------

      public static bool IsDivisibleBySmallPrime(
          BigInteger n)
      {
        foreach (int p in SmallPrimes)
        {
          if (n == p)
            return false;

          if (n % p == 0)
            return true;
        }

        return false;
      }


      // --------------------------------------------------------
      // Miller-Rabin
      // --------------------------------------------------------

      public static bool MillerRabin(
          BigInteger n)
      {
        if (n < 2)
          return false;

        foreach (int p in SmallPrimes)
        {
          if (n == p)
            return true;

          if (n % p == 0)
            return false;
        }

        BigInteger d = n - 1;
        int s = 0;

        while (d.IsEven)
        {
          d >>= 1;
          s++;
        }

        /*
         * Plusieurs bases.
         *
         * Ici Miller-Rabin est un FILTRE.
         * La décision finale vient de Pocklington.
         */
        BigInteger[] bases =
        {
                2, 3, 5, 7, 11, 13,
                17, 19, 23, 29, 31, 37
            };

        foreach (BigInteger a in bases)
        {
          if (a >= n)
            continue;

          BigInteger x =
              BigInteger.ModPow(
                  a,
                  d,
                  n);

          if (x == 1 || x == n - 1)
            continue;

          bool passed = false;

          for (int r = 1; r < s; r++)
          {
            x = (x * x) % n;

            if (x == n - 1)
            {
              passed = true;
              break;
            }
          }

          if (!passed)
            return false;
        }

        return true;
      }


      // --------------------------------------------------------
      // Pollard Rho
      // --------------------------------------------------------

      public static Dictionary<BigInteger, int> Factor(
          BigInteger n)
      {
        var result =
            new Dictionary<BigInteger, int>();

        FactorRecursive(n, result);

        return result;
      }


      private static void FactorRecursive(
          BigInteger n,
          Dictionary<BigInteger, int> result)
      {
        if (n == 1)
          return;

        if (MillerRabin(n))
        {
          AddFactor(result, n);
          return;
        }

        BigInteger divisor =
            PollardRho(n);

        FactorRecursive(divisor, result);
        FactorRecursive(n / divisor, result);
      }


      private static void AddFactor(
          Dictionary<BigInteger, int> result,
          BigInteger p)
      {
        if (result.TryGetValue(
                p,
                out int count))
        {
          result[p] = count + 1;
        }
        else
        {
          result[p] = 1;
        }
      }


      private static BigInteger PollardRho(
          BigInteger n)
      {
        if (n % 2 == 0)
          return 2;

        if (n % 3 == 0)
          return 3;

        var random =
            new Random();

        while (true)
        {
          BigInteger c =
              RandomBigInteger(
                  random,
                  1,
                  n - 1);

          BigInteger x =
              RandomBigInteger(
                  random,
                  0,
                  n - 1);

          BigInteger y = x;
          BigInteger d = 1;

          while (d == 1)
          {
            x = Iterate(x, c, n);
            y = Iterate(
                Iterate(y, c, n),
                c,
                n);

            BigInteger difference =
                BigInteger.Abs(x - y);

            d =
                BigInteger.GreatestCommonDivisor(
                    difference,
                    n);
          }

          if (d != n)
            return d;
        }
      }


      private static BigInteger Iterate(
          BigInteger x,
          BigInteger c,
          BigInteger n)
      {
        return (x * x + c) % n;
      }


      private static BigInteger RandomBigInteger(
          Random random,
          BigInteger min,
          BigInteger max)
      {
        if (min >= max)
          return min;

        BigInteger range =
            max - min;

        byte[] bytes =
            range.ToByteArray(
                isUnsigned: true,
                isBigEndian: true);

        BigInteger value;

        do
        {
          random.NextBytes(bytes);

          value =
              new BigInteger(
                  bytes,
                  isUnsigned: true,
                  isBigEndian: true);
        }
        while (value > range);

        return min + value;
      }
    }
  }
}
