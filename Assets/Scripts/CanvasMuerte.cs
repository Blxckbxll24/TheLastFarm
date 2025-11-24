using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Canvas que aparece cuando el jugador muere
/// Permite revivirlo manteniendo sus monedas
/// </summary>
public class CanvasMuerte : MonoBehaviour
{
    [Header("📱 REFERENCIAS UI")]
    [SerializeField] private Canvas canvasMuerte;
    [SerializeField] private GameObject panelMuerte;
    [SerializeField] private Button botonRevivir;
    [SerializeField] private Button botonMenuPrincipal;
    [SerializeField] private TextMeshProUGUI textoMuerte;
    [SerializeField] private TextMeshProUGUI textoMonedas;
    
    [Header("🔧 CONFIGURACIÓN")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private bool mostrarDebug = false; // REDUCIDO DEBUG
    [SerializeField] private bool conservarMonedas = true;
    
    // Variables de estado
    private static CanvasMuerte instanciaActual;
    private bool panelMostrado = false;
    
    void Awake()
    {
        // Solo mantener una instancia por escena
        if (instanciaActual != null && instanciaActual != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instanciaActual = this;
    }
    
    void Start()
    {
        // Crear UI automáticamente si es necesario
        if (crearUIAutomaticamente)
        {
            CrearUICompleta();
        }
        
        // Configurar botones
        ConfigurarBotones();
        
        // Ocultar panel al inicio
        OcultarPanel();
    }
    
    private void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI COMPLETA DE MUERTE...");
        }
        
        // Crear Canvas si no existe
        if (canvasMuerte == null)
        {
            GameObject canvasObj = new GameObject("Canvas_Muerte");
            canvasMuerte = canvasObj.AddComponent<Canvas>();
            canvasMuerte.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasMuerte.sortingOrder = 1000; // Muy arriba
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Crear panel principal
        if (panelMuerte == null)
        {
            panelMuerte = new GameObject("Panel_Muerte");
            panelMuerte.transform.SetParent(canvasMuerte.transform, false);
            
            // Fondo oscuro
            Image fondoMuerte = panelMuerte.AddComponent<Image>();
            fondoMuerte.color = new Color(0f, 0f, 0f, 0.8f);
            
            RectTransform rectPanel = panelMuerte.GetComponent<RectTransform>();
            rectPanel.anchorMin = Vector2.zero;
            rectPanel.anchorMax = Vector2.one;
            rectPanel.offsetMin = Vector2.zero;
            rectPanel.offsetMax = Vector2.zero;
        }
        
        // Crear contenido central
        GameObject panelCentral = new GameObject("Panel_Central");
        panelCentral.transform.SetParent(panelMuerte.transform, false);
        
        Image fondoCentral = panelCentral.AddComponent<Image>();
        fondoCentral.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform rectCentral = panelCentral.GetComponent<RectTransform>();
        rectCentral.sizeDelta = new Vector2(500f, 400f);
        rectCentral.anchorMin = new Vector2(0.5f, 0.5f);
        rectCentral.anchorMax = new Vector2(0.5f, 0.5f);
        rectCentral.pivot = new Vector2(0.5f, 0.5f);
        rectCentral.anchoredPosition = Vector2.zero;
        
        // Crear título
        CrearTexto(panelCentral, "💀 HAS MUERTO", new Vector2(0f, 120f), 36, FontStyles.Bold, Color.red);
        
        // Texto sobre conservar monedas
        if (conservarMonedas)
        {
            CrearTexto(panelCentral, "💰 TUS MONEDAS SE CONSERVAN", new Vector2(0f, 70f), 20, FontStyles.Normal, Color.yellow);
            
            // Mostrar cantidad actual de monedas
            textoMonedas = CrearTexto(panelCentral, "Monedas: 0", new Vector2(0f, 40f), 24, FontStyles.Bold, Color.cyan);
        }
        
        CrearTexto(panelCentral, "¿Qué deseas hacer?", new Vector2(0f, 0f), 20);
        
        // Crear botones
        botonRevivir = CrearBoton(panelCentral, "🔄 REVIVIR", new Vector2(0f, -50f), Color.green);
        botonMenuPrincipal = CrearBoton(panelCentral, "🏠 MENÚ PRINCIPAL", new Vector2(0f, -120f), Color.blue);
    }
    
    private TextMeshProUGUI CrearTexto(GameObject padre, string texto, Vector2 posicion, float tamano = 18, FontStyles estilo = FontStyles.Normal, Color? color = null)
    {
        GameObject textoObj = new GameObject("Texto_" + texto.Replace(" ", "_"));
        textoObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoComp = textoObj.AddComponent<TextMeshProUGUI>();
        textoComp.text = texto;
        textoComp.fontSize = tamano;
        textoComp.color = color ?? Color.white;
        textoComp.fontStyle = estilo;
        textoComp.alignment = TextAlignmentOptions.Center;
        textoComp.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(450f, tamano + 10f);
        rectTexto.anchoredPosition = posicion;
        
        return textoComp;
    }
    
    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject botonObj = new GameObject("Boton_" + texto.Replace(" ", "_"));
        botonObj.transform.SetParent(padre.transform, false);
        
        Button boton = botonObj.AddComponent<Button>();
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = color;
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(300f, 50f);
        rectBoton.anchoredPosition = posicion;
        
        // Texto del botón
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 20;
        textoBoton.color = Color.white;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        return boton;
    }
    
    private void ConfigurarBotones()
    {
        if (botonRevivir != null)
        {
            botonRevivir.onClick.RemoveAllListeners();
            botonRevivir.onClick.AddListener(RevivirJugador);
        }
        
        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.onClick.RemoveAllListeners();
            botonMenuPrincipal.onClick.AddListener(IrMenuPrincipal);
        }
    }
    
    // MÉTODO PRINCIPAL PARA MOSTRAR EL PANEL
    public void MostrarPanelMuerte()
    {
        if (panelMostrado) return;
        
        panelMostrado = true;
        
        // Activar UI
        if (canvasMuerte != null)
        {
            canvasMuerte.gameObject.SetActive(true);
        }
        
        if (panelMuerte != null)
        {
            panelMuerte.SetActive(true);
        }
        
        // Actualizar información de monedas
        ActualizarInfoMonedas();
        
        // NO pausar juego, solo mostrar cursor
        Time.timeScale = 1f; // MANTENER TIEMPO NORMAL
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    // Actualizar información de monedas en el panel
    private void ActualizarInfoMonedas()
    {
        if (textoMonedas != null && conservarMonedas)
        {
            SistemaMonedas sistemaMonedas = SistemaMonedas.GetInstancia();
            if (sistemaMonedas != null)
            {
                int monedasActuales = sistemaMonedas.GetMonedasActuales();
                textoMonedas.text = $"💰 Monedas conservadas: {monedasActuales}";
            }
            else
            {
                // Usar método estático como backup
                int monedasActuales = SistemaMonedas.GetMonedasStatic();
                textoMonedas.text = $"💰 Monedas conservadas: {monedasActuales}";
            }
        }
    }
    
    private void RevivirJugador()
    {
        // Buscar al jugador
        MovimientoJugador jugador = FindObjectOfType<MovimientoJugador>();
        
        if (jugador != null)
        {
            // Revivir al jugador
            jugador.ResetearEstadoJugador();
        }
        
        // Ocultar panel y restaurar juego
        OcultarPanel();
    }
    
    private void IrMenuPrincipal()
    {
        // Restaurar tiempo antes de cambiar escena
        Time.timeScale = 1f;
        
        try
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
        catch
        {
            try
            {
                SceneManager.LoadScene(0); // Intentar por índice
            }
            catch
            {
                // Al menos ocultar el panel
                OcultarPanel();
            }
        }
    }
    
    private void OcultarPanel()
    {
        panelMostrado = false;
        
        if (canvasMuerte != null)
        {
            canvasMuerte.gameObject.SetActive(false);
        }
        
        if (panelMuerte != null)
        {
            panelMuerte.SetActive(false);
        }
        
        // Mantener juego normal y cursor visible
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (mostrarDebug)
        {
            Debug.LogError("❌ PANEL DE MUERTE OCULTADO - CURSOR VISIBLE");
        }
    }
    
    // Método estático para limpiar instancia
    public static void LimpiarInstancia()
    {
        instanciaActual = null;
    }
    
    void OnDestroy()
    {
        if (instanciaActual == this)
        {
            instanciaActual = null;
        }
    }
}
