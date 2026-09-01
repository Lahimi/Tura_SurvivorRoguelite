using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class S02_PlayerController : MonoBehaviour
{
    [Header("Session Data")]
    public SO_GameSession gameSession; 
    
    [Header("Power-Up Registry")]
    [Tooltip("Reference to the power-up registry to find starting weapons")]
    public SO_PowerUpRegistry powerUpRegistry;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Character Settings")]
    [Tooltip("Set this to 'Link', 'Zelda', or 'Ezlo' in the inspector. Overwritten by JSON at runtime.")]
    public string characterName = "Link"; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private S04_HealthComponent healthComponent;
    private bool isControlDisabled = false;
    
    // Tracks direction: 0 = Front, 1 = Back
    private int lastVerticalDirection = 0; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthComponent = GetComponent<S04_HealthComponent>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (gameSession != null && gameSession.selectedCharacter != null)
        {
            moveSpeed = gameSession.selectedCharacter.stats.moveSpeed;
            characterName = gameSession.selectedCharacter.displayName;
        }
    }

    private void Start()
    {
        if (healthComponent != null)
        {
            healthComponent.OnDeath.AddListener(HandleDeath);
        }
        
        // Initialize starting weapon based on character
        InitializeStartingWeapon();
    }

    private void InitializeStartingWeapon()
    {
        if (gameSession == null || gameSession.selectedCharacter == null)
        {
            Debug.Log("[Player] No character selected, skipping starting weapon.");
            return;
        }
        
        string startingWeapon = gameSession.selectedCharacter.stats.startingWeapon;
        S11_PlayerPowerUps powerUpSystem = GetComponent<S11_PlayerPowerUps>();
        
        if (powerUpSystem == null)
        {
            Debug.LogWarning("[Player] S11_PlayerPowerUps not found!");
            return;
        }
        
        // Find the power-up by name from the registry
        SO_PowerUp startingPowerUp = GetPowerUpByName(startingWeapon);
        
        if (startingPowerUp != null)
        {
            Debug.Log($"[Player] {gameSession.selectedCharacter.displayName} starts with {startingWeapon}!");
            powerUpSystem.Apply(startingPowerUp);
        }
        else
        {
            Debug.LogWarning($"[Player] Could not find power-up: {startingWeapon}");
        }
    }
    
    private SO_PowerUp GetPowerUpByName(string weaponName)
    {
        if (powerUpRegistry == null || powerUpRegistry.allPowerUps == null)
        {
            Debug.LogWarning("[Player] Power-up registry not assigned or empty!");
            return null;
        }
        
        foreach (var powerUp in powerUpRegistry.allPowerUps)
        {
            if (powerUp != null && powerUp.displayName == weaponName)
            {
                return powerUp;
            }
        }
        
        return null;
    }

    private void Update()
    {
        if (isControlDisabled) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 0)
        {
            moveInput = moveInput.normalized;
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isControlDisabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimations()
    {
        string targetStateName;

        if (moveInput.sqrMagnitude > 0)
        {
            if (moveInput.y > 0) lastVerticalDirection = 1;
            else if (moveInput.y < 0) lastVerticalDirection = 0;

            if (lastVerticalDirection == 1) 
                targetStateName = characterName + "MoveBack";
            else 
                targetStateName = characterName + "MoveFront";
        }
        else
        {
            if (lastVerticalDirection == 1) 
                targetStateName = characterName + "IdleBack";
            else 
                targetStateName = characterName + "Idle";
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(targetStateName))
        {
            animator.Play(targetStateName);
        }
    }

    private void HandleDeath()
    {
        if (isControlDisabled) return;
        isControlDisabled = true;
        Debug.LogWarning($"[Gameplay] {gameObject.name} has fallen in battle!");
    }
}