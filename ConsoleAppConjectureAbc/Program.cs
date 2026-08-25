using System;

namespace ConsoleAppConjectureAbc
{
  internal class Program
  {
    static void Main()
    {
      Action<string> display = Console.WriteLine;
      display("Recherche des nombres satisfaisant la conjecture ABC");
      // La conjecture est formulée en termes de trois nombres entiers positifs, a, b et c (d'où son nom), qui n'ont aucun facteur commun et satisfont à a + b = c
      // Si d est le produit des facteurs premiers distincts de abc, alors la conjecture affirme à peu près que d ne peut pas être beaucoup plus petit que c
      // https://fr.wikipedia.org/wiki/Conjecture_abc



      display("Appuyez sur une touche pour terminer :");
      Console.ReadKey();
    }
  }
}
