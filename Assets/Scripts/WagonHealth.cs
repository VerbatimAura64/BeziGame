using System;
using UnityEngine;

/// <summary>Per-wagon health pool that takes collision-speed-based damage from other wagons and
/// raises an elimination event once health reaches zero.</summary>
public sealed class WagonHealth : MonoBehaviour
{
    /// <summary>Malbers Animator Controller "State" index that plays the Death state machine
    /// (see Horse AC V2.controller AnyState transition: State == DeathAnimatorStateIndex).</summary>
    private const int DeathAnimatorStateIndex = 10;
    private const string StateAnimatorParameter = "State";
    private const string StateOnAnimatorTrigger = "StateOn";

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Collision Damage")]
    [SerializeField] private float minImpactSpeedForDamage = 3f;
    [SerializeField] private float damagePerImpactSpeed = 6f;
    [SerializeField] private float invulnerabilityAfterHitSeconds = 0.4f;
    [SerializeField] private string[] damageDealingTags = { "Player", "Enemy" };

    private Animator wagonAnimator;
    private float invulnerabilityTimer;

    /// <summary>Current remaining health. Clamped between zero and <see cref="MaxHealth"/>.</summary>
    public float CurrentHealth { get; set; }

    /// <summary>Maximum health this wagon can have.</summary>
    public float MaxHealth => maxHealth;

    /// <summary>True once this wagon's health has reached zero and it has been eliminated.</summary>
    public bool IsEliminated { get; private set; }

    /// <summary>Raised once, the moment this wagon's health reaches zero.</summary>
    public event Action<WagonHealth> Eliminated;

    /// <summary>Raised whenever a non-zero amount of damage is applied, passing the amount applied.</summary>
    public event Action<WagonHealth, float> Damaged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        wagonAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsEliminated || invulnerabilityTimer > 0f)
        {
            return;
        }

        Rigidbody otherRigidbody = collision.collider.attachedRigidbody;
        string otherTag = otherRigidbody != null ? otherRigidbody.tag : collision.collider.tag;
        if (!IsDamageDealingTag(otherTag))
        {
            return;
        }

        float relativeSpeed = collision.relativeVelocity.magnitude;
        if (relativeSpeed < minImpactSpeedForDamage)
        {
            return;
        }

        float damage = (relativeSpeed - minImpactSpeedForDamage) * damagePerImpactSpeed;
        invulnerabilityTimer = invulnerabilityAfterHitSeconds;
        TakeDamage(damage);
    }

    /// <summary>Applies damage to this wagon, clamping health at zero. Ignored once eliminated.</summary>
    public void TakeDamage(float amount)
    {
        if (IsEliminated || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"WagonHealth: {gameObject.name} took {amount} damage, current health: {CurrentHealth}");
        Damaged?.Invoke(this, amount);

        if (CurrentHealth <= 0f)
        {
            IsEliminated = true;
            PlayDeathAnimation();
            Eliminated?.Invoke(this);
        }
    }

    private bool IsDamageDealingTag(string otherTag)
    {
        for (int i = 0; i < damageDealingTags.Length; i++)
        {
            if (damageDealingTags[i] == otherTag)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayDeathAnimation()
    {
        if (wagonAnimator == null)
        {
            return;
        }

        wagonAnimator.SetInteger(StateAnimatorParameter, DeathAnimatorStateIndex);
        wagonAnimator.SetTrigger(StateOnAnimatorTrigger);
    }
}
