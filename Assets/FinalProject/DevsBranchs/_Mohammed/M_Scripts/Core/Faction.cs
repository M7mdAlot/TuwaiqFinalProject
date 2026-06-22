namespace Aegis.Core
{
    /// <summary>
    /// Groups used to decide who is hostile to whom (resolved by FactionSystem in Tier 1).
    /// Robots identify friend/foe by the shoulder insignia (GDD §3).
    /// </summary>
    public enum Faction
    {
        Aegis,      // AEGIS.2 and friendly units
        Corrupted,  // corrupted AEGIS robots (AEGIS.2's enemies)
        Human,      // lab staff / security (X's enemies)
        Military,   // soldiers at the entrance (ending only)
        Neutral
    }
}
