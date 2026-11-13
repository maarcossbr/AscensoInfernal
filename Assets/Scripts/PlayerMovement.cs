using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Punto de aparicion")]
    [SerializeField] private Transform puntoAparicion;

    [Header("Movimiento horizontal")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 45f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 11.5f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float fallGravityMultiplier = 1.5f;
    [SerializeField] private float jumpCutMultiplier = 2f;

    [Header("Deteccion del suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField]
    private Vector2 groundCheckSize = new Vector2(0.65f, 0.12f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Deteccion de paredes")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;

    [SerializeField] private Vector2 wallCheckSize =
    new Vector2(0.15f, 0.8f);

    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpVerticalForce = 12f;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallJumpLockDuration = 0.15f;

    private float wallJumpLockTimer;
    private bool isWallJumping;

    [Header("Tolerancia del salto")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Doble salto")]
    [SerializeField] private int maxJumps = 2;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.15f;

    private Rigidbody2D rb;

    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;

    // Variables del dash.
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection = 1f;

    private bool isGrounded;
    private bool jumpReleased;
    private bool jumpCutApplied;

    // Indica si el jugador esta tocando una pared.
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;

    // Doble salto.
    private bool hasUsedFirstJump;
    private int airJumpsRemaining;

    // Control del dash.
    private bool isDashing;
    private bool canDash = true;
    private bool dashRequested;

    // Ultima direccion horizontal del personaje.
    private float lastDirection = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;

        maxJumps = Mathf.Max(1, maxJumps);
        airJumpsRemaining = maxJumps - 1;
    }

    private void Start()
    {
        if (puntoAparicion != null)
        {
            rb.position = new Vector2(
                puntoAparicion.position.x,
                puntoAparicion.position.y
            );

            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        ReadMovementInput();
        ReadJumpInput();
        ReadDashInput();
        CheckGround();
        CheckWalls();

        // Actualizar el tiempo de espera entre dashes.
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void ReadMovementInput()
    {
        Keyboard teclado = Keyboard.current;
        Gamepad mando = Gamepad.current;

        horizontalInput = 0f;

        if (teclado == null && mando == null)
            return;

        // TECLADO
        if (teclado != null)
        {
            if (teclado.aKey.isPressed ||
                teclado.leftArrowKey.isPressed)
            {
                horizontalInput -= 1f;
            }

            if (teclado.dKey.isPressed ||
                teclado.rightArrowKey.isPressed)
            {
                horizontalInput += 1f;
            }
        }
        
        // MANDO
        if (mando != null)
        {
            // lectura stick izq
            float stick = mando.leftStick.x.ReadValue();

            // IMPORTANTE
            // ¡¡¡¡¡ Para pruebas quitar en realeases !!!!!
            if(mando.buttonNorth.isPressed &&
               mando.dpad.up.isPressed) Start();

            // lectura d-pad
            if (mando.dpad.left.isPressed) stick = -1f;
            if (mando.dpad.right.isPressed) stick = 1f;

            if (Mathf.Abs(stick) > 0.2f) // Zona muerta (deadzone) básica
            {
                horizontalInput = stick;
            }
        }
        

        // Guardar la ultima direccion de movimiento.
        if (horizontalInput != 0f)
        {
            lastDirection = Mathf.Sign(horizontalInput);
        }
    }

    private void ReadJumpInput()
    {
        Keyboard teclado = Keyboard.current;
        Gamepad mando = Gamepad.current;
        
        bool jumpPressedFrame = false;
        bool jumpRealeasedFrame = false;
        // Detectar cuando pulsamos ESP o el boton 'sur' del mando.
        if (teclado != null &&
            teclado.spaceKey.wasPressedThisFrame)
        {
            jumpPressedFrame = true;
        }
        if (mando != null &&
            mando.buttonSouth.wasPressedThisFrame)
        {
            jumpPressedFrame = true;
        }

        if(jumpPressedFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter = Mathf.Max(
                0f,
                jumpBufferCounter - Time.deltaTime
            );
        }

        // Detectar cuando soltamos ESP o boton 'sur' del mando.
        if (teclado != null &&
            teclado.spaceKey.wasReleasedThisFrame)
        {
            jumpRealeasedFrame = true;
        }
        if (mando != null &&
            mando.buttonSouth.wasReleasedThisFrame)
        {
            jumpRealeasedFrame = true;
        }

        if (jumpRealeasedFrame) jumpReleased = true;
    }

    private void ReadDashInput()
    {
        Keyboard teclado = Keyboard.current;
        Gamepad mando = Gamepad.current;

        bool dashPressed = false;

        if (teclado != null &&
            (teclado.leftShiftKey.wasPressedThisFrame ||
             teclado.rightShiftKey.wasPressedThisFrame))
        {
            dashPressed = true;
        }
        if (mando != null && 
            (mando.buttonWest.wasPressedThisFrame))
        {
            dashPressed = true;
        }

        if (dashPressed) dashRequested = true;
    }


    private void CheckGround()
    {
        Collider2D suelo = groundCheck != null
            ? Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            )
            : null;

        float velocidadVerticalSuelo = 0f;

        if (suelo != null)
        {
            PlataformaMovil plataforma =
                suelo.GetComponent<PlataformaMovil>();

            if (plataforma != null)
            {
                velocidadVerticalSuelo =
                    plataforma.VelocidadVertical;
            }
            else if (suelo.attachedRigidbody != null)
            {
                velocidadVerticalSuelo =
                    suelo.attachedRigidbody.linearVelocity.y;
            }
        }

        bool touchingGround = suelo != null;

        isGrounded = touchingGround &&
            rb.linearVelocity.y - velocidadVerticalSuelo <= 0.01f;

        // Recuperar saltos y dash al aterrizar.
        // No recuperar durante un dash.
        if (isGrounded && !isDashing)
        {
            hasUsedFirstJump = false;
            airJumpsRemaining = maxJumps - 1;

            canDash = true;
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter = Mathf.Max(
                0f,
                coyoteCounter - Time.deltaTime
            );
        }
    }

    private void CheckWalls()
    {
        // Comprobar si hay una pared a la izquierda.
        isTouchingWallLeft = wallCheckLeft != null &&
            Physics2D.OverlapBox(
                wallCheckLeft.position,
                wallCheckSize,
                0f,
                wallLayer
            ) != null;

        // Comprobar si hay una pared a la derecha.
        isTouchingWallRight = wallCheckRight != null &&
            Physics2D.OverlapBox(
                wallCheckRight.position,
                wallCheckSize,
                0f,
                wallLayer
            ) != null;
    }

    private void FixedUpdate()
    {
        // Si estamos haciendo dash, tiene prioridad sobre
        // el movimiento y las fisicas normales.
        if (isDashing)
        {
            UpdateDash();
            dashRequested = false;
            return;
        }

        // Procesar una solicitud de dash.
        if (dashRequested)
        {
            TryStartDash();
            dashRequested = false;

            if (isDashing)
                return;
        }

        MoveHorizontally();
        HandleJump();
        AdjustGravity();
    }

    private bool TryWallJump()
    {
        // No podemos hacer Wall Jump estando en el suelo.
        if (isGrounded)
            return false;

        // Comprobamos si estamos tocando alguna pared.
        if (!isTouchingWallLeft && !isTouchingWallRight)
            return false;

        // Determinamos hacia dónde debemos impulsarnos.
        float direction = 0f;

        if (isTouchingWallLeft)
            direction = 1f;
        else if (isTouchingWallRight)
            direction = -1f;

        // Aplicamos el impulso vertical y horizontal.
        rb.linearVelocity = new Vector2(
            direction * wallJumpHorizontalForce,
            wallJumpVerticalForce
        );

        // Activamos el bloqueo temporal del movimiento horizontal.
        isWallJumping = true;
        wallJumpLockTimer = wallJumpLockDuration;

        // Actualizamos la última dirección del jugador.
        lastDirection = direction;

        return true;
    }

    // ------------------------------------------------
    // MOVIMIENTO HORIZONTAL
    // ------------------------------------------------

    private void MoveHorizontally()
    {
        // Durante el Wall Jump bloqueamos el control horizontal.
        if (wallJumpLockTimer > 0f)
        {
            wallJumpLockTimer -= Time.fixedDeltaTime;
            return;
        }

        isWallJumping = false;

        
        Collider2D suelo = groundCheck != null
            ? Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            )
            : null;

        // Velocidad heredada de las plataformas moviles.
        float velocidadPlataforma = 0f;

        if (suelo != null)
        {
            PlataformaMovil plataforma =
                suelo.GetComponent<PlataformaMovil>();

            if (plataforma != null)
            {
                velocidadPlataforma =
                    plataforma.VelocidadHorizontal;
            }
            else if (suelo.attachedRigidbody != null)
            {
                velocidadPlataforma =
                    suelo.attachedRigidbody.linearVelocity.x;
            }
        }

        float velocidadObjetivo =
            horizontalInput * moveSpeed;

        float velocidadRelativa =
            rb.linearVelocity.x - velocidadPlataforma;

        float tasa = Mathf.Abs(horizontalInput) > 0.01f
            ? acceleration
            : deceleration;

        float nuevaVelocidad = Mathf.MoveTowards(
            velocidadRelativa,
            velocidadObjetivo,
            tasa * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            nuevaVelocidad + velocidadPlataforma,
            rb.linearVelocity.y
        );
    }

    // ------------------------------------------------
    // DOBLE SALTO
    // ------------------------------------------------

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f)
        {
            // Primero intentamos realizar un Wall Jump.
            if (TryWallJump())
            {
                jumpBufferCounter = 0f;
                jumpReleased = false;
                jumpCutApplied = false;

                return;
            }
            bool canFirstJump =
                !hasUsedFirstJump && coyoteCounter > 0f;

            bool canDoubleJump =
                hasUsedFirstJump && airJumpsRemaining > 0;

            if (canFirstJump)
            {
                PerformJump(jumpForce);

                hasUsedFirstJump = true;
                coyoteCounter = 0f;
                jumpBufferCounter = 0f;
            }
            else if (canDoubleJump)
            {
                PerformJump(doubleJumpForce);

                airJumpsRemaining--;
                jumpBufferCounter = 0f;
            }
        }

        // Control de altura del salto.
        if (jumpReleased &&
            !jumpCutApplied &&
            rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y / jumpCutMultiplier
            );

            jumpCutApplied = true;
        }

        jumpReleased = false;
    }

    private void PerformJump(float force)
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            force
        );

        jumpCutApplied = false;
    }

    // ------------------------------------------------
    // DASH
    // ------------------------------------------------

    private void TryStartDash()
    {
        // Comprobar que podemos realizar otro dash.
        if (!canDash || dashCooldownTimer > 0f)
            return;

        // Establecer la direccion del dash.
        dashDirection = lastDirection;

        // Activar el dash.
        isDashing = true;
        canDash = false;

        // Iniciar el temporizador.
        dashTimer = dashDuration;

        // Desactivar temporalmente la gravedad.
        rb.gravityScale = 0f;

        // El dash es horizontal: anulamos la velocidad vertical.
        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );
    }

    private void UpdateDash()
    {
        dashTimer -= Time.fixedDeltaTime;

        // Mantener la velocidad durante el dash.
        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );

        // Finalizar cuando se agote la duracion.
        if (dashTimer <= 0f)
        {
            StopDash();
        }
    }

    private void StopDash()
    {
        isDashing = false;

        // Restaurar la gravedad.
        rb.gravityScale = gravityScale;

        // Iniciar el tiempo de recuperacion.
        dashCooldownTimer = dashCooldown;
    }

    // ------------------------------------------------
    // GRAVEDAD
    // ------------------------------------------------

    private void AdjustGravity()
    {
        if (isDashing)
            return;

        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale =
                gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    // ------------------------------------------------
    // VISUALIZACION DEL DETECTOR DE SUELO
    // ------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        // Detector del suelo.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            groundCheck.position,
            groundCheckSize
        );

        // Detector de la pared izquierda.
        if (wallCheckLeft != null)
        {
            Gizmos.color = isTouchingWallLeft ? Color.red : Color.green;

            Gizmos.DrawWireCube(
                wallCheckLeft.position,
                wallCheckSize
            );
        }

        // Detector de la pared derecha.
        if (wallCheckRight != null)
        {
            Gizmos.color = isTouchingWallRight ? Color.red : Color.cyan;

            Gizmos.DrawWireCube(
                wallCheckRight.position,
                wallCheckSize
            );
        }
    }
}
