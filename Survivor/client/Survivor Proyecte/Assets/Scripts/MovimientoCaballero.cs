using UnityEngine;

public class MovimientoCaballero : MonoBehaviour
{
    public float velocidad = 5f;

    [Header("Ataque y Efectos")]
    public Transform areaDaño;
    public Transform visualEfecto;
    public float distanciaAtaque = 0.5f;

    private Rigidbody2D rb;
    private Vector2 direccionMovimiento;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private Vector2 ultimaDireccion = Vector2.right;
    
    // NUEVA VARIABLE: El interruptor de la zona de daño
    private Collider2D colisionadorDaño; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (visualEfecto != null) visualEfecto.gameObject.SetActive(false);

        // Al arrancar, buscamos la zona invisible y LA APAGAMOS
        if (areaDaño != null)
        {
            colisionadorDaño = areaDaño.GetComponent<Collider2D>();
            if (colisionadorDaño != null) colisionadorDaño.enabled = false;
        }
    }

    void Update()
    {
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
            if (spriteEfecto != null)
            {
                if (ultimaDireccion.x < 0) spriteEfecto.flipY = true; 
                else spriteEfecto.flipY = false;
            }
        }

        if (ultimaDireccion.x < 0) spriteRenderer.flipX = true;
        else if (ultimaDireccion.x > 0) spriteRenderer.flipX = false;
    }

    void RealizarAtaque()
    {
        anim.SetTrigger("atacar");
        if (visualEfecto != null) StartCoroutine(MostrarEfecto());
    }

    System.Collections.IEnumerator MostrarEfecto()
    {
        // ¡ZAS! Encendemos la zona invisible para que corte y mostramos el dibujo
        if (colisionadorDaño != null) colisionadorDaño.enabled = true;
        visualEfecto.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.15f);

        // Se acaba el tiempo. Apagamos el dibujo y desactivamos la zona invisible
        visualEfecto.gameObject.SetActive(false);
        if (colisionadorDaño != null) colisionadorDaño.enabled = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direccionMovimiento * velocidad;
    }
}