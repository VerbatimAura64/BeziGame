using System;
using System.Collections.Generic;
using UnityEngine;
using Harborview.GameTools;

/// <summary>Tracks every active wagon's health, keeps a live wagons-remaining count, and
/// declares the match over once at most one wagon survives.</summary>
public sealed class MatchManager : MonoBehaviour
{
    [Header("Match State")]
    [SerializeField] private GM gameManager;

    private readonly List<WagonHealth> registeredWagons = new List<WagonHealth>();

    /// <summary>Number of wagons that have not yet been eliminated.</summary>
    public int WagonsRemaining { get; private set; }

    /// <summary>Raised whenever <see cref="WagonsRemaining"/> changes.</summary>
    public event Action<int> WagonsRemainingChanged;

    /// <summary>Raised once, when the match ends, passing the last surviving wagon (or null if none survived).</summary>
    public event Action<WagonHealth> MatchWon;

    private void Awake()
    {
        WagonHealth[] wagonsInScene = FindObjectsByType<WagonHealth>(FindObjectsSortMode.None);
        foreach (WagonHealth wagon in wagonsInScene)
        {
            RegisterWagon(wagon);
        }
    }

    private void OnDestroy()
    {
        foreach (WagonHealth wagon in registeredWagons)
        {
            if (wagon != null)
            {
                wagon.Eliminated -= HandleWagonEliminated;
            }
        }
    }

    /// <summary>Adds a wagon to the tracked set and subscribes to its elimination event.</summary>
    public void RegisterWagon(WagonHealth wagon)
    {
        if (wagon == null || registeredWagons.Contains(wagon))
        {
            return;
        }

        registeredWagons.Add(wagon);
        wagon.Eliminated += HandleWagonEliminated;
        WagonsRemaining = registeredWagons.Count;
        WagonsRemainingChanged?.Invoke(WagonsRemaining);
    }

    /// <summary>Removes a wagon from the tracked set and unsubscribes from its elimination event.</summary>
    public void UnregisterWagon(WagonHealth wagon)
    {
        if (wagon == null || !registeredWagons.Remove(wagon))
        {
            return;
        }

        wagon.Eliminated -= HandleWagonEliminated;
        WagonsRemaining = registeredWagons.Count;
        WagonsRemainingChanged?.Invoke(WagonsRemaining);
    }

    private void HandleWagonEliminated(WagonHealth eliminatedWagon)
    {
        WagonsRemaining = Mathf.Max(0, WagonsRemaining - 1);
        WagonsRemainingChanged?.Invoke(WagonsRemaining);

        if (WagonsRemaining > 1)
        {
            return;
        }

        WagonHealth survivor = FindSurvivingWagon(eliminatedWagon);
        MatchWon?.Invoke(survivor);

        if (gameManager != null)
        {
            ((IGameState)gameManager).PauseGame();
        }
    }

    private WagonHealth FindSurvivingWagon(WagonHealth eliminatedWagon)
    {
        foreach (WagonHealth wagon in registeredWagons)
        {
            if (wagon != eliminatedWagon && wagon != null && !wagon.IsEliminated)
            {
                return wagon;
            }
        }

        return null;
    }
}
