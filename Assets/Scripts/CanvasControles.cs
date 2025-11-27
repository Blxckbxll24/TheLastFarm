using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Canvas que muestra los controles del juego en Escena1
/// Se puede mostrar/ocultar con una tecla
/// </summary>
public class CanvasControles : MonoBehaviour
{
    [Header("🎮 CONFIGURACIÓN")]
    [SerializeField] private KeyCode teclaParaMostrar = KeyCode.H; // H de Help
    [SerializeField] private bool mostrarAlInicio = true;
    [SerializeField] private float tiempoAutoOcultar = 8f; // Segundos antes de auto-ocultar
    [SerializeField] private bool autoOcultarActivado = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("📱 REFERENCIAS UI")]
    [SerializeField] private Canvas canvasControles;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private Button botonCerrar;
    [SerializeField] private bool crearUIAutomaticamente = true;
    
    [Header("🎨 CONFIGURACIÓN VISUAL")]
    [SerializeField] private Color colorFondo = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    [SerializeField] private Color colorTitulo = new Color(0.3f, 1f, 0.4f, 1f); // Verde granja
    [SerializeField] private Color colorTexto = Color.white;
    [SerializeField] private Color colorTecla = new Color(1f, 1f, 0.3f, 1f); // Amarillo
    
    // Variables internas
    private bool panelVisible = false;
    private float tiempoMostrado = 0f;
    
    // Información de controles para Escena1
    private readonly string[] controles = {
        "🚶 MOVIMIENTO",
        "A/D o ←/→|Mover izquierda/derecha",
        "ESPACIO|Saltar",
        "",
        "🌱 CULTIVO",
        "CLIC DERECHO|Plantar zanahoria (sobre tierra)",
        "C|Cosechar cultivo maduro (cerca del cursor)",
        "",
        "⚔️ COMBATE",
        "CLIC IZQUIERDO|Atacar enemigos cercanos",
        "",
        "🎮 INTERFAZ",
        "ESC|Menú de pausa",
        "H|Mostrar/ocultar esta ayuda",
        "M|Ver estado de monedas",
        "",
        "💰 MONEDAS",
        "+/-|Agregar/quitar monedas (debug)",
        "",
        "🥕 INFO",
        "Las zanahorias te dan monedas|",
        "Cultiva de día, lucha de noche|",
        "Explora y sobrevive|"
    };
    
    void Start()
    {
        if (crearUIAutomaticamente)
        {
            CrearUICompleta();
        }
        
        ConfigurarUI();
        
        if (mostrarAlInicio)
        {
            MostrarControles();
        }
        else
        {
            OcultarControles();
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎮 CANVAS CONTROLES INICIADO - Tecla: " + teclaParaMostrar);
        }
    }
    
    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Detectar tecla para mostrar/ocultar
        if (Input.GetKeyDown(teclaParaMostrar))
        {
            if (panelVisible)
            {
                OcultarControles();
            }
            else
            {
                MostrarControles();
            }
        }
        
        // Auto-ocultar después de tiempo
        if (panelVisible && autoOcultarActivado)
        {
            tiempoMostrado += Time.deltaTime;
            
            if (tiempoMostrado >= tiempoAutoOcultar)
            {
                OcultarControles();
            }
        }
        
        // También ocultar con ESC
        if (panelVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            OcultarControles();
        }
    }
    
    private void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI DE CONTROLES");
        }
        
        // Crear Canvas
        CrearCanvas();
        
        // Crear panel principal
        CrearPanelControles();
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DE CONTROLES CREADA");
        }
    }
    
    private void CrearCanvas()
    {
        if (canvasControles == null)
        {
            GameObject canvasObj = new GameObject("Canvas_Controles");
            canvasControles = canvasObj.AddComponent<Canvas>();
            canvasControles.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasControles.sortingOrder = 9999; // 🔧 MUY ALTO para estar por encima de TODO
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // 🔧 ASEGURAR QUE ESTÉ EN LA CAPA MÁS ALTA
            canvasControles.sortingLayerName = "UI"; // O "Default" si no tienes layer UI
            canvasControles.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            canvasControles.overrideSorting = true;
            canvasControles.sortingOrder = 9999;
            
            // 🆕 CONFIGURAR CLIPPING RECT PARA MANTENER DENTRO DE LÍMITES
            canvasControles.pixelPerfect = true;
            
            if (mostrarDebug)
            {
                Debug.LogError($"📱 Canvas controles creado con sorting order: {canvasControles.sortingOrder}");
            }
        }
    }

    private void CrearPanelControles()
    {
        // Fondo semi-transparente MÁS OPACO
        GameObject fondo = new GameObject("Fondo_Controles");
        fondo.transform.SetParent(canvasControles.transform, false);
        
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = new Color(colorFondo.r, colorFondo.g, colorFondo.b, 0.98f); // 🔧 CASI OPACO
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // 🔧 ASEGURAR QUE EL FONDO CAPTURE CLICKS
        imagenFondo.raycastTarget = true; // Capturar clicks para que no pasen a elementos de atrás
        
        // 🆕 Panel principal CON LÍMITES MÁS ESTRICTOS
        panelControles = new GameObject("Panel_Controles");
        panelControles.transform.SetParent(canvasControles.transform, false);
        
        Image imagenPanel = panelControles.AddComponent<Image>();
        imagenPanel.color = new Color(0.05f, 0.15f, 0.05f, 0.98f); // 🔧 CASI OPACO
        
        RectTransform rectPanel = panelControles.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(750f, 550f); // 🔧 TAMAÑO REDUCIDO PARA QUE QUEPA MEJOR
        rectPanel.anchorMin = new Vector2(0.5f, 0.5f);
        rectPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rectPanel.pivot = new Vector2(0.5f, 0.5f);
        rectPanel.anchoredPosition = Vector2.zero;
        
        // 🔧 AGREGAR CLIPPING MASK AL PANEL
        Mask maskPanel = panelControles.AddComponent<Mask>();
        maskPanel.showMaskGraphic = true; // Mostrar el fondo del panel
        
        // 🔧 AGREGAR BORDE VISIBLE AL PANEL
        Outline outline = panelControles.AddComponent<Outline>();
        outline.effectColor = colorTitulo;
        outline.effectDistance = new Vector2(3, 3);
        
        // Shadow para más visibilidad
        Shadow shadow = panelControles.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(4, -4);
        
        // Título principal - AJUSTADO
        CrearTitulo(panelControles, "🎮 CONTROLES DE THE LAST FARM", new Vector2(0f, 240f), 30); // Tamaño reducido
        
        // Subtítulo - AJUSTADO
        CrearTexto(panelControles, "Escena 1 - La Granja", new Vector2(0f, 210f), 16, FontStyles.Italic, new Color(0.8f, 0.8f, 0.8f, 1f));
        
        // 🆕 CREAR LISTA DE CONTROLES CON SCROLL LIMITADO
        CrearListaControles(panelControles);
        
        // Botón cerrar - REPOSICIONADO
        CrearBotonCerrar(panelControles);
        
        // Texto de ayuda en la parte inferior - AJUSTADO
        CrearTexto(panelControles, $"Presiona {teclaParaMostrar} en cualquier momento para ver esta ayuda", 
                  new Vector2(0f, -240f), 14, FontStyles.Italic, new Color(0.7f, 0.7f, 0.7f, 1f));
    }
    
    // 🔧 MÉTODO SIMPLIFICADO SIN SCROLL PROBLEMÁTICO
    private void CrearListaControles(GameObject padre)
    {
        // Crear contenedor simple SIN ScrollView
        GameObject contenido = new GameObject("Contenido_Controles");
        contenido.transform.SetParent(padre.transform, false);
        
        RectTransform rectContenido = contenido.AddComponent<RectTransform>();
        rectContenido.sizeDelta = new Vector2(700f, 350f); // Tamaño fijo para que quepa
        rectContenido.anchoredPosition = new Vector2(0f, -10f); // Posicionado al inicio del panel
        
        // Fondo ligero para el contenido
        Image imagenFondo = contenido.AddComponent<Image>();
        imagenFondo.color = new Color(0.02f, 0.08f, 0.02f, 0.5f);
        
        // Agregar controles directamente al contenido
        AgregarControlesAlContenidoSimple(contenido);
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ CONTENIDO DE CONTROLES CREADO SIN SCROLL");
        }
    }
    
    // 🆕 MÉTODO SIMPLIFICADO PARA AGREGAR CONTROLES
    private void AgregarControlesAlContenidoSimple(GameObject contenido)
    {
        float yPos = 150f; // 🔧 EMPEZAR DESDE ARRIBA DEL CONTENEDOR
        float espaciado = 22f; // Espaciado compacto
        
        foreach (string control in controles)
        {
            if (string.IsNullOrEmpty(control))
            {
                // Espacio en blanco reducido
                yPos -= 10f;
                continue;
            }
            
            if (control.Contains("|"))
            {
                // Control con tecla|descripción
                string[] partes = control.Split('|');
                if (partes.Length == 2)
                {
                    CrearControlIndividualSimple(contenido, partes[0].Trim(), partes[1].Trim(), yPos);
                }
                yPos -= espaciado;
            }
            else
            {
                // Título de sección
                CrearTituloSeccionSimple(contenido, control, yPos);
                yPos -= espaciado + 8f; // Un poco más de espacio después del título
            }
            
            // 🔧 PARAR SI LLEGAMOS AL FINAL DEL CONTENEDOR
            if (yPos < -150f)
            {
                break; // No agregar más controles si no caben
            }
        }
    }
    
    // 🔧 VERSIÓN SIMPLIFICADA DE CONTROL INDIVIDUAL
    private void CrearControlIndividualSimple(GameObject padre, string tecla, string descripcion, float yPos)
    {
        // Container para la línea de control
        GameObject container = new GameObject($"Control_{tecla.Replace(" ", "_")}");
        container.transform.SetParent(padre.transform, false);
        
        RectTransform rectContainer = container.AddComponent<RectTransform>();
        rectContainer.sizeDelta = new Vector2(650f, 20f);
        rectContainer.anchoredPosition = new Vector2(0f, yPos);
        
        // Texto de la tecla (izquierda)
        GameObject textoTecla = new GameObject("Texto_Tecla");
        textoTecla.transform.SetParent(container.transform, false);
        
        TextMeshProUGUI compTecla = textoTecla.AddComponent<TextMeshProUGUI>();
        compTecla.text = tecla;
        compTecla.fontSize = 14;
        compTecla.color = colorTecla;
        compTecla.fontStyle = FontStyles.Bold;
        compTecla.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectTecla = textoTecla.GetComponent<RectTransform>();
        rectTecla.sizeDelta = new Vector2(160f, 20f);
        rectTecla.anchoredPosition = new Vector2(-200f, 0f);
        
        // Separador
        GameObject separador = new GameObject("Separador");
        separador.transform.SetParent(container.transform, false);
        
        TextMeshProUGUI compSeparador = separador.AddComponent<TextMeshProUGUI>();
        compSeparador.text = "→";
        compSeparador.fontSize = 12;
        compSeparador.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        compSeparador.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectSeparador = separador.GetComponent<RectTransform>();
        rectSeparador.sizeDelta = new Vector2(20f, 20f);
        rectSeparador.anchoredPosition = new Vector2(-100f, 0f);
        
        // Texto de la descripción (derecha)
        GameObject textoDesc = new GameObject("Texto_Descripcion");
        textoDesc.transform.SetParent(container.transform, false);
        
        TextMeshProUGUI compDesc = textoDesc.AddComponent<TextMeshProUGUI>();
        compDesc.text = descripcion;
        compDesc.fontSize = 13;
        compDesc.color = colorTexto;
        compDesc.alignment = TextAlignmentOptions.Left;
        compDesc.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform rectDesc = textoDesc.GetComponent<RectTransform>();
        rectDesc.sizeDelta = new Vector2(350f, 20f);
        rectDesc.anchoredPosition = new Vector2(100f, 0f);
    }
    
    // 🔧 VERSIÓN SIMPLIFICADA DE TÍTULO DE SECCIÓN
    private void CrearTituloSeccionSimple(GameObject padre, string titulo, float yPos)
    {
        GameObject tituloObj = new GameObject($"Titulo_{titulo.Replace(" ", "_")}");
        tituloObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI compTitulo = tituloObj.AddComponent<TextMeshProUGUI>();
        compTitulo.text = titulo;
        compTitulo.fontSize = 16;
        compTitulo.color = colorTitulo;
        compTitulo.fontStyle = FontStyles.Bold;
        compTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = tituloObj.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(650f, 22f);
        rectTitulo.anchoredPosition = new Vector2(0f, yPos);
        
        // Línea decorativa más pequeña
        GameObject linea = new GameObject("Linea_Decorativa");
        linea.transform.SetParent(padre.transform, false);
        
        Image imagenLinea = linea.AddComponent<Image>();
        imagenLinea.color = colorTitulo;
        
        RectTransform rectLinea = linea.GetComponent<RectTransform>();
        rectLinea.sizeDelta = new Vector2(200f, 1f);
        rectLinea.anchoredPosition = new Vector2(0f, yPos - 12f);
    }

    private void CrearBotonCerrar(GameObject padre)
    {
        GameObject botonObj = new GameObject("Boton_Cerrar");
        botonObj.transform.SetParent(padre.transform, false);
        
        botonCerrar = botonObj.AddComponent<Button>();
        Image imgBoton = botonObj.AddComponent<Image>();
        imgBoton.color = new Color(0.8f, 0.2f, 0.2f, 0.9f); // Rojo más opaco
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(100f, 35f); // 🔧 BOTÓN MÁS PEQUEÑO
        rectBoton.anchoredPosition = new Vector2(300f, -240f); // REPOSICIONADO
        
        // Texto del botón
        GameObject textoObj = new GameObject("Texto_Cerrar");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = "❌ CERRAR";
        textoBoton.fontSize = 14; // 🔧 TAMAÑO REDUCIDO
        textoBoton.color = Color.white;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        // Configurar colores del botón
        ColorBlock colores = botonCerrar.colors;
        colores.normalColor = imgBoton.color;
        colores.highlightedColor = imgBoton.color * 1.2f;
        colores.pressedColor = imgBoton.color * 0.8f;
        botonCerrar.colors = colores;
    }
    
    private void ConfigurarUI()
    {
        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
            botonCerrar.onClick.AddListener(OcultarControles);
        }
    }
    
    public void MostrarControles()
    {
        panelVisible = true;
        tiempoMostrado = 0f;
        
        if (canvasControles != null)
        {
            // 🔧 VERIFICAR Y FORZAR SORTING ORDER AL MOSTRAR
            canvasControles.sortingOrder = 9999;
            canvasControles.overrideSorting = true;
            canvasControles.gameObject.SetActive(true);
            
            // 🔧 VERIFICAR QUE ESTÉ EN EL FRENTE
            Canvas[] todosCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int maxSortingOrder = 0;
            foreach (Canvas canvas in todosCanvas)
            {
                if (canvas != canvasControles && canvas.sortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = canvas.sortingOrder;
                }
            }
            
            // Asegurar que esté por encima de todos
            if (canvasControles.sortingOrder <= maxSortingOrder)
            {
                canvasControles.sortingOrder = maxSortingOrder + 100;
                Debug.LogError($"🔧 Sorting order ajustado a: {canvasControles.sortingOrder}");
            }
        }
        
        if (panelControles != null)
        {
            panelControles.SetActive(true);
        }
        
        // FORZAR cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (mostrarDebug)
        {
            Debug.LogError("🎮 CONTROLES MOSTRADOS");
            Debug.LogError($"  - Canvas sorting order: {canvasControles.sortingOrder}");
            Debug.LogError($"  - Canvas override sorting: {canvasControles.overrideSorting}");
        }
    }
    
    public void OcultarControles()
    {
        panelVisible = false;
        tiempoMostrado = 0f;
        
        if (canvasControles != null)
        {
            canvasControles.gameObject.SetActive(false);
        }
        
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎮 CONTROLES OCULTADOS");
        }
    }
    
    // Métodos públicos para control externo
    public void AlternarControles()
    {
        if (panelVisible)
        {
            OcultarControles();
        }
        else
        {
            MostrarControles();
        }
    }
    
    public bool EstanControlesVisibles()
    {
        return panelVisible;
    }
    
    public void DesactivarAutoOcultar()
    {
        autoOcultarActivado = false;
    }
    
    public void ActivarAutoOcultar()
    {
        autoOcultarActivado = true;
        tiempoMostrado = 0f; // Reiniciar contador
    }
    
    // Métodos de testing
    [ContextMenu("🧪 Test - Mostrar Controles")]
    public void TestMostrarControles()
    {
        MostrarControles();
    }
    
    [ContextMenu("🧪 Test - Ocultar Controles")]
    public void TestOcultarControles()
    {
        OcultarControles();
    }
    
    [ContextMenu("🧪 Test - Alternar Controles")]
    public void TestAlternarControles()
    {
        AlternarControles();
    }
    
    // 🆕 MÉTODO PARA VERIFICAR ESTADO DEL CANVAS
    [ContextMenu("🔍 Verificar Estado Canvas")]
    public void VerificarEstadoCanvas()
    {
        Debug.LogError("🔍 ESTADO DEL CANVAS CONTROLES:");
        Debug.LogError("=====================================");
        
        if (canvasControles != null)
        {
            Debug.LogError($"📱 CANVAS:");
            Debug.LogError($"  - Nombre: {canvasControles.name}");
            Debug.LogError($"  - Activo: {canvasControles.gameObject.activeInHierarchy}");
            Debug.LogError($"  - Render Mode: {canvasControles.renderMode}");
            Debug.LogError($"  - Sorting Order: {canvasControles.sortingOrder}");
            Debug.LogError($"  - Override Sorting: {canvasControles.overrideSorting}");
            Debug.LogError($"  - Sorting Layer: {canvasControles.sortingLayerName}");
            
            // Verificar otros canvas
            Canvas[] todosCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.LogError($"📊 TODOS LOS CANVAS EN ESCENA ({todosCanvas.Length}):");
            
            foreach (Canvas canvas in todosCanvas)
            {
                Debug.LogError($"  - {canvas.name}: Order={canvas.sortingOrder}, Active={canvas.gameObject.activeInHierarchy}");
            }
        }
        else
        {
            Debug.LogError("❌ Canvas es NULL");
        }
        
        Debug.LogError("=====================================");
    }
    
    // 🆕 MÉTODO PARA FORZAR AL FRENTE
    [ContextMenu("🔧 Forzar Al Frente")]
    public void ForzarAlFrente()
    {
        if (canvasControles != null)
        {
            // Encontrar el sorting order más alto
            Canvas[] todosCanvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            int maxOrder = 0;
            
            foreach (Canvas canvas in todosCanvas)
            {
                if (canvas != canvasControles && canvas.sortingOrder > maxOrder)
                {
                    maxOrder = canvas.sortingOrder;
                }
            }
            
            // Poner 1000 puntos por encima
            canvasControles.sortingOrder = maxOrder + 1000;
            canvasControles.overrideSorting = true;
            
            Debug.LogError($"🚀 CANVAS FORZADO AL FRENTE - Nuevo sorting order: {canvasControles.sortingOrder}");
        }
    }
    
    void OnDestroy()
    {
        // Limpiar referencias
        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveAllListeners();
        }
    }
    
    // 🆕 MÉTODO FALTANTE: CREAR TÍTULO
    private void CrearTitulo(GameObject padre, string texto, Vector2 posicion, float tamaño)
    {
        GameObject titulo = new GameObject("Titulo_" + texto.Replace(" ", "_"));
        titulo.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = texto;
        textoTitulo.fontSize = tamaño;
        textoTitulo.color = colorTitulo;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(700f, tamaño + 10f);
        rectTitulo.anchoredPosition = posicion;
    }
    
    // 🆕 MÉTODO FALTANTE: CREAR TEXTO GENERAL
    private void CrearTexto(GameObject padre, string texto, Vector2 posicion, float tamaño = 16, FontStyles estilo = FontStyles.Normal, Color? color = null)
    {
        GameObject textoObj = new GameObject("Texto_" + texto.Replace(" ", "_").Substring(0, Mathf.Min(10, texto.Length)));
        textoObj.transform.SetParent(padre.transform, false);
        
        TextMeshProUGUI textoComponent = textoObj.AddComponent<TextMeshProUGUI>();
        textoComponent.text = texto;
        textoComponent.fontSize = tamaño;
        textoComponent.color = color ?? colorTexto;
        textoComponent.fontStyle = estilo;
        textoComponent.alignment = TextAlignmentOptions.Center;
        textoComponent.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.sizeDelta = new Vector2(650f, tamaño + 5f);
        rectTexto.anchoredPosition = posicion;
    }
}
