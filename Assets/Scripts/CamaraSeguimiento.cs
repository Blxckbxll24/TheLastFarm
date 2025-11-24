using UnityEngine;
using UnityEngine.SceneManagement;

public class CamaraSeguimiento : MonoBehaviour
{
    [SerializeField] private Transform objetivo; // El jugador
    [SerializeField] private float suavizado = 0.125f; // Qué tan suave es el seguimiento
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Distancia de la cámara
    [SerializeField] private bool buscarJugadorAutomatico = true; // Buscar jugador automáticamente
    [SerializeField] private bool mostrarDebug = true; // Debug para ver qué pasa
    
    void Start()
    {
        // Suscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Buscar jugador inmediatamente
        BuscarJugador();
    }
    
    void LateUpdate()
    {
        // Verificar si perdimos la referencia al jugador
        if (objetivo == null && buscarJugadorAutomatico)
        {
            BuscarJugador();
            return;
        }
        
        if (objetivo != null)
        {
            Vector3 posicionDeseada = objetivo.position + offset;
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
            transform.position = posicionSuavizada;
        }
        else if (mostrarDebug && Time.frameCount % 120 == 0) // Debug cada 2 segundos
        {
            Debug.LogError("🎥 CÁMARA: No hay jugador asignado como objetivo!");
        }
    }
    
    // Evento que se ejecuta cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎥 CÁMARA: Nueva escena cargada - " + scene.name + " | Buscando jugador...");
        }
        
        // Pequeña pausa para que el jugador se inicialice
        Invoke("BuscarJugador", 0.1f);
    }
    
    // Método para buscar al jugador en la escena
    private void BuscarJugador()
    {
        GameObject jugadorEncontrado = null;
        
        // 1. Buscar por tag "Player"
        jugadorEncontrado = GameObject.FindGameObjectWithTag("Player");
        
        // 2. Si no se encuentra por tag, buscar por script MovimientoJugador
        if (jugadorEncontrado == null)
        {
            MovimientoJugador jugadorScript = FindAnyObjectByType<MovimientoJugador>();
            if (jugadorScript != null)
            {
                jugadorEncontrado = jugadorScript.gameObject;
            }
        }
        
        // 3. Buscar entre objetos persistentes (DontDestroyOnLoad)
        if (jugadorEncontrado == null)
        {
            MovimientoJugador[] todosJugadores = FindObjectsByType<MovimientoJugador>(FindObjectsSortMode.None);
            if (todosJugadores.Length > 0)
            {
                jugadorEncontrado = todosJugadores[0].gameObject;
            }
        }
        
        // Asignar el objetivo
        if (jugadorEncontrado != null)
        {
            objetivo = jugadorEncontrado.transform;
            
            if (mostrarDebug)
            {
                Debug.LogError("🎥 ✅ CÁMARA: Jugador encontrado y asignado - " + jugadorEncontrado.name);
                Debug.LogError("    - Tag: " + jugadorEncontrado.tag);
                Debug.LogError("    - Posición: " + jugadorEncontrado.transform.position);
                Debug.LogError("    - Es persistente: " + (jugadorEncontrado.scene.name == "DontDestroyOnLoad" ? "SÍ" : "NO"));
            }
            
            // Posicionar inmediatamente la cámara cerca del jugador
            Vector3 posicionInicial = objetivo.position + offset;
            transform.position = posicionInicial;
        }
        else
        {
            if (mostrarDebug)
            {
                Debug.LogError("🎥 ❌ CÁMARA: No se pudo encontrar el jugador en la escena!");
                Debug.LogError("    Verifica que el jugador tenga:");
                Debug.LogError("    - Tag 'Player' O script 'MovimientoJugador'");
                Debug.LogError("    - Esté activo en la jerarquía");
            }
        }
    }
    
    // Método público para asignar manualmente el objetivo
    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
        
        if (mostrarDebug)
        {
            Debug.LogError("🎥 CÁMARA: Objetivo asignado manualmente - " + (nuevoObjetivo != null ? nuevoObjetivo.name : "NULL"));
        }
    }
    
    // Limpiar eventos al destruir
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}