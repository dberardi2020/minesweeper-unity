public enum Difficulty { Beginner, Intermediate, Expert }

public static class DifficultyConfig
{
    public static (int rows, int cols, int mines) Get(Difficulty d) => d switch
    {
        Difficulty.Intermediate => (16, 16, 40),
        Difficulty.Expert       => (16, 30, 99),
        _                       => (9,  9,  10),
    };
}
