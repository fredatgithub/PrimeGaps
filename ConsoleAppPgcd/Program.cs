Action<string> display = Console.WriteLine;
display("Calcul du PGCD de deux nombres");
int number1 = 48;
int number2 = 18;
display($"Calcul du PGCD de {number1} et {number2}");
int pgcd = CalculatePGCD(number1, number2);

int CalculatePGCD(int a, int b)
{
  a = Math.Abs(a);
  b = Math.Abs(b);

  while (b != 0)
  {
    int reste = a % b;
    a = b;
    b = reste;
  }

  return a;
}

display($"Le PGCD de {number1} et {number2} est : {pgcd}");

int startNumber = 1;
int endNumber = 100;
for (int i = startNumber; i < endNumber; i++)
{
  for (int j = startNumber; j < endNumber; j++)
  {
    if (i == j)
    {
      continue;
    }

    int currentPGCD = CalculatePGCD(i, j);
    if (currentPGCD == 1)
    {
      display($"Les nombres {i} et {j} sont premiers entre eux.");
    }
    else
    {
      //display($"Les nombres {i} et {j} ne sont pas premiers entre eux. PGCD = {currentPGCD}");
    }
  }
}

display("Appuyez sur une touche pour terminer :");
Console.ReadKey();
