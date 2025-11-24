using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuOpciones : MonoBehaviour
{
    [Header("🎚️ Audio Actual")]
    [SerializeField] private float volumenMaster = 1f;
    [SerializeField] private float volumenMusica = 0.8f;
    [SerializeField] private float volumenEfectos = 1f;
    
    [Header("📺 Video Actual")]
    [SerializeField] private bool pantallaCompleta = true;
    [SerializeField] private int calidadGraficos = 2; // 0=Bajo, 1=Medio, 2=Alto
    [SerializeField] private bool vsync = true;
    [SerializeField] private int fpsLimite = 60;
    
    [Header("🎮 Juego Actual")]
    [SerializeField] private float sensibilidadMouse = 1f;
    [SerializeField] private bool invertirY = false;
    
    [Header("🧟 Zombies Actual")]
    [SerializeField] private float multiplicadorVidaZombies = 1f;
    [SerializeField] private float multiplicadorVelocidadZombies = 1f;
    [SerializeField] private float multiplicadorDañoZombies = 1f;
    
    [Header("🎯 DIFICULTAD DEL JUEGO")]
    [SerializeField] private int nivelDificultad = 1; // 0=Fácil, 1=Normal, 2=Difícil, 3=Extremo, 4=Personalizado
    [SerializeField] private bool dificultadPersonalizada = false;
    
    // Configuración personalizada de dificultad
    [Header("🔧 CONFIGURACIÓN PERSONALIZADA")]
    [SerializeField] private float multiplicadorVelocidadEnemigos = 1.0f;
    [SerializeField] private float multiplicadorVidaEnemigos = 1.0f;
    [SerializeField] private float multiplicadorDañoEnemigos = 1.0f;
    [SerializeField] private int cantidadEnemigos = 10;
    [SerializeField] private float tiempoRespawnEnemigos = 30f;
    [SerializeField] private float multiplicadorRecompensas = 1.0f;
    
    [Header("🔍 Debug")]
    [SerializeField] private bool mostrarDebug = true; // AGREGAR ESTA LÍNEA
    
    // Referencias UI (se crearán automáticamente)
    private Slider sliderVolumenMaster;
    private Slider sliderVolumenMusica;
    private Slider sliderVolumenEfectos;
    private Toggle togglePantallaCompleta;
    private TMP_Dropdown dropdownCalidad;
    private Toggle toggleVSync;
    private Slider sliderFPS;
    private Slider sliderSensibilidad;
    private Toggle toggleInvertirY;
    private Slider sliderVidaZombies;
    private Slider sliderVelocidadZombies;
    private Slider sliderDañoZombies;
    
    // Textos de valores
    private TextMeshProUGUI textoVolumenMaster;
    private TextMeshProUGUI textoVolumenMusica;
    private TextMeshProUGUI textoVolumenEfectos;
    private TextMeshProUGUI textoFPS;
    private TextMeshProUGUI textoSensibilidad;
    private TextMeshProUGUI textoVidaZombies;
    private TextMeshProUGUI textoVelocidadZombies;
    private TextMeshProUGUI textoDañoZombies;
    
    // Referencias UI adicionales para dificultad
    private Button botonDificultad; // CAMBIO: De dropdown a botón
    private TextMeshProUGUI textoDificultadActual; // NUEVO: Para mostrar la dificultad actual
    private Slider sliderVelocidadEnemigos;
    private Slider sliderVidaEnemigos;
    private Slider sliderDañoEnemigos;
    private Slider sliderCantidadEnemigos;
    private Slider sliderTiempoRespawn;
    private Slider sliderRecompensas;
    
    // Textos de valores para dificultad
    private TextMeshProUGUI textoVelocidadEnemigos;
    private TextMeshProUGUI textoVidaEnemigos;
    private TextMeshProUGUI textoDañoEnemigos;
    private TextMeshProUGUI textoCantidadEnemigos;
    private TextMeshProUGUI textoTiempoRespawn;
    private TextMeshProUGUI textoRecompensas;
    
    // Panel de configuración personalizada
    private GameObject panelPersonalizado;
    
    private bool uiCreada = false;
    
    void Start()
    {
        // Cargar configuración guardada
        CargarConfiguracion();
        
        // 🔧 IMPORTANTE: NO crear UI automáticamente para evitar bucles
        // La UI solo se crea cuando MenuPausa lo solicita EXPLÍCITAMENTE
        if (mostrarDebug)
        {
            Debug.LogError("🎨 MenuOpciones iniciado - Esperando creación de UI");
        }
    }
    
    public void CrearOpcionesUI(GameObject panelPadre)
    {
        // 🔧 VERIFICACIÓN CRÍTICA PARA EVITAR BUCLES
        if (panelPadre == null)
        {
            Debug.LogError("❌ ERROR CRÍTICO: panelPadre es NULL!");
            return;
        }
        
        if (uiCreada) 
        {
            Debug.LogError("⚠️ UI ya creada, saltando recreación");
            return; // 🔧 NO FORZAR RECREACIÓN
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI DE OPCIONES...");
        }
        
        try
        {
            // 🔧 CREAR CONTENIDO MÁS SIMPLE PARA EVITAR PROBLEMAS
            CrearContenidoBasico(panelPadre);
            
            uiCreada = true;
            
            Debug.LogError("✅ UI DE OPCIONES CREADA EXITOSAMENTE");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR CREANDO UI: " + e.Message);
            uiCreada = false;
        }
    }
    
    // 🔧 NUEVO MÉTODO SIMPLIFICADO
    private void CrearContenidoBasico(GameObject padre)
    {
        // Crear solo elementos básicos para evitar problemas de rendimiento
        
        GameObject contenido = new GameObject("Contenido_Opciones");
        contenido.transform.SetParent(padre.transform, false);
        
        RectTransform rectContenido = contenido.AddComponent<RectTransform>();
        rectContenido.sizeDelta = new Vector2(600f, 400f);
        rectContenido.anchoredPosition = Vector2.zero;
        
        // Solo crear texto básico por ahora
        GameObject texto = new GameObject("Texto_Opciones");
        texto.transform.SetParent(contenido.transform, false);
        
        TextMeshProUGUI textoComp = texto.AddComponent<TextMeshProUGUI>();
        textoComp.text = "⚙️ OPCIONES\n\n(En desarrollo)\n\nUsa ESC para volver";
        textoComp.fontSize = 24;
        textoComp.color = Color.white;
        textoComp.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = texto.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(500f, 300f);
        rectTexto.anchoredPosition = Vector2.zero;
    }
    
    private GameObject CrearContenidoScrollMejorado(GameObject padre)
    {
        // ScrollView MÁS GRANDE
        GameObject scrollView = new GameObject("ScrollView_OpcionesMejorado");
        scrollView.transform.SetParent(padre.transform, false);
        
        RectTransform rectScroll = scrollView.AddComponent<RectTransform>();
        rectScroll.sizeDelta = new Vector2(850f, 500f); // MÁS GRANDE
        rectScroll.anchoredPosition = new Vector2(0f, -30f);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        
        Image imagenFondo = scrollView.AddComponent<Image>();
        imagenFondo.color = new Color(0.05f, 0.05f, 0.05f, 0.9f); // Más opaco
        
        scrollView.AddComponent<Mask>();
        
        // Configurar scrollbar para mejor UX
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        
        // Crear contenido
        GameObject contenido = new GameObject("Content");
        contenido.transform.SetParent(scrollView.transform, false);
        
        RectTransform rectContenido = contenido.AddComponent<RectTransform>();
        rectContenido.anchorMin = new Vector2(0f, 1f);
        rectContenido.anchorMax = new Vector2(1f, 1f);
        rectContenido.pivot = new Vector2(0.5f, 1f);
        rectContenido.sizeDelta = new Vector2(-20f, 1500f); // MÁS ALTURA
        rectContenido.anchoredPosition = Vector2.zero;
        
        scroll.content = rectContenido;
        
        return contenido;
    }
    
    private void CrearTitulo(GameObject padre, string texto, Vector2 posicion)
    {
        GameObject titulo = new GameObject("Titulo_" + texto.Replace(" ", ""));
        titulo.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = texto;
        textoTitulo.fontSize = 28;
        textoTitulo.color = Color.yellow;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rect = titulo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(700f, 40f);
        rect.anchoredPosition = posicion;
    }
    
    private Slider CrearSliderSimple(GameObject padre, string etiqueta, Vector2 posicion, float min, float max, float valorInicial, out TextMeshProUGUI textoValor)
    {
        // Etiqueta
        GameObject labelObj = new GameObject("Label_" + etiqueta.Replace(" ", ""));
        labelObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = etiqueta + ":";
        label.fontSize = 16; // Más pequeño
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectLabel = labelObj.GetComponent<RectTransform>();
        rectLabel.sizeDelta = new Vector2(180f, 25f); // Más compacto
        rectLabel.anchoredPosition = new Vector2(posicion.x - 120f, posicion.y);
        
        // Slider
        GameObject sliderObj = new GameObject("Slider_" + etiqueta.Replace(" ", ""));
        sliderObj.transform.SetParent(padre.transform, false);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = valorInicial;
        
        Image fondoSlider = sliderObj.AddComponent<Image>();
        fondoSlider.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform rectSlider = sliderObj.GetComponent<RectTransform>();
        rectSlider.sizeDelta = new Vector2(180f, 18f); // Más compacto
        rectSlider.anchoredPosition = posicion;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderObj.transform, false);
        
        Image imagenFill = fill.AddComponent<Image>();
        imagenFill.color = Color.cyan;
        
        RectTransform rectFill = fill.GetComponent<RectTransform>();
        rectFill.anchorMin = Vector2.zero;
        rectFill.anchorMax = Vector2.one;
        rectFill.offsetMin = Vector2.zero;
        rectFill.offsetMax = Vector2.zero;
        
        slider.fillRect = rectFill;
        
        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform, false);
        
        Image imagenHandle = handle.AddComponent<Image>();
        imagenHandle.color = Color.white;
        
        RectTransform rectHandle = handle.GetComponent<RectTransform>();
        rectHandle.sizeDelta = new Vector2(12f, 20f); // Más pequeño
    
        slider.handleRect = rectHandle;
        
        // Texto valor
        GameObject valorObj = new GameObject("Valor_" + etiqueta.Replace(" ", ""));
        valorObj.transform.SetParent(padre.transform, false);
        
        textoValor = valorObj.AddComponent<TextMeshProUGUI>();
        textoValor.text = valorInicial.ToString("F1");
        textoValor.fontSize = 14; // Más pequeño
        textoValor.color = Color.cyan;
        textoValor.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectValor = valorObj.GetComponent<RectTransform>();
        rectValor.sizeDelta = new Vector2(60f, 25f); // Más compacto
        rectValor.anchoredPosition = new Vector2(posicion.x + 120f, posicion.y);
        
        return slider;
    }
    
    private Toggle CrearToggleSimple(GameObject padre, string etiqueta, Vector2 posicion, bool valorInicial)
    {
        GameObject toggleObj = new GameObject("Toggle_" + etiqueta.Replace(" ", ""));
        toggleObj.transform.SetParent(padre.transform, false);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = valorInicial;
        
        RectTransform rectToggle = toggleObj.GetComponent<RectTransform>();
        rectToggle.sizeDelta = new Vector2(300f, 30f);
        rectToggle.anchoredPosition = posicion;
        
        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);
        
        Image imagenBackground = background.AddComponent<Image>();
        imagenBackground.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        RectTransform rectBackground = background.GetComponent<RectTransform>();
        rectBackground.sizeDelta = new Vector2(20f, 20f);
        rectBackground.anchoredPosition = new Vector2(-100f, 0f);
        
        toggle.targetGraphic = imagenBackground;
        
        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        
        Image imagenCheckmark = checkmark.AddComponent<Image>();
        imagenCheckmark.color = Color.green;
        
        RectTransform rectCheckmark = checkmark.GetComponent<RectTransform>();
        rectCheckmark.anchorMin = Vector2.zero;
        rectCheckmark.anchorMax = Vector2.one;
        rectCheckmark.offsetMin = Vector2.zero;
        rectCheckmark.offsetMax = Vector2.zero;
        
        toggle.graphic = imagenCheckmark;
        
        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI textoLabel = label.AddComponent<TextMeshProUGUI>();
        textoLabel.text = etiqueta;
        textoLabel.fontSize = 18;
        textoLabel.color = Color.white;
        textoLabel.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectLabel = label.GetComponent<RectTransform>();
        rectLabel.sizeDelta = new Vector2(200f, 30f);
        rectLabel.anchoredPosition = new Vector2(-20f, 0f);
        
        return toggle;
    }
    
    private TMP_Dropdown CrearDropdownSimple(GameObject padre, string etiqueta, Vector2 posicion, string[] opciones, int valorInicial)
    {
        // Label
        GameObject labelObj = new GameObject("Label_" + etiqueta.Replace(" ", ""));
        labelObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = etiqueta + ":";
        label.fontSize = 18;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectLabel = labelObj.GetComponent<RectTransform>();
        rectLabel.sizeDelta = new Vector2(200f, 30f);
        rectLabel.anchoredPosition = new Vector2(posicion.x - 100f, posicion.y);
        
        // Dropdown
        GameObject dropdownObj = new GameObject("Dropdown_" + etiqueta.Replace(" ", ""));
        dropdownObj.transform.SetParent(padre.transform, false);
        
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        Image imagenDropdown = dropdownObj.AddComponent<Image>();
        imagenDropdown.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        
        RectTransform rectDropdown = dropdownObj.GetComponent<RectTransform>();
        rectDropdown.sizeDelta = new Vector2(150f, 30f);
        rectDropdown.anchoredPosition = new Vector2(posicion.x + 75f, posicion.y);
        
        // Llenar opciones
        dropdown.options.Clear();
        foreach (string opcion in opciones)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(opcion));
        }
        dropdown.value = valorInicial;
        
        return dropdown;
    }
    
    private GameObject CrearPanelPersonalizado(GameObject padre, float yPos)
    {
        GameObject panel = new GameObject("Panel_DificultadPersonalizada");
        panel.transform.SetParent(padre.transform, false);
        
        RectTransform rectPanel = panel.AddComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(800f, 280f);
        rectPanel.anchoredPosition = new Vector2(0f, yPos - 140f);
        
        Image fondoPanel = panel.AddComponent<Image>();
        fondoPanel.color = new Color(0.2f, 0.1f, 0.1f, 0.8f); // Fondo rojizo para destacar
        
        float yPosPanel = 100f;
        float espaciadoPanel = 35f;
        
        // CONFIGURACIÓN DE ENEMIGOS
        CrearTexto(panel, "⚔️ CONFIGURACIÓN DE ENEMIGOS", new Vector2(0, yPosPanel), 20, FontStyles.Bold, Color.yellow);
        yPosPanel -= espaciadoPanel;
        
        // Fila 1: Velocidad y Vida de enemigos
        sliderVelocidadEnemigos = CrearSliderCompacto(panel, "Velocidad", new Vector2(-200, yPosPanel), 0.5f, 3f, multiplicadorVelocidadEnemigos, out textoVelocidadEnemigos);
        sliderVelocidadEnemigos.onValueChanged.AddListener(OnVelocidadEnemigosChanged);
        
        sliderVidaEnemigos = CrearSliderCompacto(panel, "Vida", new Vector2(200, yPosPanel), 0.5f, 3f, multiplicadorVidaEnemigos, out textoVidaEnemigos);
        sliderVidaEnemigos.onValueChanged.AddListener(OnVidaEnemigosChanged);
        yPosPanel -= espaciadoPanel;
        
        // Fila 2: Daño y Cantidad
        sliderDañoEnemigos = CrearSliderCompacto(panel, "Daño", new Vector2(-200, yPosPanel), 0.5f, 3f, multiplicadorDañoEnemigos, out textoDañoEnemigos);
        sliderDañoEnemigos.onValueChanged.AddListener(OnDañoEnemigosChanged);
        
        sliderCantidadEnemigos = CrearSliderCompacto(panel, "Cantidad", new Vector2(200, yPosPanel), 5f, 50f, cantidadEnemigos, out textoCantidadEnemigos);
        sliderCantidadEnemigos.onValueChanged.AddListener(OnCantidadEnemigosChanged);
        yPosPanel -= espaciadoPanel;
        
        // Fila 3: Respawn y Recompensas
        sliderTiempoRespawn = CrearSliderCompacto(panel, "Respawn (s)", new Vector2(-200, yPosPanel), 5f, 120f, tiempoRespawnEnemigos, out textoTiempoRespawn);
        sliderTiempoRespawn.onValueChanged.AddListener(OnTiempoRespawnChanged);
        
        sliderRecompensas = CrearSliderCompacto(panel, "Recompensas", new Vector2(200, yPosPanel), 0.5f, 3f, multiplicadorRecompensas, out textoRecompensas);
        sliderRecompensas.onValueChanged.AddListener(OnRecompensasChanged);
        
        // Empezar oculto
        panel.SetActive(false);
        
        return panel;
    }
    
    private Slider CrearSliderCompacto(GameObject padre, string etiqueta, Vector2 posicion, float min, float max, float valorInicial, out TextMeshProUGUI textoValor)
    {
        // Etiqueta compacta
        GameObject labelObj = new GameObject("Label_" + etiqueta.Replace(" ", ""));
        labelObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = etiqueta + ":";
        label.fontSize = 14;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectLabel = labelObj.GetComponent<RectTransform>();
        rectLabel.sizeDelta = new Vector2(120f, 20f);
        rectLabel.anchoredPosition = new Vector2(posicion.x - 60f, posicion.y + 15f);
        
        // Slider más compacto
        GameObject sliderObj = new GameObject("Slider_" + etiqueta.Replace(" ", ""));
        sliderObj.transform.SetParent(padre.transform, false);
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = valorInicial;
        
        Image fondoSlider = sliderObj.AddComponent<Image>();
        fondoSlider.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        
        RectTransform rectSlider = sliderObj.GetComponent<RectTransform>();
        rectSlider.sizeDelta = new Vector2(120f, 16f);
        rectSlider.anchoredPosition = posicion;
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderObj.transform, false);
        
        Image imagenFill = fill.AddComponent<Image>();
        imagenFill.color = Color.red; // Color rojizo para dificultad
        
        RectTransform rectFill = fill.GetComponent<RectTransform>();
        rectFill.anchorMin = Vector2.zero;
        rectFill.anchorMax = Vector2.one;
        rectFill.offsetMin = Vector2.zero;
        rectFill.offsetMax = Vector2.zero;
        
        slider.fillRect = rectFill;
        
        // Handle más pequeño
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform, false);
        
        Image imagenHandle = handle.AddComponent<Image>();
        imagenHandle.color = Color.white;
        
        RectTransform rectHandle = handle.GetComponent<RectTransform>();
        rectHandle.sizeDelta = new Vector2(8f, 16f);
        
        slider.handleRect = rectHandle;
        
        // Texto valor compacto
        GameObject valorObj = new GameObject("Valor_" + etiqueta.Replace(" ", ""));
        valorObj.transform.SetParent(padre.transform, false);
        
        textoValor = valorObj.AddComponent<TextMeshProUGUI>();
        textoValor.text = valorInicial.ToString("F1");
        textoValor.fontSize = 12;
        textoValor.color = Color.cyan;
        textoValor.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectValor = valorObj.GetComponent<RectTransform>();
        rectValor.sizeDelta = new Vector2(50f, 20f);
        rectValor.anchoredPosition = new Vector2(posicion.x + 80f, posicion.y);
        
        return slider;
    }
    
    private TextMeshProUGUI CrearTexto(GameObject padre, string texto, Vector2 posicion, float tamano = 16, FontStyles estilo = FontStyles.Normal, Color? color = null)
    {
        GameObject textoObj = new GameObject("Texto_" + texto.Replace(" ", "_"));
        textoObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoComp = textoObj.AddComponent<TextMeshProUGUI>();
        textoComp.text = texto;
        textoComp.fontSize = tamano;
        textoComp.color = color ?? Color.white;
        textoComp.fontStyle = estilo;
        textoComp.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(400f, tamano + 5f);
        rectTexto.anchoredPosition = posicion;
        
        return textoComp;
    }
    
    // === EVENTOS ===
    
    private void OnVolumenMasterChanged(float valor)
    {
        volumenMaster = valor;
        textoVolumenMaster.text = (valor * 100f).ToString("F0") + "%";
        AudioListener.volume = valor;
    }
    
    private void OnVolumenMusicaChanged(float valor)
    {
        volumenMusica = valor;
        textoVolumenMusica.text = (valor * 100f).ToString("F0") + "%";
        // Aplicar a fuentes de música
        AplicarVolumenMusica(valor);
    }
    
    private void OnVolumenEfectosChanged(float valor)
    {
        volumenEfectos = valor;
        textoVolumenEfectos.text = (valor * 100f).ToString("F0") + "%";
        // Aplicar a fuentes de efectos
        AplicarVolumenEfectos(valor);
    }
    
    private void OnPantallaCompletaChanged(bool valor)
    {
        pantallaCompleta = valor;
        Screen.fullScreen = valor;
    }
    
    private void OnCalidadChanged(int valor)
    {
        calidadGraficos = valor;
        QualitySettings.SetQualityLevel(valor);
    }
    
    private void OnVSyncChanged(bool valor)
    {
        vsync = valor;
        QualitySettings.vSyncCount = valor ? 1 : 0;
    }
    
    private void OnFPSChanged(float valor)
    {
        fpsLimite = (int)valor;
        textoFPS.text = valor.ToString("F0");
        Application.targetFrameRate = fpsLimite;
    }
    
    private void OnSensibilidadChanged(float valor)
    {
        sensibilidadMouse = valor;
        textoSensibilidad.text = valor.ToString("F1");
    }
    
    private void OnInvertirYChanged(bool valor)
    {
        invertirY = valor;
    }
    
    private void OnVidaZombiesChanged(float valor)
    {
        multiplicadorVidaZombies = valor;
        textoVidaZombies.text = "x" + valor.ToString("F1");
        AplicarCambiosZombies();
    }
    
    private void OnVelocidadZombiesChanged(float valor)
    {
        multiplicadorVelocidadZombies = valor;
        textoVelocidadZombies.text = "x" + valor.ToString("F1");
        AplicarCambiosZombies();
    }
    
    private void OnDañoZombiesChanged(float valor)
    {
        multiplicadorDañoZombies = valor;
        textoDañoZombies.text = "x" + valor.ToString("F1");
        AplicarCambiosZombies();
    }
    
    // === APLICAR CONFIGURACIONES ===
    
    private void AplicarVolumenMusica(float volumen)
    {
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var source in audioSources)
        {
            if (source.gameObject.name.ToLower().Contains("music") || 
                source.gameObject.name.ToLower().Contains("musica"))
            {
                source.volume = volumen * volumenMaster;
            }
        }
    }
    
    private void AplicarVolumenEfectos(float volumen)
    {
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (var source in audioSources)
        {
            if (!source.gameObject.name.ToLower().Contains("music") && 
                !source.gameObject.name.ToLower().Contains("musica"))
            {
                source.volume = volumen * volumenMaster;
            }
        }
    }
    
    private void AplicarCambiosZombies()
    {
        ControladorEnemigo[] zombies = FindObjectsByType<ControladorEnemigo>(FindObjectsSortMode.None);
        
        foreach (ControladorEnemigo zombie in zombies)
        {
            // Valores base
            int saludBase = 30;
            float velocidadBase = 2.5f;
            int dañoBase = 20;
            
            // Aplicar multiplicadores
            zombie.salud = Mathf.RoundToInt(saludBase * multiplicadorVidaZombies);
            zombie.velocidadMovimiento = velocidadBase * multiplicadorVelocidadZombies;
            zombie.daño = Mathf.RoundToInt(dañoBase * multiplicadorDañoZombies);
        }
        
        Debug.LogError($"🧟 ZOMBIES ACTUALIZADOS: Vida x{multiplicadorVidaZombies:F1}, Velocidad x{multiplicadorVelocidadZombies:F1}, Daño x{multiplicadorDañoZombies:F1}");
    }
    
    // === EVENTOS DE DIFICULTAD ===

    private void OnDificultadChanged(int valor)
    {
        nivelDificultad = valor;
        dificultadPersonalizada = (valor == 4); // Índice 4 = "Personalizado"
        
        if (!dificultadPersonalizada)
        {
            // Aplicar preset de dificultad
            AplicarPresetDificultad(valor);
        }
        
        ActualizarPanelPersonalizado();
        
        if (mostrarDebug)
        {
            Debug.LogError("⚔️ DIFICULTAD CAMBIADA: " + new string[] { "Fácil", "Normal", "Difícil", "Extremo", "Personalizado" }[valor]);
        }
    }

    private void AplicarPresetDificultad(int nivel)
    {
        switch (nivel)
        {
            case 0: // Fácil
                multiplicadorVelocidadEnemigos = 0.7f;
                multiplicadorVidaEnemigos = 0.6f;
                multiplicadorDañoEnemigos = 0.5f;
                cantidadEnemigos = 8;
                tiempoRespawnEnemigos = 45f;
                multiplicadorRecompensas = 1.2f;
                break;
                
            case 1: // Normal
                multiplicadorVelocidadEnemigos = 1.0f;
                multiplicadorVidaEnemigos = 1.0f;
                multiplicadorDañoEnemigos = 1.0f;
                cantidadEnemigos = 12;
                tiempoRespawnEnemigos = 30f;
                multiplicadorRecompensas = 1.0f;
                break;
                
            case 2: // Difícil
                multiplicadorVelocidadEnemigos = 1.3f;
                multiplicadorVidaEnemigos = 1.5f;
                multiplicadorDañoEnemigos = 1.4f;
                cantidadEnemigos = 18;
                tiempoRespawnEnemigos = 20f;
                multiplicadorRecompensas = 0.8f;
                break;
                
            case 3: // Extremo
                multiplicadorVelocidadEnemigos = 1.8f;
                multiplicadorVidaEnemigos = 2.5f;
                multiplicadorDañoEnemigos = 2.0f;
                cantidadEnemigos = 25;
                tiempoRespawnEnemigos = 15f;
                multiplicadorRecompensas = 0.6f;
                break;
        }
        
        // Actualizar los sliders si existen
        ActualizarSlidersPersonalizados();
    }

    private void ActualizarPanelPersonalizado()
    {
        if (panelPersonalizado != null)
        {
            panelPersonalizado.SetActive(dificultadPersonalizada);
        }
    }

    private void ActualizarSlidersPersonalizados()
    {
        if (sliderVelocidadEnemigos != null) sliderVelocidadEnemigos.value = multiplicadorVelocidadEnemigos;
        if (sliderVidaEnemigos != null) sliderVidaEnemigos.value = multiplicadorVidaEnemigos;
        if (sliderDañoEnemigos != null) sliderDañoEnemigos.value = multiplicadorDañoEnemigos;
        if (sliderCantidadEnemigos != null) sliderCantidadEnemigos.value = cantidadEnemigos;
        if (sliderTiempoRespawn != null) sliderTiempoRespawn.value = tiempoRespawnEnemigos;
        if (sliderRecompensas != null) sliderRecompensas.value = multiplicadorRecompensas;
        
        ActualizarTextosPersonalizados();
    }

    private void OnVelocidadEnemigosChanged(float valor)
    {
        multiplicadorVelocidadEnemigos = valor;
        if (textoVelocidadEnemigos != null)
            textoVelocidadEnemigos.text = "x" + valor.ToString("F1");
        AplicarDificultadPersonalizada();
    }

    private void OnVidaEnemigosChanged(float valor)
    {
        multiplicadorVidaEnemigos = valor;
        if (textoVidaEnemigos != null)
            textoVidaEnemigos.text = "x" + valor.ToString("F1");
        AplicarDificultadPersonalizada();
    }

    private void OnDañoEnemigosChanged(float valor)
    {
        multiplicadorDañoEnemigos = valor;
        if (textoDañoEnemigos != null)
            textoDañoEnemigos.text = "x" + valor.ToString("F1");
        AplicarDificultadPersonalizada();
    }

    private void OnCantidadEnemigosChanged(float valor)
    {
        cantidadEnemigos = (int)valor;
        if (textoCantidadEnemigos != null)
            textoCantidadEnemigos.text = cantidadEnemigos.ToString();
        AplicarDificultadPersonalizada();
    }

    private void OnTiempoRespawnChanged(float valor)
    {
        tiempoRespawnEnemigos = valor;
        if (textoTiempoRespawn != null)
            textoTiempoRespawn.text = valor.ToString("F0") + "s";
        AplicarDificultadPersonalizada();
    }

    private void OnRecompensasChanged(float valor)
    {
        multiplicadorRecompensas = valor;
        if (textoRecompensas != null)
            textoRecompensas.text = "x" + valor.ToString("F1");
        AplicarDificultadPersonalizada();
    }

    private void AplicarDificultadPersonalizada()
    {
        // Aplicar a zombies existentes
        ControladorZombies[] controladores = FindObjectsByType<ControladorZombies>(FindObjectsSortMode.None);
        foreach (var controlador in controladores)
        {
            controlador.SetCantidadZombies(cantidadEnemigos);
            // Aquí aplicarías otros valores según tu implementación
        }
        
        // Aplicar a enemigos individuales
        ControladorEnemigo[] enemigos = FindObjectsByType<ControladorEnemigo>(FindObjectsSortMode.None);
        foreach (var enemigo in enemigos)
        {
            enemigo.velocidadMovimiento = 2.5f * multiplicadorVelocidadEnemigos;
            enemigo.salud = Mathf.RoundToInt(30 * multiplicadorVidaEnemigos);
            enemigo.daño = Mathf.RoundToInt(20 * multiplicadorDañoEnemigos);
        }
        
        if (mostrarDebug && Time.frameCount % 60 == 0)
        {
            Debug.LogError($"🎯 DIFICULTAD PERSONALIZADA APLICADA:");
            Debug.LogError($"  Velocidad: x{multiplicadorVelocidadEnemigos:F1} | Vida: x{multiplicadorVidaEnemigos:F1} | Daño: x{multiplicadorDañoEnemigos:F1}");
            Debug.LogError($"  Cantidad: {cantidadEnemigos} | Respawn: {tiempoRespawnEnemigos}s | Recompensas: x{multiplicadorRecompensas:F1}");
        }
    }

    private void ActualizarTextosPersonalizados()
    {
        if (textoVelocidadEnemigos != null) textoVelocidadEnemigos.text = "x" + multiplicadorVelocidadEnemigos.ToString("F1");
        if (textoVidaEnemigos != null) textoVidaEnemigos.text = "x" + multiplicadorVidaEnemigos.ToString("F1");
        if (textoDañoEnemigos != null) textoDañoEnemigos.text = "x" + multiplicadorDañoEnemigos.ToString("F1");
        if (textoCantidadEnemigos != null) textoCantidadEnemigos.text = cantidadEnemigos.ToString();
        if (textoTiempoRespawn != null) textoTiempoRespawn.text = tiempoRespawnEnemigos.ToString("F0") + "s";
        if (textoRecompensas != null) textoRecompensas.text = "x" + multiplicadorRecompensas.ToString("F1");
    }
    
    // ACTUALIZAR LOS MÉTODOS DE TEXTO EXISTENTES PARA INCLUIR VERIFICACIÓN NULL
    private void ActualizarTextosValores()
    {
        if (textoVolumenMaster != null) textoVolumenMaster.text = (volumenMaster * 100f).ToString("F0") + "%";
        if (textoVolumenMusica != null) textoVolumenMusica.text = (volumenMusica * 100f).ToString("F0") + "%";
        if (textoVolumenEfectos != null) textoVolumenEfectos.text = (volumenEfectos * 100f).ToString("F0") + "%";
        if (textoFPS != null) textoFPS.text = fpsLimite.ToString("F0");
        if (textoSensibilidad != null) textoSensibilidad.text = sensibilidadMouse.ToString("F1");
        if (textoVidaZombies != null) textoVidaZombies.text = "x" + multiplicadorVidaZombies.ToString("F1");
        if (textoVelocidadZombies != null) textoVelocidadZombies.text = "x" + multiplicadorVelocidadZombies.ToString("F1");
        if (textoDañoZombies != null) textoDañoZombies.text = "x" + multiplicadorDañoZombies.ToString("F1");
        
        // También actualizar textos personalizados
        ActualizarTextosPersonalizados();
    }
    
    // === PERSISTENCIA SIMPLE ===
    
    public void GuardarConfiguracion()
    {
        PlayerPrefs.SetFloat("VolumenMaster", volumenMaster);
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
        PlayerPrefs.SetFloat("VolumenEfectos", volumenEfectos);
        PlayerPrefs.SetInt("PantallaCompleta", pantallaCompleta ? 1 : 0);
        PlayerPrefs.SetInt("CalidadGraficos", calidadGraficos);
        PlayerPrefs.SetInt("VSync", vsync ? 1 : 0);
        PlayerPrefs.SetInt("FPSLimite", fpsLimite);
        PlayerPrefs.SetFloat("SensibilidadMouse", sensibilidadMouse);
        PlayerPrefs.SetInt("InvertirY", invertirY ? 1 : 0);
        PlayerPrefs.SetFloat("MultVidaZombies", multiplicadorVidaZombies);
        PlayerPrefs.SetFloat("MultVelocidadZombies", multiplicadorVelocidadZombies);
        PlayerPrefs.SetFloat("MultDañoZombies", multiplicadorDañoZombies);
        
        // Guardar configuración de dificultad
        PlayerPrefs.SetInt("NivelDificultad", nivelDificultad);
        PlayerPrefs.SetFloat("MultVelocidadEnemigos", multiplicadorVelocidadEnemigos);
        PlayerPrefs.SetFloat("MultVidaEnemigos", multiplicadorVidaEnemigos);
        PlayerPrefs.SetFloat("MultDañoEnemigos", multiplicadorDañoEnemigos);
        PlayerPrefs.SetInt("CantidadEnemigos", cantidadEnemigos);
        PlayerPrefs.SetFloat("TiempoRespawn", tiempoRespawnEnemigos);
        PlayerPrefs.SetFloat("MultRecompensas", multiplicadorRecompensas);
        
        PlayerPrefs.Save();
        Debug.LogError("💾 CONFIGURACIÓN COMPLETA GUARDADA (incluye dificultad)");
    }
    
    public void CargarConfiguracion()
    {
        volumenMaster = PlayerPrefs.GetFloat("VolumenMaster", 1f);
        volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 0.8f);
        volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);
        pantallaCompleta = PlayerPrefs.GetInt("PantallaCompleta", 1) == 1;
        calidadGraficos = PlayerPrefs.GetInt("CalidadGraficos", 2);
        vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        fpsLimite = PlayerPrefs.GetInt("FPSLimite", 60);
        sensibilidadMouse = PlayerPrefs.GetFloat("SensibilidadMouse", 1f);
        invertirY = PlayerPrefs.GetInt("InvertirY", 0) == 1;
        multiplicadorVidaZombies = PlayerPrefs.GetFloat("MultVidaZombies", 1f);
        multiplicadorVelocidadZombies = PlayerPrefs.GetFloat("MultVelocidadZombies", 1f);
        multiplicadorDañoZombies = PlayerPrefs.GetFloat("MultDañoZombies", 1f);
        
        // Cargar configuración de dificultad
        nivelDificultad = PlayerPrefs.GetInt("NivelDificultad", 1);
        multiplicadorVelocidadEnemigos = PlayerPrefs.GetFloat("MultVelocidadEnemigos", 1.0f);
        multiplicadorVidaEnemigos = PlayerPrefs.GetFloat("MultVidaEnemigos", 1.0f);
        multiplicadorDañoEnemigos = PlayerPrefs.GetFloat("MultDañoEnemigos", 1.0f);
        cantidadEnemigos = PlayerPrefs.GetInt("CantidadEnemigos", 12);
        tiempoRespawnEnemigos = PlayerPrefs.GetFloat("TiempoRespawn", 30f);
        multiplicadorRecompensas = PlayerPrefs.GetFloat("MultRecompensas", 1.0f);
        
        dificultadPersonalizada = (nivelDificultad == 4);
        
        Debug.LogError("📂 CONFIGURACIÓN COMPLETA CARGADA (incluye dificultad)");
    }
    
    public void CargarConfiguracionEnUI()
    {
        if (!uiCreada) return;
        
        // Actualizar sliders
        if (sliderVolumenMaster != null) sliderVolumenMaster.value = volumenMaster;
        if (sliderVolumenMusica != null) sliderVolumenMusica.value = volumenMusica;
        if (sliderVolumenEfectos != null) sliderVolumenEfectos.value = volumenEfectos;
        if (sliderFPS != null) sliderFPS.value = fpsLimite;
        if (sliderSensibilidad != null) sliderSensibilidad.value = sensibilidadMouse;
        if (sliderVidaZombies != null) sliderVidaZombies.value = multiplicadorVidaZombies;
        if (sliderVelocidadZombies != null) sliderVelocidadZombies.value = multiplicadorVelocidadZombies;
        if (sliderDañoZombies != null) sliderDañoZombies.value = multiplicadorDañoZombies;
        
        // Actualizar toggles
        if (togglePantallaCompleta != null) togglePantallaCompleta.isOn = pantallaCompleta;
        if (toggleVSync != null) toggleVSync.isOn = vsync;
        if (toggleInvertirY != null) toggleInvertirY.isOn = invertirY;
        
        // Actualizar dropdown
        if (dropdownCalidad != null) dropdownCalidad.value = calidadGraficos;
        
        // ACTUALIZAR BOTÓN DE DIFICULTAD
        if (botonDificultad != null && textoDificultadActual != null)
        {
            textoDificultadActual.text = ObtenerNombreDificultad(nivelDificultad);
            
            Image imagenBoton = botonDificultad.GetComponent<Image>();
            if (imagenBoton != null)
            {
                ActualizarColoresDificultad(imagenBoton, textoDificultadActual);
                
                // Actualizar colores del botón
                ColorBlock colores = botonDificultad.colors;
                colores.normalColor = imagenBoton.color;
                colores.highlightedColor = imagenBoton.color * 1.2f;
                colores.pressedColor = imagenBoton.color * 0.8f;
                colores.selectedColor = imagenBoton.color * 1.1f;
                botonDificultad.colors = colores;
            }
        }
        
        ActualizarPanelPersonalizado();
        ActualizarSlidersPersonalizados();
        
        ActualizarTextosValores();
    }
    
    public void AplicarCambios()
    {
        // Aplicar configuraciones inmediatamente
        AudioListener.volume = volumenMaster;
        Screen.fullScreen = pantallaCompleta;
        QualitySettings.SetQualityLevel(calidadGraficos);
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        Application.targetFrameRate = fpsLimite;
        
        AplicarVolumenMusica(volumenMusica);
        AplicarVolumenEfectos(volumenEfectos);
        AplicarCambiosZombies();
        AplicarDificultadPersonalizada();
        
        GuardarConfiguracion();
        
        Debug.LogError("✅ CAMBIOS APLICADOS Y GUARDADOS");
    }
    
    // NUEVO MÉTODO PARA RESETEAR TODO
    public void ResetearATodosPorDefecto()
    {
        volumenMaster = 1f;
        volumenMusica = 0.8f;
        volumenEfectos = 1f;
        pantallaCompleta = true;
        calidadGraficos = 2;
        vsync = true;
        fpsLimite = 60;
        sensibilidadMouse = 1f;
        invertirY = false;
        multiplicadorVidaZombies = 1f;
        multiplicadorVelocidadZombies = 1f;
        multiplicadorDañoZombies = 1f;
        
        // Configuración de dificultad
        nivelDificultad = 1;
        dificultadPersonalizada = false;
        multiplicadorVelocidadEnemigos = 1.0f;
        multiplicadorVidaEnemigos = 1.0f;
        multiplicadorDañoEnemigos = 1.0f;
        cantidadEnemigos = 10;
        tiempoRespawnEnemigos = 30f;
        multiplicadorRecompensas = 1.0f;
        
        // Actualizar UI
        CargarConfiguracionEnUI();
        
        // Aplicar inmediatamente
        AplicarCambios();
        
        Debug.LogError("🔄 TODAS LAS OPCIONES RESETEADAS A VALORES POR DEFECTO");
    }
    
    // NUEVO MÉTODO PARA VERIFICAR SI LA UI ESTÁ CREADA
    public bool EstaUICreada()
    {
        return uiCreada;
    }
    
    // NUEVO MÉTODO PARA FORZAR RECREACIÓN DE UI
    public void ForzarRecreacionUI()
    {
        uiCreada = false;
        Debug.LogError("🔄 UI marcada para recreación");
    }
    
    // Getters para otros scripts
    public float GetSensibilidadMouse() { return sensibilidadMouse; }
    public bool GetInvertirY() { return invertirY; }
    public float GetMultiplicadorVidaZombies() { return multiplicadorVidaZombies; }
    public float GetMultiplicadorVelocidadZombies() { return multiplicadorVelocidadZombies; }
    public float GetMultiplicadorDañoZombies() { return multiplicadorDañoZombies; }
    
    // Métodos públicos para acceder a la configuración de dificultad
    public float GetMultiplicadorVelocidadEnemigos() { return multiplicadorVelocidadEnemigos; }
    public float GetMultiplicadorVidaEnemigos() { return multiplicadorVidaEnemigos; }
    public float GetMultiplicadorDañoEnemigos() { return multiplicadorDañoEnemigos; }
    public int GetCantidadEnemigos() { return cantidadEnemigos; }
    public float GetTiempoRespawnEnemigos() { return tiempoRespawnEnemigos; }
    public float GetMultiplicadorRecompensas() { return multiplicadorRecompensas; }
    public int GetNivelDificultad() { return nivelDificultad; }
    
    // NUEVO MÉTODO: Crear botón de dificultad
    private Button CrearBotonDificultad(GameObject padre, Vector2 posicion)
    {
        // Contenedor para el botón y texto
        GameObject contenedor = new GameObject("Contenedor_Dificultad");
        contenedor.transform.SetParent(padre.transform, false);
        
        RectTransform rectContenedor = contenedor.AddComponent<RectTransform>();
        rectContenedor.sizeDelta = new Vector2(400f, 40f);
        rectContenedor.anchoredPosition = posicion;
        
        // Label "Dificultad:"
        GameObject labelObj = new GameObject("Label_Dificultad");
        labelObj.transform.SetParent(contenedor.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "Dificultad:";
        label.fontSize = 18;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectLabel = labelObj.GetComponent<RectTransform>();
        rectLabel.sizeDelta = new Vector2(150f, 40f);
        rectLabel.anchoredPosition = new Vector2(-125f, 0f);
        
        // Botón principal
        GameObject botonObj = new GameObject("Boton_Dificultad");
        botonObj.transform.SetParent(contenedor.transform, false);
        
        Button boton = botonObj.AddComponent<Button>();
        Image imagenBoton = botonObj.AddComponent<Image>();
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(200f, 35f);
        rectBoton.anchoredPosition = new Vector2(25f, 0f);
        
        // Texto del botón (muestra la dificultad actual)
        GameObject textoObj = new GameObject("Texto_Dificultad");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        textoDificultadActual = textoObj.AddComponent<TextMeshProUGUI>();
        textoDificultadActual.text = ObtenerNombreDificultad(nivelDificultad);
        textoDificultadActual.fontSize = 16;
        textoDificultadActual.color = Color.white;
        textoDificultadActual.fontStyle = FontStyles.Bold;
        textoDificultadActual.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        // Configurar colores según dificultad
        ActualizarColoresDificultad(imagenBoton, textoDificultadActual);
        
        // Configurar colores del botón
        ColorBlock colores = boton.colors;
        colores.normalColor = imagenBoton.color;
        colores.highlightedColor = imagenBoton.color * 1.2f;
        colores.pressedColor = imagenBoton.color * 0.8f;
        colores.selectedColor = imagenBoton.color * 1.1f;
        boton.colors = colores;
        
        // Flecha indicadora (lado derecho)
        GameObject flechaObj = new GameObject("Flecha_Dificultad");
        flechaObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI flecha = flechaObj.AddComponent<TextMeshProUGUI>();
        flecha.text = "▶";
        flecha.fontSize = 14;
        flecha.color = Color.yellow;
        flecha.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectFlecha = flechaObj.GetComponent<RectTransform>();
        rectFlecha.sizeDelta = new Vector2(20f, 35f);
        rectFlecha.anchoredPosition = new Vector2(80f, 0f);
        
        return boton;
    }
    
    // NUEVO MÉTODO: Obtener nombre de la dificultad
    private string ObtenerNombreDificultad(int nivel)
    {
        return nivel switch
        {
            0 => "🟢 FÁCIL",
            1 => "🟡 NORMAL",
            2 => "🟠 DIFÍCIL",
            3 => "🔴 EXTREMO",
            4 => "⚙️ PERSONALIZADO",
            _ => "🟡 NORMAL"
        };
    }
    
    // NUEVO MÉTODO: Actualizar colores según dificultad
    private void ActualizarColoresDificultad(Image imagen, TextMeshProUGUI texto)
    {
        Color colorFondo, colorTexto;
        
        switch (nivelDificultad)
        {
            case 0: // Fácil
                colorFondo = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Verde
                colorTexto = Color.white;
                break;
            case 1: // Normal
                colorFondo = new Color(0.6f, 0.6f, 0.2f, 0.8f); // Amarillo
                colorTexto = Color.black;
                break;
            case 2: // Difícil
                colorFondo = new Color(0.8f, 0.4f, 0.1f, 0.8f); // Naranja
                colorTexto = Color.white;
                break;
            case 3: // Extremo
                colorFondo = new Color(0.8f, 0.1f, 0.1f, 0.8f); // Rojo
                colorTexto = Color.white;
                break;
            case 4: // Personalizado
                colorFondo = new Color(0.4f, 0.2f, 0.8f, 0.8f); // Púrpura
                colorTexto = Color.white;
                break;
            default:
                colorFondo = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gris
                colorTexto = Color.white;
                break;
        }
        
        imagen.color = colorFondo;
        texto.color = colorTexto;
    }
    
    // NUEVO MÉTODO: Cambiar dificultad al hacer clic
    private void CambiarDificultad()
    {
        // Ciclar entre las dificultades
        nivelDificultad = (nivelDificultad + 1) % 5; // 0-4
        
        Debug.LogError("🔄 DIFICULTAD CAMBIADA: " + ObtenerNombreDificultad(nivelDificultad));
        
        // Actualizar texto del botón
        if (textoDificultadActual != null)
        {
            textoDificultadActual.text = ObtenerNombreDificultad(nivelDificultad);
        }
        
        // Actualizar colores del botón
        if (botonDificultad != null)
        {
            Image imagenBoton = botonDificultad.GetComponent<Image>();
            ActualizarColoresDificultad(imagenBoton, textoDificultadActual);
            
            // Actualizar colores del botón
            ColorBlock colores = botonDificultad.colors;
            colores.normalColor = imagenBoton.color;
            colores.highlightedColor = imagenBoton.color * 1.2f;
            colores.pressedColor = imagenBoton.color * 0.8f;
            colores.selectedColor = imagenBoton.color * 1.1f;
            botonDificultad.colors = colores;
        }
        
        // Lógica existente
        dificultadPersonalizada = (nivelDificultad == 4); // Índice 4 = "Personalizado"
        
        if (!dificultadPersonalizada)
        {
            // Aplicar preset de dificultad
            AplicarPresetDificultad(nivelDificultad);
        }
        
        ActualizarPanelPersonalizado();
        
        if (mostrarDebug)
        {
            Debug.LogError("⚔️ NUEVA DIFICULTAD: " + ObtenerNombreDificultad(nivelDificultad));
        }
    }
}