namespace Aegis.Weapons
{
    /// <summary>Which side is allowed to use a weapon (GDD §9).</summary>
    public enum WeaponAvailability
    {
        Good, // AEGIS.2 only
        Evil, // X only
        Both  // shared
    }
}
