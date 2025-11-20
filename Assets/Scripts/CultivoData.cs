using UnityEngine;

// Usa 'System.Serializable' para que se vea en el Inspector si decides usarlo en una lista
[System.Serializable]
public class CultivoData
{
    public Vector3Int posicionCelda; // La posición en el Tilemap
    public string tipoCultivo;       // "Zanahoria", "Maiz", etc.
    public int etapaActual;          // 0-9 (10 etapas total)
    public float tiempoPlantado;      // El tiempo (Time.time) en el que se plantó

    // Sistema de 10 etapas con tiempo constante por etapa
    public const float TIEMPO_POR_ETAPA = 2f; // 2 segundos por etapa
    public const int ETAPA_MAXIMA = 9; // Última etapa (índice 9)
    
    // Compatibilidad con sistema anterior
    public const float TIEMPO_A_CRECIMIENTO_1 = TIEMPO_POR_ETAPA; // 2 segundos
    public const float TIEMPO_A_MADURO = TIEMPO_POR_ETAPA * ETAPA_MAXIMA; // 18 segundos total

    public bool EstaMaduro()
    {
        return etapaActual >= ETAPA_MAXIMA;
    }
    
    // Nuevo método para obtener progreso
    public float ObtenerProgreso()
    {
        return (float)etapaActual / (float)ETAPA_MAXIMA;
    }
    
    // Método para obtener tiempo restante hasta madurez
    public float TiempoRestanteParaMadurez(float tiempoActual)
    {
        float tiempoTranscurrido = tiempoActual - tiempoPlantado;
        float tiempoTotalNecesario = TIEMPO_POR_ETAPA * ETAPA_MAXIMA;
        return Mathf.Max(0, tiempoTotalNecesario - tiempoTranscurrido);
    }
}
