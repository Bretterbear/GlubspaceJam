using Godot;

namespace GlubspaceJam.Scripts;

public class Rand
{
    private static Rand _instance;
    private RandomNumberGenerator _randomNumberGenerator;

    private Rand()
    {
        _randomNumberGenerator = new RandomNumberGenerator();
        _randomNumberGenerator.Randomize();
    }

    public static Rand GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Rand();
        }

        return _instance;
    }

    public int RandiRange(int min, int max)
    {
        return _randomNumberGenerator.RandiRange(min, max);
    }
}