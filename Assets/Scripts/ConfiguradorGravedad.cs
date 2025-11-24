using UnityEngine;

/// <summary>
/// Script para configurar la gravedad del jugador de manera persistente
/// Agrega este script al mismo GameObject que MovimientoJugador
/// </summary>
public class ConfiguradorGravedad : MonoBehaviour
{
    [Header("🎯 Configuración de Gravedad")]
    [SerializeField] private float gravedadDeseada = 3f;
    [SerializeField] private bool aplicarAlInicio = true;
    [SerializeField] private bool verificarContinuamente = true;
    [SerializeField] private bool mostrarDebug = true;
    
    private MovimientoJugador movimientoJugador;
    private Rigidbody2D rb2D;
    
    void Start()
    {
        // Obtener componentes
        movimientoJugador = GetComponent<MovimientoJugador>();
        rb2D = GetComponent<Rigidbody2D>();
        
        if (aplicarAlInicio)
        {
            AplicarGravedad();
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎯 CONFIGURADOR DE GRAVEDAD INICIADO");
            Debug.LogError($"  - Gravedad deseada: {gravedadDeseada}");
            Debug.LogError($"  - Verificación continua: {verificarContinuamente}");
        }
    }
    
    void Update()
    {
        if (verificarContinuamente && rb2D != null)
        {
            // Verificar cada segundo si la gravedad es correcta
            if (Time.frameCount % 60 == 0)
            {
                if (Mathf.Abs(rb2D.gravityScale - gravedadDeseada) > 0.01f)
                {
                    if (mostrarDebug)
                    {
                        Debug.LogError($"🔧 CORRIGIENDO GRAVEDAD: {rb2D.gravityScale} → {gravedadDeseada}");
                    }
                    AplicarGravedad();
                }
            }
        }
    }
    
    [ContextMenu("🎯 Aplicar Gravedad Configurada")]
    public void AplicarGravedad()
    {
        if (rb2D != null)
        {
            float gravedadAnterior = rb2D.gravityScale;
            rb2D.gravityScale = gravedadDeseada;
            
            if (mostrarDebug)
            {
                Debug.LogError("🎯 GRAVEDAD APLICADA:");
                Debug.LogError($"  - Anterior: {gravedadAnterior}");
                Debug.LogError($"  - Nueva: {rb2D.gravityScale}");
            }
        }
        
        // También actualizar en MovimientoJugador si existe
        if (movimientoJugador != null)
        {
            movimientoJugador.SetGravedad(gravedadDeseada);
        }
    }
    
    // Método para cambiar la gravedad en runtime
    public void CambiarGravedad(float nuevaGravedad)
    {
        gravedadDeseada = nuevaGravedad;
        AplicarGravedad();
    }
}
