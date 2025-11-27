using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Canvas que muestra la historia del juego antes de comenzar
/// Se activa después de presionar "JUGAR" en el menú principal
/// </summary>
public class CanvasHistoriaIntro : MonoBehaviour
{
    [Header("📱 CONFIGURACIÓN UI")]
    [SerializeField] private Canvas canvasHistoria;
    [SerializeField] private GameObject panelHistoria;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoHistoria;
    [SerializeField] private Button botonSaltar;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎨 CONFIGURACIÓN VISUAL")]
    [SerializeField] private string escenaDestino = "Escena1";
    [SerializeField] private float velocidadEscritura = 0.05f; // Tiempo entre caracteres
    [SerializeField] private bool efectoMaquinaEscribir = true;
    [SerializeField] private bool pausarTiempoMientrasLee = true;
    
    [Header("🎵 AUDIO")]
    [SerializeField] private AudioClip musicaIntro;
    [SerializeField] private AudioClip sonidoTecleo;
    [SerializeField] private AudioSource audioSource;
    
    // Texto de la historia
    private string historiaCompleta = @"THE LAST FARM

El mundo cambió. Hace tres años, el virus se propagó sin control. La civilización colapsó en semanas. Las ciudades se convirtieron en cementerios. Los gobiernos cayeron. La ley desapareció.

Los muertos no descansaban. Se levantaban, hambrientos, sin fin. Las hordas crecían cada noche, fusionándose en oleadas de destrucción pura. Los sobrevivientes se volvieron cazadores entre sí, luchas que se tornaban tan brutales como los mismísimos no-muertos.

Pero tú encontraste algo raro: una granja apartada en las afueras, a kilómetros de toda civilización. Parece que el tiempo se detuvo aquí. Los campos todavía germinan. La casa aún tiene vida.

Es tu último refugio. Tu única oportunidad. De día cultivas zanahorias, tu única fuente de vida. Cuando terminas de sembrar y cosechar, entras al Modo Noche: un apocalipsis preparado donde debes enfrentar hordas zombies.

¿Cuántas noches de combate podrás aguantar?";
    
    // Variables internas
    private bool saltando = false;
    private bool mostrandoTexto = false;
    private Coroutine corrutinaEscritura;
    
    // Colores y estilos
    private readonly Color colorFondo = new Color(0.05f, 0.05f, 0.05f, 0.95f);
    private readonly Color colorTitulo = new Color(0.8f, 0.2f, 0.2f, 1f); // Rojo apocalíptico
    private readonly Color colorTexto = new Color(0.9f, 0.9f, 0.8f, 1f); // Blanco amarillento
    private readonly Color colorBotonSaltar = new Color(0.7f, 0.7f, 0.2f, 0.8f); // Amarillo
    private readonly Color colorBotonContinuar = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Verde
    
    void Awake()
    {
        // Asegurar que el cursor esté visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (pausarTiempoMientrasLee)
        {
            Time.timeScale = 1f; // Asegurar que el tiempo esté normal
        }
    }
    
    void Start()
    {
        if (mostrarDebug)
        {
            Debug.LogError("📖 CANVAS HISTORIA INICIADO");
        }
        
        if (crearUIAutomaticamente)
        {
            CrearUICompleta();
        }
        
        ConfigurarAudio();
        
        // Mostrar inmediatamente
        MostrarHistoria();
    }
    
    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Detectar teclas para saltar
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (mostrandoTexto)
            {
                CompletarTextoInmediatamente();
            }
            else
            {
                IrAEscena1();
            }
        }
        
        // Click para saltar
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (mostrandoTexto)
            {
                CompletarTextoInmediatamente();
            }
        }
    }
    
    private void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI DE HISTORIA");
        }
        
        // Crear Canvas
        CrearCanvas();
        
        // Crear panel principal
        CrearPanelHistoria();
        
        // Crear botones
        CrearBotones();
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DE HISTORIA CREADA");
        }
    }
    
    private void CrearCanvas()
    {
        if (canvasHistoria == null)
        {
            GameObject canvasObj = new GameObject("Canvas_HistoriaIntro");
            canvasHistoria = canvasObj.AddComponent<Canvas>();
            canvasHistoria.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasHistoria.sortingOrder = 999; // Muy alto
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }
    
    private void CrearPanelHistoria()
    {
        // Fondo completo
        GameObject fondo = new GameObject("Fondo_Historia");
        fondo.transform.SetParent(canvasHistoria.transform, false);
        
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = colorFondo;
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // Panel central
        panelHistoria = new GameObject("Panel_Historia");
        panelHistoria.transform.SetParent(canvasHistoria.transform, false);
        
        RectTransform rectPanel = panelHistoria.AddComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = new Vector2(100f, 50f);
        rectPanel.offsetMax = new Vector2(-100f, -50f);
        
        // Título
        CrearTitulo();
        
        // Texto de historia
        CrearTextoHistoria();
        
        // Indicador de "Presiona para continuar"
        CrearIndicadorContinuar();
    }
    
    private void CrearTitulo()
    {
        GameObject tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(panelHistoria.transform, false);
        
        textoTitulo = tituloObj.AddComponent<TextMeshProUGUI>();
        textoTitulo.fontSize = 64;
        textoTitulo.color = colorTitulo;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        // Efecto de sombra/contorno (TextMeshPro usa outlineColor/outlineWidth, no existe enableOutline)
        textoTitulo.outlineColor = Color.black;
        textoTitulo.outlineWidth = 0.3f;
        
        RectTransform rectTitulo = tituloObj.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(800f, 100f);
        rectTitulo.anchorMin = new Vector2(0.5f, 0.9f);
        rectTitulo.anchorMax = new Vector2(0.5f, 0.9f);
        rectTitulo.pivot = new Vector2(0.5f, 0.5f);
        rectTitulo.anchoredPosition = Vector2.zero;
    }
    
    private void CrearTextoHistoria()
    {
        GameObject historiaObj = new GameObject("Texto_Historia");
        historiaObj.transform.SetParent(panelHistoria.transform, false);
        
        textoHistoria = historiaObj.AddComponent<TextMeshProUGUI>();
        textoHistoria.text = ""; // Comenzar vacío para efecto de escritura
        textoHistoria.fontSize = 24;
        textoHistoria.color = colorTexto;
        textoHistoria.alignment = TextAlignmentOptions.TopLeft;
        textoHistoria.textWrappingMode = TextWrappingModes.Normal;
        textoHistoria.lineSpacing = 10f; // Espaciado entre líneas
        
        // Configurar scroll si es necesario
        RectTransform rectHistoria = historiaObj.GetComponent<RectTransform>();
        rectHistoria.anchorMin = new Vector2(0.1f, 0.15f);
        rectHistoria.anchorMax = new Vector2(0.9f, 0.75f);
        rectHistoria.offsetMin = Vector2.zero;
        rectHistoria.offsetMax = Vector2.zero;
        
        // Crear ScrollRect para textos largos
        GameObject scrollViewObj = new GameObject("ScrollView_Historia");
        scrollViewObj.transform.SetParent(panelHistoria.transform, false);
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.content = rectHistoria;
        scrollRect.viewport = rectHistoria;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        RectTransform rectScrollView = scrollViewObj.GetComponent<RectTransform>();
        rectScrollView.anchorMin = new Vector2(0.1f, 0.15f);
        rectScrollView.anchorMax = new Vector2(0.9f, 0.75f);
        rectScrollView.offsetMin = Vector2.zero;
        rectScrollView.offsetMax = Vector2.zero;
    }
    
    private void CrearIndicadorContinuar()
    {
        GameObject indicadorObj = new GameObject("Indicador_Continuar");
        indicadorObj.transform.SetParent(panelHistoria.transform, false);
        
        TextMeshProUGUI textoIndicador = indicadorObj.AddComponent<TextMeshProUGUI>();
        textoIndicador.text = "Presiona ESPACIO o clic para continuar...";
        textoIndicador.fontSize = 18;
        textoIndicador.color = new Color(colorTexto.r, colorTexto.g, colorTexto.b, 0.7f);
        textoIndicador.fontStyle = FontStyles.Italic;
        textoIndicador.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectIndicador = indicadorObj.GetComponent<RectTransform>();
        rectIndicador.sizeDelta = new Vector2(600f, 30f);
        rectIndicador.anchorMin = new Vector2(0.5f, 0.05f);
        rectIndicador.anchorMax = new Vector2(0.5f, 0.05f);
        rectIndicador.pivot = new Vector2(0.5f, 0.5f);
        rectIndicador.anchoredPosition = Vector2.zero;
        
        // Efecto de parpadeo
        StartCoroutine(EfectoParpadeoIndicador(textoIndicador));
    }
    
    private void CrearBotones()
    {
        // Botón Saltar (esquina superior derecha)
        GameObject botonSaltarObj = new GameObject("Boton_Saltar");
        botonSaltarObj.transform.SetParent(canvasHistoria.transform, false);
        
        botonSaltar = botonSaltarObj.AddComponent<Button>();
        Image imgSaltar = botonSaltarObj.AddComponent<Image>();
        imgSaltar.color = colorBotonSaltar;
        
        RectTransform rectSaltar = botonSaltarObj.GetComponent<RectTransform>();
        rectSaltar.sizeDelta = new Vector2(120f, 50f);
        rectSaltar.anchorMin = new Vector2(1f, 1f);
        rectSaltar.anchorMax = new Vector2(1f, 1f);
        rectSaltar.pivot = new Vector2(1f, 1f);
        rectSaltar.anchoredPosition = new Vector2(-20f, -20f);
        
        // Texto del botón
        GameObject textoSaltarObj = new GameObject("Texto_Saltar");
        textoSaltarObj.transform.SetParent(botonSaltarObj.transform, false);
        
        TextMeshProUGUI textoSaltar = textoSaltarObj.AddComponent<TextMeshProUGUI>();
        textoSaltar.text = "SALTAR";
        textoSaltar.fontSize = 18;
        textoSaltar.color = Color.black;
        textoSaltar.fontStyle = FontStyles.Bold;
        textoSaltar.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTextoSaltar = textoSaltarObj.GetComponent<RectTransform>();
        rectTextoSaltar.anchorMin = Vector2.zero;
        rectTextoSaltar.anchorMax = Vector2.one;
        rectTextoSaltar.offsetMin = Vector2.zero;
        rectTextoSaltar.offsetMax = Vector2.zero;
        
        // Botón Continuar (esquina inferior derecha) - Inicialmente oculto
        GameObject botonContinuarObj = new GameObject("Boton_Continuar");
        botonContinuarObj.transform.SetParent(canvasHistoria.transform, false);
        
        botonContinuar = botonContinuarObj.AddComponent<Button>();
        Image imgContinuar = botonContinuarObj.AddComponent<Image>();
        imgContinuar.color = colorBotonContinuar;
        
        RectTransform rectContinuar = botonContinuarObj.GetComponent<RectTransform>();
        rectContinuar.sizeDelta = new Vector2(150f, 60f);
        rectContinuar.anchorMin = new Vector2(1f, 0f);
        rectContinuar.anchorMax = new Vector2(1f, 0f);
        rectContinuar.pivot = new Vector2(1f, 0f);
        rectContinuar.anchoredPosition = new Vector2(-20f, 20f);
        
        // Texto del botón continuar
        GameObject textoContinuarObj = new GameObject("Texto_Continuar");
        textoContinuarObj.transform.SetParent(botonContinuarObj.transform, false);
        
        TextMeshProUGUI textoContinuar = textoContinuarObj.AddComponent<TextMeshProUGUI>();
        textoContinuar.text = "COMENZAR";
        textoContinuar.fontSize = 20;
        textoContinuar.color = Color.white;
        textoContinuar.fontStyle = FontStyles.Bold;
        textoContinuar.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTextoContinuar = textoContinuarObj.GetComponent<RectTransform>();
        rectTextoContinuar.anchorMin = Vector2.zero;
        rectTextoContinuar.anchorMax = Vector2.one;
        rectTextoContinuar.offsetMin = Vector2.zero;
        rectTextoContinuar.offsetMax = Vector2.zero;
        
        // Configurar eventos
        botonSaltar.onClick.AddListener(IrAEscena1);
        botonContinuar.onClick.AddListener(IrAEscena1);
        
        // Ocultar botón continuar inicialmente
        botonContinuar.gameObject.SetActive(false);
    }
    
    private void ConfigurarAudio()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicaIntro != null)
        {
            audioSource.clip = musicaIntro;
            audioSource.loop = true;
            audioSource.volume = 0.6f;
            audioSource.Play();
            
            if (mostrarDebug)
            {
                Debug.LogError("🎵 MÚSICA DE INTRO INICIADA");
            }
        }
    }
    
    public void MostrarHistoria()
    {
        if (mostrarDebug)
        {
            Debug.LogError("📖 MOSTRANDO HISTORIA");
        }
        
        // Mostrar canvas
        if (canvasHistoria != null)
        {
            canvasHistoria.gameObject.SetActive(true);
        }
        
        // Comenzar efecto de escritura
        if (efectoMaquinaEscribir)
        {
            corrutinaEscritura = StartCoroutine(EfectoMaquinaEscribir());
        }
        else
        {
            textoHistoria.text = historiaCompleta;
            MostrarBotonContinuar();
        }
    }
    
    private IEnumerator EfectoMaquinaEscribir()
    {
        mostrandoTexto = true;
        textoHistoria.text = "";
        
        for (int i = 0; i < historiaCompleta.Length; i++)
        {
            if (saltando) break;
            
            textoHistoria.text += historiaCompleta[i];
            
            // Sonido de tecleo ocasional
            if (sonidoTecleo != null && i % 3 == 0)
            {
                audioSource.PlayOneShot(sonidoTecleo, 0.3f);
            }
            
            // Pausa más larga en puntos y líneas nuevas
            float pausa = velocidadEscritura;
            if (historiaCompleta[i] == '.' || historiaCompleta[i] == '\n')
            {
                pausa *= 3f;
            }
            else if (historiaCompleta[i] == ',' || historiaCompleta[i] == ';')
            {
                pausa *= 1.5f;
            }
            
            yield return new WaitForSeconds(pausa);
        }
        
        mostrandoTexto = false;
        MostrarBotonContinuar();
    }
    
    private void CompletarTextoInmediatamente()
    {
        if (corrutinaEscritura != null)
        {
            StopCoroutine(corrutinaEscritura);
        }
        
        saltando = true;
        mostrandoTexto = false;
        textoHistoria.text = historiaCompleta;
        MostrarBotonContinuar();
        
        if (mostrarDebug)
        {
            Debug.LogError("⏩ TEXTO COMPLETADO INMEDIATAMENTE");
        }
    }
    
    private void MostrarBotonContinuar()
    {
        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(true);
            
            // Efecto de aparición
            StartCoroutine(EfectoAparicionBoton());
        }
    }
    
    private IEnumerator EfectoAparicionBoton()
    {
        Image imgBoton = botonContinuar.GetComponent<Image>();
        TextMeshProUGUI textoBoton = botonContinuar.GetComponentInChildren<TextMeshProUGUI>();
        
        Color colorOriginalImg = imgBoton.color;
        Color colorOriginalTexto = textoBoton.color;
        
        // Empezar transparente
        imgBoton.color = new Color(colorOriginalImg.r, colorOriginalImg.g, colorOriginalImg.b, 0f);
        textoBoton.color = new Color(colorOriginalTexto.r, colorOriginalTexto.g, colorOriginalTexto.b, 0f);
        
        float duracion = 0.5f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = tiempo / duracion;
            
            imgBoton.color = new Color(colorOriginalImg.r, colorOriginalImg.g, colorOriginalImg.b, alpha * colorOriginalImg.a);
            textoBoton.color = new Color(colorOriginalTexto.r, colorOriginalTexto.g, colorOriginalTexto.b, alpha * colorOriginalTexto.a);
            
            yield return null;
        }
        
        // Asegurar color final
        imgBoton.color = colorOriginalImg;
        textoBoton.color = colorOriginalTexto;
    }
    
    private IEnumerator EfectoParpadeoIndicador(TextMeshProUGUI texto)
    {
        Color colorOriginal = texto.color;
        
        while (texto != null)
        {
            // Solo parpadear mientras se está mostrando el texto
            if (mostrandoTexto)
            {
                float alpha = (Mathf.Sin(Time.time * 3f) + 1f) / 2f;
                texto.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha * 0.7f);
            }
            else
            {
                texto.color = colorOriginal;
            }
            
            yield return null;
        }
    }
    
    public void IrAEscena1()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🚀 YENDO A " + escenaDestino);
        }
        
        // Restaurar tiempo si se había pausado
        Time.timeScale = 1f;
        
        // Detener audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        try
        {
            SceneManager.LoadScene(escenaDestino);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR AL CARGAR " + escenaDestino + ": " + e.Message);
            
            // Fallback por índice
            try
            {
                SceneManager.LoadScene(1);
            }
            catch
            {
                Debug.LogError("❌ Error total cargando escena de juego");
            }
        }
    }
    
    // Método para ser llamado desde MenuPrincipal
    public static void MostrarHistoriaDesdeMenu()
    {
        // Crear el canvas de historia en la escena actual
        GameObject historiaObj = new GameObject("HistoriaIntro");
        CanvasHistoriaIntro historiaScript = historiaObj.AddComponent<CanvasHistoriaIntro>();
        
        // La historia se mostrará automáticamente en Start()
    }
    
    void OnDestroy()
    {
        // Limpiar corrutinas
        if (corrutinaEscritura != null)
        {
            StopCoroutine(corrutinaEscritura);
        }
        StopAllCoroutines();
    }
    
    // Métodos de testing
    [ContextMenu("🧪 Test - Mostrar Historia")]
    public void TestMostrarHistoria()
    {
        MostrarHistoria();
    }
    
    [ContextMenu("🧪 Test - Saltar a Escena1")]
    public void TestIrAEscena1()
    {
        IrAEscena1();
    }
    
    [ContextMenu("📖 Test - Completar Texto")]
    public void TestCompletarTexto()
    {
        CompletarTextoInmediatamente();
    }
}
