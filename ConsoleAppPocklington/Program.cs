using System.Diagnostics;
using System.Numerics;

namespace ConsoleAppPocklington
{
  internal class Program
  {
    static readonly int[] SmallPrimes =
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
        31, 37, 41, 43, 47, 53, 59, 61, 67,
        71, 73, 79, 83, 89, 97
    };

    static void Main()
    {
      BigInteger start =
          BigInteger.Parse("101412319996363309069");

      int wanted = 2;
      int found = 0;
      int tested = 0;

      Stopwatch stopwatch = Stopwatch.StartNew();

      Console.WriteLine($"Départ : {start}");
      Console.WriteLine("Recherche...");
      Console.WriteLine();

      BigInteger candidate = start + 1;

      while (found < wanted)
      {
        tested++;

        // Élimination rapide des nombres pairs.
        if (candidate > 2 && candidate.IsEven)
        {
          candidate++;
          continue;
        }

        if (IsPrime(candidate))
        {
          found++;

          Console.WriteLine(
              $"Premier #{found} : {candidate}");

          Console.WriteLine(
              $"Écart avec le départ : {candidate - start}");

          Console.WriteLine();
        }

        candidate++;
      }

      stopwatch.Stop();

      Console.WriteLine("----------------------------------------");
      Console.WriteLine($"Candidats examinés : {tested:N0}");
      Console.WriteLine(
          $"Temps : {stopwatch.Elapsed.TotalMilliseconds:N3} ms");
    }

    // ============================================================
    // Test complet de primalité
    // ============================================================

    static bool IsPrime(BigInteger n)
    {
      if (n < 2)
        return false;

      // Petits nombres premiers.
      foreach (int p in SmallPrimes)
      {
        if (n == p)
          return true;

        if (n % p == 0)
          return false;
      }

      /*
       * Première étape :
       * Miller-Rabin pour éliminer rapidement les composés.
       */
      if (!MillerRabin(n))
        return false;

      /*
       * Deuxième étape :
       * preuve de primalité par Pocklington.
       */
      return Pocklington(n);
    }

    // ============================================================
    // Miller-Rabin
    // ============================================================

    static bool MillerRabin(BigInteger n)
    {
      BigInteger d = n - 1;
      int s = 0;

      while (d.IsEven)
      {
        d >>= 1;
        s++;
      }

      // Bases supplémentaires pour une excellente sécurité
      // sur les entiers de cette taille.
      BigInteger[] bases =
      {
            2, 3, 5, 7, 11, 13, 17,
            19, 23, 29, 31, 37
        };

      foreach (BigInteger a in bases)
      {
        if (a >= n)
          continue;

        BigInteger x = BigInteger.ModPow(a, d, n);

        if (x == 1 || x == n - 1)
          continue;

        bool witnessPassed = false;

        for (int r = 1; r < s; r++)
        {
          x = (x * x) % n;

          if (x == n - 1)
          {
            witnessPassed = true;
            break;
          }
        }

        if (!witnessPassed)
          return false;
      }

      return true;
    }

    // ============================================================
    // Pocklington
    // ============================================================

    static bool Pocklington(BigInteger n)
    {
      /*
       * On cherche une factorisation connue de F,
       * facteur de n - 1, telle que F > sqrt(n).
       *
       * Pour notre nombre, n - 1 est factorisé.
       */
      BigInteger nMinusOne = n - 1;

      Dictionary<BigInteger, int> factors =
          FactorUsingSmallPrimes(nMinusOne);

      if (factors.Count == 0)
        return false;

      BigInteger F = BigInteger.One;

      foreach (var factor in factors)
      {
        BigInteger p = factor.Key;
        int exponent = factor.Value;

        for (int i = 0; i < exponent; i++)
          F *= p;
      }

      // Il faut F > sqrt(n).
      BigInteger sqrt = IntegerSqrt(n);

      if (F <= sqrt)
        return false;

      /*
       * Théorème de Pocklington :
       *
       * pour chaque facteur premier q de F,
       * il suffit de trouver a tel que :
       *
       * a^(n-1) = 1 mod n
       *
       * et
       *
       * gcd(a^((n-1)/q) - 1, n) = 1
       */
      foreach (var factor in factors)
      {
        BigInteger q = factor.Key;

        bool found = false;

        for (BigInteger a = 2; a < 1000; a++)
        {
          BigInteger first =
              BigInteger.ModPow(a, nMinusOne, n);

          if (first != 1)
            continue;

          BigInteger exponent = nMinusOne / q;

          BigInteger value =
              BigInteger.ModPow(a, exponent, n) - 1;

          BigInteger gcd =
              BigInteger.GreatestCommonDivisor(value, n);

          if (gcd == 1)
          {
            found = true;
            break;
          }
        }

        if (!found)
          return false;
      }

      return true;
    }

    // ============================================================
    // Factorisation partielle de n - 1
    // ============================================================

    static Dictionary<BigInteger, int> FactorUsingSmallPrimes(
        BigInteger n)
    {
      var result = new Dictionary<BigInteger, int>();

      foreach (int p in SmallPrimes)
      {
        int count = 0;

        while (n % p == 0)
        {
          n /= p;
          count++;
        }

        if (count > 0)
          result[p] = count;
      }

      /*
       * Si le reste est lui-même premier, on l'ajoute.
       *
       * Cette méthode est volontairement simple :
       * pour une vraie preuve générale, il faudrait
       * factoriser complètement une partie suffisante
       * de n-1 et prouver également la primalité de
       * chacun des facteurs utilisés.
       */
      if (n > 1 && IsSmallPrimeCandidate(n))
      {
        result[n] = result.GetValueOrDefault(n) + 1;
      }

      return result;
    }

    static bool IsSmallPrimeCandidate(BigInteger n)
    {
      if (n < 2)
        return false;

      foreach (int p in SmallPrimes)
      {
        if ((BigInteger)p * p > n)
          return true;

        if (n % p == 0)
          return n == p;
      }

      return false;
    }

    // ============================================================
    // Racine carrée entière
    // ============================================================

    static BigInteger IntegerSqrt(BigInteger n)
    {
      if (n < 0)
        throw new ArgumentException();

      if (n < 2)
        return n;

      BigInteger x0 = BigInteger.One <<
                      ((BitLength(n) + 1) / 2);

      BigInteger x1 =
          (x0 + n / x0) >> 1;

      while (x1 < x0)
      {
        x0 = x1;
        x1 = (x0 + n / x0) >> 1;
      }

      return x0;
    }

    static int BitLength(BigInteger n)
    {
      if (n.IsZero)
        return 0;

      byte[] bytes = n.ToByteArray(
          isUnsigned: true,
          isBigEndian: true);

      int bits = (bytes.Length - 1) * 8;

      byte first = bytes[0];

      while (first != 0)
      {
        bits++;
        first >>= 1;
      }

      return bits;
    }
  }
}