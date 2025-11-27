using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Sistema de transición de escena universal - Configurable en el Inspector
/// Cambia el nombre del archivo por el destino específico (ej: IrAEscena3, IrAMina, IrACiudad, etc.)
/// </summary>
public class IrAEscena : MonoBehaviour
{
    [Header("🎯 CONFIGURACIÓN DE DESTINO")]
    [SerializeField] private string escenaDestino = "Escena3"; // ← CAMBIAR AQUÍ EL DESTINO
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;
    [SerializeField] private bool mostrarDebug = true;

    [Header("🎨 CONFIGURACIÓN DE UI")]
    [SerializeField] private Canvas canvasConfirmacion;
    [SerializeField] private GameObject panelConfirmacion;
    [SerializeField] private Button botonIr;
    [SerializeField] private Button botonQuedarseAqui;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoDescripcion;
    [SerializeField] private TextMeshProUGUI textoInteraccion;
    [SerializeField] private bool crearUIAutomaticamente = true;

    [Header("✨ PERSONALIZACIÓN VISUAL")]
    [SerializeField] private string tituloVentana = "🚀 VIAJAR A NUEVA ZONA";
    [SerializeField] private string descripcionVentana = "¿Estás seguro de que quieres ir a esta nueva zona?\n\n⚔️ Prepárate para nuevos desafíos\n💀 Enemigos más fuertes\n🎁 Mejores recompensas";
    [SerializeField] private string textoBotonIr = "🚀 ¡VAMOS!";
    [SerializeField] private string textoBotonQuedarseAqui = "❌ CANCELAR";
    [SerializeField] private Color colorTema = new Color(0.3f, 0.6f, 1f, 1f); // Azul por defecto
    [SerializeField] private string emojiZona = "🚀"; // Emoji para identificar la zona

    [Header("🔧 CONFIGURACIÓN AVANZADA")]
    [SerializeField] private bool requiereConfirmacion = true;
    [SerializeField] private bool pausarJuegoEnConfirmacion = false;
    [SerializeField] private bool limpiarSistemasAntesCambio = true;
    [SerializeField] private bool guardarProgreso = false; // Futuro: guardar estado antes de cambiar

    // Variables de estado
    private bool jugadorEnArea = false;
    private bool ventanaAbierta = false;
    private MovimientoJugador jugador;

    void Start()
    {
        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} TRANSICIÓN A {escenaDestino.ToUpper()} INICIADA");

        // Asegurar collider trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"❌ Falta Collider2D en IrA{escenaDestino}!");
            return;
        }
        if (!col.isTrigger) col.isTrigger = true;

        // Auto-configurar textos según destino
        AutoConfigurarSegunDestino();

        // Crear UI si es necesario
        if (crearUIAutomaticamente && canvasConfirmacion == null)
            CrearUICompleta();

        ConfigurarUI();
        OcultarVentana();

        // Asegurar texto de interacción
        AsegurarTextoInteraccion();
    }

    // 🎯 AUTO-CONFIGURACIÓN SEGÚN EL DESTINO
    private void AutoConfigurarSegunDestino()
    {
        string destino = escenaDestino.ToLower();
        
        // Configuraciones predefinidas según la escena
        if (destino.Contains("escena3") || destino.Contains("3"))
        {
            tituloVentana = "🌋 IR A LA ZONA VOLCÁNICA";
            descripcionVentana = "¿Preparado para explorar la zona volcánica?\n\n🔥 Temperaturas extremas\n👹 Enemigos de fuego\n💎 Minerales raros";
            textoBotonIr = "🌋 ¡A LA LAVA!";
            emojiZona = "🌋";
            colorTema = new Color(1f, 0.3f, 0.1f, 1f); // Rojo volcánico
        }
        else if (destino.Contains("mina") || destino.Contains("mine"))
        {
            tituloVentana = "⛏️ ENTRAR A LAS MINAS";
            descripcionVentana = "¿Listo para adentrarte en las profundidades?\n\n💎 Minerales preciosos\n🕷️ Criaturas subterráneas\n🌑 Oscuridad total";
            textoBotonIr = "⛏️ ¡A MINAR!";
            emojiZona = "⛏️";
            colorTema = new Color(0.4f, 0.2f, 0.6f, 1f); // Púrpura oscuro
        }
        else if (destino.Contains("ciudad") || destino.Contains("city"))
        {
            tituloVentana = "🏙️ VIAJAR A LA CIUDAD";
            descripcionVentana = "¿Quieres ir a la ciudad?\n\n🏪 Tiendas y comercio\n👥 Muchos NPCs\n🔧 Mejoras disponibles";
            textoBotonIr = "🏙️ ¡A LA CIUDAD!";
            emojiZona = "🏙️";
            colorTema = new Color(0.2f, 0.4f, 0.8f, 1f); // Azul ciudad
        }
        else if (destino.Contains("boss") || destino.Contains("jefe"))
        {
            tituloVentana = "💀 ENFRENTAR AL JEFE";
            descripcionVentana = "¡ZONA DE JEFE DETECTADA!\n\n💀 Enemigo muy poderoso\n⚔️ Combate épico\n🎁 Recompensas únicas";
            textoBotonIr = "💀 ¡AL COMBATE!";
            emojiZona = "💀";
            colorTema = new Color(0.8f, 0.1f, 0.1f, 1f); // Rojo intenso
        }
        // Agregar más configuraciones según necesites...
        
        if (mostrarDebug)
        {
            Debug.LogError($"🎨 AUTO-CONFIGURADO PARA: {escenaDestino}");
            Debug.LogError($"  - Tema: {emojiZona} | Color: {colorTema}");
        }
    }

    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (jugadorEnArea && Input.GetKeyDown(teclaInteraccion))
        {
            if (requiereConfirmacion)
            {
                if (!ventanaAbierta)
                    MostrarVentanaConfirmacion();
                else
                    CerrarVentana();
            }
            else
            {
                // Ir directamente sin confirmación
                IrAEscenaDestino();
            }
        }

        if (ventanaAbierta && Input.GetKeyDown(KeyCode.Escape))
            CerrarVentana();
    }

    // CREAR el texto si no existe
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
            GameObject canvasObj = new GameObject($"Canvas_TextoInteraccion_{escenaDestino}");
            canvasEscena = canvasObj.AddComponent<Canvas>();
            canvasEscena.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasEscena.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject textoObj = new GameObject($"Texto_Interaccion_{escenaDestino}");
        textoObj.transform.SetParent(canvasEscena.transform, false);

        textoInteraccion = textoObj.AddComponent<TextMeshProUGUI>();
        textoInteraccion.text = $"Presiona {teclaInteraccion} para ir a {escenaDestino} {emojiZona}";
        textoInteraccion.fontSize = 28;
        textoInteraccion.color = colorTema;
        textoInteraccion.fontStyle = FontStyles.Bold;
        textoInteraccion.alignment = TextAlignmentOptions.Center;

        RectTransform rect = textoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.85f);
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700f, 60f);
        rect.anchoredPosition = Vector2.zero;

        textoInteraccion.gameObject.SetActive(false);

        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} TEXTO DE INTERACCIÓN CREADO PARA {escenaDestino}");
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

        // Pausar solo si está configurado
        if (pausarJuegoEnConfirmacion)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} VENTANA DE CONFIRMACIÓN ABIERTA PARA {escenaDestino}");
    }

    private void CerrarVentana()
    {
        ventanaAbierta = false;
        OcultarVentana();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Solo mostrar texto si seguimos en el área
        if (jugadorEnArea && !ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) 
            Debug.LogError($"❌ VENTANA DE {escenaDestino} CERRADA");
    }

    private void OcultarVentana()
    {
        if (canvasConfirmacion != null) canvasConfirmacion.gameObject.SetActive(false);
        if (panelConfirmacion != null) panelConfirmacion.SetActive(false);
    }

    // 🚀 MÉTODO PRINCIPAL PARA IR A LA ESCENA
    private void IrAEscenaDestino()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} CARGANDO DESTINO: {escenaDestino}");

        // Guardar progreso si está activado
        if (guardarProgreso)
        {
            GuardarProgresoAntesCambio();
        }

        // Limpiar sistemas si está activado
        if (limpiarSistemasAntesCambio)
        {
            LimpiarSistemasAntesCambioEscena();
        }

        try
        {
            SceneManager.LoadScene(escenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ERROR AL CARGAR {escenaDestino}: " + e.Message);
            
            // Intentar por índice como fallback
            try
            {
                int indiceEscena = ObtenerIndiceEscena(escenaDestino);
                if (indiceEscena >= 0)
                {
                    SceneManager.LoadScene(indiceEscena);
                    Debug.LogError($"✅ Cargado {escenaDestino} por índice {indiceEscena}");
                }
                else
                {
                    Debug.LogError($"❌ No se pudo determinar índice para {escenaDestino}");
                }
            }
            catch
            {
                Debug.LogError($"❌ Error total cargando {escenaDestino}");
            }
        }
    }

    // 🔍 OBTENER ÍNDICE DE ESCENA COMO FALLBACK
    private int ObtenerIndiceEscena(string nombreEscena)
    {
        string nombre = nombreEscena.ToLower();
        
        // Mapeo básico de nombres a índices
        if (nombre.Contains("menu")) return 0;
        if (nombre.Contains("escena1") || nombre.Contains("1")) return 1;
        if (nombre.Contains("escena2") || nombre.Contains("2")) return 2;
        if (nombre.Contains("escena3") || nombre.Contains("3")) return 3;
        if (nombre.Contains("escena4") || nombre.Contains("4")) return 4;
        
        return -1; // No encontrado
    }

    // 💾 GUARDAR PROGRESO (FUTURO)
    private void GuardarProgresoAntesCambio()
    {
        Debug.LogError("💾 GUARDANDO PROGRESO...");
        
        // Aquí puedes agregar lógica de guardado específica
        // Ejemplo: guardar posición, vida, items, etc.
        
        // Guardar escena actual
        PlayerPrefs.SetString("UltimaEscena", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        
        Debug.LogError("✅ PROGRESO GUARDADO");
    }

    private void LimpiarSistemasAntesCambioEscena()
    {
        Debug.LogError($"🧹 LIMPIANDO SISTEMAS ANTES DE IR A {escenaDestino}...");

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

        Debug.LogError("✅ LIMPIEZA COMPLETA - LISTO PARA VIAJAR");
    }

    // TRIGGERS
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        jugadorEnArea = true;
        jugador = other.GetComponent<MovimientoJugador>();

        // Solo mostrar si la ventana NO está abierta
        if (!ventanaAbierta)
            MostrarTextoInteraccion();

        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} JUGADOR ENTRÓ EN ZONA DE {escenaDestino} → Texto mostrado");
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

        if (mostrarDebug) 
            Debug.LogError($"{emojiZona} JUGADOR SALIÓ DE ZONA DE {escenaDestino} → Texto ocultado");
    }

    // === CREACIÓN DE UI COMPLETA ===
    private void CrearUICompleta()
    {
        if (mostrarDebug) 
            Debug.LogError($"🎨 CREANDO UI COMPLETA PARA {escenaDestino}");

        GameObject canvasObj = new GameObject($"Canvas_{escenaDestino}");
        canvasConfirmacion = canvasObj.AddComponent<Canvas>();
        canvasConfirmacion.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasConfirmacion.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fondo con color del tema
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgFondo = fondo.AddComponent<Image>();
        Color colorFondo = colorTema;
        colorFondo.a = 0.6f; // Semi-transparente
        imgFondo.color = colorFondo;
        RectTransform rtFondo = fondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = rtFondo.offsetMax = Vector2.zero;

        // Panel central
        panelConfirmacion = new GameObject($"Panel_Confirmacion_{escenaDestino}");
        panelConfirmacion.transform.SetParent(canvasConfirmacion.transform, false);
        Image imgPanel = panelConfirmacion.AddComponent<Image>();
        Color colorPanel = colorTema;
        colorPanel.a = 0.95f;
        imgPanel.color = colorPanel;
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
        textoTitulo.color = Color.white;
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
        botonIr = CrearBoton(panelConfirmacion, textoBotonIr, new Vector2(-120f, -100f), new Color(0.2f, 0.8f, 0.2f));
        botonQuedarseAqui = CrearBoton(panelConfirmacion, textoBotonQuedarseAqui, new Vector2(120f, -100f), new Color(0.8f, 0.2f, 0.2f));

        if (mostrarDebug) 
            Debug.LogError($"✅ UI COMPLETA CREADA PARA {escenaDestino}");
    }

    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject btnObj = new GameObject($"Boton_{texto.Replace(" ", "").Replace(emojiZona, "")}");
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
        if (botonIr != null)
        {
            botonIr.onClick.RemoveAllListeners();
            botonIr.onClick.AddListener(IrAEscenaDestino);
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

    [ContextMenu("🧪 Test - Ir Directo")]
    public void TestIrDirecto()
    {
        Debug.LogError($"🧪 TEST: Yendo directamente a {escenaDestino}");
        IrAEscenaDestino();
    }

    [ContextMenu("📋 Mostrar Configuración")]
    public void MostrarConfiguracion()
    {
        Debug.LogError($"📋 CONFIGURACIÓN DE {gameObject.name}:");
        Debug.LogError($"  🎯 Destino: {escenaDestino}");
        Debug.LogError($"  {emojiZona} Tema: {colorTema}");
        Debug.LogError($"  🔧 Confirmación: {requiereConfirmacion}");
        Debug.LogError($"  ⏸️ Pausar: {pausarJuegoEnConfirmacion}");
        Debug.LogError($"  🧹 Limpiar: {limpiarSistemasAntesCambio}");
        Debug.LogError($"  💾 Guardar: {guardarProgreso}");
    }
}
