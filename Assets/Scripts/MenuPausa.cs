using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("📱 UI Referencias")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private Canvas canvasMenu;
    
    [Header("🎮 Botones Menú Principal")]
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonOpciones; // AGREGAR ESTA LÍNEA
    [SerializeField] private Button botonMenuPrincipal;
    [SerializeField] private Button botonSalirJuego;
    
    [Header("🔧 Configuración")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private bool pausarTiempoAlAbrir = true;
    [SerializeField] private KeyCode teclaPausa = KeyCode.Escape;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎨 Estilo")]
    [SerializeField] private Color colorFondoPausa = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] private Color colorBotonNormal = new Color(0.2f, 0.3f, 0.5f, 0.9f);
    [SerializeField] private Color colorBotonHover = new Color(0.3f, 0.4f, 0.6f, 1f);
    [SerializeField] private Color colorTexto = Color.white;
    
    // Estados del menú
    private bool juegoEnPausa = false;
    private float timeScaleAnterior = 1f;
    private CursorLockMode lockModeAnterior;
    private bool cursorVisibleAnterior;
    
    // Agregar variables para panel de opciones y estado
    private GameObject panelOpciones;
    private bool enMenuOpciones = false;
    private MenuOpciones menuOpciones;
    
    void Start()
    {
        if (crearUIAutomaticamente)
        {
            CrearUICompleta();
        }
        
        // Buscar MenuOpciones
        menuOpciones = GetComponent<MenuOpciones>();
        if (menuOpciones == null)
        {
            menuOpciones = gameObject.AddComponent<MenuOpciones>();
        }
        
        // Configurar eventos de botones
        ConfigurarBotones();
        
        // CRUCIAL: Inicialmente ocultar TODOS los paneles
        OcultarTodosLosPaneles();
        
        // ASEGURAR que el canvas esté desactivado al inicio
        if (canvasMenu != null)
        {
            canvasMenu.gameObject.SetActive(false);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎮 MENÚ PAUSA INICIALIZADO CORRECTAMENTE");
        }
    }
    
    void Update()
    {
        // Detectar tecla de pausa
        if (Input.GetKeyDown(teclaPausa))
        {
            if (juegoEnPausa)
            {
                if (enMenuOpciones)
                {
                    CerrarOpciones();
                }
                else
                {
                    ContinuarJuego();
                }
            }
            else
            {
                PausarJuego();
            }
        }
        
        // Tecla de escape adicional para volver
        if (Input.GetKeyDown(KeyCode.Escape) && teclaPausa != KeyCode.Escape)
        {
            if (enMenuOpciones)
            {
                CerrarOpciones();
            }
            else if (juegoEnPausa)
            {
                ContinuarJuego();
            }
        }
    }
    
    private void CrearPanelPausa()
    {
        if (panelPausa == null)
        {
            panelPausa = new GameObject("Panel_Pausa");
            panelPausa.transform.SetParent(canvasMenu.transform, false);
            
            // Fondo completo
            Image fondoPausa = panelPausa.AddComponent<Image>();
            fondoPausa.color = colorFondoPausa;
            
            RectTransform rectPausa = panelPausa.GetComponent<RectTransform>();
            rectPausa.anchorMin = Vector2.zero;
            rectPausa.anchorMax = Vector2.one;
            rectPausa.offsetMin = Vector2.zero;
            rectPausa.offsetMax = Vector2.zero;
            
            // Panel central para botones
            GameObject panelCentral = new GameObject("Panel_Central");
            panelCentral.transform.SetParent(panelPausa.transform, false);
            
            Image fondoCentral = panelCentral.AddComponent<Image>();
            fondoCentral.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            RectTransform rectCentral = panelCentral.GetComponent<RectTransform>();
            rectCentral.sizeDelta = new Vector2(400f, 600f);
            rectCentral.anchorMin = new Vector2(0.5f, 0.5f);
            rectCentral.anchorMax = new Vector2(0.5f, 0.5f);
            rectCentral.pivot = new Vector2(0.5f, 0.5f);
            
            // Título
            CrearTitulo(panelCentral, "JUEGO PAUSADO");
            
            // Botones - GUARDAR REFERENCIA DE OPCIONES
            botonContinuar = CrearBoton(panelCentral, "CONTINUAR", new Vector2(0f, 100f));
            botonOpciones = CrearBoton(panelCentral, "OPCIONES", new Vector2(0f, 20f)); // GUARDAR EN LA VARIABLE
            botonMenuPrincipal = CrearBoton(panelCentral, "MENÚ PRINCIPAL", new Vector2(0f, -60f));
            botonSalirJuego = CrearBoton(panelCentral, "SALIR DEL JUEGO", new Vector2(0f, -140f));
            
            // Configurar evento del botón opciones AHORA QUE TENEMOS LA REFERENCIA
            if (botonOpciones != null)
            {
                botonOpciones.onClick.AddListener(AbrirOpciones);
                Debug.LogError("✅ BOTÓN OPCIONES CREADO Y CONFIGURADO");
            }
            else
            {
                Debug.LogError("❌ ERROR: No se pudo crear el botón opciones!");
            }
        }
    }
    
    private void CrearPanelOpciones()
    {
        if (panelOpciones == null)
        {
            panelOpciones = new GameObject("Panel_Opciones");
            panelOpciones.transform.SetParent(canvasMenu.transform, false);
            
            // Fondo completo
            Image fondoOpciones = panelOpciones.AddComponent<Image>();
            fondoOpciones.color = colorFondoPausa;
            
            RectTransform rectOpciones = panelOpciones.GetComponent<RectTransform>();
            rectOpciones.anchorMin = Vector2.zero;
            rectOpciones.anchorMax = Vector2.one;
            rectOpciones.offsetMin = Vector2.zero;
            rectOpciones.offsetMax = Vector2.zero;
            
            // Panel central MÁS GRANDE para acomodar dificultad personalizada
            GameObject panelCentralOpciones = new GameObject("Panel_Central_Opciones");
            panelCentralOpciones.transform.SetParent(panelOpciones.transform, false);
            
            Image fondoCentralOpciones = panelCentralOpciones.AddComponent<Image>();
            fondoCentralOpciones.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            
            RectTransform rectCentralOpciones = panelCentralOpciones.GetComponent<RectTransform>();
            rectCentralOpciones.sizeDelta = new Vector2(1000f, 750f); // MÁS GRANDE
            rectCentralOpciones.anchorMin = new Vector2(0.5f, 0.5f);
            rectCentralOpciones.anchorMax = new Vector2(0.5f, 0.5f);
            rectCentralOpciones.pivot = new Vector2(0.5f, 0.5f);
            
            // Título opciones
            CrearTitulo(panelCentralOpciones, "⚙️ OPCIONES COMPLETAS");
            
            // BOTONES DE CONTROL - POSICIONES AJUSTADAS
            Button botonAplicar = CrearBoton(panelCentralOpciones, "APLICAR", new Vector2(-250f, -350f));
            Button botonReset = CrearBoton(panelCentralOpciones, "RESETEAR", new Vector2(0f, -350f));
            Button botonVolver = CrearBoton(panelCentralOpciones, "VOLVER", new Vector2(250f, -350f));
            
            if (botonAplicar != null)
                botonAplicar.onClick.AddListener(AplicarOpciones);
            
            if (botonReset != null)
                botonReset.onClick.AddListener(ResetearOpciones);
            
            if (botonVolver != null)
                botonVolver.onClick.AddListener(CerrarOpciones);
            
            // CRÍTICO: Empezar completamente OCULTO
            panelOpciones.SetActive(false);
            
            if (mostrarDebug)
            {
                Debug.LogError("📱 PANEL OPCIONES MEJORADO CREADO Y OCULTO");
            }
        }
    }
    
    [ContextMenu("🎮 Crear UI Completa")]
    public void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🚀 CREANDO UI DEL MENÚ DE PAUSA");
        }
        
        // 1. Crear o configurar Canvas
        CrearCanvas();
        
        // 2. Crear panel principal de pausa
        CrearPanelPausa();
        
        // 3. Crear panel de opciones (VACÍO)
        CrearPanelOpciones();
        
        // 4. ASEGURAR que todo esté oculto inicialmente
        if (canvasMenu != null)
            canvasMenu.gameObject.SetActive(false);
    
        if (panelPausa != null)
            panelPausa.SetActive(false);
            
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DEL MENÚ DE PAUSA CREADA - TODO OCULTO");
        }
    }
    
    private void CrearCanvas()
    {
        if (canvasMenu == null)
        {
            GameObject canvasObj = new GameObject("Canvas_MenuPausa");
            canvasMenu = canvasObj.AddComponent<Canvas>();
            canvasMenu.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasMenu.sortingOrder = 1000; // Muy arriba
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            if (mostrarDebug)
            {
                Debug.LogError("📱 Canvas del menú creado");
            }
        }
    }
    
    private void CrearTitulo(GameObject padre, string texto)
    {
        GameObject titulo = new GameObject("Titulo");
        titulo.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = texto;
        textoTitulo.fontSize = 48;
        textoTitulo.color = colorTexto;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(380f, 80f);
        rectTitulo.anchoredPosition = new Vector2(0f, 250f);
    }
    
    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion)
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
        textoBoton.fontSize = 24;
        textoBoton.color = colorTexto;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        // Configurar RectTransforms
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(300f, 60f);
        rectBoton.anchoredPosition = posicion;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        return boton;
    }
    
    private void ConfigurarBotones()
    {
        if (botonContinuar != null)
            botonContinuar.onClick.AddListener(ContinuarJuego);
        
        if (botonOpciones != null) // AGREGAR ESTA CONFIGURACIÓN
            botonOpciones.onClick.AddListener(AbrirOpciones);
        
        if (botonMenuPrincipal != null)
            botonMenuPrincipal.onClick.AddListener(IrMenuPrincipal);
        
        if (botonSalirJuego != null)
            botonSalirJuego.onClick.AddListener(SalirJuego);
    }
    
    // === MÉTODOS PÚBLICOS ===
    
    public void PausarJuego()
    {
        if (juegoEnPausa) return;
        
        juegoEnPausa = true;
        
        // Guardar estado anterior
        timeScaleAnterior = Time.timeScale;
        lockModeAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;
        
        // 🔧 PAUSAR EL TIEMPO EN ESCENA2
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esEscena2 = escenaActual.Contains("Escena2") || escenaActual.Contains("2");
        
        if (esEscena2)
        {
            Time.timeScale = 0f; // PAUSAR en Escena2
            Debug.LogError("⏸️ ESCENA2 - TIEMPO PAUSADO");
        }
        else
        {
            Time.timeScale = 1f; // NO pausar en otras escenas
            Debug.LogError("⏸️ OTRAS ESCENAS - TIEMPO NO PAUSADO");
        }
        
        // FORZAR cursor visible SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // ACTIVAR EL CANVAS PRIMERO
        if (canvasMenu != null)
        {
            canvasMenu.gameObject.SetActive(true);
        }
        
        // FORZAR que solo el panel de pausa esté visible
        enMenuOpciones = false; // Resetear estado
        
        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }
        
        // ASEGURAR que opciones esté completamente oculto
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError($"⏸️ JUEGO PAUSADO - Escena: {escenaActual} | TimeScale: {Time.timeScale}");
        }
    }
    
    public void ContinuarJuego()
    {
        if (!juegoEnPausa) return;
        
        juegoEnPausa = false;
        enMenuOpciones = false;
        
        // Restaurar tiempo
        Time.timeScale = timeScaleAnterior;
        
        // FORZAR cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // OCULTAR TODO EL CANVAS
        if (canvasMenu != null)
        {
            canvasMenu.gameObject.SetActive(false);
        }
        
        // También ocultar paneles por seguridad
        OcultarTodosLosPaneles();
        
        if (mostrarDebug)
        {
            Debug.LogError($"▶️ JUEGO REANUDADO - TimeScale: {Time.timeScale}");
        }
    }
    
    public void IrMenuPrincipal()
    {
        // 🔧 LIMPIEZA COMPLETA ANTES DE CAMBIAR ESCENA
        LimpiezaCompletaAntesCambioEscena();
        
        // Restaurar tiempo antes de cambiar escena
        Time.timeScale = 1f;
        
        if (mostrarDebug)
        {
            Debug.LogError("🏠 VOLVIENDO AL MENÚ PRINCIPAL");
        }
        
        try
        {
            // Cargar escena del menú principal
            SceneManager.LoadScene("MenuPrincipal");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR AL CARGAR MENÚ PRINCIPAL: " + e.Message);
            Debug.LogError("💡 Verifica que la escena 'MenuPrincipal' esté en Build Settings");
            
            // Alternativa: intentar cargar por índice 0 (usualmente el menú principal)
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
    
    // 🔧 NUEVO MÉTODO: LIMPIEZA COMPLETA ANTES DEL CAMBIO DE ESCENA
    private void LimpiezaCompletaAntesCambioEscena()
    {
        Debug.LogError("🧹 INICIANDO LIMPIEZA COMPLETA...");
        
        // 1. Parar todas las corrutinas
        StopAllCoroutines();
        
        // 2. Cancelar todos los Invoke
        CancelInvoke();
        
        // 3. Limpiar sistema de opciones
        if (menuOpciones != null)
        {
            menuOpciones.CancelInvoke();
            menuOpciones.StopAllCoroutines();
            menuOpciones.ForzarRecreacionUI();
        }
        
        // 4. Limpiar jugador persistente si existe
        JugadorPersistente jugadorPersistente = FindObjectOfType<JugadorPersistente>();
        if (jugadorPersistente != null)
        {
            Debug.LogError("🧹 Destruyendo jugador persistente...");
            jugadorPersistente.DestruirPersistencia();
        }
        
        // 5. Limpiar sistemas de cultivo
        CultivoManager cultivoManager = FindObjectOfType<CultivoManager>();
        if (cultivoManager != null)
        {
            cultivoManager.StopAllCoroutines();
            cultivoManager.CancelInvoke();
        }
        
        // 6. Limpiar controladores de zombies
        ControladorZombies[] controladores = FindObjectsByType<ControladorZombies>(FindObjectsSortMode.None);
        foreach (var controlador in controladores)
        {
            controlador.StopAllCoroutines();
            controlador.CancelInvoke();
            controlador.DestruirTodosLosZombies();
        }
        
        // 7. Limpiar canvas de muerte
        CanvasMuerte canvasMuerte = FindObjectOfType<CanvasMuerte>();
        if (canvasMuerte != null)
        {
            canvasMuerte.StopAllCoroutines();
            canvasMuerte.CancelInvoke();
        }
        
        // 8. Restaurar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.LogError("✅ LIMPIEZA COMPLETA TERMINADA");
    }
    
    public void SalirJuego()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🚪 SALIENDO DEL JUEGO");
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void AbrirOpciones()
    {
        Debug.LogError("🎯 ABRIENDO OPCIONES - INICIO");
        Debug.LogError("  - Panel pausa activo: " + (panelPausa != null && panelPausa.activeInHierarchy));
        Debug.LogError("  - Panel opciones existe: " + (panelOpciones != null));
        Debug.LogError("  - MenuOpciones existe: " + (menuOpciones != null));
    
        enMenuOpciones = true;
    
        // VERIFICAR Y CREAR PANEL DE OPCIONES SI NO EXISTE
        if (panelOpciones == null)
        {
            Debug.LogError("❌ Panel de opciones no existe, creándolo...");
            CrearPanelOpciones();
        }
        
        // VERIFICAR Y CREAR MENUOPCIONES SI NO EXISTE
        if (menuOpciones == null)
        {
            Debug.LogError("❌ MenuOpciones no existe, creándolo...");
            menuOpciones = GetComponent<MenuOpciones>();
            if (menuOpciones == null)
            {
                menuOpciones = gameObject.AddComponent<MenuOpciones>();
                Debug.LogError("✅ MenuOpciones creado como componente");
            }
        }
        
        // OCULTAR PANEL DE PAUSA PRIMERO
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
            Debug.LogError("✅ Panel pausa ocultado");
        }
    
        // MOSTRAR PANEL DE OPCIONES
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
            Debug.LogError("✅ Panel opciones mostrado");
            
            // FORZAR CREACIÓN DEL CONTENIDO UI
            GameObject panelCentralOpciones = panelOpciones.transform.Find("Panel_Central_Opciones")?.gameObject;
            if (panelCentralOpciones != null)
            {
                Debug.LogError("🎨 Panel central encontrado, creando UI...");
                
                // VERIFICAR SI MENUOPCIONES ESTÁ LISTO
                if (menuOpciones != null)
                {
                    // FORZAR RECREACIÓN SI ES NECESARIO
                    if (!menuOpciones.EstaUICreada())
                    {
                        menuOpciones.CrearOpcionesUI(panelCentralOpciones);
                        Debug.LogError("✅ UI de opciones creada");
                    }
                    else
                    {
                        Debug.LogError("🔄 UI ya existe, cargando configuración...");
                    }
                    
                    menuOpciones.CargarConfiguracionEnUI();
                    Debug.LogError("✅ Configuración cargada en UI");
                }
                else
                {
                    Debug.LogError("❌ MenuOpciones es NULL!");
                }
            }
            else
            {
                Debug.LogError("❌ No se encontró Panel_Central_Opciones!");
            }
        }
        else
        {
            Debug.LogError("❌ ERROR CRÍTICO: No se pudo crear/encontrar panel de opciones!");
            return;
        }
    
        if (mostrarDebug)
        {
            Debug.LogError("⚙️ OPCIONES ABIERTAS - Estado final:");
            Debug.LogError("  - Panel opciones activo: " + (panelOpciones != null && panelOpciones.activeInHierarchy));
            Debug.LogError("  - MenuOpciones existe: " + (menuOpciones != null));
            Debug.LogError("  - UI creada: " + (menuOpciones != null && menuOpciones.EstaUICreada()));
            Debug.LogError("  - Canvas activo: " + (canvasMenu != null && canvasMenu.gameObject.activeInHierarchy));
        }
    }
    
    public void CerrarOpciones()
    {
        enMenuOpciones = false;
        
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
        
        if (panelPausa != null)
            panelPausa.SetActive(true);
        
        if (mostrarDebug)
        {
            Debug.LogError("❌ OPCIONES CERRADAS");
        }
    }
    
    public void AplicarOpciones()
    {
        if (menuOpciones != null)
        {
            menuOpciones.AplicarCambios();
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("💾 OPCIONES APLICADAS Y GUARDADAS");
        }
    }
    
    // NUEVO MÉTODO PARA RESETEAR OPCIONES
    public void ResetearOpciones()
    {
        if (menuOpciones != null)
        {
            menuOpciones.ResetearATodosPorDefecto();
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🔄 OPCIONES RESETEADAS");
        }
    }
    
    private void OcultarTodosLosPaneles()
    {
        if (panelPausa != null)
            panelPausa.SetActive(false);
        
        if (panelOpciones != null)
            panelOpciones.SetActive(false);
    }
    
    // Propiedades públicas
    public bool JuegoEnPausa => juegoEnPausa;
    
    // MÉTODO DE DIAGNÓSTICO PARA DEBUG
    [ContextMenu("🔍 Diagnosticar Estado UI")]
    public void DiagnosticarEstadoUI()
    {
        Debug.LogError("🔍 DIAGNÓSTICO COMPLETO DEL ESTADO UI:");
        Debug.LogError("===========================================");
        
        // Canvas
        Debug.LogError("📱 CANVAS:");
        if (canvasMenu != null)
        {
            Debug.LogError("  ✅ Canvas existe: " + canvasMenu.name);
            Debug.LogError("  - Activo en jerarquía: " + canvasMenu.gameObject.activeInHierarchy);
            Debug.LogError("  - Activo en self: " + canvasMenu.gameObject.activeSelf);
            Debug.LogError("  - Render mode: " + canvasMenu.renderMode);
            Debug.LogError("  - Sorting order: " + canvasMenu.sortingOrder);
        }
        else
        {
            Debug.LogError("  ❌ Canvas es NULL");
        }
        
        // Panel Pausa
        Debug.LogError("⏸️ PANEL PAUSA:");
        if (panelPausa != null)
        {
            Debug.LogError("  ✅ Panel existe: " + panelPausa.name);
            Debug.LogError("  - Activo: " + panelPausa.activeInHierarchy);
            Debug.LogError("  - Hijos: " + panelPausa.transform.childCount);
        }
        else
        {
            Debug.LogError("  ❌ Panel pausa es NULL");
        }
        
        // Panel Opciones
        Debug.LogError("⚙️ PANEL OPCIONES:");
        if (panelOpciones != null)
        {
            Debug.LogError("  ✅ Panel existe: " + panelOpciones.name);
            Debug.LogError("  - Activo: " + panelOpciones.activeInHierarchy);
            Debug.LogError("  - Hijos: " + panelOpciones.transform.childCount);
            
            Transform panelCentral = panelOpciones.transform.Find("Panel_Central_Opciones");
            if (panelCentral != null)
            {
                Debug.LogError("  ✅ Panel central encontrado con " + panelCentral.childCount + " hijos");
            }
            else
            {
                Debug.LogError("  ❌ Panel central NO encontrado");
            }
        }
        else
        {
            Debug.LogError("  ❌ Panel opciones es NULL");
        }
        
        // MenuOpciones Component
        Debug.LogError("🎛️ COMPONENT MENUOPCIONES:");
        if (menuOpciones != null)
        {
            Debug.LogError("  ✅ Component existe");
            Debug.LogError("  - UI creada: " + menuOpciones.EstaUICreada());
            Debug.LogError("  - Enabled: " + menuOpciones.enabled);
        }
        else
        {
            Debug.LogError("  ❌ Component es NULL");
        }
        
        // Botones
        Debug.LogError("🎮 BOTONES:");
        Debug.LogError("  - Continuar: " + (botonContinuar != null ? "✅" : "❌"));
        Debug.LogError("  - Opciones: " + (botonOpciones != null ? "✅" : "❌"));
        Debug.LogError("  - Menu Principal: " + (botonMenuPrincipal != null ? "✅" : "❌"));
        Debug.LogError("  - Salir: " + (botonSalirJuego != null ? "✅" : "❌"));
        
        Debug.LogError("===========================================");
    }
    
    // MÉTODO PARA TESTEAR MANUALMENTE
    [ContextMenu("🧪 Test Manual - Abrir Opciones")]
    public void TestManualAbrirOpciones()
    {
        Debug.LogError("🧪 INICIANDO TEST MANUAL DE OPCIONES");
        
        // Simular que el juego está pausado
        if (!juegoEnPausa)
        {
            PausarJuego();
        }
        
        // Intentar abrir opciones
        AbrirOpciones();
        
        // Diagnóstico después
        Invoke("DiagnosticarEstadoUI", 0.5f);
    }
}