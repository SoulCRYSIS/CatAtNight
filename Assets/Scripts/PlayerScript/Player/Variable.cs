using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class Player : MonoBehaviour, IDamagable
{
    [Header("Status")]

    [SerializeField]
    private float health = 9f;

    [SerializeField]
    private float maxHealth = 9f;

    [SerializeField]
    private float stamina = 100f;

    [SerializeField]
    private float maxStamina = 100f;

    [SerializeField]
    private float immortalDuration = 1f;

    [SerializeField]
    private int skillUnlockedCount = 1;

    [SerializeField]
    private bool isLoadSave = true;

    [Header("Power Values")]

    [SerializeField]
    public float maxVerticalVelocity = 15f;

    [SerializeField]
    public float maxFloatingVelocity = 150f;

    [SerializeField]
    public float moveSpeed = 6f;

    [SerializeField]
    private float floatSpeed = 10f;

    [SerializeField]
    private float runMultiplier = 2f;

    [SerializeField]
    private float staminaDrainRate = 20f;

    [SerializeField]
    private float staminaRegenRate = 10f;

    [SerializeField]
    private float staminaRegenDelay = 1f;

    [SerializeField]
    private float wallSlideSpeed = 3f;

    [SerializeField]
    private float wallClimbSpeed = 8f;

    [SerializeField]
    private float wallClimbStaminaDrain = 2f;

    [SerializeField]
    private float wallJumpSpeed = 12f;

    [SerializeField]
    private float wallJumpDuration = 0.3f;

    [SerializeField]
    private float wallJumpStaminaUsage = 10f;

    [SerializeField]
    private float runJumpStaminaUsage = 10f;

    [SerializeField]
    private float minimumStamina = 30f;

    [SerializeField]
    private float jumpPower = 8f;

    [SerializeField]
    private float chargeDuration = 0.75f;

    [SerializeField]
    private float dashDuration = 0.2f;

    [SerializeField]
    private float dashSpeed = 15f;

    [SerializeField]
    private int maxDashCount = 1;

    [SerializeField]
    private float groundedDelay = 0.1f;

    [Header("Gravity Values")]

    [SerializeField]
    private float gravityScale = 3f;

    [SerializeField]
    private float risingGravityMultiplier = 1f;

    [SerializeField]
    private float fallingGravityMultiplier = 1.5f;

    [Header("Collider Values")]

    [SerializeField]
    private Transform platformCheck;

    [SerializeField]
    private LayerMask platformLayer;

    [SerializeField]
    private Transform wallCheckPivot;

    [SerializeField]
    private Transform wallCheck;

    [SerializeField]
    private LayerMask wallLayer;

    [Header("Zone 1")]

    [SerializeField]
    private GameObject yarnBall;

    [SerializeField]
    private Vector3 facingRightOffset;

    [SerializeField]
    private Vector3 facingLeftOffset;

    private YarnBall pickedUpYarnBall;

    private Vector2 yarnBallVel;

    [SerializeField]
    private GameObject zone1;

    [SerializeField]
    private float yarnBallSmoothTime;

    [Header("Liquid Mode Values")]
    [SerializeField]
    private bool isLiquid = false;

    [SerializeField]
    private float l_moveSpeed = 25f;

    [SerializeField]
    private float l_jumpPower = 75f;

    [SerializeField]
    private Rigidbody2D[] bone_rigidbodies;

    [SerializeField]
    private Vector3[] bone_localPositions;

    [SerializeField]
    private Quaternion[] bone_localRotations;

    [SerializeField]
    private Transform l_platformCheck;

    [SerializeField]
    private float lastSetCameraFollowTime = 0f;

    [SerializeField]
    private float cameraFollowDelay = 0.15f;

    [SerializeField]
    private float l_dashDuration = 0.2f;

    [SerializeField]
    private float l_dashSpeed = 15f;

    [SerializeField]
    private Vector2 l_dashVelocity = Vector2.zero;

    [SerializeField]
    private float l_dashEndTime = 0f;

    [Header("Debug Values")]

    [SerializeField]
    List<Platform> passThroughPlatformList;

    [Header("--Input")]

    [SerializeField]
    private float horizontalInput = 0f;

    [SerializeField]
    private float verticalInput = 0f;

    [SerializeField]
    private float horizontalSmooth = 0f;

    [SerializeField]
    private float verticalSmooth = 0f;

    [Header("--States")]

    [SerializeField]
    private bool isOnSlope = false;

    [SerializeField]
    private bool isFacingRight = false;

    [SerializeField]
    private bool isFreeze = false;

    [SerializeField]
    private bool isInterrupted = false;

    [SerializeField]
    private float lastInterruptedTime = 0f;

    [SerializeField]
    private float interruptedDuration = 0f;

    [SerializeField]
    private bool isBouncing = false;

    [SerializeField]
    private float lastBounceTime = 0f;

    [SerializeField]
    private float bounceDuration = 0f;

    [SerializeField]
    private Vector2 bounceVelocity;

    [SerializeField]
    private float chargePercent = 0f;

    [SerializeField]
    private int dashCount = 0;

    [SerializeField]
    private float dashEndTime = 0f;

    [SerializeField]
    private Vector2 dashVelocity = Vector2.zero;

    [SerializeField]
    private Vector2 wallJumpVelocity = Vector2.zero;

    [SerializeField]
    private float wallJumpEndTime = 0f;

    [SerializeField]
    private bool isGrounded = true;

    [SerializeField]
    private float lastGroundedTime = 0f;

    [SerializeField]
    private bool isFloating = false;

    [SerializeField]
    private int hardPlatformCount = 0;

    [SerializeField]
    private bool isRayhitPlatform;

    [SerializeField]
    private bool isOnHardPlatform = false;

    [SerializeField]
    private bool isWalled = false;

    [SerializeField]
    private bool isChargeJumping = false;

    [SerializeField]
    private float lastRunningTime = 0f;

    [SerializeField]
    private bool isTryingToRun = false;

    [SerializeField]
    private bool isRunning = false;

    [SerializeField]
    private bool isWallSliding = false;

    [SerializeField]
    private bool isClimbingWall = false;

    [SerializeField]
    private bool isStaminaOut = false;

    [SerializeField]
    private bool noClip = false;

    [SerializeField]
    private float lastDamagedTime = 0f;

    [SerializeField]
    private float initialScaleY;

    [SerializeField]
    private Vector3 initialSpritePos;

    [Header("References")]

    [SerializeField]
    private Transform cameraFollowTransform;

    [SerializeField]
    private GameObject normalColliders;

    [SerializeField]
    private GameObject liquidForm;

    [SerializeField]
    private Rigidbody2D liquid_rb;

    private SpriteRenderer sprite;

    private PlayerInputActions playerInputActions;

    [SerializeField]
    private Animator animator;

    private Rigidbody2D normal_rb;

    private Rigidbody2D rb;

}