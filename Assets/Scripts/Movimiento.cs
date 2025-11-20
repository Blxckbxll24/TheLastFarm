using UnityEngine;

public class Movimiento : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //haz el  codigo de movimiento con las teclas w a s d tambien saltar 
    public float velocidad = 5f;
    public float fuerzaSalto = 1f;
    private bool enSuelo;
    private Rigidbody rb;
    void Start()
    {
        enSuelo = true;
        rb = GetComponent<Rigidbody>();
        
        // Si no hay Rigidbody, agregar uno automáticamente
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }
    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal"); // A y D o flechas izquierda y derecha
        float movimientoVertical = Input.GetAxis("Vertical"); // W y S o flechas arriba y abajo

        Vector3 movimiento = new Vector3(movimientoHorizontal, 0f, movimientoVertical) * velocidad * Time.deltaTime;
        transform.Translate(movimiento);

        if (Input.GetKeyDown(KeyCode.Space) && enSuelo && rb != null)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    // Detectar cuando toca el suelo
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = true;
        }
    }

    // También puedes usar este método si tienes un tag "Ground" en el suelo
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            enSuelo = true;
        }
    }
}
