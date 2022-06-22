using System;

public class Seed
{
    private readonly Random RandomGenerator;
    private readonly int SeedValue;

    public Seed()
    {
        SeedValue = Environment.TickCount; // Get a "random" number
        RandomGenerator = new Random(SeedValue);
    }
    public Seed(string serializedString)
    {
        SeedValue = ParseSeed(serializedString);
        RandomGenerator = new Random(SeedValue);
    }
    
    public override string ToString()
    {
        return SeedValue.ToString();
    }

    public int RandomInteger(int inclusiveLowerBounds, int exclusiveUpperBounds)
    {
        return RandomGenerator.Next(inclusiveLowerBounds, exclusiveUpperBounds);
    }

    private int ParseSeed(string serializedSeed)
    {
        if (!int.TryParse(serializedSeed, out int seedValue))
        {
            throw new ArgumentException("Seed was not valid.");
        }

        return seedValue;
    }
}
