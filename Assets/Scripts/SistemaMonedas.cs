using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema completo de monedas con persistencia entre escenas y muerte
/// </summary>
public class SistemaMonedas : MonoBehaviour
{
    [Header("💰 CONFIGURACIÓN DE MONEDAS")]
    [SerializeField] private int monedasIniciales = 0;
    [SerializeField] private bool mostrarDebug = true;
    [SerializeField] private bool crearUIAutomaticamente = true;
    
    [Header("📱 REFERENCIAS UI")]
    [SerializeField] private TextMeshProUGUI textoMonedas;
    [SerializeField] private Canvas canvasMonedas;
    
    // Sistema Singleton
    private static SistemaMonedas instancia;
    private static int monedasActuales = 0;
    private static bool datosInicializados = false;
    
    // Variables para animación
    private int monedasMostradas = 0;
    private bool animandoCambio = false;
    
    void Awake()
    {
        // Patrón Singleton con persistencia entre escenas
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            
            // 📂 CARGAR MONEDAS GUARDADAS AL INICIO
            if (!datosInicializados)
            {
                CargarMonedas();
                datosInicializados = true;
            }
            
            // Suscribirse a eventos de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (mostrarDebug)
            {
                Debug.LogError("💰 SISTEMA DE MONEDAS INICIALIZADO COMO SINGLETON");
                Debug.LogError($"  - Monedas cargadas: {monedasActuales}");
            }
        }
        else if (instancia != this)
        {
            // Ya existe una instancia, destruir esta
            if (mostrarDebug)
            {
                Debug.LogError("💰 DESTRUYENDO SISTEMA DE MONEDAS DUPLICADO");
            }
            Destroy(gameObject);
            return;
        }
        
        // Actualizar instancia actual
        instancia = this;
    }
    
    void Start()
    {
        // 🔧 CONFIGURAR UI ESPECÍFICA PARA ESTA ESCENA
        ConfigurarUIEscenaActual();
        
        // Actualizar display con las monedas actuales
        ActualizarDisplay();
        
        if (mostrarDebug)
        {
            Debug.LogError($"💰 SISTEMA LISTO EN ESCENA: {SceneManager.GetActiveScene().name}");
            Debug.LogError($"  - Monedas actuales: {monedasActuales}");
        }
    }
    
    // Cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instancia != this) return; // Solo procesar en la instancia principal
        
        if (mostrarDebug)
        {
            Debug.LogError($"💰 NUEVA ESCENA: {scene.name} - Configurando UI...");
        }
        
        // Buscar o crear UI en la nueva escena
        Invoke("ConfigurarUIEscenaActual", 0.5f); // Dar tiempo a que se cargue la escena
    }
    
    // 🔧 CONFIGURAR UI ESPECÍFICA PARA LA ESCENA ACTUAL
    private void ConfigurarUIEscenaActual()
    {
        // Buscar UI existente en la escena
        BuscarUIExistente();
        
        // Si no hay UI y está configurado para crear automáticamente
        if (textoMonedas == null && crearUIAutomaticamente)
        {
            CrearUIMonedas();
        }
        
        // Actualizar display
        ActualizarDisplay();
        
        if (mostrarDebug)
        {
            Debug.LogError($"💰 UI CONFIGURADA - Texto encontrado: {textoMonedas != null}");
        }
    }
    
    // 🔍 BUSCAR UI EXISTENTE EN LA ESCENA
    private void BuscarUIExistente()
    {
        // Resetear referencias (pueden ser de escena anterior)
        textoMonedas = null;
        canvasMonedas = null;
        
        // Buscar UIManagerZanahorias primero
        UIManagerZanahorias uiManager = FindObjectOfType<UIManagerZanahorias>();
        if (uiManager != null)
        {
            // VERIFICAR QUE EL MÉTODO EXISTE ANTES DE LLAMARLO
            try
            {
                textoMonedas = uiManager.GetTextoMonedas();
                if (mostrarDebug && textoMonedas != null)
                {
                    Debug.LogError("💰 UI ENCONTRADA VÍA UIManagerZanahorias");
                }
            }
            catch (System.Exception e)
            {
                if (mostrarDebug)
                {
                    Debug.LogError("❌ ERROR ACCEDIENDO A UIManagerZanahorias: " + e.Message);
                }
            }
        }
        
        // Si no se encontró, buscar por nombre
        if (textoMonedas == null)
        {
            GameObject textoObj = GameObject.Find("TextoMonedas");
            if (textoObj == null) textoObj = GameObject.Find("Texto_Monedas");
            if (textoObj == null) textoObj = GameObject.Find("MoneyText");
            if (textoObj == null) textoObj = GameObject.Find("Texto_Zanahorias");
            
            if (textoObj != null)
            {
                textoMonedas = textoObj.GetComponent<TextMeshProUGUI>();
                if (mostrarDebug && textoMonedas != null)
                {
                    Debug.LogError("💰 UI ENCONTRADA POR NOMBRE: " + textoObj.name);
                }
            }
        }
        
        // Buscar en todos los TextMeshProUGUI de la escena
        if (textoMonedas == null)
        {
            TextMeshProUGUI[] todosTextos = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var texto in todosTextos)
            {
                if (texto.name.ToLower().Contains("moneda") || 
                    texto.name.ToLower().Contains("money") ||
                    texto.name.ToLower().Contains("coin") ||
                    texto.name.ToLower().Contains("zanahoria") ||
                    texto.text.Contains("$") ||
                    texto.text.Contains("💰") ||
                    texto.text.Contains("🥕"))
                {
                    textoMonedas = texto;
                    if (mostrarDebug)
                    {
                        Debug.LogError("💰 UI ENCONTRADA POR CONTENIDO: " + texto.name);
                    }
                    break;
                }
            }
        }
        
        // FALLBACK: Crear UI si no se encuentra nada
        if (textoMonedas == null && mostrarDebug)
        {
            Debug.LogError("⚠️ NO SE ENCONTRÓ UI DE MONEDAS EXISTENTE - Se creará automáticamente");
        }
    }
    
    // 🎨 CREAR UI DE MONEDAS AUTOMÁTICAMENTE
    private void CrearUIMonedas()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI DE MONEDAS AUTOMÁTICAMENTE...");
        }
        
        // Buscar canvas existente
        Canvas canvasExistente = FindObjectOfType<Canvas>();
        
        if (canvasExistente == null)
        {
            // Crear canvas nuevo
            GameObject canvasObj = new GameObject("Canvas_Monedas");
            canvasMonedas = canvasObj.AddComponent<Canvas>();
            canvasMonedas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasMonedas.sortingOrder = 10; // Encima de otros elementos
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasMonedas = canvasExistente;
        }
        
        // Crear texto de monedas
        GameObject textoObj = new GameObject("TextoMonedas");
        textoObj.transform.SetParent(canvasMonedas.transform, false);
        
        textoMonedas = textoObj.AddComponent<TextMeshProUGUI>();
        textoMonedas.text = $"💰 {monedasActuales}";
        textoMonedas.fontSize = 24;
        textoMonedas.color = Color.yellow;
        textoMonedas.fontStyle = FontStyles.Bold;
        textoMonedas.alignment = TextAlignmentOptions.TopRight;
        
        // Posicionar en esquina superior derecha
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = new Vector2(1f, 1f);
        rectTexto.anchorMax = new Vector2(1f, 1f);
        rectTexto.pivot = new Vector2(1f, 1f);
        rectTexto.sizeDelta = new Vector2(200f, 50f);
        rectTexto.anchoredPosition = new Vector2(-20f, -20f);
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DE MONEDAS CREADA AUTOMÁTICAMENTE");
        }
    }
    
    // 💰 AGREGAR MONEDAS
    public void AgregarMonedas(int cantidad)
    {
        if (cantidad <= 0) return;
        
        monedasActuales += cantidad;
        
        // Guardar inmediatamente
        GuardarMonedas();
        
        if (mostrarDebug)
        {
            Debug.LogError($"💰 MONEDAS AGREGADAS: +{cantidad} | Total: {monedasActuales}");
        }
        
        // Animar cambio en UI
        AnimarCambioMonedas();
    }
    
    // 💸 GASTAR MONEDAS
    public bool GastarMonedas(int cantidad)
    {
        if (cantidad <= 0) return false;
        
        if (monedasActuales >= cantidad)
        {
            monedasActuales -= cantidad;
            
            // Guardar inmediatamente
            GuardarMonedas();
            
            if (mostrarDebug)
            {
                Debug.LogError($"💸 MONEDAS GASTADAS: -{cantidad} | Total: {monedasActuales}");
            }
            
            // Animar cambio en UI
            AnimarCambioMonedas();
            
            return true;
        }
        else
        {
            if (mostrarDebug)
            {
                Debug.LogError($"❌ NO HAY SUFICIENTES MONEDAS: Tienes {monedasActuales}, necesitas {cantidad}");
            }
            return false;
        }
    }
    
    // 🎬 ANIMAR CAMBIO EN LA UI
    private void AnimarCambioMonedas()
    {
        if (animandoCambio) return;
        
        StartCoroutine(AnimacionCambioMonedas());
    }
    
    private System.Collections.IEnumerator AnimacionCambioMonedas()
    {
        animandoCambio = true;
        
        // Animación simple de conteo
        int diferencia = monedasActuales - monedasMostradas;
        int pasos = Mathf.Min(Mathf.Abs(diferencia), 20); // Máximo 20 pasos
        
        if (pasos > 1)
        {
            for (int i = 1; i <= pasos; i++)
            {
                monedasMostradas = Mathf.RoundToInt(Mathf.Lerp(monedasMostradas, monedasActuales, (float)i / pasos));
                ActualizarDisplay();
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        // Asegurar que muestre el valor exacto
        monedasMostradas = monedasActuales;
        ActualizarDisplay();
        
        animandoCambio = false;
    }
    
    // 🖼️ ACTUALIZAR DISPLAY DE MONEDAS
    private void ActualizarDisplay()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = $"💰 {monedasMostradas}";
        }
    }
    
    // 💾 GUARDAR MONEDAS EN PLAYERPREFS
    private void GuardarMonedas()
    {
        PlayerPrefs.SetInt("Monedas", monedasActuales);
        PlayerPrefs.Save();
        
        if (mostrarDebug && Time.frameCount % 60 == 0) // Debug ocasional
        {
            Debug.LogError($"💾 MONEDAS GUARDADAS: {monedasActuales}");
        }
    }
    
    // 📂 CARGAR MONEDAS DESDE PLAYERPREFS
    private void CargarMonedas()
    {
        monedasActuales = PlayerPrefs.GetInt("Monedas", monedasIniciales);
        monedasMostradas = monedasActuales;
        
        if (mostrarDebug)
        {
            Debug.LogError($"📂 MONEDAS CARGADAS: {monedasActuales}");
        }
    }
    
    // 🔧 MÉTODOS PÚBLICOS PARA OTROS SCRIPTS (ARREGLADOS)
    public int GetMonedasActuales() 
    { 
        // FORZAR CURSOR VISIBLE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return monedasActuales; 
    }
    
    public int GetMonedas() => GetMonedasActuales(); // Alias para compatibilidad
    public int GetCantidadMonedas() => GetMonedasActuales(); // NUEVO: Alias requerido
    public bool TieneSuficientesMonedas(int cantidad) => monedasActuales >= cantidad;
    public bool TieneMonedas(int cantidad) => monedasActuales >= cantidad; // Alias para compatibilidad
    public bool QuitarMonedas(int cantidad) => GastarMonedas(cantidad); // Alias para compatibilidad
    
    public void SetMonedas(int cantidad)
    {
        monedasActuales = Mathf.Max(0, cantidad);
        GuardarMonedas();
        AnimarCambioMonedas();
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔧 MONEDAS ESTABLECIDAS: {monedasActuales}");
        }
    }

    // 🎯 MÉTODO PARA RESETEAR MONEDAS (PARA TESTING) - ARREGLADO
    public void ResetearMonedas()
    {
        SetMonedas(monedasIniciales);
        if (mostrarDebug)
        {
            Debug.LogError("🔄 MONEDAS RESETEADAS A VALOR INICIAL: " + monedasIniciales);
        }
    }
    
    // 🆕 NUEVO: Resetear a cero
    public void ResetearMonedasACero()
    {
        SetMonedas(0);
        if (mostrarDebug)
        {
            Debug.LogError("🔄 MONEDAS RESETEADAS A CERO");
        }
    }

    // 🆕 MÉTODOS ESTÁTICOS PARA COMPATIBILIDAD
    public static int GetMonedasStatic()
    {
        if (instancia != null)
        {
            return instancia.GetMonedasActuales();
        }
        return PlayerPrefs.GetInt("Monedas", 0);
    }
    
    public static void AgregarMonedasStatic(int cantidad)
    {
        if (instancia != null)
        {
            instancia.AgregarMonedas(cantidad);
        }
        else
        {
            int monedasActuales = PlayerPrefs.GetInt("Monedas", 0);
            PlayerPrefs.SetInt("Monedas", monedasActuales + cantidad);
            PlayerPrefs.Save();
        }
    }

    // 🔧 MÉTODO PARA OBTENER LA INSTANCIA (PATRÓN SINGLETON)
    public static SistemaMonedas GetInstancia()
    {
        return instancia;
    }
    
    // 🆕 MÉTODO ESPECIAL PARA CUANDO EL JUGADOR REVIVE
    public void RestaurarMonedasPostMuerte()
    {
        // Las monedas YA están guardadas, solo necesitamos actualizar la UI
        ActualizarDisplay();
        
        if (mostrarDebug)
        {
            Debug.LogError($"💀➡️💰 MONEDAS RESTAURADAS POST-MUERTE: {monedasActuales}");
        }
    }
    
    void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            // Guardar antes de destruir
            GuardarMonedas();
        }
    }
    
    // 🔧 MÉTODOS DE TESTING MANUAL
    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            MostrarEstadoMonedas();
        }
        
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            AgregarMonedas(10);
        }
        
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            GastarMonedas(5);
        }
    }
    
    [ContextMenu("📊 Mostrar Estado")]
    public void MostrarEstadoMonedas()
    {
        Debug.LogError("📊 ESTADO DEL SISTEMA DE MONEDAS:");
        Debug.LogError($"  - Monedas actuales: {monedasActuales}");
        Debug.LogError($"  - Monedas mostradas: {monedasMostradas}");
        Debug.LogError($"  - Instancia activa: {(instancia == this ? "SÍ" : "NO")}");
        Debug.LogError($"  - UI texto asignado: {textoMonedas != null}");
        Debug.LogError($"  - Canvas asignado: {canvasMonedas != null}");
        Debug.LogError($"  - Escena actual: {SceneManager.GetActiveScene().name}");
        Debug.LogError($"  - Guardado en PlayerPrefs: {PlayerPrefs.GetInt("Monedas", -1)}");
    }
    
    [ContextMenu("💰 Agregar 100 monedas")]
    public void TestAgregar100() => AgregarMonedas(100);
    
    [ContextMenu("💸 Gastar 50 monedas")]
    public void TestGastar50() => GastarMonedas(50);
    
    [ContextMenu("🔄 Resetear monedas")]
    public void TestResetear() => ResetearMonedas();
    
    [ContextMenu("🔧 Reconfigurar UI")]
    public void TestReconfigurarUI() => ConfigurarUIEscenaActual();
}