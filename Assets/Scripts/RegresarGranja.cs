using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Sistema de regreso a la granja (Escenario1) con ventana de confirmación
/// </summary>
public class RegresarGranja : MonoBehaviour
{
    [Header("CONFIGURACIÓN DE ESCENA")]
    [SerializeField] private string escenaGranja = "Escena1";
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;
    [SerializeField] private bool mostrarDebug = true;

    [Header("CONFIGURACIÓN DE UI")]
    [SerializeField] private Canvas canvasConfirmacion;
    [SerializeField] private GameObject panelConfirmacion;
    [SerializeField] private Button botonRegresar;
    [SerializeField] private Button botonQuedarseAqui;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private TextMeshProUGUI textoInteraccion; // Puedes dejar vacío en el Inspector
    [SerializeField] private bool crearUIAutomaticamente = true;

    [Header("PERSONALIZACIÓN")]
    [SerializeField] private string tituloVentana = "🏡 REGRESO A LA GRANJA";
    [SerializeField] private string descripcionVentana = "¿Estás seguro de que quieres regresar a tu granja?\n\n🌱 Podrás revisar tus cultivos\n💰 Gestionar recursos\n🔧 Descansar y prepararte";
    [SerializeField] private string textoBotonRegresar = "🏡 REGRESAR";
    [SerializeField] private string textoBotonQuedarseAqui = "❌ CANCELAR";

    // Variables de estado
    private bool jugadorEnArea = false;
    private bool ventanaAbierta = false;
    private MovimientoJugador jugador;

    void Start()
    {
        if (mostrarDebug) Debug.LogError("🏡 REGRESO A GRANJA INICIADO");

        // Asegurar collider trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("❌ Falta Collider2D en RegresarGranja!");
            return;
        }
        if (!col.isTrigger) col.isTrigger = true;

        // Crear UI completa si no existe
        if (crearUIAutomaticamente && canvasConfirmacion == null)
            CrearUICompleta();

        ConfigurarUI();
        OcultarVentana();

        // ASEGURAR que el texto de interacción exista siempre
        AsegurarTextoInteraccion();
    }

    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (jugadorEnArea && Input.GetKeyDown(teclaInteraccion))
        {
            if (!ventanaAbierta)
                MostrarVentanaConfirmacion();
            else
                CerrarVentana();
        }

        if (ventanaAbierta && Input.GetKeyDown(KeyCode.Escape))
            CerrarVentana();
    }

    // CREAR el texto si no existe o está destruido (funciona entre escenas)
    private void AsegurarTextoInteraccion()
    {
        if (textoInteraccion != null && textoInteraccion.gameObject != null)
            return;

        CrearTextoInteraccion();
    }

    private void CrearTextoInteraccion()
    {
        Canvas canvasEscena = FindObjectOfType<Canvas>();
        if (canvasEscena == null)
        {
            GameObject canvasObj = new GameObject("Canvas_TextoInteraccionRegreso");
            canvasEscena = canvasObj.AddComponent<Canvas>();
            canvasEscena.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasEscena.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject textoObj = new GameObject("Texto_Interaccion_RegresarGranja");
        textoObj.transform.SetParent(canvasEscena.transform, false);

        textoInteraccion = textoObj.AddComponent<TextMeshProUGUI>();
        textoInteraccion.text = $"Presiona {teclaInteraccion} para regresar a la granja 🏡";
        textoInteraccion.fontSize = 28;
        textoInteraccion.color = new Color(0.2f, 1f, 0.3f, 1f); // Verde brillante
        textoInteraccion.fontStyle = FontStyles.Bold;
        textoInteraccion.alignment = TextAlignmentOptions.Center;

        RectTransform rect = textoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f); // Parte superior
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700f, 60f);
        rect.anchoredPosition = Vector2.zero;

        textoInteraccion.gameObject.SetActive(false);

        if (mostrarDebug) Debug.LogError("🏡 TEXTO DE INTERACCIÓN CREADO");
    }

    private void MostrarTextoInteraccion()
    {
        AsegurarTextoInteraccion();
        if (textoInteraccion != null)
            textoInteraccion.gameObject.SetActive(true);
    }

    private void OcultarTextoInteraccion()
    {
        if (textoInteraccion != null)
            textoInteraccion.gameObject.SetActive(false);
    }

    private void MostrarVentanaConfirmacion()
    {
        ventanaAbierta = true;
        OcultarTextoInteraccion();

        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(true);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);

        // NO pausar el tiempo - mantener fluido
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug) Debug.LogError("🏡 VENTANA DE REGRESO ABIERTA");
    }

    private void CerrarVentana()
    {
        ventanaAbierta = false;
        OcultarVentana();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Solo mostramos el texto si seguimos dentro del área
        if (jugadorEnArea && !ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) Debug.LogError("❌ VENTANA DE REGRESO CERRADA");
    }

    private void OcultarVentana()
    {
        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(false);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    private void RegresarAGranja()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug) Debug.LogError($"🏡 CARGANDO GRANJA: {escenaGranja}");

        // Limpiar sistemas antes del cambio
        LimpiarSistemasAntesCambioEscena();

        try
        {
            SceneManager.LoadScene(escenaGranja);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR AL CARGAR GRANJA: " + e.Message);
            
            // Intentar cargar por índice 0 como fallback
            try
            {
                SceneManager.LoadScene(0);
            }
            catch
            {
                Debug.LogError("❌ Tampoco se pudo cargar escena por índice 0");
            }
        }
    }

    private void LimpiarSistemasAntesCambioEscena()
    {
        Debug.LogError("🧹 LIMPIANDO SISTEMAS ANTES DE REGRESAR...");

        // Limpiar controladores de zombies
        ControladorZombies[] controladores = FindObjectsByType<ControladorZombies>(FindObjectsSortMode.None);
        foreach (var controlador in controladores)
        {
            if (controlador != null)
            {
                controlador.StopAllCoroutines();
                controlador.CancelInvoke();
            }
        }

        // Limpiar enemigos individuales
        ControladorEnemigo[] enemigos = FindObjectsByType<ControladorEnemigo>(FindObjectsSortMode.None);
        foreach (var enemigo in enemigos)
        {
            if (enemigo != null)
            {
                enemigo.StopAllCoroutines();
                enemigo.CancelInvoke();
            }
        }

        // Limpiar canvas de muerte si existe
        CanvasMuerte canvasMuerte = FindObjectOfType<CanvasMuerte>();
        if (canvasMuerte != null)
        {
            canvasMuerte.StopAllCoroutines();
            canvasMuerte.CancelInvoke();
        }

        Debug.LogError("✅ LIMPIEZA COMPLETA - LISTO PARA REGRESAR");
    }

    // TRIGGERS → Funciona igual que SalidaNoche
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnArea = true;
        jugador = other.GetComponent<MovimientoJugador>();

        // Solo mostrar si la ventana NO está abierta
        if (!ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) Debug.LogError("🏡 JUGADOR ENTRÓ → Texto mostrado");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnArea = false;

        // Siempre ocultar al salir
        OcultarTextoInteraccion();

        // Si la ventana estaba abierta y sales → cerrarla
        if (ventanaAbierta)
            CerrarVentana();

        if (mostrarDebug) Debug.LogError("🏡 JUGADOR SALIÓ → Texto ocultado");
    }

    // === CREACIÓN DE UI COMPLETA ===
    private void CrearUICompleta()
    {
        if (mostrarDebug) Debug.LogError("🎨 CREANDO UI COMPLETA DE REGRESO");

        GameObject canvasObj = new GameObject("Canvas_RegresarGranja");
        canvasConfirmacion = canvasObj.AddComponent<Canvas>();
        canvasConfirmacion.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasConfirmacion.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fondo semi-transparente verde
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0f, 0.2f, 0f, 0.6f); // Verde sutil
        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = rtFondo.offsetMax = Vector2.zero;

        // Panel central
        panelConfirmacion = new GameObject("Panel_ConfirmacionRegreso");
        panelConfirmacion.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgPanel = panelConfirmacion.AddComponent<Image>();
        imgPanel.color = new Color(0.1f, 0.3f, 0.1f, 0.95f); // Verde oscuro
        RectTransform rtPanel = panelConfirmacion.GetComponent<RectTransform>();
        rtPanel.sizeDelta = new Vector2(550f, 400f);
        rtPanel.anchorMin = rtPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rtPanel.pivot = new Vector2(0.5f, 0.5f);

        // Título
        GameObject titulo = new GameObject("Titulo");
        titulo.transform.SetParent(panelConfirmacion.transform, false);
        textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = tituloVentana;
        textoTitulo.fontSize = 36;
        textoTitulo.color = new Color(0.3f, 1f, 0.4f, 1f); // Verde brillante
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        RectTransform rtTitulo = titulo.GetComponent<RectTransform>();
        rtTitulo.sizeDelta = new Vector2(500f, 70f);
        rtTitulo.anchoredPosition = new Vector2(0f, 120f);

        // Descripción
        GameObject desc = new GameObject("Descripcion");
        desc.transform.SetParent(panelConfirmacion.transform, false);
        textoDescripcion = desc.AddComponent<TextMeshProUGUI>();
        textoDescripcion.text = descripcionVentana;
        textoDescripcion.fontSize = 22;
        textoDescripcion.color = Color.white;
        textoDescripcion.alignment = TextAlignmentOptions.Center;
        textoDescripcion.textWrappingMode = TextWrappingModes.Normal;
        RectTransform rtDesc = desc.GetComponent<RectTransform>();
        rtDesc.sizeDelta = new Vector2(480f, 120f);
        rtDesc.anchoredPosition = new Vector2(0f, 20f);

        // Botones
        botonRegresar = CrearBoton(panelConfirmacion, textoBotonRegresar, new Vector2(-120f, -100f), new Color(0.2f, 0.7f, 0.3f));
        botonQuedarseAqui = CrearBoton(panelConfirmacion, textoBotonQuedarseAqui, new Vector2(120f, -100f), new Color(0.7f, 0.3f, 0.2f));

        if (mostrarDebug) Debug.LogError("✅ UI COMPLETA DE REGRESO CREADA");
    }

    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject btnObj = new GameObject("Boton_" + texto.Replace(" ", "").Replace("🏡", "Casa").Replace("❌", "Cancelar"));
        btnObj.transform.SetParent(padre.transform, false);

        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = color;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180f, 60f);
        rt.anchoredPosition = posicion;

        GameObject txtObj = new GameObject("Texto");
        txtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = texto;
        txt.fontSize = 18;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;

        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = rtTxt.offsetMax = Vector2.zero;

        return btn;
    }

    private void ConfigurarUI()
    {
        if (botonRegresar != null)
        {
            botonRegresar.onClick.RemoveAllListeners();
            botonRegresar.onClick.AddListener(RegresarAGranja);
        }

        if (botonQuedarseAqui != null)
        {
            botonQuedarseAqui.onClick.RemoveAllListeners();
            botonQuedarseAqui.onClick.AddListener(CerrarVentana);
        }
    }

    // === MÉTODOS DE TESTING ===
    [ContextMenu("🧪 Test - Mostrar Ventana")]
    public void TestMostrarVentana()
    {
        jugadorEnArea = true;
        MostrarVentanaConfirmacion();
    }

    [ContextMenu("🧪 Test - Regresar Directo")]
    public void TestRegresarDirecto()
    {
        Debug.LogError("🧪 TEST: Regresando directamente a la granja");
        RegresarAGranja();
    }
}
