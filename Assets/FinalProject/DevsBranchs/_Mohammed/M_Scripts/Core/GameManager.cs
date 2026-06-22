using System;
using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// Top-level application state machine (GDD §12). Holds the current
    /// <see cref="GameState"/> and broadcasts transitions via <see cref="StateChanged"/>
    /// so other systems can react without referencing this manager directly.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Tooltip("Automatically leave Boot and enter Main Menu on the first frame.")]
        [SerializeField] private bool _autoAdvanceFromBoot = true;

        /// <summary>The current application state.</summary>
        public GameState CurrentState { get; private set; } = GameState.Boot;

        /// <summary>Raised after every state change, with the new state.</summary>
        public event Action<GameState> StateChanged;

        private void Start()
        {
            if (_autoAdvanceFromBoot && CurrentState == GameState.Boot)
            {
                GoToMainMenu();
            }
        }

        /// <summary>Transition to <paramref name="newState"/> and notify listeners. No-op if already there.</summary>
        public void SetState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            CurrentState = newState;
            StateChanged?.Invoke(newState);
        }

        // Convenience transitions — readable call sites for menus, character select, etc.
        public void GoToMainMenu() => SetState(GameState.MainMenu);
        public void GoToCharacterSelect() => SetState(GameState.CharacterSelect);
        public void StartCampaign() => SetState(GameState.Campaign);
        public void GoToEnding() => SetState(GameState.Ending);
        public void GoToResults() => SetState(GameState.Results);
    }
}
