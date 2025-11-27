using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuPrincipal : MonoBehaviour
{
    [Header("🎮 Configuración del Menú")]
    [SerializeField] private bool crearMenuAutomaticamente = true;
    [SerializeField] private string nombreEscenaJuego = "Escena1"; // Cambia por el nombre de tu escena de juego
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎨 Estilo")]
    [SerializeField] private Color colorFondo = new Color(0.1f, 0.2f, 0.3f, 1f);
    [SerializeField] private Color colorBotonNormal = new Color(0.2f, 0.4f, 0.6f, 0.9f);
    [SerializeField] private Color colorBotonHover = new Color(0.3f, 0.5f, 0.7f, 1f);
    [SerializeField] private Color colorTexto = Color.white;
    [SerializeField] private Color colorTitulo = Color.yellow;
    
    [Header("📱 Referencias UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject panelMenuPrincipal;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelCreditos;
    
    [Header("🎵 Audio")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip sonidoBoton;
    [SerializeField] private AudioSource audioSource;
    
    // Referencias de botones
    private Button botonJugar;
    private Button botonOpciones;
    private Button botonCreditos;
    private Button botonSalir;
    private Button botonVolverOpciones;
    private Button botonVolverCreditos;
    
    // Variables de estado
    private bool enMenuOpciones = false;
    private bool enMenuCreditos = false;
    
    void Start()
    {
        // 🔧 LIMPIEZA AL LLEGAR AL MENÚ PRINCIPAL
        LimpiarEstadoJuego();
        
        if (crearMenuAutomaticamente)
        {
            CrearMenuCompleto();
        }
        
        ConfigurarAudio();
        
        if (mostrarDebug)
        {
            Debug.LogError("🎮 MENÚ PRINCIPAL INICIADO");
        }
    }
    
    // 🔧 NUEVO: LIMPIAR ESTADO DEL JUEGO AL VOLVER AL MENÚ
    private void LimpiarEstadoJuego()
    {
        Debug.LogError("🧹 LIMPIANDO ESTADO DEL JUEGO EN MENÚ PRINCIPAL...");
        
        // 1. Asegurar tiempo normal
        Time.timeScale = 1f;
        
        // 2. FORZAR cursor visible SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 🆕 3. RESETEAR MONEDAS/ZANAHORIAS COMPLETAMENTE
        ResetearMonedasCompleto();
        
        // 4. Limpiar canvas de muerte si existe
        CanvasMuerte.LimpiarInstancia();
        
        // 5. Limpiar jugadores persistentes
        JugadorPersistente[] jugadoresPersistentes = FindObjectsByType<JugadorPersistente>(FindObjectsSortMode.None);
        foreach (var jugador in jugadoresPersistentes)
        {
            if (jugador != null)
            {
                jugador.DestruirPersistencia();
            }
        }
        
        // 6. Limpiar menús de pausa que puedan quedar
        MenuPausa[] menusPausa = FindObjectsByType<MenuPausa>(FindObjectsSortMode.None);
        foreach (var menu in menusPausa)
        {
            if (menu != null)
            {
                menu.StopAllCoroutines();
                menu.CancelInvoke();
                Destroy(menu.gameObject);
            }
        }
        
        // 7. Forzar garbage collection
        System.GC.Collect();
        
        Debug.LogError("✅ ESTADO DEL JUEGO LIMPIADO EN MENÚ PRINCIPAL - CURSOR VISIBLE");
    }

    // 🆕 MÉTODO PARA RESETEAR MONEDAS COMPLETAMENTE EN EL MENÚ PRINCIPAL
    private void ResetearMonedasCompleto()
    {
        Debug.LogError("💰 RESETEANDO MONEDAS EN MENÚ PRINCIPAL...");
        
        // 1. Limpiar PlayerPrefs de todas las variantes de monedas
        PlayerPrefs.SetInt("Zanahorias", 0);
        PlayerPrefs.SetInt("Monedas", 0);
        PlayerPrefs.SetFloat("DineroJugador", 0f);
        PlayerPrefs.Save();
        
        // 2. Buscar y resetear SistemaMonedas si existe
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.SetMonedas(0);
            sistemaMonedas.GuardarConfiguracion();
        }
        
        // 3. Buscar y resetear UIManagerZanahorias si existe
        UIManagerZanahorias uiZanahorias = FindObjectOfType<UIManagerZanahorias>();
        if (uiZanahorias != null)
        {
            uiZanahorias.ResetearZanahorias();
        }
        
        // 4. Destruir instancias persistentes de sistemas de monedas
        SistemaMonedas[] todosSistemas = FindObjectsByType<SistemaMonedas>(FindObjectsSortMode.None);
        foreach (var sistema in todosSistemas)
        {
            if (sistema != null)
            {
                sistema.StopAllCoroutines();
                sistema.CancelInvoke();
                // No destruir aquí para evitar errores, solo resetear
                sistema.SetMonedas(0);
            }
        }
        
        Debug.LogError("✅ MONEDAS RESETEADAS COMPLETAMENTE EN MENÚ PRINCIPAL");
    }

    [ContextMenu("🎮 Crear Menú Completo")]
    public void CrearMenuCompleto()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🚀 CREANDO MENÚ PRINCIPAL COMPLETO");
        }
        
        // 1. Crear Canvas principal
        CrearCanvas();
        
        // 2. Crear fondo
        CrearFondoMenu();
        
        // 3. Crear menú principal
        CrearPanelMenuPrincipal();
        
        // 4. Crear menú de opciones
        CrearPanelOpciones();
        
        // 5. Crear menú de créditos
        CrearPanelCreditos();
        
        // 6. Configurar eventos
        ConfigurarEventos();
        
        // 7. Mostrar solo menú principal
        MostrarMenuPrincipal();
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ MENÚ PRINCIPAL CREADO COMPLETAMENTE");
        }
    }
    
    private void CrearCanvas()
    {
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas_MenuPrincipal");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            if (mostrarDebug)
                Debug.LogError("📱 Canvas principal creado");
        }
    }
    
    private void CrearFondoMenu()
    {
        GameObject fondo = new GameObject("Fondo_Menu");
        fondo.transform.SetParent(canvas.transform, false);
        
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = colorFondo;
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // Efecto de gradiente (opcional)
        CreateGradientEffect(fondo);
    }
    
    private void CreateGradientEffect(GameObject fondo)
    {
        GameObject gradiente = new GameObject("Gradiente");
        gradiente.transform.SetParent(fondo.transform, false);
        
        Image imagenGradiente = gradiente.AddComponent<Image>();
        
        // Crear textura de gradiente
        Texture2D textura = new Texture2D(1, 256);
        for (int i = 0; i < 256; i++)
        {
            float alpha = 1f - (i / 255f) * 0.5f;
            textura.SetPixel(0, i, new Color(0, 0, 0, alpha));
        }
        textura.Apply();
        
        Sprite spriteGradiente = Sprite.Create(textura, new Rect(0, 0, 1, 256), Vector2.one * 0.5f);
        imagenGradiente.sprite = spriteGradiente;
        
        RectTransform rectGradiente = gradiente.GetComponent<RectTransform>();
        rectGradiente.anchorMin = Vector2.zero;
        rectGradiente.anchorMax = Vector2.one;
        rectGradiente.offsetMin = Vector2.zero;
        rectGradiente.offsetMax = Vector2.zero;
    }
    
    private void CrearPanelMenuPrincipal()
    {
        panelMenuPrincipal = new GameObject("Panel_MenuPrincipal");
        panelMenuPrincipal.transform.SetParent(canvas.transform, false);
        
        RectTransform rectPanel = panelMenuPrincipal.AddComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;
        
        // Título del juego
        CrearTituloJuego(panelMenuPrincipal);
        
        // Container para botones
        GameObject containerBotones = new GameObject("Container_Botones");
        containerBotones.transform.SetParent(panelMenuPrincipal.transform, false);
        
        RectTransform rectContainer = containerBotones.AddComponent<RectTransform>();
        rectContainer.sizeDelta = new Vector2(400f, 600f);
        rectContainer.anchorMin = new Vector2(0.5f, 0.5f);
        rectContainer.anchorMax = new Vector2(0.5f, 0.5f);
        rectContainer.pivot = new Vector2(0.5f, 0.5f);
        rectContainer.anchoredPosition = new Vector2(0f, -50f);
        
        // Crear botones principales
        botonJugar = CrearBoton(containerBotones, "JUGAR", new Vector2(0f, 150f), true);
        botonOpciones = CrearBoton(containerBotones, "OPCIONES", new Vector2(0f, 50f));
        botonCreditos = CrearBoton(containerBotones, "CRÉDITOS", new Vector2(0f, -50f));
        botonSalir = CrearBoton(containerBotones, "SALIR", new Vector2(0f, -150f));
        
        // Información de versión
        CrearInfoVersion(panelMenuPrincipal);
    }
    
    private void CrearTituloJuego(GameObject padre)
    {
        GameObject titulo = new GameObject("Titulo_Juego");
        titulo.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = "THE LAST FARM";
        textoTitulo.fontSize = 72;
        textoTitulo.color = colorTitulo;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        // Efecto de sombra
        textoTitulo.fontMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(800f, 120f);
        rectTitulo.anchorMin = new Vector2(0.5f, 0.8f);
        rectTitulo.anchorMax = new Vector2(0.5f, 0.8f);
        rectTitulo.pivot = new Vector2(0.5f, 0.5f);
        rectTitulo.anchoredPosition = Vector2.zero;
        
        // Animación de título
        AgregarAnimacionTitulo(titulo);
    }
    
    private void AgregarAnimacionTitulo(GameObject titulo)
    {
        // Animación simple de escala pulsante
        titulo.AddComponent<AnimacionTitulo>();
    }
    
    private void CrearInfoVersion(GameObject padre)
    {
        GameObject version = new GameObject("Version_Info");
        version.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoVersion = version.AddComponent<TextMeshProUGUI>();
        textoVersion.text = "v1.0.0 - by Blxckbxll24";
        textoVersion.fontSize = 16;
        textoVersion.color = new Color(colorTexto.r, colorTexto.g, colorTexto.b, 0.7f);
        textoVersion.alignment = TextAlignmentOptions.BottomRight;
        
        RectTransform rectVersion = version.GetComponent<RectTransform>();
        rectVersion.sizeDelta = new Vector2(300f, 30f);
        rectVersion.anchorMin = new Vector2(1f, 0f);
        rectVersion.anchorMax = new Vector2(1f, 0f);
        rectVersion.pivot = new Vector2(1f, 0f);
        rectVersion.anchoredPosition = new Vector2(-20f, 20f);
    }
    
    private void CrearPanelOpciones()
    {
        panelOpciones = new GameObject("Panel_Opciones");
        panelOpciones.transform.SetParent(canvas.transform, false);
        
        RectTransform rectPanel = panelOpciones.AddComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;
        
        // Fondo semi-transparente
        Image fondoOpciones = panelOpciones.AddComponent<Image>();
        fondoOpciones.color = new Color(0f, 0f, 0f, 0.8f);
        
        // Panel central
        GameObject panelCentral = new GameObject("Panel_Central_Opciones");
        panelCentral.transform.SetParent(panelOpciones.transform, false);
        
        Image fondoCentral = panelCentral.AddComponent<Image>();
        fondoCentral.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform rectCentral = panelCentral.GetComponent<RectTransform>();
        rectCentral.sizeDelta = new Vector2(600f, 500f);
        rectCentral.anchorMin = new Vector2(0.5f, 0.5f);
        rectCentral.anchorMax = new Vector2(0.5f, 0.5f);
        rectCentral.pivot = new Vector2(0.5f, 0.5f);
        rectCentral.anchoredPosition = Vector2.zero;
        
        // Título opciones
        CrearTituloSeccion(panelCentral, "OPCIONES", new Vector2(0f, 200f));
        
        // Opciones básicas
        CrearTextoInfo(panelCentral, "Configuración de audio, video y controles", new Vector2(0f, 120f));
        CrearTextoInfo(panelCentral, "🔊 Volumen Master: 100%", new Vector2(0f, 60f));
        CrearTextoInfo(panelCentral, "🎵 Música: 80%", new Vector2(0f, 20f));
        CrearTextoInfo(panelCentral, "🎮 Efectos: 100%", new Vector2(0f, -20f));
        CrearTextoInfo(panelCentral, "📺 Pantalla completa: Activada", new Vector2(0f, -60f));
        CrearTextoInfo(panelCentral, "⚔️ Dificultad: Normal", new Vector2(0f, -100f));
        
        // Botón volver
        botonVolverOpciones = CrearBoton(panelCentral, "VOLVER", new Vector2(0f, -180f));
    }
    
    private void CrearPanelCreditos()
    {
        panelCreditos = new GameObject("Panel_Creditos");
        panelCreditos.transform.SetParent(canvas.transform, false);
        
        RectTransform rectPanel = panelCreditos.AddComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;
        
        // Fondo semi-transparente
        Image fondoCreditos = panelCreditos.AddComponent<Image>();
        fondoCreditos.color = new Color(0f, 0f, 0f, 0.8f);
        
        // Panel central
        GameObject panelCentral = new GameObject("Panel_Central_Creditos");
        panelCentral.transform.SetParent(panelCreditos.transform, false);
        
        Image fondoCentral = panelCentral.AddComponent<Image>();
        fondoCentral.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform rectCentral = panelCentral.GetComponent<RectTransform>();
        rectCentral.sizeDelta = new Vector2(600f, 600f);
        rectCentral.anchorMin = new Vector2(0.5f, 0.5f);
        rectCentral.anchorMax = new Vector2(0.5f, 0.5f);
        rectCentral.pivot = new Vector2(0.5f, 0.5f);
        rectCentral.anchoredPosition = Vector2.zero;
        
        // Título créditos
        CrearTituloSeccion(panelCentral, "CRÉDITOS", new Vector2(0f, 250f));
        
        // Información de créditos
        CrearTextoInfo(panelCentral, "THE LAST FARM", new Vector2(0f, 180f), 36, FontStyles.Bold);
        CrearTextoInfo(panelCentral, "Un juego de supervivencia y agricultura", new Vector2(0f, 140f), 20, FontStyles.Italic);
        
        CrearTextoInfo(panelCentral, "DESARROLLADO POR:", new Vector2(0f, 80f), 24, FontStyles.Bold);
        CrearTextoInfo(panelCentral, "Blxckbxll24", new Vector2(0f, 40f), 28, FontStyles.Bold, Color.yellow);
        
        CrearTextoInfo(panelCentral, "TECNOLOGÍAS:", new Vector2(0f, -20f), 24, FontStyles.Bold);
        CrearTextoInfo(panelCentral, "• Unity 2D Engine", new Vector2(0f, -60f));
        CrearTextoInfo(panelCentral, "• C# Programming", new Vector2(0f, -90f));
        
        CrearTextoInfo(panelCentral, "GRACIAS POR JUGAR!", new Vector2(0f, -180f), 24, FontStyles.Bold, Color.green);
        
        // Botón volver
        botonVolverCreditos = CrearBoton(panelCentral, "VOLVER", new Vector2(0f, -250f));
    }
    
    private void CrearTituloSeccion(GameObject padre, string texto, Vector2 posicion)
    {
        GameObject titulo = new GameObject("Titulo_" + texto.Replace(" ", ""));
        titulo.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = texto;
        textoTitulo.fontSize = 48;
        textoTitulo.color = colorTitulo;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(500f, 80f);
        rectTitulo.anchoredPosition = posicion;
    }
    
    private void CrearTextoInfo(GameObject padre, string texto, Vector2 posicion, float tamano = 20, FontStyles estilo = FontStyles.Normal, Color? color = null)
    {
        GameObject textoObj = new GameObject("Texto_Info");
        textoObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoComponent = textoObj.AddComponent<TextMeshProUGUI>();
        textoComponent.text = texto;
        textoComponent.fontSize = tamano;
        textoComponent.color = color ?? colorTexto;
        textoComponent.fontStyle = estilo;
        textoComponent.alignment = TextAlignmentOptions.Center;
        textoComponent.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(550f, tamano + 10f);
        rectTexto.anchoredPosition = posicion;
    }
    
    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, bool esBotonPrincipal = false)
    {
        GameObject botonObj = new GameObject("Boton_" + texto.Replace(" ", ""));
        botonObj.transform.SetParent(padre.transform, false);
        
        // Image para el fondo del botón
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = colorBotonNormal;
        
        // Button component
        Button boton = botonObj.AddComponent<Button>();
        
        // Configurar colores del botón
        ColorBlock colores = boton.colors;
        colores.normalColor = colorBotonNormal;
        colores.highlightedColor = colorBotonHover;
        colores.pressedColor = colorBotonHover * 0.8f;
        colores.selectedColor = colorBotonHover;
        boton.colors = colores;
        
        // Texto del botón
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = esBotonPrincipal ? 32 : 24;
        textoBoton.color = colorTexto;
        textoBoton.fontStyle = esBotonPrincipal ? FontStyles.Bold : FontStyles.Normal;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        // Configurar RectTransforms
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = esBotonPrincipal ? new Vector2(350f, 70f) : new Vector2(300f, 60f);
        rectBoton.anchoredPosition = posicion;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        // Efecto hover
        if (esBotonPrincipal)
        {
            boton.gameObject.AddComponent<EfectoHoverBoton>();
        }
        
        return boton;
    }
    
    private void ConfigurarEventos()
    {
        if (botonJugar != null)
            botonJugar.onClick.AddListener(IniciarJuego);
        
        if (botonOpciones != null)
            botonOpciones.onClick.AddListener(AbrirOpciones);
        
        if (botonCreditos != null)
            botonCreditos.onClick.AddListener(AbrirCreditos);
        
        if (botonSalir != null)
            botonSalir.onClick.AddListener(SalirJuego);
        
        if (botonVolverOpciones != null)
            botonVolverOpciones.onClick.AddListener(VolverMenuPrincipal);
        
        if (botonVolverCreditos != null)
            botonVolverCreditos.onClick.AddListener(VolverMenuPrincipal);
    }
    
    private void ConfigurarAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicaMenu != null)
        {
            audioSource.clip = musicaMenu;
            audioSource.loop = true;
            audioSource.volume = 0.6f; // Volumen fijo
            
            // 🔧 REPRODUCIR INMEDIATAMENTE
            audioSource.Play();
            
            if (mostrarDebug)
            {
                Debug.LogError("🎵 MÚSICA DEL MENÚ INICIADA");
            }
        }
    }
    
    // === MÉTODOS DE NAVEGACIÓN ===
    
    public void IniciarJuego()
    {
        ReproducirSonidoBoton();
        
        if (mostrarDebug)
            Debug.LogError("🎮 INICIANDO JUEGO - Mostrando historia primero...");
        
        // 🆕 MOSTRAR HISTORIA ANTES DE IR AL JUEGO
        CanvasHistoriaIntro.MostrarHistoriaDesdeMenu();
        
        // El resto del código original como fallback
        /* 
        try
        {
            // Intentar cargar la escena directamente
            SceneManager.LoadScene(nombreEscenaJuego);
            
            if (mostrarDebug)
                Debug.LogError("✅ Escena cargada exitosamente!");
        }
        catch (System.ArgumentException ex)
        {
            Debug.LogError("❌ ERROR: No se puede cargar la escena '" + nombreEscenaJuego + "'");
            Debug.LogError("🔍 Detalle del error: " + ex.Message);
            MostrarSolucionesEscena();
            
            // Intentar cargar por índice como alternativa
            if (SceneManager.sceneCountInBuildSettings > 1)
            {
                Debug.LogError("🔄 Intentando cargar escena por índice...");
                try
                {
                    SceneManager.LoadScene(1); // Intentar cargar índice 1
                    Debug.LogError("✅ Escena cargada por índice!");
                }
                catch
                {
                    Debug.LogError("❌ Tampoco se pudo cargar por índice. Revisa Build Settings.");
                }
            }
        }
        */
    }
    
    private void MostrarSolucionesEscena()
    {
        Debug.LogError("📋 SOLUCIONES:");
        Debug.LogError("1. Ve a File > Build Settings y añade tu escena de juego");
        Debug.LogError("2. O cambia 'nombreEscenaJuego' en el inspector del MenuPrincipal");
        Debug.LogError("3. Asegúrate de que la escena existe en tu proyecto");
        
        #if UNITY_EDITOR
        Debug.LogError("🎯 ESCENAS PRINCIPALES ENCONTRADAS:");
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Scene");
        bool encontroEscenas = false;
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Solo mostrar escenas en Assets/Scenes
            if (path.StartsWith("Assets/Scenes/"))
            {
                Debug.LogError("   🎮 " + sceneName + " ← DISPONIBLE PARA USAR");
                encontroEscenas = true;
            }
        }
        
        if (!encontroEscenas)
        {
            Debug.LogError("   ❌ No se encontraron escenas en Assets/Scenes/");
            Debug.LogError("   📁 Verifica que tus escenas estén en la carpeta correcta");
        }
        
        Debug.LogError("💡 SOLUCIÓN RÁPIDA: Ve a File > Build Settings y arrastra Escena1.unity");
        #endif
    }
    
    public void AbrirOpciones()
    {
        ReproducirSonidoBoton();
        enMenuOpciones = true;
        
        panelMenuPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
        panelCreditos.SetActive(false);
        
        if (mostrarDebug)
            Debug.LogError("⚙️ OPCIONES ABIERTAS");
    }
    
    public void AbrirCreditos()
    {
        ReproducirSonidoBoton();
        enMenuCreditos = true;
        
        panelMenuPrincipal.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(true);
        
        if (mostrarDebug)
            Debug.LogError("📜 CRÉDITOS ABIERTOS");
    }
    
    public void VolverMenuPrincipal()
    {
        ReproducirSonidoBoton();
        MostrarMenuPrincipal();
    }
    
    public void SalirJuego()
    {
        ReproducirSonidoBoton();
        
        if (mostrarDebug)
            Debug.LogError("🚪 SALIENDO DEL JUEGO");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void MostrarMenuPrincipal()
    {
        enMenuOpciones = false;
        enMenuCreditos = false;
        
        panelMenuPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
        
        if (mostrarDebug)
            Debug.LogError("🏠 MENÚ PRINCIPAL MOSTRADO");
    }
    
    private void ReproducirSonidoBoton()
    {
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton, 0.7f);
        }
    }
    
    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE EN MENÚ
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Navegación con teclas
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (enMenuOpciones || enMenuCreditos)
            {
                VolverMenuPrincipal();
            }
        }
    }
}

// Componente para animación del título
public class AnimacionTitulo : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector3 escalaOriginal;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        escalaOriginal = rectTransform.localScale;
    }
    
    void Update()
    {
        float escala = 1f + Mathf.Sin(Time.time * 2f) * 0.1f;
        rectTransform.localScale = escalaOriginal * escala;
    }
}

// Componente para efecto hover en botones
public class EfectoHoverBoton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 escalaOriginal;
    private bool mouseEncima = false;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        escalaOriginal = rectTransform.localScale;
    }
    
    void Update()
    {
        if (mouseEncima)
        {
            float escala = 1f + Mathf.Sin(Time.time * 8f) * 0.05f;
            rectTransform.localScale = escalaOriginal * escala;
        }
        else
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, escalaOriginal, Time.deltaTime * 5f);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEncima = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEncima = false;
    }
}