namespace Aegis.Core
{
    /// <summary>
    /// Top-level application states driven by <see cref="GameManager"/>.
    /// Mirrors the game flow in the GDD (§12): Boot → Main Menu → Character Select
    /// → Campaign → Ending → Results.
    /// </summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        CharacterSelect,
        Campaign,
        Ending,
        Results
    }
}
