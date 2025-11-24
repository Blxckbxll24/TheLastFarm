using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sistema para mantener los cultivos entre escenas
/// Se asegura de que los cultivos plantados persistan al cambiar de escena
/// </summary>
public class CultivosPersistentes : MonoBehaviour
{
    [Header("🌱 CONFIGURACIÓN DE PERSISTENCIA")]
    [SerializeField] private bool mantenerCultivosEntreTodas = true;
    [SerializeField] private string[] escenasConCultivos = {"Escena1", "Escena2"};
    [SerializeField] private bool mostrarDebug = true;
    
    // Singleton para evitar duplicados
    private static CultivosPersistentes instancia;
    
    // Datos persistentes por escena
    private Dictionary<string, DatosCultivosPorEscena> cultivosPorEscena = new Dictionary<string, DatosCultivosPorEscena>();
    
    // Escena actual
    private string escenaActual;
    
    [System.Serializable]
    public class DatosCultivosPorEscena
    {
        public string nombreEscena;
        public List<CultivoData> cultivosGuardados = new List<CultivoData>();
        public float tiempoGuardado; // Para ajustar el tiempo de los cultivos
        
        public DatosCultivosPorEscena(string nombre)
        {
            nombreEscena = nombre;
            tiempoGuardado = Time.time;
        }
    }
    
    void Awake()
    {
        // Patrón Singleton
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse a eventos de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            if (mostrarDebug)
            {
                Debug.LogError("🌱 SISTEMA DE CULTIVOS PERSISTENTES INICIADO");
                Debug.LogError("  - Mantener en todas las escenas: " + mantenerCultivosEntreTodas);
                Debug.LogError("  - Escenas configuradas: " + string.Join(", ", escenasConCultivos));
            }
        }
        else if (instancia != this)
        {
            if (mostrarDebug)
            {
                Debug.LogError("🔄 DESTRUYENDO DUPLICADO DE CULTIVOS PERSISTENTES");
            }
            Destroy(gameObject);
        }
    }
    
    // Cuando se descarga una escena (antes de cambiar)
    private void OnSceneUnloaded(Scene escenaAnterior)
    {
        string nombreEscenaAnterior = escenaAnterior.name;
        
        if (mostrarDebug)
        {
            Debug.LogError($"📤 ESCENA DESCARGÁNDOSE: {nombreEscenaAnterior}");
        }
        
        if (EsEscenaConCultivos(nombreEscenaAnterior))
        {
            GuardarCultivosDeEscena(nombreEscenaAnterior);
        }
    }
    
    // 🔧 NUEVO: Guardado automático cada cierto tiempo
    void Start()
    {
        escenaActual = SceneManager.GetActiveScene().name;
        
        // 🆕 GUARDADO AUTOMÁTICO cada 5 segundos
        InvokeRepeating("GuardadoAutomatico", 5f, 5f);
        
        if (mostrarDebug)
        {
            Debug.LogError($"🌱 CULTIVOS PERSISTENTES - Escena inicial: {escenaActual}");
            Debug.LogError("💾 GUARDADO AUTOMÁTICO activado cada 5 segundos");
        }
        
        // 🔧 INICIALIZACIÓN INMEDIATA
        // Verificar si ya hay cultivos para restaurar en esta escena
        if (EsEscenaConCultivos(escenaActual) && cultivosPorEscena.ContainsKey(escenaActual))
        {
            Debug.LogError("🔄 HAY CULTIVOS GUARDADOS PARA ESTA ESCENA - Restaurando en 1 segundo...");
            Invoke("RestaurarCultivosDeEscena", 1f); // Dar tiempo a que se inicialice el CultivoManager
        }
    }
    
    // 🆕 MÉTODO PARA GUARDADO AUTOMÁTICO PERIÓDICO
    private void GuardadoAutomatico()
    {
        if (EsEscenaConCultivos(escenaActual))
        {
            CultivoManager cultivoManager = FindObjectOfType<CultivoManager>();
            
            if (cultivoManager != null)
            {
                var cultivosActuales = cultivoManager.ObtenerTodosCultivos();
                
                if (cultivosActuales.Count > 0)
                {
                    GuardarCultivosDeEscena(escenaActual);
                    
                    if (mostrarDebug && Time.frameCount % 300 == 0) // Debug ocasional
                    {
                        Debug.LogError($"💾 GUARDADO AUTOMÁTICO: {cultivosActuales.Count} cultivos en {escenaActual}");
                    }
                }
            }
        }
    }
    
    // Cuando se carga una nueva escena
    private void OnSceneLoaded(Scene nuevaEscena, LoadSceneMode modo)
    {
        escenaActual = nuevaEscena.name;
        
        if (mostrarDebug)
        {
            Debug.LogError($"📥 NUEVA ESCENA CARGADA: {escenaActual}");
        }
        
        if (EsEscenaConCultivos(escenaActual))
        {
            // Verificar si hay datos guardados para esta escena
            if (cultivosPorEscena.ContainsKey(escenaActual))
            {
                Debug.LogError($"🔄 ENCONTRADOS CULTIVOS GUARDADOS PARA {escenaActual} - Restaurando...");
                // Dar tiempo suficiente para que se inicialice el CultivoManager
                Invoke("RestaurarCultivosDeEscena", 1.5f);
            }
            else
            {
                Debug.LogError($"📭 NO HAY CULTIVOS GUARDADOS PARA {escenaActual}");
            }
        }
    }
    
    // Verificar si la escena debe tener cultivos persistentes
    private bool EsEscenaConCultivos(string nombreEscena)
    {
        if (mantenerCultivosEntreTodas) return true;
        
        return escenasConCultivos.Contains(nombreEscena);
    }
    
    // Guardar cultivos de la escena actual
    private void GuardarCultivosDeEscena(string nombreEscena)
    {
        CultivoManager cultivoManager = FindObjectOfType<CultivoManager>();
        
        if (cultivoManager == null)
        {
            if (mostrarDebug)
            {
                Debug.LogWarning($"⚠️ No se encontró CultivoManager en {nombreEscena}");
            }
            return;
        }
        
        // Obtener los cultivos actuales
        var cultivosActuales = cultivoManager.ObtenerTodosCultivos();
        
        if (cultivosActuales.Count > 0)
        {
            // Crear o actualizar datos de la escena
            if (!cultivosPorEscena.ContainsKey(nombreEscena))
            {
                cultivosPorEscena[nombreEscena] = new DatosCultivosPorEscena(nombreEscena);
            }
            
            var datosEscena = cultivosPorEscena[nombreEscena];
            datosEscena.cultivosGuardados.Clear();
            datosEscena.tiempoGuardado = Time.time;
            
            // Copiar cultivos
            foreach (var cultivo in cultivosActuales)
            {
                var cultivoCopiado = new CultivoData
                {
                    posicionCelda = cultivo.Key,
                    tipoCultivo = cultivo.Value.tipoCultivo,
                    etapaActual = cultivo.Value.etapaActual,
                    tiempoPlantado = cultivo.Value.tiempoPlantado
                };
                datosEscena.cultivosGuardados.Add(cultivoCopiado);
            }
            
            if (mostrarDebug)
            {
                Debug.LogError($"💾 CULTIVOS GUARDADOS de {nombreEscena}: {datosEscena.cultivosGuardados.Count}");
                foreach (var cultivo in datosEscena.cultivosGuardados)
                {
                    Debug.LogError($"  - {cultivo.tipoCultivo} etapa {cultivo.etapaActual} en {cultivo.posicionCelda}");
                }
            }
        }
        else if (mostrarDebug)
        {
            Debug.LogError($"📭 NO HAY CULTIVOS para guardar en {nombreEscena}");
        }
    }
    
    // Restaurar cultivos en la escena actual
    private void RestaurarCultivosDeEscena()
    {
        if (!cultivosPorEscena.ContainsKey(escenaActual))
        {
            if (mostrarDebug)
            {
                Debug.LogError($"📂 NO HAY CULTIVOS GUARDADOS para {escenaActual}");
            }
            return;
        }
        
        CultivoManager cultivoManager = FindObjectOfType<CultivoManager>();
        
        if (cultivoManager == null)
        {
            if (mostrarDebug)
            {
                Debug.LogError($"❌ NO SE ENCONTRÓ CultivoManager en {escenaActual}");
                Debug.LogError("  - Reintentando en 2 segundos...");
            }
            
            // Reintentar en unos segundos
            Invoke("RestaurarCultivosDeEscena", 2f);
            return;
        }
        
        var datosEscena = cultivosPorEscena[escenaActual];
        float tiempoTranscurridoFueraEscena = Time.time - datosEscena.tiempoGuardado;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔄 RESTAURANDO {datosEscena.cultivosGuardados.Count} cultivos en {escenaActual}");
            Debug.LogError($"⏱️ Tiempo fuera de escena: {tiempoTranscurridoFueraEscena:F1} segundos");
        }
        
        // Restaurar cada cultivo
        int cultivosRestaurados = 0;
        foreach (var cultivoGuardado in datosEscena.cultivosGuardados)
        {
            try
            {
                // Ajustar tiempo plantado considerando el tiempo fuera de escena
                float nuevoTiempoPlantado = cultivoGuardado.tiempoPlantado + tiempoTranscurridoFueraEscena;
                
                // Usar el método público del CultivoManager para restaurar
                cultivoManager.RestaurarCultivo(
                    cultivoGuardado.posicionCelda,
                    cultivoGuardado.tipoCultivo,
                    cultivoGuardado.etapaActual,
                    nuevoTiempoPlantado
                );
                
                cultivosRestaurados++;
                
                if (mostrarDebug)
                {
                    Debug.LogError($"✅ Restaurado: {cultivoGuardado.tipoCultivo} etapa {cultivoGuardado.etapaActual} en {cultivoGuardado.posicionCelda}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ ERROR restaurando cultivo: {e.Message}");
            }
        }
        
        if (mostrarDebug)
        {
            Debug.LogError($"✅ CULTIVOS RESTAURADOS en {escenaActual}: {cultivosRestaurados}/{datosEscena.cultivosGuardados.Count}");
        }
    }
    
    // Método público para forzar guardado manual
    public void ForzarGuardado()
    {
        if (EsEscenaConCultivos(escenaActual))
        {
            GuardarCultivosDeEscena(escenaActual);
        }
    }
    
    // Método público para obtener información
    public void MostrarEstadisticas()
    {
        Debug.LogError("📊 ESTADÍSTICAS DE CULTIVOS PERSISTENTES:");
        Debug.LogError($"  - Escena actual: {escenaActual}");
        Debug.LogError($"  - Escenas con datos: {cultivosPorEscena.Count}");
        
        foreach (var escena in cultivosPorEscena)
        {
            Debug.LogError($"    * {escena.Key}: {escena.Value.cultivosGuardados.Count} cultivos");
            foreach (var cultivo in escena.Value.cultivosGuardados)
            {
                Debug.LogError($"      - {cultivo.tipoCultivo} etapa {cultivo.etapaActual} en {cultivo.posicionCelda}");
            }
        }
    }
    
    // Limpiar datos (útil para debugging)
    public void LimpiarTodosLosDatos()
    {
        cultivosPorEscena.Clear();
        if (mostrarDebug)
        {
            Debug.LogError("🧹 TODOS LOS DATOS DE CULTIVOS LIMPIADOS");
        }
    }
    
    // Getters públicos
    public static CultivosPersistentes GetInstancia() => instancia;
    public int GetCantidadEscenasConCultivos() => cultivosPorEscena.Count;
    public bool TieneDataParaEscena(string escena) => cultivosPorEscena.ContainsKey(escena);
    
    void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
    
    // 🔧 MÉTODOS PARA TESTING MANUAL
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            MostrarEstadisticas();
        }
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            ForzarGuardado();
            Debug.LogError("💾 GUARDADO MANUAL FORZADO!");
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            LimpiarTodosLosDatos();
        }
        
        // 🆕 NUEVO: Forzar restauración manual
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.LogError("🔄 FORZANDO RESTAURACIÓN MANUAL...");
            RestaurarCultivosDeEscena();
        }
    }
    
    // Método de contexto para testing en editor
    [ContextMenu("🔍 Mostrar Estadísticas")]
    public void MostrarEstadisticasContexto() => MostrarEstadisticas();
    
    [ContextMenu("🧹 Limpiar Datos")]
    public void LimpiarDatosContexto() => LimpiarTodosLosDatos();
    
    [ContextMenu("💾 Forzar Guardado")]
    public void ForzarGuardadoContexto() => ForzarGuardado();
    
    [ContextMenu("🔄 Forzar Restauración")]
    public void ForzarRestauracionContexto() => RestaurarCultivosDeEscena();
}
