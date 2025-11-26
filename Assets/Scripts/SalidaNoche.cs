using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Sistema de transición de escena con ventana de confirmación + texto de interacción
/// </summary>
public class SalidaNoche : MonoBehaviour
{
    [Header("CONFIGURACIÓN DE ESCENA")]
    [SerializeField] private string escenaDestino = "Escena2";
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;
    [SerializeField] private bool mostrarDebug = true;

    [Header("CONFIGURACIÓN DE UI")]
    [SerializeField] private Canvas canvasConfirmacion;
    [SerializeField] private GameObject panelConfirmacion;
    [SerializeField] private Button botonIr;
    [SerializeField] private Button botonQuedarse;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private TextMeshProUGUI textoInteraccion; // Puedes dejar vacío en el Inspector
    [SerializeField] private bool crearUIAutomaticamente = true;

    [Header("PERSONALIZACIÓN")]
    [SerializeField] private string tituloVentana = "TRANSICIÓN NOCTURNA";
    [SerializeField] private string descripcionVentana = "¿Estás seguro de que quieres ir a la zona nocturna?";
    [SerializeField] private string textoBotonIr = "IR";
    [SerializeField] private string textoBotonQuedarse = "CANCELAR";

    private bool jugadorEnArea = false;
    private bool ventanaAbierta = false;
    private MovimientoJugador jugador;

    void Start()
    {
        if (mostrarDebug) Debug.Log("SALIDA NOCTURNA INICIADA");

        // Asegurar collider trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("Falta Collider2D en SalidaNoche!");
            return;
        }
        if (!col.isTrigger) col.isTrigger = true;

        // Crear UI completa si no existe
        if (crearUIAutomaticamente && canvasConfirmacion == null)
            CrearUICompleta();

        ConfigurarUI();
        OcultarVentana();

        // FIX 1: Asegurar que el texto de interacción exista siempre
        AsegurarTextoInteraccion();
    }

    void Update()
    {
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

    // FIX 1: Crea el texto si no existe o está destruido (funciona entre escenas)
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
            GameObject canvasObj = new GameObject("Canvas_TextoInteraccion");
            canvasEscena = canvasObj.AddComponent<Canvas>();
            canvasEscena.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasEscena.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject textoObj = new GameObject("Texto_Interaccion_SalidaNoche");
        textoObj.transform.SetParent(canvasEscena.transform, false);

        textoInteraccion = textoObj.AddComponent<TextMeshProUGUI>();
        textoInteraccion.text = $"Presiona {teclaInteraccion} para ir a {escenaDestino}";
        textoInteraccion.fontSize = 28;
        textoInteraccion.color = Color.yellow;
        textoInteraccion.fontStyle = FontStyles.Bold;
        textoInteraccion.alignment = TextAlignmentOptions.Center;

        RectTransform rect = textoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.15f);
        rect.anchorMax = new Vector2(0.5f, 0.15f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700f, 60f);
        rect.anchoredPosition = Vector2.zero;

        textoInteraccion.gameObject.SetActive(false);

        if (mostrarDebug) Debug.Log("TEXTO DE INTERACCIÓN CREADO");
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
        OcultarTextoInteraccion(); // ← Aquí estaba el problema: se mostraba al cerrar

        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(true);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug) Debug.Log("VENTANA ABIERTA");
    }

    private void CerrarVentana()
    {
        ventanaAbierta = false;
        OcultarVentana();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Solo mostramos el texto si seguimos dentro del área
        if (jugadorEnArea && !ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) Debug.Log("VENTANA CERRADA");
    }

    private void OcultarVentana()
    {
        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(false);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    private void IrAEscenaDestino()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mostrarDebug) Debug.Log($"CARGANDO ESCENA: {escenaDestino}");

        try
        {
            SceneManager.LoadScene(escenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR AL CARGAR: " + e.Message);
        }
    }

    // TRIGGERS → Aquí está el fix principal que querías
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnArea = true;
        jugador = other.GetComponent<MovimientoJugador>();

        // Solo mostrar si la ventana NO está abierta
        if (!ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) Debug.Log("JUGADOR ENTRÓ → Texto mostrado");
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

        if (mostrarDebug) Debug.Log("JUGADOR SALIÓ → Texto ocultado");
    }

    // ================== CREACIÓN DE UI COMPLETA (100% original) ==================
    private void CrearUICompleta()
    {
        if (mostrarDebug) Debug.Log("CREANDO UI COMPLETA");

        GameObject canvasObj = new GameObject("Canvas_SalidaNoche");
        canvasConfirmacion = canvasObj.AddComponent<Canvas>();
        canvasConfirmacion.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasConfirmacion.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fondo oscuro
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0f, 0f, 0f, 0.8f);
        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = rtFondo.offsetMax = Vector2.zero;

        // Panel central
        panelConfirmacion = new GameObject("Panel_Confirmacion");
        panelConfirmacion.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgPanel = panelConfirmacion.AddComponent<Image>();
        imgPanel.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        RectTransform rtPanel = panelConfirmacion.GetComponent<RectTransform>();
        rtPanel.sizeDelta = new Vector2(500f, 300f);
        rtPanel.anchorMin = rtPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rtPanel.pivot = new Vector2(0.5f, 0.5f);

        // Título
        GameObject titulo = new GameObject("Titulo");
        titulo.transform.SetParent(panelConfirmacion.transform, false);
        textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = tituloVentana;
        textoTitulo.fontSize = 32;
        textoTitulo.color = Color.yellow;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        RectTransform rtTitulo = titulo.GetComponent<RectTransform>();
        rtTitulo.sizeDelta = new Vector2(450f, 60f);
        rtTitulo.anchoredPosition = new Vector2(0f, 80f);

        // Descripción
        GameObject desc = new GameObject("Descripcion");
        desc.transform.SetParent(panelConfirmacion.transform, false);
        textoDescripcion = desc.AddComponent<TextMeshProUGUI>();
        textoDescripcion.text = descripcionVentana;
        textoDescripcion.fontSize = 20;
        textoDescripcion.color = Color.white;
        textoDescripcion.alignment = TextAlignmentOptions.Center;
        RectTransform rtDesc = desc.GetComponent<RectTransform>();
        rtDesc.sizeDelta = new Vector2(450f, 80f);
        rtDesc.anchoredPosition = new Vector2(0f, 10f);

        // Botones
        botonIr = CrearBoton(panelConfirmacion, textoBotonIr, new Vector2(-100f, -60f), new Color(0.2f, 0.6f, 0.2f));
        botonQuedarse = CrearBoton(panelConfirmacion, textoBotonQuedarse, new Vector2(100f, -60f), new Color(0.6f, 0.2f, 0.2f));

        if (mostrarDebug) Debug.Log("UI COMPLETA CREADA");
    }

    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject btnObj = new GameObject("Boton_" + texto.Replace(" ", ""));
        btnObj.transform.SetParent(padre.transform, false);

        Button btn = btnObj.AddComponent<Button>();
        Image img = btnObj.AddComponent<Image>();
        img.color = color;

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150f, 50f);
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
}