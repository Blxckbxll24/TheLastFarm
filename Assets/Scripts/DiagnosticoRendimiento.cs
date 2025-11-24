using UnityEngine;
using System.Collections;

public class DiagnosticoRendimiento : MonoBehaviour
{
    [Header("🔍 DIAGNÓSTICO DE RENDIMIENTO")]
    [SerializeField] private bool activarDiagnostico = true;
    [SerializeField] private float intervaloReporte = 3f;
    
    private float ultimoReporte = 0f;
    private int framesContados = 0;
    private float tiempoAcumulado = 0f;
    
    void Start()
    {
        if (activarDiagnostico)
        {
            Debug.LogError("🔍 DIAGNÓSTICO DE RENDIMIENTO ACTIVADO");
            StartCoroutine(MonitorearRendimiento());
        }
    }
    
    void Update()
    {
        if (!activarDiagnostico) return;
        
        framesContados++;
        tiempoAcumulado += Time.unscaledDeltaTime;
        
        // Detectar congelamiento
        if (Time.unscaledDeltaTime > 0.1f) // Frame tomó más de 100ms
        {
            Debug.LogError($"🚨 FRAME LENTO DETECTADO: {Time.unscaledDeltaTime * 1000f:F1}ms");
        }
    }
    
    private IEnumerator MonitorearRendimiento()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(intervaloReporte);
            
            float fps = framesContados / tiempoAcumulado;
            
            Debug.LogError($"📊 REPORTE DE RENDIMIENTO:");
            Debug.LogError($"  - FPS: {fps:F1}");
            Debug.LogError($"  - Frame time: {(tiempoAcumulado / framesContados) * 1000f:F1}ms");
            Debug.LogError($"  - GameObjects activos: {FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length}");
            Debug.LogError($"  - Scripts activos: {FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Length}");
            
            // Revisar scripts específicos que pueden causar problemas
            var cultivoManager = FindObjectOfType<CultivoManager>();
            if (cultivoManager != null)
            {
                Debug.LogError($"  - CultivoManager: {(cultivoManager.enabled ? "ACTIVO" : "INACTIVO")}");
            }
            
            var menuPausa = FindObjectOfType<MenuPausa>();
            if (menuPausa != null)
            {
                Debug.LogError($"  - MenuPausa: {(menuPausa.enabled ? "ACTIVO" : "INACTIVO")}");
            }
            
            // Reset contadores
            framesContados = 0;
            tiempoAcumulado = 0f;
        }
    }
    
    [ContextMenu("🔍 Diagnóstico Inmediato")]
    public void DiagnosticoInmediato()
    {
        Debug.LogError("🔍 DIAGNÓSTICO INMEDIATO:");
        Debug.LogError($"  - Time.timeScale: {Time.timeScale}");
        Debug.LogError($"  - Application.targetFrameRate: {Application.targetFrameRate}");
        Debug.LogError($"  - QualitySettings.vSyncCount: {QualitySettings.vSyncCount}");
        
        // Verificar scripts problemáticos
        var scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int scriptsActivos = 0;
        foreach (var script in scripts)
        {
            if (script.enabled) scriptsActivos++;
        }
        
        Debug.LogError($"  - Scripts totales: {scripts.Length}");
        Debug.LogError($"  - Scripts activos: {scriptsActivos}");
    }
}
