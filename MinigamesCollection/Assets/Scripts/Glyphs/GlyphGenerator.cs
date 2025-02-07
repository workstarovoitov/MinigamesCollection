using System;
using System.Collections.Generic;
using System.Linq;

public static class GlyphGenerator
{
    private static Random rng = new Random();
    private static string[] symbolsAll = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", " " , "'"};
    public static string[] SymbolsAll { get => symbolsAll; }
     
    public static List<int> GetRandomUniqueIndexes(int count, int range)
    {
        List<int> indexes = new List<int>();

        // Populate the list with values from 0 to range - 1
        for (int i = 0; i < range; i++)
        {
            indexes.Add(i);
        }

        // Shuffle the list
        Shuffle(indexes);

        // Take 'count' number of elements from the shuffled list
        List<int> randomIndexes = new List<int>();
        for (int i = 0; i < count; i++)
        {
            randomIndexes.Add(indexes[i]);
        }

        return randomIndexes;
    }
   
    public static List<string> GetRandomUniqueSymbols(int count, int range)
    {
        List<int> indexes = GetRandomUniqueIndexes(count, range);

        List<string> symbols = new List<string>();
        for (int i = 0; i < count; i++)
        {
            symbols.Add(symbolsAll[indexes[i]]);
        }

        return symbols;
    }
   
    // Fisher-Yates shuffle algorithm
    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<float> GetAngles(int count, RingsPlacementPattern hintPlacement, bool firstHintRandomPlaced)
    {
        List<float> angles = new List<float>();
        float angleStep = 360f / count;
        float sectorAngle = 360f / 8f;
        float angle = firstHintRandomPlaced ? UnityEngine.Random.Range(0f, sectorAngle) : 0f;

        for (int i = 0; i < count; i++)
        {
            switch (hintPlacement)
            {
                case RingsPlacementPattern.InSector:
                    angles.Add(i * sectorAngle + angle);
                    break;
                case RingsPlacementPattern.SameAngle:
                    angles.Add(i * angleStep + angle);
                    break;
                case RingsPlacementPattern.RandomAngle:
                    if (i == 0)
                    {
                        angles.Add(angle);
                    }
                    else
                    {
                        float remainingAngle = 360f - angle;
                        float maxStep = remainingAngle / (count - i + 1);
                        angle += UnityEngine.Random.Range(sectorAngle, maxStep);
                        angles.Add(angle);
                    }
                    break;
            }
        }

        if (hintPlacement == RingsPlacementPattern.InSector)
        {
            angles = angles.OrderBy(x => rng.Next()).ToList();
        }

        return angles;
    }
}
