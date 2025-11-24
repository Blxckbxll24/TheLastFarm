using UnityEngine;

/// <summary>
/// Script para las zanahorias que se pueden recolectar
/// Otorga monedas cuando el jugador las toca
/// </summary>
public class Zanahoria : MonoBehaviour
{
    [Header("💰 CONFIGURACIÓN")]
    [SerializeField] private int valor = 1; // Monedas que da la zanahoria
    [SerializeField] private bool mostrarDebug = false; // REDUCIDO DEBUG
    [SerializeField] private bool destruirAlRecolectar = true;
    [SerializeField] private string tagJugador = "Player";
    
    [Header("🎬 EFECTOS VISUALES")]
    [SerializeField] private float tiempoDestruccion = 0.1f;
    [SerializeField] private bool efectoRecoleccion = true;
    [SerializeField] private AudioClip sonidoRecoleccion;
    
    // Variables internas
    private bool yaRecolectada = false;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    void Start()
    {
        // Obtener componentes
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Configurar audio source si no existe
        if (audioSource == null && sonidoRecoleccion != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = sonidoRecoleccion;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
        
        // Verificar configuración
        if (valor <= 0)
        {
            valor = 1;
        }
        
        // Configurar para que se destruya automáticamente después de un tiempo
        Invoke("DestruirPorTiempo", 30f); // 30 segundos máximo en escena
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Evitar recolección múltiple
        if (yaRecolectada) return;
        
        // Verificar si es el jugador
        bool esJugador = false;
        
        // Verificar por tag
        if (other.CompareTag(tagJugador))
        {
            esJugador = true;
        }
        // Verificar por script MovimientoJugador
        else if (other.GetComponent<MovimientoJugador>() != null)
        {
            esJugador = true;
        }
        
        if (esJugador)
        {
            RecolectarZanahoria();
        }
    }
    
    private void RecolectarZanahoria()
    {
        if (yaRecolectada) return;
        yaRecolectada = true;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🥕 ZANAHORIA RECOLECTADA: +{valor} monedas");
        }
        
        // Agregar monedas al sistema
        SistemaMonedas sistemaMonedas = SistemaMonedas.GetInstancia();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.AgregarMonedas(valor);
        }
        else
        {
            // Backup: usar métodos estáticos
            SistemaMonedas.AgregarMonedasStatic(valor);
            
            if (mostrarDebug)
            {
                Debug.LogError("💰 BACKUP: Monedas agregadas vía método estático");
            }
        }
        
        // Reproducir sonido
        if (audioSource != null && sonidoRecoleccion != null)
        {
            audioSource.Play();
        }
        
        // Efectos visuales
        if (efectoRecoleccion)
        {
            EjecutarEfectoRecoleccion();
        }
        
        // Destruir la zanahoria
        if (destruirAlRecolectar)
        {
            // Si hay sonido, esperar a que termine
            float tiempoEspera = (sonidoRecoleccion != null) ? Mathf.Min(sonidoRecoleccion.length, 1f) : tiempoDestruccion;
            Destroy(gameObject, tiempoEspera);
        }
    }
    
    private void EjecutarEfectoRecoleccion()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(EfectoDesvanecimiento());
        }
    }
    
    private System.Collections.IEnumerator EfectoDesvanecimiento()
    {
        Color colorInicial = spriteRenderer.color;
        float tiempo = 0f;
        float duracion = 0.3f;
        
        // Efecto de escala creciente y desvanecimiento
        Vector3 escalaInicial = transform.localScale;
        Vector3 escalaFinal = escalaInicial * 1.5f;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / duracion;
            
            // Desvanecer alpha
            Color nuevoColor = colorInicial;
            nuevoColor.a = Mathf.Lerp(1f, 0f, progreso);
            spriteRenderer.color = nuevoColor;
            
            // Aumentar escala
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progreso);
            
            yield return null;
        }
    }
    
    // Método para destruir por tiempo límite
    private void DestruirPorTiempo()
    {
        if (!yaRecolectada)
        {
            if (mostrarDebug)
            {
                Debug.LogError($"⏰ ZANAHORIA {name} DESTRUIDA POR TIEMPO LÍMITE");
            }
            Destroy(gameObject);
        }
    }
    
    // Método público para establecer el valor (usado por CultivoManager)
    public void SetValor(int nuevoValor)
    {
        valor = Mathf.Max(1, nuevoValor); // Mínimo 1
    }
    
    public int GetValor() => valor;
    
    // Método para testing
    [ContextMenu("🧪 Probar Recolección")]
    public void TestRecoleccion()
    {
        RecolectarZanahoria();
    }
    
    // Debug visual en editor
    void OnDrawGizmos()
    {
        // Dibujar área de recolección
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = yaRecolectada ? Color.gray : Color.yellow;
            
            if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawWireSphere(transform.position, circleCol.radius);
            }
            else if (col is BoxCollider2D boxCol)
            {
                Gizmos.DrawWireCube(transform.position, boxCol.size);
            }
        }
    }
}