using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Punto de aparición")]
    [SerializeField] private Transform puntoAparicion;

    [Header("Movimiento horizontal")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 35f;
    [SerializeField] private float deceleration = 45f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float fallGravityMultiplier = 1.5f;
    [SerializeField] private float jumpCutMultiplier = 2f;

    [Header("Deteccion del suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField]
    private Vector2 groundCheckSize = new Vector2(0.5f, 0.12f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Tolerancia del salto")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float coyoteCounter;
    private float jumpBufferCounter;

    private bool isGrounded;
    private bool jumpReleased;
    private bool jumpCutApplied;
    private bool jumpConsumed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
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
        var teclado = UnityEngine.InputSystem.Keyboard.current;

        horizontalInput = 0f;

        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed)
                horizontalInput -= 1f;

            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed)
                horizontalInput += 1f;
        }

        // Registrar la pulsacion de salto.
        if (teclado != null && teclado.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (teclado != null && teclado.spaceKey.wasReleasedThisFrame)
        {
            jumpReleased = true;
        }

        // Detectar el suelo usando exclusivamente las capas configuradas.
        isGrounded = groundCheck != null &&
            Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer
            ) != null;

        // Recuperar el salto al aterrizar.
        if (isGrounded)
        {
            jumpConsumed = false;
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        MoveHorizontally();
        HandleJump();
        AdjustGravity();
    }



    private void MoveHorizontally()
    {
        // Detectar la superficie sobre la que está el jugador.
        Collider2D suelo = groundCheck != null
            ? Physics2D.OverlapBox(
                groundCheck.position,
                groundCheckSize,
                0f,
                groundLayer)
            : null;

        // Obtener la velocidad de la plataforma móvil.
        float velocidadPlataforma = 0f;

        if (suelo != null)
        {
            PlataformaMovil plataforma =
                suelo.GetComponent<PlataformaMovil>();

            if (plataforma != null)
            {
                velocidadPlataforma = plataforma.VelocidadHorizontal;
            }
            else if (suelo.attachedRigidbody != null)
            {
                velocidadPlataforma =
                    suelo.attachedRigidbody.linearVelocity.x;
            }
        }

        // Calcular el movimiento relativo a la plataforma.
        float velocidadObjetivo = horizontalInput * moveSpeed;
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



    private void HandleJump()
    {
        // Solo permitir un salto por aterrizaje.
        if (jumpBufferCounter > 0f &&
            coyoteCounter > 0f &&
            !jumpConsumed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            jumpConsumed = true;
            jumpCutApplied = false;
        }

        // Soltar espacio antes del punto mas alto reduce el salto.
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

    private void AdjustGravity()
    {
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            groundCheck.position,
            groundCheckSize
        );
    }
}
