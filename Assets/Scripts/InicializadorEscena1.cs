using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InicializadorEscena1 : MonoBehaviour
{
    [Header("🔧 CONFIGURACIÓN ANTI-LAG ESCENA1")]
    [SerializeField] private bool optimizarParaEscena1 = true;
    [SerializeField] private float tiempoInicializacionEscalonada = 0.5f;
    [SerializeField] private bool mostrarDebug = true;
    
    void Start()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        bool esEscena1 = escenaActual.Contains("Escena1") || escenaActual.Contains("1");
        
        if (esEscena1 && optimizarParaEscena1)
        {
            Debug.LogError("🏗️ INICIALIZANDO ESCENA1 DE FORMA OPTIMIZADA...");
            StartCoroutine(InicializacionEscalonada());
        }
    }
    
    private IEnumerator InicializacionEscalonada()
    {
        Debug.LogError("⏳ Iniciando sistemas de forma escalonada para evitar lag...");
        
        // Esperar un frame para que todo se estabilice
        yield return null;
        
        // 🔧 VERIFICAR QUE EL CULTIVOMANAGER ESTÉ FUNCIONANDO
        yield return new WaitForSeconds(0.2f);
        VerificarCultivoManager();
        
        // 1. Crear SistemaMonedas
        yield return new WaitForSeconds(tiempoInicializacionEscalonada);
        CrearSistemaMonedasSiNoExiste();
        
        // 2. Crear SistemaMejoras
        yield return new WaitForSeconds(tiempoInicializacionEscalonada);
        CrearSistemaMejorasSiNoExiste();
        
        // 3. Crear UIManagerZanahorias
        yield return new WaitForSeconds(tiempoInicializacionEscalonada);
        CrearUIManagerSiNoExiste();
        
        // 4. Verificar CanvasMuerte
        yield return new WaitForSeconds(tiempoInicializacionEscalonada);
        CrearCanvasMuerteSiNoExiste();
        
        Debug.LogError("✅ INICIALIZACIÓN ESCALONADA COMPLETADA");
        Debug.LogError("🌱 CULTIVOS DISPONIBLES: Clic derecho para plantar | C para cosechar");
    }
    
    // 🔧 NUEVO: VERIFICAR QUE EL CULTIVOMANAGER FUNCIONE
    private void VerificarCultivoManager()
    {
        CultivoManager cultivoManager = FindObjectOfType<CultivoManager>();
        
        if (cultivoManager == null)
        {
            Debug.LogError("❌ NO SE ENCONTRÓ CULTIVOMANAGER!");
            Debug.LogError("  Necesitas un GameObject con el script CultivoManager en la escena");
            return;
        }
        
        if (!cultivoManager.enabled)
        {
            Debug.LogError("🔧 REACTIVANDO CULTIVOMANAGER DESACTIVADO...");
            cultivoManager.enabled = true;
        }
        
        Debug.LogError("🌱 CULTIVOMANAGER VERIFICADO:");
        Debug.LogError("  - Estado: " + (cultivoManager.enabled ? "ACTIVO ✅" : "INACTIVO ❌"));
        Debug.LogError("  - Controles: Clic derecho = Plantar | C = Cosechar");
        Debug.LogError("  - Cultivos plantados: " + cultivoManager.ObtenerTodosCultivos().Count);
    }
    
    private void CrearSistemaMonedasSiNoExiste()
    {
        if (SistemaMonedas.GetInstancia() == null)
        {
            GameObject sistemaObj = new GameObject("SistemaMonedas");
            sistemaObj.AddComponent<SistemaMonedas>();
            
            if (mostrarDebug)
            {
                Debug.LogError("💰 SistemaMonedas creado dinámicamente");
            }
        }
    }
    
    private void CrearSistemaMejorasSiNoExiste()
    {
        if (FindObjectOfType<SistemaMejoras>() == null)
        {
            GameObject mejorasObj = new GameObject("SistemaMejoras");
            mejorasObj.AddComponent<SistemaMejoras>();
            
            if (mostrarDebug)
            {
                Debug.LogError("💪 SistemaMejoras creado dinámicamente");
            }
        }
    }
    
    private void CrearUIManagerSiNoExiste()
    {
        if (FindObjectOfType<UIManagerZanahorias>() == null)
        {
            GameObject uiObj = new GameObject("UIManagerZanahorias");
            uiObj.AddComponent<UIManagerZanahorias>();
            
            if (mostrarDebug)
            {
                Debug.LogError("📱 UIManagerZanahorias creado dinámicamente");
            }
        }
    }
    
    private void CrearCanvasMuerteSiNoExiste()
    {
        if (FindObjectOfType<CanvasMuerte>() == null)
        {
            GameObject canvasObj = new GameObject("CanvasMuerte");
            canvasObj.AddComponent<CanvasMuerte>();
            
            if (mostrarDebug)
            {
                Debug.LogError("💀 CanvasMuerte creado dinámicamente");
            }
        }
    }
}
