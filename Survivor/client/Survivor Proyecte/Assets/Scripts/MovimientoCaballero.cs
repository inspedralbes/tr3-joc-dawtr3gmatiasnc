using UnityEngine;

public class MovimientoCaballero : MonoBehaviour
{
    public float velocidad = 5f;

    [Header("Ataque y Efectos")]
    public Transform areaDaño;
    public Transform visualEfecto;
    public float distanciaAtaque = 0.5f;

    [Header("WebSockets (Multijugador)")]
    public bool esMiJugador = true; 

    private Rigidbody2D rb;
    private Vector2 direccionMovimiento;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private Vector2 ultimaDireccion = Vector2.right;
    private Collider2D colisionadorDaño;

    void Start()
    {
        // Forcem ser el nostre jugador en cas que estiguem jugant al mode solitari (sense WebSockets)
        if (WebSocketManager.Instance == null) esMiJugador = true;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (visualEfecto != null) visualEfecto.gameObject.SetActive(false);
        if (areaDaño != null)
        {
            colisionadorDaño = areaDaño.GetComponent<Collider2D>();
            if (colisionadorDaño != null) colisionadorDaño.enabled = false;
        }

        if (esMiJugador)
        {
            SeguirJugador camara = Object.FindFirstObjectByType<SeguirJugador>();
            if (camara != null)
            {
                camara.objetivo = this.transform;
            }
            else
            {
                Debug.LogWarning("Avís: No s'ha trobat el script SeguirJugador a la càmera de l'escena.");
            }
        }
        else
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f); 
            rb.isKinematic = true; 
        }
    }

    void Update()
    {
        if (GameManager.Instancia != null && !GameManager.Instancia.partidaEmpezada) return;

        if (!esMiJugador)
        {
            ActualizarPosicionAtaque();
            return; 
        }

        float movX = Input.GetAxisRaw("Horizontal");
        float movY = Input.GetAxisRaw("Vertical");

        direccionMovimiento = new Vector2(movX, movY).normalized;

        if (direccionMovimiento.magnitude > 0)
        {
            ultimaDireccion = direccionMovimiento;
            anim.SetBool("caminando", true);
        }
        else
        {
            anim.SetBool("caminando", false);
        }

        ActualizarPosicionAtaque();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            RealizarAtaque();
            
            if (WebSocketManager.Instance != null)
            {
                WebSocketManager.Instance.SendAttack();
            }
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instancia != null && !GameManager.Instancia.partidaEmpezada)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!esMiJugador) return; 
        
        rb.linearVelocity = direccionMovimiento * velocidad;

        if (direccionMovimiento.magnitude > 0 && WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.SendMove(transform.position.x, transform.position.y, ultimaDireccion.x, ultimaDireccion.y);
        }
    }

    void ActualizarPosicionAtaque()
    {
        if (areaDaño == null) return;
        areaDaño.localPosition = ultimaDireccion * distanciaAtaque;

        float angulo = Mathf.Atan2(ultimaDireccion.y, ultimaDireccion.x) * Mathf.Rad2Deg;
        areaDaño.localRotation = Quaternion.Euler(0, 0, angulo);

        if (visualEfecto != null)
        {
            visualEfecto.localPosition = Vector3.zero;
            visualEfecto.localRotation = Quaternion.identity;

            SpriteRenderer spriteEfecto = visualEfecto.GetComponent<SpriteRenderer>();
            if (spriteEfecto != null) spriteEfecto.flipY = (ultimaDireccion.x < 0);
        }

        if (ultimaDireccion.x < 0) spriteRenderer.flipX = true;
        else if (ultimaDireccion.x > 0) spriteRenderer.flipX = false;
    }

    public void RealizarAtaque()
    {
        anim.SetTrigger("atacar");
        if (visualEfecto != null) StartCoroutine(MostrarEfecto());
    }

    System.Collections.IEnumerator MostrarEfecto()
    {
        if (colisionadorDaño != null) colisionadorDaño.enabled = true;
        visualEfecto.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        visualEfecto.gameObject.SetActive(false);
        if (colisionadorDaño != null) colisionadorDaño.enabled = false;
    }

    public void ActualizarPosicionDesdeServidor(Vector2 nuevaPosicion, Vector2 nuevaDireccion)
    {
        if (esMiJugador) return;

        transform.position = Vector3.Lerp(transform.position, nuevaPosicion, Time.deltaTime * 15f);
        ultimaDireccion = nuevaDireccion;
        
        anim.SetBool("caminando", (nuevaDireccion.magnitude > 0));
    }
}