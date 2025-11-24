using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja la persistencia del jugador entre escenas
/// Agrega este script al jugador para que persista automáticamente
/// </summary>
public class JugadorPersistente : MonoBehaviour
{
    [Header("🔄 CONFIGURACIÓN DE PERSISTENCIA")]
    [SerializeField] private bool persistirEntreTodas = true; // Cambiado a true por defecto
    [SerializeField] private string[] escenasPermitidas = {"Escena1", "Escena2"}; // Escenas donde puede existir
    [SerializeField] private bool mostrarDebug = true;
    
    private static JugadorPersistente instancia;
    private string escenaActual;
    
    void Awake()
    {
        // Patrón Singleton para evitar duplicados
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse a eventos de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (mostrarDebug)
            {
                Debug.LogError("✅ JUGADOR CONFIGURADO COMO PERSISTENTE");
                Debug.LogError("    - Nombre: " + gameObject.name);
                Debug.LogError("    - Persistir en todas: " + persistirEntreTodas);
                Debug.LogError("    - Escenas permitidas: " + string.Join(", ", escenasPermitidas));
            }
        }
        else if (instancia != this)
        {
            // Ya existe una instancia, destruir esta
            if (mostrarDebug)
            {
                Debug.LogError("🔄 DESTRUYENDO JUGADOR DUPLICADO: " + gameObject.name);
            }
            Destroy(gameObject);
        }
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        escenaActual = scene.name;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔄 JUGADOR CARGADO EN ESCENA: {escenaActual}");
        }
        
        // Verificar si el jugador debería estar en esta escena
        if (!DeberiaEstarEnEstaEscena())
        {
            if (mostrarDebug)
            {
                Debug.LogError($"❌ JUGADOR NO DEBERÍA ESTAR EN '{escenaActual}' - OCULTANDO");
            }
            gameObject.SetActive(false);
            return;
        }
        
        // Asegurar que el jugador esté activo y posicionado correctamente
        gameObject.SetActive(true);
        PosicionarJugadorEnEscena();
        
        // 💖 RESETEAR ESTADO DEL JUGADOR AL CAMBIAR DE ESCENA (NUEVO)
        RestaurarEstadoJugador();
        
        // Notificar a la cámara que hay un nuevo jugador
        NotificarCamara();
    }

    void RestaurarEstadoJugador()
    {
        var movimiento = GetComponent<MovimientoJugador>();
        if (movimiento != null)
        {
            // 🔧 ANTES DE RESETEAR, VERIFICAR GRAVEDAD ORIGINAL
            float gravedadAnterior = movimiento.GetGravedadActual();
            
            // 💖 RESETEAR ESTADO COMPLETO AL CAMBIAR DE ESCENA
            movimiento.ResetearEstadoJugador();
            
            // 🔧 VERIFICAR QUE LA GRAVEDAD SE MANTUVO CORRECTA
            float gravedadDespues = movimiento.GetGravedadActual();
            
            if (mostrarDebug)
            {
                Debug.LogError("🎮 ESTADO DEL JUGADOR RESTAURADO EN NUEVA ESCENA");
                Debug.LogError($"  - Gravedad antes: {gravedadAnterior}");
                Debug.LogError($"  - Gravedad después: {gravedadDespues}");
                Debug.LogError($"  - Gravedad original configurada: {movimiento.GetGravedadOriginal()}");
            }
        }
    }
    
    bool DeberiaEstarEnEstaEscena()
    {
        // No aparecer en escenas de menú
        if (escenaActual.ToLower().Contains("menu"))
        {
            return false;
        }
        
        if (persistirEntreTodas) return true;
        
        foreach (string escenaPermitida in escenasPermitidas)
        {
            if (escenaActual.Equals(escenaPermitida, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void PosicionarJugadorEnEscena()
    {
        // Buscar punto de spawn específico para esta escena
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        
        if (spawnPoint == null)
        {
            // Buscar por nombre si no hay tag
            spawnPoint = GameObject.Find("SpawnPlayer") ?? GameObject.Find("PlayerStart");
        }
        
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            
            if (mostrarDebug)
            {
                Debug.LogError($"📍 JUGADOR POSICIONADO EN SPAWN: {spawnPoint.name} | Pos: {transform.position}");
            }
        }
        else
        {
            // Posición por defecto para cada escena
            Vector3 posicionDefault = GetPosicionDefaultParaEscena();
            transform.position = posicionDefault;
            
            if (mostrarDebug)
            {
                Debug.LogError($"📍 JUGADOR EN POSICIÓN DEFAULT: {posicionDefault}");
            }
        }
    }
    
    Vector3 GetPosicionDefaultParaEscena()
    {
        // Posiciones por defecto según la escena
        return escenaActual.ToLower() switch
        {
            var x when x.Contains("escena1") => new Vector3(0, 0, 0),
            var x when x.Contains("escena2") => new Vector3(-5, 0, 0),
            var x when x.Contains("menu") => new Vector3(0, -10, 0), // Fuera de vista en menú
            _ => new Vector3(0, 0, 0)
        };
    }
    
    void NotificarCamara()
    {
        // Buscar cámara con script de seguimiento
        CamaraSeguimiento camaraSeguimiento = FindAnyObjectByType<CamaraSeguimiento>();
        
        if (camaraSeguimiento != null)
        {
            // Asignar este jugador como objetivo
            camaraSeguimiento.AsignarObjetivo(transform);
            
            if (mostrarDebug)
            {
                Debug.LogError("📷 CÁMARA NOTIFICADA - Nuevo objetivo asignado");
            }
        }
        else if (mostrarDebug)
        {
            Debug.LogWarning("⚠️ No se encontró script CamaraSeguimiento en la escena");
        }
    }
    
    // Método para forzar al jugador a ir a una escena específica
    public void IrAEscena(string nombreEscena)
    {
        if (mostrarDebug)
        {
            Debug.LogError($"🚀 ENVIANDO JUGADOR A: {nombreEscena}");
        }
        SceneManager.LoadScene(nombreEscena);
    }
    
    // Método para destruir la persistencia (útil para volver al menú)
    public void DestruirPersistencia()
    {
        if (mostrarDebug)
        {
            Debug.LogError("💀 INICIANDO DESTRUCCIÓN DE PERSISTENCIA DEL JUGADOR...");
        }
        
        // 🔧 LIMPIEZA COMPLETA ANTES DE DESTRUIR
        StopAllCoroutines();
        CancelInvoke();
        
        // Limpiar el MovimientoJugador si existe
        var movimiento = GetComponent<MovimientoJugador>();
        if (movimiento != null)
        {
            movimiento.StopAllCoroutines();
            movimiento.CancelInvoke();
            movimiento.ForzarDetenerAtaque();
        }
        
        // Desuscribirse de eventos
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Limpiar instancia estática
        if (instancia == this)
        {
            instancia = null;
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("💀 PERSISTENCIA DEL JUGADOR DESTRUIDA COMPLETAMENTE");
        }
        
        // Destruir el GameObject
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    // Método estático para acceso fácil
    public static JugadorPersistente GetInstancia()
    {
        return instancia;
    }
    
    // Método para configurar escenas permitidas dinámicamente
    public void ConfigurarEscenasPermitidas(params string[] nuevasEscenas)
    {
        escenasPermitidas = nuevasEscenas;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔧 ESCENAS PERMITIDAS ACTUALIZADAS: {string.Join(", ", nuevasEscenas)}");
        }
    }
}