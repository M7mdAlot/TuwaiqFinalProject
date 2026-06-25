using Aegis.Core;

namespace Aegis.Enemies
{
    /// <summary>
    /// Decides who is hostile to whom (GDD §3 — robots tell friend from foe by the
    /// shoulder insignia, which is mechanically a <see cref="Faction"/>). This is a
    /// static rulebook: call <see cref="AreHostile"/> from anywhere, no setup needed.
    /// Tier 1 — depends only on the Faction enum (Tier 0).
    /// </summary>
    public static class FactionSystem
    {
        /// <summary>True if the two factions should fight each other.</summary>
        public static bool AreHostile(Faction a, Faction b)
        {
            if (a == Faction.Neutral || b == Faction.Neutral) return false;
            if (a == b) return false; // same faction = allies

            // Symmetric: hostile if either side considers the other an enemy.
            return ConsidersEnemy(a, b) || ConsidersEnemy(b, a);
        }

        private static bool ConsidersEnemy(Faction self, Faction other)
        {
            switch (self)
            {
                // AEGIS.2 and friends fight the corrupted robots; the military guns them down.
                case Faction.Aegis: return other == Faction.Corrupted || other == Faction.Military;

                // Corrupted/rogue robots (incl. X) attack humans and AEGIS.2 alike.
                case Faction.Corrupted: return other == Faction.Aegis || other == Faction.Human || other == Faction.Military;

                // Humans fight back against the rogue robots.
                case Faction.Human: return other == Faction.Corrupted;

                // The military destroys any robot at the entrance.
                case Faction.Military: return other == Faction.Aegis || other == Faction.Corrupted;

                default: return false;
            }
        }
    }
}
