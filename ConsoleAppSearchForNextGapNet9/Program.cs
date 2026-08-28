using System.Numerics;

namespace ConsoleAppSearchForNextGapNet9
{
  internal class Program
  {
    static void Main()
    {
      BigInteger start = BigInteger.Parse("101412319996363309069");

      Console.WriteLine($"Nombre de départ : {start:N0}");
      Console.WriteLine("Recherche des deux nombres premiers suivants...");
      Console.WriteLine();

      BigInteger n = start + 1;
      int found = 0;

      while (found < 2)
      {
        if (IsPrime(n))
        {
          found++;

          Console.WriteLine($"{found}. {n}");
          Console.WriteLine($"   Écart : {n - start}");
          Console.WriteLine($"   Vérification : {IsPrime(n)}");
          Console.WriteLine();
        }

        n++;
      }
    }

    /// <summary>
    /// Test de primalité déterministe pour les entiers
    /// de la taille utilisée ici.
    ///
    /// On combine :
    ///  - divisions par de petits nombres premiers
    ///  - Miller-Rabin avec plusieurs bases
    /// </summary>
    private static bool IsPrime(BigInteger n)
    {
      if (n < 2)
        return false;

      // Petits nombres premiers.
      int[] smallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };

      foreach (int p in smallPrimes)
      {
        if (n == p)
          return true;

        if (n % p == 0)
          return false;
      }

      // Écrit n - 1 = d * 2^s
      BigInteger d = n - 1;
      int s = 0;

      while (d.IsEven)
      {
        d >>= 1;
        s++;
      }

      /*
       * Bases de Miller-Rabin.
       *
       * Ton nombre est inférieur à 2^67 environ.
       *
       * Ces bases donnent une vérification très robuste
       * pour cette plage. Pour une preuve formelle de
       * primalité, on pourrait ensuite utiliser une méthode
       * comme Pocklington/ECPP.
       */
      BigInteger[] bases = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };

      foreach (BigInteger a in bases)
      {
        if (a >= n)
          continue;

        if (!MillerRabinWitness(a, n, d, s))
          return false;
      }

      return true;
    }

    private static bool MillerRabinWitness(
        BigInteger a,
        BigInteger n,
        BigInteger d,
        int s)
    {
      BigInteger x = BigInteger.ModPow(a, d, n);

      if (x == 1 || x == n - 1)
        return true;

      for (int r = 1; r < s; r++)
      {
        x = (x * x) % n;

        if (x == n - 1)
          return true;

        if (x == 1)
          return false;
      }

      return false;
    }
  }
}
