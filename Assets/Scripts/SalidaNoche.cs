using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Sistema de transición de escena con ventana de confirmación
/// Agrega este script al GameObject con Collider2D que representa la salida nocturna
/// </summary>
public class SalidaNoche : MonoBehaviour
{
    [Header("🌙 CONFIGURACIÓN DE ESCENA")]
    [SerializeField] private string escenaDestino = "Escena2"; // Escena a la que ir
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E; // Tecla para interactuar
    [SerializeField] private bool mostrarDebug = true; // Debug
    
    [Header("📱 CONFIGURACIÓN DE UI")]
    [SerializeField] private Canvas canvasConfirmacion; // Canvas para la ventana
    [SerializeField] private GameObject panelConfirmacion; // Panel de confirmación
    [SerializeField] private Button botonIr; // Botón para ir a la escena
    [SerializeField] private Button botonQuedarse; // Botón para cancelar
    [SerializeField] private TextMeshProUGUI textoTitulo; // Título de la ventana
    [SerializeField] private TextMeshProUGUI textoDescripcion; // Descripción
    [SerializeField] private TextMeshProUGUI textoInteraccion; // Texto de instrucción
    [SerializeField] private bool crearUIAutomaticamente = true; // Crear UI si no existe
    
    [Header("🎨 PERSONALIZACIÓN")]
    [SerializeField] private string tituloVentana = "🌙 TRANSICIÓN NOCTURNA";
    [SerializeField] private string descripcionVentana = "¿Estás seguro de que quieres ir a la zona nocturna?";
    [SerializeField] private string textoBotonIr = "🌙 IR";
    [SerializeField] private string textoBotonQuedarse = "❌ CANCELAR";
    
    // Variables de estado
    private bool jugadorEnArea = false;
    private bool ventanaAbierta = false;
    private MovimientoJugador jugador;
    
    void Start()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🌙 SALIDA NOCTURNA INICIADA");
        }
        
        // Verificar que tenemos un collider para detección
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogError("❌ SalidaNoche necesita un Collider2D marcado como trigger!");
            return;
        }
        
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogError("🔧 Collider configurado como trigger automáticamente");
        }
        
        // Crear UI si es necesario
        if (crearUIAutomaticamente && canvasConfirmacion == null)
        {
            CrearUICompleta();
        }
        
        // Configurar UI existente
        ConfigurarUI();
        
        // Asegurar que la ventana esté oculta al inicio
        OcultarVentana();
        
        // 🔧 CREAR TEXTO DE INTERACCIÓN SI NO EXISTE
        if (textoInteraccion == null)
        {
            CrearTextoInteraccion();
        }
    }
    
    void Update()
    {
        // Detectar si el jugador presiona E para interactuar
        if (jugadorEnArea && Input.GetKeyDown(teclaInteraccion))
        {
            if (!ventanaAbierta)
            {
                MostrarVentanaConfirmacion();
            }
            else
            {
                CerrarVentana();
            }
        }
        
        // Permitir cerrar con Escape
        if (ventanaAbierta && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarVentana();
        }
    }
    
    // 🔧 NUEVO: Crear texto de interacción si no existe
    private void CrearTextoInteraccion()
    {
        // Buscar un canvas en la escena
        Canvas canvasEscena = FindObjectOfType<Canvas>();
        
        if (canvasEscena == null)
        {
            // Crear canvas simple para el texto
            GameObject canvasObj = new GameObject("Canvas_TextoInteraccion");
            canvasEscena = canvasObj.AddComponent<Canvas>();
            canvasEscena.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasEscena.sortingOrder = 100;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Crear el texto de interacción
        GameObject textoObj = new GameObject("Texto_Interaccion_SalidaNoche");
        textoObj.transform.SetParent(canvasEscena.transform, false);
        
        textoInteraccion = textoObj.AddComponent<TextMeshProUGUI>();
        textoInteraccion.text = $"Presiona {teclaInteraccion} para ir a {escenaDestino}";
        textoInteraccion.fontSize = 24;
        textoInteraccion.color = Color.yellow;
        textoInteraccion.fontStyle = FontStyles.Bold;
        textoInteraccion.alignment = TextAlignmentOptions.Center;
        
        // Configurar posición (centro-abajo de la pantalla)
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(600f, 50f);
        rectTexto.anchorMin = new Vector2(0.5f, 0.2f);
        rectTexto.anchorMax = new Vector2(0.5f, 0.2f);
        rectTexto.pivot = new Vector2(0.5f, 0.5f);
        rectTexto.anchoredPosition = Vector2.zero;
        
        // Empezar oculto
        textoInteraccion.gameObject.SetActive(false);
        
        if (mostrarDebug)
        {
            Debug.LogError("🔧 TEXTO DE INTERACCIÓN CREADO AUTOMÁTICAMENTE");
        }
    }
    
    private void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI COMPLETA DE SALIDA NOCTURNA");
        }
        
        // Crear Canvas
        GameObject canvasObj = new GameObject("Canvas_SalidaNoche");
        canvasConfirmacion = canvasObj.AddComponent<Canvas>();
        canvasConfirmacion.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasConfirmacion.sortingOrder = 999; // Muy alto
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Crear fondo
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(canvasConfirmacion.transform, false);
        
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = new Color(0f, 0f, 0f, 0.8f);
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // Crear panel central
        panelConfirmacion = new GameObject("Panel_Confirmacion");
        panelConfirmacion.transform.SetParent(canvasConfirmacion.transform, false);
        
        Image imagenPanel = panelConfirmacion.AddComponent<Image>();
        imagenPanel.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        
        RectTransform rectPanel = panelConfirmacion.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(500f, 300f);
        rectPanel.anchorMin = new Vector2(0.5f, 0.5f);
        rectPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rectPanel.pivot = new Vector2(0.5f, 0.5f);
        rectPanel.anchoredPosition = Vector2.zero;
        
        // Crear título
        GameObject titulo = new GameObject("Titulo");
        titulo.transform.SetParent(panelConfirmacion.transform, false);
        
        textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = tituloVentana;
        textoTitulo.fontSize = 32;
        textoTitulo.color = Color.yellow;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(450f, 60f);
        rectTitulo.anchoredPosition = new Vector2(0f, 80f);
        
        // Crear descripción
        GameObject descripcion = new GameObject("Descripcion");
        descripcion.transform.SetParent(panelConfirmacion.transform, false);
        
        textoDescripcion = descripcion.AddComponent<TextMeshProUGUI>();
        textoDescripcion.text = descripcionVentana;
        textoDescripcion.fontSize = 20;
        textoDescripcion.color = Color.white;
        textoDescripcion.alignment = TextAlignmentOptions.Center;
        textoDescripcion.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform rectDescripcion = descripcion.GetComponent<RectTransform>();
        rectDescripcion.sizeDelta = new Vector2(450f, 80f);
        rectDescripcion.anchoredPosition = new Vector2(0f, 10f);
        
        // Crear botón IR
        botonIr = CrearBoton(panelConfirmacion, textoBotonIr, new Vector2(-100f, -60f), new Color(0.2f, 0.6f, 0.2f));
        
        // Crear botón CANCELAR
        botonQuedarse = CrearBoton(panelConfirmacion, textoBotonQuedarse, new Vector2(100f, -60f), new Color(0.6f, 0.2f, 0.2f));
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI COMPLETA DE SALIDA NOCTURNA CREADA");
        }
    }
    
    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject botonObj = new GameObject("Boton_" + texto.Replace(" ", ""));
        botonObj.transform.SetParent(padre.transform, false);
        
        Button boton = botonObj.AddComponent<Button>();
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = color;
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(150f, 50f);
        rectBoton.anchoredPosition = posicion;
        
        // Texto del botón
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 18;
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
    
    private void ConfigurarUI()
    {
        if (botonIr != null)
        {
            botonIr.onClick.RemoveAllListeners();
            botonIr.onClick.AddListener(IrAEscenaDestino);
        }
        
        if (botonQuedarse != null)
        {
            botonQuedarse.onClick.RemoveAllListeners();
            botonQuedarse.onClick.AddListener(CerrarVentana);
        }
    }
    
    private void MostrarVentanaConfirmacion()
    {
        ventanaAbierta = true;
        
        if (canvasConfirmacion != null)
        {
            canvasConfirmacion.gameObject.SetActive(true);
        }
        
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(true);
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Ocultar texto de interacción cuando se abre la ventana
        if (textoInteraccion != null)
        {
            textoInteraccion.gameObject.SetActive(false);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🌙 VENTANA DE SALIDA NOCTURNA MOSTRADA");
        }
    }
    
    private void CerrarVentana()
    {
        ventanaAbierta = false;
        OcultarVentana();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Mostrar texto de interacción nuevamente si el jugador sigue en el área
        if (jugadorEnArea && textoInteraccion != null)
        {
            textoInteraccion.gameObject.SetActive(true);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("❌ VENTANA DE SALIDA NOCTURNA CERRADA");
        }
    }
    
    private void OcultarVentana()
    {
        if (canvasConfirmacion != null)
        {
            canvasConfirmacion.gameObject.SetActive(false);
        }
        
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
        }
    }
    
    private void IrAEscenaDestino()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🚀 CARGANDO ESCENA: " + escenaDestino);
        }
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        try
        {
            SceneManager.LoadScene(escenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR AL CARGAR ESCENA: " + e.Message);
            try
            {
                int indiceEscena = escenaDestino.Contains("2") ? 2 : 1;
                SceneManager.LoadScene(indiceEscena);
                Debug.LogError("✅ Escena cargada por índice: " + indiceEscena);
            }
            catch
            {
                Debug.LogError("❌ Tampoco se pudo cargar por índice");
                CerrarVentana();
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnArea = true;
            jugador = other.GetComponent<MovimientoJugador>();
            
            if (mostrarDebug)
            {
                Debug.LogError("🌙 JUGADOR ENTRÓ EN ÁREA DE SALIDA NOCTURNA");
            }
            
            // 🔧 MOSTRAR TEXTO DE INTERACCIÓN
            if (textoInteraccion != null && !ventanaAbierta)
            {
                textoInteraccion.gameObject.SetActive(true);
                textoInteraccion.text = $"Presiona {teclaInteraccion} para ir a {escenaDestino}";
                
                if (mostrarDebug)
                {
                    Debug.LogError("💬 TEXTO DE INTERACCIÓN MOSTRADO");
                }
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnArea = false;
            jugador = null;
            
            if (ventanaAbierta)
            {
                CerrarVentana();
            }
            
            // 🔧 OCULTAR TEXTO DE INTERACCIÓN
            if (textoInteraccion != null)
            {
                textoInteraccion.gameObject.SetActive(false);
                
                if (mostrarDebug)
                {
                    Debug.LogError("💬 TEXTO DE INTERACCIÓN OCULTADO");
                }
            }
            
            if (mostrarDebug)
            {
                Debug.LogError("🌙 JUGADOR SALIÓ DEL ÁREA");
            }
        }
    }
}