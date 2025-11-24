using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    [Header("🔗 Referencias")]
    public MovimientoJugador jugador; // Referencia al script del jugador
    
    [Header("🔍 Debug")]
    [SerializeField] private bool mostrarDebug = false;
    
    void Start()
    {
        // Si no se asignó manualmente, buscar el componente en el padre
        if (jugador == null)
        {
            jugador = GetComponentInParent<MovimientoJugador>();
        }
        
        // 🔧 VERIFICACIÓN ADICIONAL: Buscar en el root si no se encuentra en el padre directo
        if (jugador == null)
        {
            Transform actual = transform;
            while (actual.parent != null && jugador == null)
            {
                actual = actual.parent;
                jugador = actual.GetComponent<MovimientoJugador>();
            }
        }
        
        // Verificar que el componente esté configurado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            
            if (mostrarDebug)
            {
                Debug.LogError("🔍 TRIGGER DETECTOR INICIALIZADO:");
                Debug.LogError("  - Collider configurado como trigger: " + col.isTrigger);
                Debug.LogError("  - Jugador asignado: " + (jugador != null ? "✅" : "❌"));
                if (transform.parent != null)
                {
                    Debug.LogError("  - GameObject padre: " + transform.parent.name);
                }
                else
                {
                    Debug.LogError("  - Sin GameObject padre");
                }
            }
        }
        else
        {
            Debug.LogError("❌ ERROR: TriggerDetector necesita un Collider2D!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jugador == null)
        {
            Debug.LogError("❌ TriggerDetector: No hay referencia al jugador!");
            return;
        }
        
        // 🔧 VERIFICACIÓN ADICIONAL: Asegurar que el jugador esté vivo
        if (jugador.EstaMuerto())
        {
            if (mostrarDebug)
            {
                Debug.LogError("💀 TriggerDetector: Jugador está muerto, ignorando trigger");
            }
            return;
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("⚔️ TRIGGER DETECTADO en espada:");
            Debug.LogError("  - Objeto: " + other.name);
            Debug.LogError("  - Tag: " + other.tag);
            Debug.LogError("  - Layer: " + LayerMask.LayerToName(other.gameObject.layer));
        }
        
        // Enviar la detección al jugador para que procese el ataque
        jugador.ProcesarTriggerAtaque(other);
    }
}