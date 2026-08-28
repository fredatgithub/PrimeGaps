using System.Text;

Action<string> display = Console.WriteLine;
Action<string> displayWithoutReturn = Console.Write;
display("Calcul du plus grand ecart entre deux nombres premiers");
/*
 *Gap	After	  Gap	After	  Gap	After	    Gap	After
    0	2	      33	1327	  117	1349533	  247	191912783
    1	3	      35	9551	  131	1357201	  249	387096133
    3	7	      43	15683	  147	2010733	  281	436273009
    5	23	    51	19609	  153	4652353	  287	1294268491
    7	89	    71	31397	  179	17051707	291	1453168141
    13	113	  85	155921	209	20831323	319	2300942549
    17	523	  95	360653	219	47326693	335	3842610773
    19	887	  111	370261	221	122164747	353	4302407359
    21	1129  113	492113	233	189695659	381	10_726_904_659 
 * 
 * */

/*
 * 
---- --------------------  ----------------------------
gap   following the prime  reference
---- ----------------------  ----------------------------
   0                      2
   1                      3
   3                      7
   5                     23
   7                     89
  13                    113
  17                    523
  19                    887
  21                   1129
  33                   1327
  35                   9551
  43                  15683
  51                  19609
  71                  31397
  85                 155921
  95                 360653
 111                 370261
 113                 492113
 117                1349533
 131                1357201
 147                2010733
 153                4652353
 179               17051707
 209               20831323
 219               47326693
 221              122164747
 233              189695659
 247              191912783
 249              387096133
 281              436273009
 287             1294268491
 291             1453168141
 319             2300942549
 335             3842610773
 353             4302407359
 381            10726904659
 383            20678048297
 393            22367084959
 455            25056082087
 463            42652618343
 467           127976334671
 473           182226896239
 485           241160624143
 489           297501075799
 499           303371455241
 513           304599508537
 515           416608695821
 531           461690510011
 533           614487453523
 539           738832927927
 581          1346294310749
 587          1408695493609
 601          1968188556461
 651          2614941710599
 673          7177162611713
 715         13829048559701  [YP89]
 765         19581334192423  [YP89]
 777         42842283925351  [YP89]
 803         90874329411493  [Nicely99]
 805        171231342420521  [Nicely99]
 905        218209405436543  [Nicely99]
 915       1189459969825483  [NN99]
 923       1686994940955803  [NN99]
1131       1693182318746371  [NN99]
1183      43841547845541059  [NN2002]
1197      55350776431903243  Tomás Oliveira e Silva
1219      80873624627234849  Tomás Oliveira e Silva
1223     203986478517455989  Tomás Oliveira e Silva
1247     218034721194214273  Tomás Oliveira e Silva 
1271     305405826521087869  Tomás Oliveira e Silva
1327     352521223451364323  Tomás Oliveira e Silva
1355     401429925999153707  Donald E. Knuth
1369     418032645936712127  Donald E. Knuth
1441     804212830686677669  Siegfried Herzog & Tomás Oliveira e Silva
1475    1425172824437699411  Tomás Oliveira e Silva
1487    5733241593241196731  Anand S. Nair
1509    6787988999657777797
1525   15570628755536096243
1529   17678654157568189057  Bertil Nyman
1549   18361375334787046697  Bertil Nyman
1551   18470057946260698231  Craig Loizides
1571   18571673432051830099  Craig Loizides
1675   20733746510561442863  Brian Kehrig
1723   68068810283234182907  Martin Raab & Brian Kehrig
1853  101412319996363309069  Robert Smith & Brian Kehrig 101_412_319_996_363_309_069
---- ----------------------  ----------------------------
(If you know of results beyond those in this table, please let us know.)
 * */

/*
*Gap	After	  Gap	After	  Gap	After	    Gap	After
0	2	      33	1327	  117	1349533	  247	191912783
1	3	      35	9551	  131	1357201	  249	387096133
3	7	      43	15683	  147	2010733	  281	436273009
5	23	    51	19609	  153	4652353	  287	1294268491
7	89	    71	31397	  179	17051707	291	1453168141
13	113	  85	155921	209	20831323	319	2300942549
17	523	  95	360653	219	47326693	335	3842610773
19	887	  111	370261	221	122164747	353	4302407359
21	1129  113	492113	233	189695659	381	10_726_904_659 
* 
* */

/*
 * 
---- --------------------  ----------------------------
gap   following the prime  reference
---- ----------------------  ----------------------------
   0                      2
   1                      3
   3                      7
   5                     23
   7                     89
  13                    113
  17                    523
  19                    887
  21                   1129
  33                   1327
  35                   9551
  43                  15683
  51                  19609
  71                  31397
  85                 155921
  95                 360653
 111                 370261
 113                 492113
 117                1349533
 131                1357201
 147                2010733
 153                4652353
 179               17051707
 209               20831323
 219               47326693
 221              122164747
 233              189695659
 247              191912783
 249              387096133
 281              436273009
 287             1294268491
 291             1453168141
 319             2300942549
 335             3842610773
 353             4302407359
 381            10726904659
 383            20678048297
 393            22367084959
 455            25056082087
 463            42652618343
 467           127976334671
 473           182226896239
 485           241160624143
 489           297501075799
 499           303371455241
 513           304599508537
 515           416608695821
 531           461690510011
 533           614487453523
 539           738832927927
 581          1346294310749
 587          1408695493609
 601          1968188556461
 651          2614941710599
 673          7177162611713
 715         13829048559701  [YP89]
 765         19581334192423  [YP89]
 777         42842283925351  [YP89]
 803         90874329411493  [Nicely99]
 805        171231342420521  [Nicely99]
 905        218209405436543  [Nicely99]
 915       1189459969825483  [NN99]
 923       1686994940955803  [NN99]
1131       1693182318746371  [NN99]
1183      43841547845541059  [NN2002]
1197      55350776431903243  Tomás Oliveira e Silva
1219      80873624627234849  Tomás Oliveira e Silva
1223     203986478517455989  Tomás Oliveira e Silva
1247     218034721194214273  Tomás Oliveira e Silva 
1271     305405826521087869  Tomás Oliveira e Silva
1327     352521223451364323  Tomás Oliveira e Silva
1355     401429925999153707  Donald E. Knuth
1369     418032645936712127  Donald E. Knuth
1441     804212830686677669  Siegfried Herzog & Tomás Oliveira e Silva
1475    1425172824437699411  Tomás Oliveira e Silva
1487    5733241593241196731  Anand S. Nair
1509    6787988999657777797
1525   15570628755536096243
1529   17678654157568189057  Bertil Nyman
1549   18361375334787046697  Bertil Nyman
1551   18470057946260698231  Craig Loizides
1571   18571673432051830099  Craig Loizides
1675   20733746510561442863  Brian Kehrig
1723   68068810283234182907  Martin Raab & Brian Kehrig
1853  101412319996363309069  Robert Smith & Brian Kehrig 101_412_319_996_363_309_069
---- ----------------------  ----------------------------
(If you know of results beyond those in this table, please let us know.)
 * */

const ulong maxLong = ulong.MaxValue; // 18_446_744_073_709_551_615
UInt128 maxUint128 = UInt128.MaxValue; // 340_282_366_920_938_463_463_374_607_431_768_211_455
UInt128 startNumber = maxLong; // last known prime gap of 1853, after the prime 101_412_319_996_363_309_069
UInt128 endNumber = 2147483647UL + 1_000_000; //
List<UInt128> primeNumbers = new();
List<string> primeGaps = new();
for (UInt128 i = startNumber; i < endNumber; i += 2)
{
  if (IsPrime(i))
  {
    displayWithoutReturn($"{i} ");
    primeNumbers.Add(i);
  }
}

// calculer le plus grand écart entre deux nombres premiers consécutifs
UInt128 maxGap = 0;
for (int i = 1; i < primeNumbers.Count; i++)
{
  UInt128 gap = primeNumbers[i] - primeNumbers[i - 1];
  if (gap > maxGap)
  {
    maxGap = gap;
    primeGaps.Add($"{primeNumbers[i - 1]} et {primeNumbers[i]} : {gap}");
  }
}

display("\n\nLes écarts entre deux nombres premiers consécutifs sont :");
for (int i = 0; i < primeGaps.Count; i++)
{
  display(primeGaps[i]);
  if (i < primeGaps.Count - 1)
  {
    displayWithoutReturn(" ");
  }
}

display($"\n\nLe plus grand écart entre deux nombres premiers consécutifs est : {maxGap}");
WriteToFile(primeGaps, "prime_gaps_Int128.txt");
display("\nAppuyez sur une touche pour terminer :");
Console.ReadKey();

static void WriteToFile(List<string> primeGaps, string filename)
{
  try
  {
    using StreamWriter sw = new(filename, false, Encoding.UTF8);
    foreach (string gap in primeGaps)
    {
      sw.WriteLine(gap);
    }
  }
  catch (Exception exception)
  {
    Console.WriteLine($"\nErreur lors de l'écriture dans le fichier {filename} : {exception.Message}");
  }
}

static bool IsPrime(UInt128 number)
{
  if (number <= 1)
  {
    return false;
  }

  if (number == 2 || number == 3 || number == 5)
  {
    // 2, 3 et 5 sont premiers
    return true;
  }

  if (number % 2 == 0)
  {
    // nombre pair, donc pas premier sauf 2
    return false;
  }

  if (number % 3 == 0)
  {
    // nombre divisible par 3, donc pas premier sauf 3
    return false;
  }

  if (number % 5 == 0)
  {
    // nombre divisible par 5, donc pas premier sauf 5
    return false;
  }

  UInt128 squareRoot = (UInt128)Math.Sqrt((double)number);
  for (UInt128 divisor = 7; divisor <= squareRoot; divisor += 2)
  {
    if (number % divisor == 0)
    {
      return false;
    }
  }

  return true;
}
