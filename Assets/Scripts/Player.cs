using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour, IDamagable
{
    private const bool IS_DEBUG = true;

    [Header("Status")]
    [SerializeField] private float health = 9f;
    [SerializeField] private float maxHealth = 9f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [Header("Power Values")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float floatSpeed = 10f;
    [SerializeField] private float runMultiplier = 2f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenDelay = 1f;
    [SerializeField] private float runJumpStaminaUsage = 10f;
    [SerializeField] private float minimumStamina = 30f;
    [SerializeField] private float jumpPower = 8f;
    [SerializeField] private float chargeDuration = 0.75f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private int maxDashCount = 1;
    [SerializeField] private float groundedDelay = 0.1f;
    [Header("Gravity Values")]
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float risingGravityMultiplier = 1f;
    [SerializeField] private float fallingGravityMultiplier = 1.5f;

    [Header("Debug Values")]
    [SerializeField] private bool isFreeze = false;
    [SerializeField] private bool isInterrupted = false;
    [SerializeField] private float lastInterruptedTime = 0f;
    [SerializeField] private float interruptedDuration = 0f;
    [SerializeField] private bool isBouncing = false;
    [SerializeField] private float lastBounceTime = 0f;
    [SerializeField] private float bounceDuration = 0f;
    [SerializeField] private Vector2 bounceVelocity;
    [SerializeField] private float horizontalInput = 0f;
    [SerializeField] private float verticalInput = 0f;
    [SerializeField] private float horizontalSmooth = 0f;
    [SerializeField] private float verticalSmooth = 0f;
    [SerializeField] private float chargePercent = 0f;
    [SerializeField] private Vector2 dashVelocity = Vector2.zero;
    [SerializeField] private float dashEndTime = 0f;
    [SerializeField] private int dashCount = 0;
    [SerializeField] private float lastGroundedTime = 0f;
    [SerializeField] private int platformCount = 0;
    [SerializeField] private bool isOnPlatform = false;
    [SerializeField] private bool isChargeJumping = false;
    [SerializeField] private float lastRunningTime = 0f;
    [SerializeField] private bool isTryingToRun = false;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool isStaminaOut = false;
    [SerializeField] private bool noClip = false;

    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        health = maxHealth;
        stamina = maxStamina;
        StatusUIManager.Instance.UpdateHealthBar(health, maxHealth);
    }

    void Update()
    {
        UpdateStatus();
        if (isFreeze)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        ReadInput();
        UpdateSprite();
        UpdateStamina();
        if (IS_DEBUG && noClip)
        {
            transform.Translate(3f * moveSpeed * Time.deltaTime * new Vector3(horizontalSmooth, verticalSmooth, 0f));
            return;
        }
        if(isInterrupted)
        {
            UpdateInterrupted();
            return;
        }
        UpdateMovement();
        UpdateJumping();
        CountChargePercent();
    }

    private void UpdateStatus()
    {
        bool willFreeze = false;
        if(DialogueManager.Instance != null)
        {
            willFreeze |= DialogueManager.Instance.isDialogueActive;
        }
        isFreeze = willFreeze;
    }

    private void ReadInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalSmooth = Input.GetAxis("Horizontal");
        verticalSmooth = Input.GetAxis("Vertical");
        isTryingToRun = Input.GetKey(KeyCode.LeftControl);
        if (IS_DEBUG && Input.GetKeyDown(KeyCode.C))
        {
            noClip = !noClip;
            rb.bodyType = (noClip) ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            Color newColor = sprite.color;
            newColor.a = (noClip) ? 0.5f : 1f;
            sprite.color = newColor;
            rb.velocity = Vector2.zero;
        }
    }

    private void UpdateSprite()
    {
        if (horizontalInput != 0)
        {
            if (horizontalInput > 0)
            {
                sprite.flipX = true;
            }
            else
            {
                sprite.flipX = false;
            }
        }

        Vector3 newScale = sprite.transform.localScale;
        newScale.y = 0.25f * (1 - 0.25f / 100 * chargePercent);
        sprite.transform.localScale = newScale;
    }

    private void UpdateInterrupted()
    {
        UpdateGravity();
        float currentTime = Time.time;
        if(isBouncing)
        {
            if (currentTime - lastBounceTime > bounceDuration)
            {
                isBouncing = false;
            }
            else
            {
                rb.velocity = bounceVelocity;
            }
        }
        if (currentTime - lastInterruptedTime > interruptedDuration)
        {
            isInterrupted = false;
        }
    }

    private void UpdateStamina()
    {
        if(isRunning)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
        }
        else if(Time.time - lastRunningTime > staminaRegenDelay)
        {
            stamina += staminaRegenRate * Time.deltaTime;
        }

        if(stamina <= 0)
        {
            stamina = 0;
            isStaminaOut = true;
        }
        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }

        if(stamina >= minimumStamina)
        {
            isStaminaOut = false;
        }

        bool wasRunning = isRunning;

        if(isTryingToRun)
        {
            isRunning = !isStaminaOut;
        }
        else
        {
            isRunning = false;
        }

        if(wasRunning && !isRunning)
        {
            lastRunningTime = Time.time;
        }
    }

    private void UpdateMovement()
    {
        if (isChargeJumping)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
            return;
        }
        UpdateGravity();

        Vector2 newVelocity = rb.velocity;

        bool isGrounded = isOnPlatform && Mathf.Abs(rb.velocity.y) <= 0.1f; 
        float speed = (isGrounded) ? moveSpeed : floatSpeed;
        if(isRunning)
        {
            speed *= runMultiplier;
        }

        if (Time.time < dashEndTime)
        {
            newVelocity = dashVelocity;
            newVelocity.x += horizontalSmooth * speed;
        }
        else
        {
            
            newVelocity.x = horizontalSmooth * speed;
        }

        rb.velocity = newVelocity;
    }

    private void UpdateGravity()
    {
        rb.gravityScale = gravityScale * ((rb.velocity.y < 0) ? fallingGravityMultiplier : risingGravityMultiplier);
    }

    private void UpdateJumping()
    {

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
        {

            // RealJump
            if (isOnPlatform)
            {
                if(isRunning)
                {
                    Jump(false);
                    stamina -= runJumpStaminaUsage;
                }
                else
                {
                    isChargeJumping = true;
                }
            }
            else
            {
                if (Time.time - lastGroundedTime < groundedDelay) return;
                Dash();
            }
        }
        if (isChargeJumping && (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton0)))
        {
            Jump(true);
            chargePercent = 0;
            isChargeJumping = false;
        }
    }

    private void Jump(bool isChargeJump)
    {
        float jumpForce = jumpPower;
        if (isChargeJump)
        {
            jumpForce += chargePercent / 100 * jumpPower;
        }
        //Vector2 direction = (Vector2.up + Vector2.right * horizontalInput + Vector2.up * verticalInput).normalized;
        Vector2 direction = Vector2.up;
        rb.AddForce(jumpForce * direction, ForceMode2D.Impulse);
    }

    private void Dash()
    {
        if (dashCount >= maxDashCount) return;
        Vector2 direction = (new Vector2(horizontalInput, verticalInput)).normalized;
        dashVelocity = dashSpeed * direction;
        dashEndTime = Time.time + dashDuration;
        dashCount++;
    }

    private void CountChargePercent()
    {
        if (!isChargeJumping) return;

        chargePercent += (100f / chargeDuration) * Time.deltaTime;
        if (chargePercent > 100)
        {
            chargePercent = 100;
        }
    }

    private void AddPlatformCount(int count)
    {
        platformCount += count;
        SetOnPlatform(platformCount > 0);
    }

    private void SetOnPlatform(bool onPlatform)
    {
        if (isOnPlatform == onPlatform) return;
        isOnPlatform = onPlatform;
        if (isOnPlatform)
        {
            dashCount = 0;
        }
        else
        {
            lastGroundedTime = Time.time;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Platform>(out _))
        {
            AddPlatformCount(1);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Platform>(out _))
        {
            AddPlatformCount(-1);
        }
    }

    public EntityType GetEntityType()
    {
        return EntityType.Player;
    }

    public void RecieveDamage(DamageInfo damageInfo, Vector2 attackerPos)
    {
        health -= damageInfo.damage;
        if(health < 0)
        {
            health = 0;
        }
        SoundManager.TryPlayNew("TestCatHurt");
        StatusUIManager.Instance.UpdateHealthBar(health, maxHealth);
        if(damageInfo.isInterrupt)
        {
            isInterrupted = true;
            lastInterruptedTime = Time.time;
            interruptedDuration = damageInfo.interruptDuration;
        }
        if(damageInfo.isBounce)
        {
            isBouncing = true;
            lastBounceTime = Time.time;
            bounceDuration = damageInfo.bounceDuration;
            bounceVelocity = damageInfo.bounceSpeed * ((Vector2)transform.position - attackerPos).normalized;
        }
    }
}
