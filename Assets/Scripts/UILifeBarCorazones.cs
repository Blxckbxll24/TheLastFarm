using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UILifeBarCorazones : MonoBehaviour
{
    [Header("💖 Referencias UI")]
    [SerializeField] private Image imagenCorazones; // Tu sprite de 5 corazones
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("🎨 Configuración Visual")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private Vector2 posicionEnPantalla = new Vector2(50f, -50f); // Desde esquina superior izquierda
    [SerializeField] private Vector2 tamanoSprite = new Vector2(300f, 60f);
    
    [Header("⚡ Animaciones")]
    [SerializeField] private bool animacionSuave = true;
    [SerializeField] private float velocidadAnimacion = 2f;
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
    private float vidaMaxima = 100f; // Vida máxima fija
    private float vidaObjetivo;
    private Vector3 escalaOriginal;
    private Vector3 posicionOriginal;
    private Coroutine animacionActiva;
    private Material materialCorazones; // Para efectos de shader
    
    void Start()
    {
        if (crearUIAutomaticamente && imagenCorazones == null)
        {
            CrearBarraCorazonesCompleta();
        }
        
        BuscarJugador();
        
        if (imagenCorazones != null)
        {
            escalaOriginal = imagenCorazones.transform.localScale;
            posicionOriginal = imagenCorazones.transform.localPosition;
            
            // Crear material para efectos
            CrearMaterialEfectos();
        }
        
        InicializarBarra();
        
        if (mostrarDebug)
        {
            Debug.LogError("💊 LIFEBAR CORAZONES INICIALIZADA");
        }
    }
    
    void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }
        
        ActualizarBarraVida();
        
        // Test manual
        if (mostrarDebug)
        {
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                SimularDaño(20);
            }
            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
            {
                SimularCuracion(20);
            }
        }
    }
    
    private void CrearBarraCorazonesCompleta()
    {
        if (mostrarDebug)
            Debug.LogError("🔧 CREANDO BARRA DE CORAZONES AUTOMÁTICAMENTE...");
        
        // Buscar o crear Canvas
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_LifeBarCorazones");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        
        // Crear contenedor principal
        GameObject contenedor = new GameObject("LifeBar_Corazones");
        contenedor.transform.SetParent(canvas.transform, false);
        
        RectTransform rectContenedor = contenedor.AddComponent<RectTransform>();
        rectContenedor.anchorMin = new Vector2(0f, 1f); // Esquina superior izquierda
        rectContenedor.anchorMax = new Vector2(0f, 1f);
        rectContenedor.pivot = new Vector2(0f, 1f);
        rectContenedor.anchoredPosition = posicionEnPantalla;
        rectContenedor.sizeDelta = tamanoSprite;
        
        // Agregar CanvasGroup para efectos
        canvasGroup = contenedor.AddComponent<CanvasGroup>();
        
        // Crear Image para los corazones
        GameObject corazonesObj = new GameObject("Imagen_Corazones");
        corazonesObj.transform.SetParent(contenedor.transform, false);
        
        imagenCorazones = corazonesObj.AddComponent<Image>();
        
        RectTransform rectCorazones = corazonesObj.GetComponent<RectTransform>();
        rectCorazones.anchorMin = Vector2.zero;
        rectCorazones.anchorMax = Vector2.one;
        rectCorazones.offsetMin = Vector2.zero;
        rectCorazones.offsetMax = Vector2.zero;
        
        if (mostrarDebug)
            Debug.LogError("✅ CONTENEDOR DE CORAZONES CREADO - Ahora asigna tu sprite en el Inspector");
    }
    
    private void CrearMaterialEfectos()
    {
        if (imagenCorazones != null && imagenCorazones.sprite != null)
        {
            // Crear material para efectos (copia del material por defecto)
            materialCorazones = new Material(Shader.Find("UI/Default"));
            imagenCorazones.material = materialCorazones;
        }
    }
    
    private void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        
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
    }
    
    private void InicializarBarra()
    {
        if (jugador == null) return;
        
        vidaMaxima = jugador.GetSaludMaxima();
        vidaActual = jugador.GetSaludActual();
        vidaObjetivo = vidaActual;
        
        ActualizarFillAmount();
    }
    
    private void ActualizarBarraVida()
    {
        if (jugador == null || imagenCorazones == null) return;
        
        float nuevaVida = jugador.GetSaludActual();
        float nuevaVidaMaxima = jugador.GetSaludMaxima();
        
        if (nuevaVidaMaxima != vidaMaxima)
        {
            vidaMaxima = nuevaVidaMaxima;
        }
        
        if (Mathf.Abs(nuevaVida - vidaObjetivo) > 0.1f)
        {
            float diferencia = nuevaVida - vidaActual;
            vidaObjetivo = nuevaVida;
            
            if (diferencia < 0)
            {
                AnimarDaño(Mathf.Abs(diferencia));
            }
            else
            {
                AnimarCuracion(diferencia);
            }
        }
        
        if (animacionSuave)
        {
            AnimarBarraSuave();
        }
        else
        {
            vidaActual = vidaObjetivo;
            ActualizarFillAmount();
        }
        
        // Efectos especiales en vida baja
        float porcentaje = vidaActual / vidaMaxima;
        if (porcentaje <= 0.3f && efectoParpadeo)
        {
            IniciarParpadeo();
        }
        else
        {
            DetenerParpadeo();
        }
    }
    
    private void AnimarBarraSuave()
    {
        if (Mathf.Abs(vidaActual - vidaObjetivo) > 0.1f)
        {
            vidaActual = Mathf.Lerp(vidaActual, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
            ActualizarFillAmount();
        }
    }
    
    private void ActualizarFillAmount()
    {
        if (imagenCorazones == null) return;
        
        float porcentaje = vidaActual / vidaMaxima;
        
        // Usar fillAmount para mostrar los corazones parcialmente
        imagenCorazones.fillMethod = Image.FillMethod.Horizontal;
        imagenCorazones.type = Image.Type.Filled;
        imagenCorazones.fillAmount = porcentaje;
        
        // Cambiar color según el porcentaje de vida
        Color colorCorazones;
        if (porcentaje > 0.6f)
        {
            colorCorazones = Color.white; // Color normal
        }
        else if (porcentaje > 0.3f)
        {
            colorCorazones = Color.yellow; // Advertencia
        }
        else
        {
            colorCorazones = Color.red; // Peligro
        }
        
        imagenCorazones.color = colorCorazones;
    }
    
    // === ANIMACIONES DE EFECTOS ===
    
    private void AnimarDaño(float cantidadDaño)
    {
        if (mostrarDebug)
            Debug.LogError($"💔 ANIMANDO DAÑO: {cantidadDaño}");
        
        if (efectoTemblor)
        {
            StartCoroutine(EfectoTemblor());
        }
        
        if (efectoPulso)
        {
            StartCoroutine(EfectoPulsoDaño());
        }
        
        StartCoroutine(EfectoFlashDaño());
    }
    
    private void AnimarCuracion(float cantidadCuracion)
    {
        if (mostrarDebug)
            Debug.LogError($"💚 ANIMANDO CURACIÓN: {cantidadCuracion}");
        
        if (efectoPulso)
        {
            StartCoroutine(EfectoPulsoCuracion());
        }
        
        StartCoroutine(EfectoFlashCuracion());
    }
    
    private System.Collections.IEnumerator EfectoTemblor()
    {
        if (imagenCorazones == null) yield break;
        
        float duracion = 0.3f;
        float intensidad = 5f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            Vector3 offset = new Vector3(
                Random.Range(-intensidad, intensidad),
                Random.Range(-intensidad, intensidad),
                0
            );
            
            imagenCorazones.transform.localPosition = posicionOriginal + offset;
            
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        
        imagenCorazones.transform.localPosition = posicionOriginal;
    }
    
    private System.Collections.IEnumerator EfectoPulsoDaño()
    {
        if (imagenCorazones == null) yield break;
        
        float duracion = 0.5f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            float escala = 1f + Mathf.Sin(tiempo * velocidadPulso * 2f) * (intensidadPulso - 1f) * 0.5f;
            imagenCorazones.transform.localScale = escalaOriginal * escala;
            
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        
        imagenCorazones.transform.localScale = escalaOriginal;
    }
    
    private System.Collections.IEnumerator EfectoPulsoCuracion()
    {
        if (imagenCorazones == null) yield break;
        
        float duracion = 0.4f;
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            float escala = 1f + Mathf.Sin(tiempo * velocidadPulso) * (intensidadPulso - 1f) * 0.3f;
            imagenCorazones.transform.localScale = escalaOriginal * escala;
            
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
        
        imagenCorazones.transform.localScale = escalaOriginal;
    }
    
    private System.Collections.IEnumerator EfectoFlashDaño()
    {
        if (imagenCorazones == null) yield break;
        
        Color colorOriginal = imagenCorazones.color;
        imagenCorazones.color = Color.red;
        
        yield return new WaitForSecondsRealtime(0.1f);
        
        imagenCorazones.color = colorOriginal;
    }
    
    private System.Collections.IEnumerator EfectoFlashCuracion()
    {
        if (imagenCorazones == null) yield break;
        
        Color colorOriginal = imagenCorazones.color;
        imagenCorazones.color = Color.green;
        
        yield return new WaitForSecondsRealtime(0.15f);
        
        imagenCorazones.color = colorOriginal;
    }
    
    private void IniciarParpadeo()
    {
        if (animacionActiva == null && imagenCorazones != null)
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
            
            if (imagenCorazones != null)
            {
                Color color = imagenCorazones.color;
                color.a = 1f;
                imagenCorazones.color = color;
            }
        }
    }
    
    private System.Collections.IEnumerator EfectoParpadeo()
    {
        while (true)
        {
            if (imagenCorazones == null) yield break;
            
            // Fade out
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                Color color = imagenCorazones.color;
                color.a = Mathf.Lerp(1f, 0.3f, t / 0.5f);
                imagenCorazones.color = color;
                yield return null;
            }
            
            // Fade in
            for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
            {
                Color color = imagenCorazones.color;
                color.a = Mathf.Lerp(0.3f, 1f, t / 0.5f);
                imagenCorazones.color = color;
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
            jugador.SetSalud(15);
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
    
    public void ConfigurarPosicion(Vector2 nuevaPosicion)
    {
        posicionEnPantalla = nuevaPosicion;
        if (imagenCorazones != null && imagenCorazones.transform.parent != null)
        {
            imagenCorazones.transform.parent.GetComponent<RectTransform>().anchoredPosition = nuevaPosicion;
        }
    }
    
    public void ConfigurarTamano(Vector2 nuevoTamano)
    {
        tamanoSprite = nuevoTamano;
        if (imagenCorazones != null && imagenCorazones.transform.parent != null)
        {
            imagenCorazones.transform.parent.GetComponent<RectTransform>().sizeDelta = nuevoTamano;
        }
    }
    
    public void ActivarEfectos(bool pulso = true, bool parpadeo = true, bool temblor = true)
    {
        efectoPulso = pulso;
        efectoParpadeo = parpadeo;
        efectoTemblor = temblor;
    }
    
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
        
        // Limpiar material creado
        if (materialCorazones != null)
        {
            Destroy(materialCorazones);
        }
    }
}
