using UnityEngine;
using System.Collections;

public class WP_SwordWeapon : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private GameObject slashPrefab;

    [Header("Balancing Attributes")]
    [SerializeField] private float baseAttackInterval = 1.8f;
    [SerializeField] private float baseDamage = 22f;
    [SerializeField] private float slashDuration = 0.2f;
    [SerializeField] private float horizontalOffset = 1.5f;
    [SerializeField] private float delayBetweenSlashes = 0.1f;

    [Header("Four Sword Evolution")]
    [SerializeField] private float fourSwordAttackInterval = 0.25f; // How fast Four Sword attacks

    [Header("Evolution States")]
    public int weaponTier = 0;
    public bool isFourSwordUnlocked = false;

    private float _cooldownTimer;
    private Vector2 _lastFacingDirection = Vector2.right;
    private Coroutine _attackCoroutine;

    private void Update()
    {
        // Don't process if game is paused or no weapon tier
        if (Time.timeScale == 0f || weaponTier <= 0) return;

        if (isFourSwordUnlocked)
        {
            // Four Sword: Rapid-fire cardinal slashes
            _cooldownTimer += Time.deltaTime;
            if (_cooldownTimer >= fourSwordAttackInterval)
            {
                _cooldownTimer = 0f;
                TriggerFourSword();
            }
        }
        else
        {
            // Standard sword: Track facing direction and execute sequenced attacks
            TrackFacingDirection();
            _cooldownTimer += Time.deltaTime;
            if (_cooldownTimer >= baseAttackInterval)
            {
                _cooldownTimer = 0f;
                if (_attackCoroutine != null) StopCoroutine(_attackCoroutine);
                _attackCoroutine = StartCoroutine(ExecuteSequencedAttack());
            }
        }
    }

    private void TrackFacingDirection()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(inputX) > 0.1f) _lastFacingDirection = new Vector2(Mathf.Sign(inputX), 0f);
    }

    private IEnumerator ExecuteSequencedAttack()
    {
        Vector3 pos = transform.position;
        float lookX = _lastFacingDirection.x;

        if (weaponTier == 1)
        {
            // Tier 1: Single slash
            SpawnSlash(pos + new Vector3(lookX * horizontalOffset, 0, 0), lookX);
        }
        else if (weaponTier == 2)
        {
            // Tier 2: Double slash (left + right)
            SpawnSlash(pos + new Vector3(lookX * horizontalOffset, 0, 0), lookX);
            yield return new WaitForSeconds(delayBetweenSlashes);
            SpawnSlash(pos + new Vector3(-lookX * horizontalOffset, 0, 0), -lookX);
        }
        else if (weaponTier >= 3)
        {
            // Tier 3+: Diagonal cross slashes (up-left, down-right, etc.)
            SpawnSlash(pos + new Vector3(lookX * horizontalOffset, 0.2f, 0f), lookX);
            yield return new WaitForSeconds(delayBetweenSlashes);
            SpawnSlash(pos + new Vector3(-lookX * horizontalOffset, -0.2f, 0f), -lookX);
            yield return new WaitForSeconds(delayBetweenSlashes);
            SpawnSlash(pos + new Vector3(lookX * horizontalOffset, -0.2f, 0f), lookX);
            yield return new WaitForSeconds(delayBetweenSlashes);
            SpawnSlash(pos + new Vector3(-lookX * horizontalOffset, 0.2f, 0f), -lookX);
        }
    }

    private void TriggerFourSword()
    {
        Vector3 pos = transform.position;
        
        // Spawn slashes in all 4 cardinal directions: Up, Down, Left, Right
        // Up
        SpawnSlash(pos + new Vector3(0, horizontalOffset, 0), 1f, 1f, 0f, 90f);
        // Down
        SpawnSlash(pos + new Vector3(0, -horizontalOffset, 0), 1f, 1f, 0f, -90f);
        // Left
        SpawnSlash(pos + new Vector3(-horizontalOffset, 0, 0), -1f, 1f, 0f, 0f);
        // Right
        SpawnSlash(pos + new Vector3(horizontalOffset, 0, 0), 1f, 1f, 0f, 0f);
        
        Debug.Log("[FOUR SWORD] Cardinal slash attack executed!");
    }

    private void SpawnSlash(Vector3 pos, float dirX, float scale = 1f, float damageMult = 0f, float rotationZ = 0f)
    {
        if (slashPrefab == null)
        {
            Debug.LogError("[WP_SwordWeapon] Slash prefab is missing!");
            return;
        }

        GameObject slash = Instantiate(slashPrefab, pos, Quaternion.Euler(0, 0, rotationZ), transform);
        slash.transform.localScale = new Vector3(Mathf.Abs(slash.transform.localScale.x) * Mathf.Sign(dirX), slash.transform.localScale.y * scale, 1);
        
        if (slash.TryGetComponent<WP_SwordSlash>(out var comp))
        {
            comp.Initialize(baseDamage + damageMult, slashDuration, new Vector3(scale, scale, 1));
        }
    }

    /// <summary>
    /// Public method to force an immediate Four Sword attack (useful for testing)
    /// </summary>
    public void ForceFourSwordAttack()
    {
        if (isFourSwordUnlocked)
        {
            TriggerFourSword();
        }
    }

    /// <summary>
    /// Resets the cooldown timer (useful when evolving mid-combat)
    /// </summary>
    public void ResetCooldown()
    {
        _cooldownTimer = 0f;
    }
}