using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class EnemigoAgente : Agent
{
    [Header("Configuración Principal")]
    public Transform objetivo; 
    public float velocidadMovimiento = 5f;
    
    // Variables de ataque
    private float tiempoUltimoAtaque;
    public float velocidadAtaque = 1f; // Pega 1 hachazo por segundo
    
    // Componentes ocultos
    private Rigidbody2D rb;
    private Animator anim; 
    private SpriteRenderer spriteRenderer; 

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        
        rb.gravityScale = 0; 
        
        // Si no tiene objetivo, busca el Trono automáticamente
        if (objetivo == null) {
            GameObject t = GameObject.FindGameObjectWithTag("Trono");
            if (t != null) objetivo = t.transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        // El enemigo nace donde diga el Spawner, solo reiniciamos su velocidad física
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Seguro anti-destrucción: si el trono o el enemigo mueren, rellenamos con ceros para no dar error
        if (objetivo == null || this == null || gameObject == null) 
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            return;
        }

        // Observaciones reales
        Vector2 direccion = (objetivo.position - transform.position).normalized;
        sensor.AddObservation(direccion.x);
        sensor.AddObservation(direccion.y);

        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Limitamos la orden de la IA
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveY = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        
        rb.linearVelocity = new Vector2(moveX, moveY) * velocidadMovimiento;

        // --- ANIMACIONES Y GIRO ---
        if (anim != null) {
            anim.SetFloat("Velocidad", rb.linearVelocity.magnitude);
        }

        if (rb.linearVelocity.x > 0.1f) {
            spriteRenderer.flipX = false; // Mira a la derecha
        }
        else if (rb.linearVelocity.x < -0.1f) {
            spriteRenderer.flipX = true;  // Mira a la izquierda
        }

        float distancia = Vector2.Distance(transform.position, objetivo.position);
        if (distancia < 10f) {
            AddReward(0.001f); 
        } else {
            AddReward(-0.01f); 
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Usamos Stay para que ataque continuamente mientras lo toca
        if (collision.gameObject.CompareTag("Trono")) 
        {
            // 1. Activamos la animación de ataque
            if (anim != null) {
                anim.SetBool("Atacando", true);
            }

            // 2. Comprobamos si ya ha pasado 1 segundo desde el último hachazo
            if (Time.time >= tiempoUltimoAtaque + velocidadAtaque)
            {
                // Buscamos el script de VidaTrono y le quitamos 1 de vida
                VidaTrono scriptVida = collision.gameObject.GetComponent<VidaTrono>();
                if (scriptVida != null)
                {
                    scriptVida.RecibirDaño(1);
                }
                
                // Reiniciamos el reloj para el siguiente golpe
                tiempoUltimoAtaque = Time.time;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Si el orco es empujado y deja de tocar el trono, apagamos la animación
        if (collision.gameObject.CompareTag("Trono")) 
        {
            if (anim != null) {
                anim.SetBool("Atacando", false);
            }
        }
    }
}