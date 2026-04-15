using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class EnemigoAgente : Agent
{
    public Transform objetivo; 
    public float velocidadMovimiento = 5f;
    private Rigidbody2D rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Por si acaso
        if (objetivo == null) {
            GameObject t = GameObject.FindGameObjectWithTag("Trono");
            if (t != null) objetivo = t.transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Spawneamos MUY CERCA para que aprenda rápido (radio de 3 metros)
        float offsetX = Random.Range(-3f, 3f);
        float offsetY = Random.Range(-3f, 3f);
        transform.position = objetivo.position + new Vector3(offsetX, offsetY, 0f);
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (objetivo == null) return;

        // Mandamos la dirección normalizada (valores entre -1 y 1 siempre)
        Vector2 direccion = (objetivo.position - transform.position).normalized;
        sensor.AddObservation(direccion.x);
        sensor.AddObservation(direccion.y);

        // Mandamos la velocidad (valores pequeños)
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Limitamos la orden de la IA para que no pueda dar saltos bruscos
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveY = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        
        rb.linearVelocity = new Vector2(moveX, moveY) * velocidadMovimiento;

        // Recompensa por estar cerca (esto ayuda a que no huya)
        float distancia = Vector2.Distance(transform.position, objetivo.position);
        if (distancia < 10f) {
            AddReward(0.001f); 
        } else {
            AddReward(-0.01f); // Castigo si se aleja demasiado
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trono")) 
        {
            AddReward(2.0f); // Gran premio
            EndEpisode();
        }
    }
}