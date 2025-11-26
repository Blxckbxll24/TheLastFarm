using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UILifeBar : MonoBehaviour
{
    [Header("🩸 Referencias UI")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private Image imagenFill; // La parte que se llena
    [SerializeField] private Image imagenBackground; // El fondo de la barra
    [SerializeField] private TextMeshProUGUI textoVida; // Texto "100/100"
    [SerializeField] private CanvasGroup canvasGroup; // Para efectos de fade
    
    [Header("🎨 Configuración Visual")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private Color colorVidaCompleta = Color.green;
    [SerializeField] private Color colorVidaMedia = Color.yellow;
    [SerializeField] private Color colorVidaBaja = Color.red;
    [SerializeField] private Color colorFondo = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Header("⚡ Animaciones")]
    [SerializeField] private float velocidadAnimacion = 2f;
    [SerializeField] private bool animacionSuave = true;
    [SerializeField] private AnimationCurve curvaAnimacion = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("💫 Efectos")]
    [SerializeField] private bool efectoPulso = true;
    [SerializeField] private bool efectoParpadeo = true;
    [SerializeField] private bool efectoTemblor = true;
    [SerializeField] private float intensidadPulso = 1.1f;
    [SerializeField] private float velocidadPulso = 3f;
    
    [Header("🔍 Debug")]
    [SerializeField] private bool mostrarDebug = true;
    
    // Variables privadas
    private MovimientoJugador jugador;
    private float vidaActual;
    private float vidaMaxima;
    private float vidaObjetivo;
    private bool animando = false;
    private Vector3 escalaOriginal;
    private Vector3 posicionOriginal;
    private Coroutine animacionActiva;
    
    void Start()
    {
        // Crear UI automáticamente si no está asignada
        if (crearUIAutomaticamente && sliderVida == null)
        {
            CrearBarraVidaCompleta();
        }
        
        // Buscar al jugador
        BuscarJugador();
        
        // Guardar valores originales para efectos
        if (sliderVida != null)
        {
            escalaOriginal = sliderVida.transform.localScale;
            posicionOriginal = sliderVida.transform.localPosition;
        }
        
        // Inicializar valores
        InicializarBarra();
        
        if (mostrarDebug)
        {
            Debug.LogError("💊 LIFEBAR INICIALIZADA");
        }
    }
    
    void Update()
    {
        // Verificar si perdimos la referencia al jugador
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }
        
        // Actualizar barra de vida
        ActualizarBarraVida();
        
        // Test manual con teclas
        if (mostrarDebug)
        {
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                SimularDaño(10);
            }
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
            {
                SimularCuracion(20);
            }
        }
    }
    
    private void CrearBarraVidaCompleta()
    {
        if (mostrarDebug)
            Debug.LogError("🔧 CREANDO BARRA DE VIDA AUTOMÁTICAMENTE...");
        
        // Buscar o crear Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas_LifeBar");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Crear contenedor principal
        GameObject contenedor = new GameObject("LifeBar_Container");
        contenedor.transform.SetParent(canvas.transform, false);
        
        RectTransform rectContenedor = contenedor.AddComponent<RectTransform>();
        rectContenedor.anchorMin = new Vector2(0f, 1f); // Esquina superior izquierda
        rectContenedor.anchorMax = new Vector2(0f, 1f);
        rectContenedor.pivot = new Vector2(0f, 1f);
        rectContenedor.anchoredPosition = new Vector2(800f, -20f); // Margin desde la esquina
        rectContenedor.sizeDelta = new Vector2(300f, 60f);
        
        // Agregar CanvasGroup para efectos
        canvasGroup = contenedor.AddComponent<CanvasGroup>();
        
        // Crear fondo decorativo
        CrearFondoLifeBar(contenedor);
        
        // Crear slider principal
        CrearSliderVida(contenedor);
        
        // Crear texto de vida
        CrearTextoVida(contenedor);
        
        if (mostrarDebug)
            Debug.LogError("✅ BARRA DE VIDA CREADA AUTOMÁTICAMENTE");
    }
    
    private void CrearFondoLifeBar(GameObject padre)
    {
        GameObject fondo = new GameObject("Background");
        fondo.transform.SetParent(padre.transform, false);
        
        imagenBackground = fondo.AddComponent<Image>();
        imagenBackground.color = colorFondo;
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // Efecto de borde redondeado
        imagenBackground.type = Image.Type.Sliced;
    }
    
    private void CrearSliderVida(GameObject padre)
    {
        GameObject sliderObj = new GameObject("Slider_Vida");
        sliderObj.transform.SetParent(padre.transform, false);
        
        sliderVida = sliderObj.AddComponent<Slider>();
        sliderVida.minValue = 0f;
        sliderVida.maxValue = 1f;
        sliderVida.value = 1f;
        sliderVida.interactable = false; // Solo visual
        
        RectTransform rectSlider = sliderObj.GetComponent<RectTransform>();
        rectSlider.anchorMin = new Vector2(0.1f, 0.3f);
        rectSlider.anchorMax = new Vector2(0.9f, 0.7f);
        rectSlider.offsetMin = Vector2.zero;
        rectSlider.offsetMax = Vector2.zero;
        
        // Crear área de fill
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform rectFillArea = fillArea.AddComponent<RectTransform>();
        rectFillArea.anchorMin = Vector2.zero;
        rectFillArea.anchorMax = Vector2.one;
        rectFillArea.offsetMin = Vector2.zero;
        rectFillArea.offsetMax = Vector2.zero;
        
        // Crear fill (la parte que se llena)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        imagenFill = fill.AddComponent<Image>();
        imagenFill.color = colorVidaCompleta;
        imagenFill.type = Image.Type.Filled;
        imagenFill.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform rectFill = fill.GetComponent<RectTransform>();
        rectFill.anchorMin = Vector2.zero;
        rectFill.anchorMax = Vector2.one;
        rectFill.offsetMin = Vector2.zero;
        rectFill.offsetMax = Vector2.zero;
        
        sliderVida.fillRect = rectFill;
    }
    
    private void CrearTextoVida(GameObject padre)
    {
        GameObject textoObj = new GameObject("Texto_Vida");
        textoObj.transform.SetParent(padre.transform, false);
        
        textoVida = textoObj.AddComponent<TextMeshProUGUI>();
        textoVida.text = "100/100";
        textoVida.fontSize = 16;
        textoVida.color = Color.white;
        textoVida.fontStyle = FontStyles.Bold;
        textoVida.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
    }
    
    private void BuscarJugador()
    {
        // Buscar por tag
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        
        // Si no se encuentra por tag, buscar por script
        if (jugadorObj == null)
        {
            MovimientoJugador movScript = FindObjectOfType<MovimientoJugador>();
            if (movScript != null)
            {
                jugadorObj = movScript.gameObject;
            }
        }
        
        if (jugadorObj != null)
        {
            jugador = jugadorObj.GetComponent<MovimientoJugador>();
            
            if (mostrarDebug && jugador != null)
            {
                Debug.LogError("💊 JUGADOR ENCONTRADO: " + jugadorObj.name);
            }
        }
        else if (mostrarDebug)
        {
            Debug.LogError("❌ NO SE ENCONTRÓ JUGADOR PARA LA LIFEBAR");
        }
    }
    
    private void InicializarBarra()
    {
        if (jugador == null || sliderVida == null) return;
        
        vidaMaxima = jugador.GetSaludMaxima();
        vidaActual = jugador.GetSaludActual();
        vidaObjetivo = vidaActual;
        
        // Configurar slider
        sliderVida.value = vidaActual / vidaMaxima;
        
        // Actualizar color
        ActualizarColorBarra();
        
        // Actualizar texto
        if (textoVida != null)
        {
            textoVida.text = $"{Mathf.RoundToInt(vidaActual)}/{Mathf.RoundToInt(vidaMaxima)}";
        }
    }
    
    private void ActualizarBarraVida()
    {
        if (jugador == null || sliderVida == null) return;
        
        float nuevaVida = jugador.GetSaludActual();
        float nuevaVidaMaxima = jugador.GetSaludMaxima();
        
        // Verificar si cambió la vida máxima
        if (nuevaVidaMaxima != vidaMaxima)
        {
            vidaMaxima = nuevaVidaMaxima;
            ActualizarTexto();
        }
        
        // Verificar si cambió la vida actual
        if (Mathf.Abs(nuevaVida - vidaObjetivo) > 0.1f)
        {
            float diferencia = nuevaVida - vidaActual;
            vidaObjetivo = nuevaVida;
            
            if (diferencia < 0)
            {
                // Recibió daño
                AnimarDaño(Mathf.Abs(diferencia));
            }
            else
            {
                // Se curó
                AnimarCuracion(diferencia);
            }
        }
        
        // Animar la barra hacia el valor objetivo
        if (animacionSuave)
        {
            AnimarBarraSuave();
        }
        else
        {
            vidaActual = vidaObjetivo;
            sliderVida.value = vidaActual / vidaMaxima;
        }
        
        // Actualizar color y texto
        ActualizarColorBarra();
        ActualizarTexto();
    }
    
    private void AnimarBarraSuave()
    {
        if (Mathf.Abs(vidaActual - vidaObjetivo) > 0.1f)
        {
            vidaActual = Mathf.Lerp(vidaActual, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
            float porcentaje = vidaActual / vidaMaxima;
            
            if (sliderVida != null)
            {
                // Aplicar curva de animación
                float valorCurva = curvaAnimacion.Evaluate(porcentaje);
                sliderVida.value = porcentaje;
                
                // Efecto fill animado
                if (imagenFill != null)
                {
                    imagenFill.fillAmount = porcentaje;
                }
            }
        }
    }
    
    private void ActualizarColorBarra()
    {
        if (imagenFill == null) return;
        
        float porcentaje = vidaActual / vidaMaxima;
        Color colorObjetivo;
        
        if (porcentaje > 0.6f)
        {
            // Vida alta: Verde
            colorObjetivo = colorVidaCompleta;
        }
        else if (porcentaje > 0.3f)
        {
            // Vida media: Amarillo
            colorObjetivo = Color.Lerp(colorVidaBaja, colorVidaMedia, (porcentaje - 0.3f) / 0.3f);
        }
        else
        {
            // Vida baja: Rojo
            colorObjetivo = colorVidaBaja;
        }
        
        imagenFill.color = colorObjetivo;
        
        // Efectos especiales en vida baja
        if (porcentaje <= 0.3f && efectoParpadeo)
        {
            IniciarParpadeo();
        }
        else
        {
            DetenerParpadeo();
        }
    }
    
    private void ActualizarTexto()
    {
        if (textoVida != null)
        {
            textoVida.text = $"{Mathf.RoundToInt(vidaActual)}/{Mathf.RoundToInt(vidaMaxima)}";
        }
    }
    
    // === ANIMACIONES DE EFECTOS ===
    
    private void AnimarDaño(float cantidadDaño)
    {
        if (mostrarDebug)
            Debug.LogError($"💔 ANIMANDO DAÑO: {cantidadDaño}");
        
        // Efecto de temblor
        if (efectoTemblor)
        {
            StartCoroutine(EfectoTemblor());
        }
        
        // Efecto de pulso de daño
        if (efectoPulso)
        {
            StartCoroutine(EfectoPulsoDaño());
        }
        
        // Efecto de fade
        StartCoroutine(EfectoFlashDaño());
    }
    
    private void AnimarCuracion(float cantidadCuracion)
    {
        if (mostrarDebug)
            Debug.LogError($"💚 ANIMANDO CURACIÓN: {cantidadCuracion}");
        
        // Efecto de pulso de curación
        if (efectoPulso)
        {
            StartCoroutine(EfectoPulsoCuracion());
        }
        
        // Efecto de brillo
        StartCoroutine(EfectoFlashCuracion());
    }
    
    private System.Collections.IEnumerator EfectoTemblor()
    {
        if (sliderVida == null) yield break;
        
        float duracion = 0.3f;
        float intensidad = 3f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            Vector3 offset = new Vector3(
                Random.Range(-intensidad, intensidad),
                Random.Range(-intensidad, intensidad),
                0
            );
            
            sliderVida.transform.localPosition = posicionOriginal + offset;
            
            tiempo += Time.unscaledDeltaTime; // Usar unscaled para que funcione aunque el tiempo esté pausado
            yield return null;
        }
        
        sliderVida.transform.localPosition = posicionOriginal;
    }
    
    private System.Collections.IEnumerator EfectoPulsoDaño()
    {
        if (sliderVida == null) yield break;
        
        float duracion = 0.5f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            float escala = 1f + Mathf.Sin(tiempo * velocidadPulso * 2f) * (intensidadPulso - 1f) * 0.5f;
            sliderVida.transform.localScale = escalaOriginal * escala;
            
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        
        sliderVida.transform.localScale = escalaOriginal;
    }
    
    private System.Collections.IEnumerator EfectoPulsoCuracion()
    {
        if (sliderVida == null) yield break;
        
        float duracion = 0.4f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            float escala = 1f + Mathf.Sin(tiempo * velocidadPulso) * (intensidadPulso - 1f) * 0.3f;
            sliderVida.transform.localScale = escalaOriginal * escala;
            
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        
        sliderVida.transform.localScale = escalaOriginal;
    }
    
    private System.Collections.IEnumerator EfectoFlashDaño()
    {
        if (canvasGroup == null) yield break;
        
        // Flash rojo
        Color colorOriginal = imagenFill.color;
        imagenFill.color = Color.red;
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        imagenFill.color = colorOriginal;
    }
    
    private System.Collections.IEnumerator EfectoFlashCuracion()
    {
        if (canvasGroup == null) yield break;
        
        // Flash verde brillante
        Color colorOriginal = imagenFill.color;
        imagenFill.color = Color.white;
        
        yield return new WaitForSecondsRealtime(0.15f);
        
        imagenFill.color = colorOriginal;
    }
    
    private void IniciarParpadeo()
    {
        if (animacionActiva == null && imagenFill != null)
        {
            animacionActiva = StartCoroutine(EfectoParpadeo());
        }
    }
    
    private void DetenerParpadeo()
    {
        if (animacionActiva != null)
        {
            StopCoroutine(animacionActiva);
            animacionActiva = null;
            
            if (imagenFill != null)
            {
                Color color = imagenFill.color;
                color.a = 1f;
                imagenFill.color = color;
            }
        }
    }
    
    private System.Collections.IEnumerator EfectoParpadeo()
    {
        while (true)
        {
            if (imagenFill == null) yield break;
            
            // Fade out
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                Color color = imagenFill.color;
                color.a = Mathf.Lerp(1f, 0.3f, t / 0.5f);
                imagenFill.color = color;
                yield return null;
            }
            
            // Fade in
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                Color color = imagenFill.color;
                color.a = Mathf.Lerp(0.3f, 1f, t / 0.5f);
                imagenFill.color = color;
                yield return null;
            }
        }
    }
    
    // === MÉTODOS DE TESTING ===
    
    [ContextMenu("🧪 Test - Simular Daño")]
    public void TestSimularDaño()
    {
        SimularDaño(25);
    }
    
    [ContextMenu("🧪 Test - Simular Curación")]
    public void TestSimularCuracion()
    {
        SimularCuracion(30);
    }
    
    [ContextMenu("🧪 Test - Vida Crítica")]
    public void TestVidaCritica()
    {
        if (jugador != null)
        {
            jugador.SetSalud(15); // Vida muy baja para probar efectos
        }
    }
    
    private void SimularDaño(int cantidad)
    {
        if (jugador != null)
        {
            jugador.RecibirDaño(cantidad);
        }
    }
    
    private void SimularCuracion(int cantidad)
    {
        if (jugador != null)
        {
            jugador.Curar(cantidad);
        }
    }
    
    // === MÉTODOS PÚBLICOS ===
    
    public void ConfigurarColores(Color vidaCompleta, Color vidaMedia, Color vidaBaja)
    {
        colorVidaCompleta = vidaCompleta;
        colorVidaMedia = vidaMedia;
        colorVidaBaja = vidaBaja;
        
        ActualizarColorBarra();
    }
    
    public void ConfigurarAnimacion(float velocidad, bool suave = true)
    {
        velocidadAnimacion = velocidad;
        animacionSuave = suave;
    }
    
    public void ActivarEfectos(bool pulso = true, bool parpadeo = true, bool temblor = true)
    {
        efectoPulso = pulso;
        efectoParpadeo = parpadeo;
        efectoTemblor = temblor;
    }
    
    // Getter para obtener información
    public float GetPorcentajeVida()
    {
        return vidaMaxima > 0 ? vidaActual / vidaMaxima : 0f;
    }
    
    public bool EstaEnVidaCritica()
    {
        return GetPorcentajeVida() <= 0.3f;
    }
    
    void OnDestroy()
    {
        DetenerParpadeo();
        StopAllCoroutines();
    }
}
