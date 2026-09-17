using UnityEngine;
using TMPro;

/// <summary>Displays the live wagons-remaining count and a match-won banner, driven by
/// <see cref="MatchManager"/> events.</summary>
public sealed class DerbyHud : MonoBehaviour
{
    private const string WagonsRemainingLabelFormat = "Wagons Remaining: {0}";
    private const string MatchWonLabelFormat = "{0} Wins!";
    private const string NoSurvivorLabel = "Draw!";

    [Header("References")]
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private GameObject playerWagon;
    [SerializeField] private TextMeshProUGUI wagonsRemainingText;
    [SerializeField] private GameObject matchWonPanel;
    [SerializeField] private TextMeshProUGUI matchWonText;
    [SerializeField] private TextMeshProUGUI wagonHealthText;

    private void Awake()
    {
        if (matchWonPanel != null)
        {
            matchWonPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (matchManager == null)
        {
            return;
        }
        
        

        matchManager.WagonsRemainingChanged += OnWagonsRemainingChanged;
        matchManager.MatchWon += OnMatchWon;
        OnWagonsRemainingChanged(matchManager.WagonsRemaining);
    }

    private void OnDisable()
    {
        if (matchManager == null)
        {
            return;
        }

        matchManager.WagonsRemainingChanged -= OnWagonsRemainingChanged;
        matchManager.MatchWon -= OnMatchWon;
    }

    private void Update()
    {
        if (playerWagon != null)
        {
            WagonHealth wagonHealth = playerWagon.GetComponent<WagonHealth>();
            if (wagonHealth != null)
            {
                wagonHealthText.text = "Current health: " + wagonHealth.CurrentHealth.ToString("F0");//string.Format("Health: {0}/{1}", wagonHealth.CurrentHealth, wagonHealth.MaxHealth);
                //wagonHealth.HealthChanged += OnWagonHealthChanged;
                //OnWagonHealthChanged(wagonHealth.CurrentHealth);
            }
        }
    }

    /// <summary>Updates the wagons-remaining label. Wired to <see cref="MatchManager.WagonsRemainingChanged"/>.</summary>
    public void OnWagonsRemainingChanged(int remaining)
    {
        if (wagonsRemainingText != null)
        {
            wagonsRemainingText.text = string.Format(WagonsRemainingLabelFormat, remaining);
        }
    }

    /// <summary>Shows the match-won banner. Wired to <see cref="MatchManager.MatchWon"/>.</summary>
    public void OnMatchWon(WagonHealth winner)
    {
        if (matchWonPanel != null)
        {
            matchWonPanel.SetActive(true);
        }

        if (matchWonText != null)
        {
            matchWonText.text = winner != null
                ? string.Format(MatchWonLabelFormat, winner.name)
                : NoSurvivorLabel;
        }
    }
}
