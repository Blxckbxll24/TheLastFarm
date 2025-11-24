using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManagerZanahorias : MonoBehaviour
{
    [Header("🥕 CONFIGURACIÓN UI")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private bool persistirEntreTodas = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎨 ESTILO")]
    [SerializeField] private Vector2 posicionUI = new Vector2(10f, -10f);
    [SerializeField] private float tamañoTexto = 24f;
    [SerializeField] private Color colorTexto = Color.yellow;
    [SerializeField] private Color colorFondo = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private bool animarCambios = true;
    
    [Header("📱 REFERENCIAS")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject panelZanahorias;
    [SerializeField] private TextMeshProUGUI textoZanahorias;
    [SerializeField] private Image fondoPanel;
    [SerializeField] private Animator animatorTexto;
    
    private SistemaMonedas sistemaMonedas;
    private int ultimasMonedas = 0;
    
    void Start()
    {
        // Buscar sistema de monedas
        BuscarSistemaMonedas();
        
        // Crear UI si es necesario
        if (crearUIAutomaticamente)
        {
            CrearUIZanahorias();
        }
        
        // Configurar UI
        ConfigurarUI();
        
        if (mostrarDebug)
        {
            Debug.LogError("📱 UI MANAGER ZANAHORIAS INICIADO");
        }
    }
    
    void Update()
    {
        // Actualizar UI con las monedas actuales
        ActualizarUIMonedas();
    }
    
    private void BuscarSistemaMonedas()
    {
        sistemaMonedas = SistemaMonedas.GetInstancia();
        if (sistemaMonedas == null)
        {
            sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        }
    }
    
    private void CrearUIZanahorias()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI DE ZANAHORIAS...");
        }
        
        // Crear Canvas
        GameObject canvasObj = new GameObject("Canvas_ZanahoriasUI");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Alto para estar visible
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Crear panel contenedor
        panelZanahorias = new GameObject("Panel_Zanahorias");
        panelZanahorias.transform.SetParent(canvas.transform, false);
        
        // Fondo del panel
        fondoPanel = panelZanahorias.AddComponent<Image>();
        fondoPanel.color = colorFondo;
        
        RectTransform rectPanel = panelZanahorias.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(220f, 60f);
        rectPanel.anchorMin = new Vector2(0f, 1f); // Esquina superior izquierda
        rectPanel.anchorMax = new Vector2(0f, 1f);
        rectPanel.pivot = new Vector2(0f, 1f);
        rectPanel.anchoredPosition = posicionUI;
        
        // Crear texto
        GameObject textoObj = new GameObject("Texto_Zanahorias");
        textoObj.transform.SetParent(panelZanahorias.transform, false);
        
        textoZanahorias = textoObj.AddComponent<TextMeshProUGUI>();
        textoZanahorias.text = "🥕 0";
        textoZanahorias.fontSize = tamañoTexto;
        textoZanahorias.color = colorTexto;
        textoZanahorias.fontStyle = FontStyles.Bold;
        textoZanahorias.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = new Vector2(10f, 5f);
        rectTexto.offsetMax = new Vector2(-10f, -5f);
        
        // Agregar animator para efectos
        if (animarCambios)
        {
            animatorTexto = textoObj.AddComponent<Animator>();
            CrearAnimatorController();
        }
        
        // Configurar como DontDestroyOnLoad si es persistente
        if (persistirEntreTodas && canvas != null)
        {
            DontDestroyOnLoad(canvas.gameObject);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DE ZANAHORIAS CREADA");
        }
    }
    
    private void CrearAnimatorController()
    {
        // Crear animator controller básico para efectos
        if (animatorTexto != null)
        {
            // Aquí podrías crear un AnimatorController programáticamente si lo necesitas
            // Por simplicidad, solo configuramos el componente
        }
    }
    
    private void ConfigurarUI()
    {
        if (textoZanahorias == null) return;
        
        // Configuración inicial del texto
        textoZanahorias.fontSize = tamañoTexto;
        textoZanahorias.color = colorTexto;
    }
    
    private void ActualizarUIMonedas()
    {
        if (sistemaMonedas == null)
        {
            BuscarSistemaMonedas();
            return;
        }
        
        int monedasActuales = sistemaMonedas.GetMonedasActuales();
        
        // Solo actualizar si cambió
        if (monedasActuales != ultimasMonedas)
        {
            ultimasMonedas = monedasActuales;
            ActualizarTextoZanahorias(monedasActuales);
            
            // Animar cambio si está activado
            if (animarCambios && animatorTexto != null)
            {
                AnimarCambioZanahorias();
            }
        }
    }
    
    private void ActualizarTextoZanahorias(int cantidad)
    {
        if (textoZanahorias != null)
        {
            textoZanahorias.text = $"🥕 {cantidad}";
        }
    }
    
    private void AnimarCambioZanahorias()
    {
        if (animatorTexto != null)
        {
            // Trigger de animación simple
            try
            {
                animatorTexto.SetTrigger("CambioZanahorias");
            }
            catch
            {
                // Si no tiene el parámetro, crear efectos simples
                StartCoroutine(EfectoEscalaSimple());
            }
        }
    }
    
    private System.Collections.IEnumerator EfectoEscalaSimple()
    {
        if (textoZanahorias == null) yield break;
        
        Vector3 escalaOriginal = textoZanahorias.transform.localScale;
        Vector3 escalaGrande = escalaOriginal * 1.2f;
        
        // Crecer
        float tiempo = 0f;
        while (tiempo < 0.2f)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / 0.2f;
            textoZanahorias.transform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, progreso);
            yield return null;
        }
        
        // Volver al tamaño normal
        tiempo = 0f;
        while (tiempo < 0.2f)
        {
            tiempo += Time.deltaTime;
            float progreso = tiempo / 0.2f;
            textoZanahorias.transform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, progreso);
            yield return null;
        }
        
        textoZanahorias.transform.localScale = escalaOriginal;
    }
    
    // 🆕 MÉTODO PÚBLICO PARA OBTENER EL TEXTO (REQUERIDO POR SistemaMonedas)
    public TextMeshProUGUI GetTextoMonedas()
    {
        return textoZanahorias;
    }
    
    public void MostrarUI()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
    }
    
    public void OcultarUI()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }
    
    [ContextMenu("🔧 Test - Recrear UI")]
    public void TestRecrearUI()
    {
        if (canvas != null)
        {
            DestroyImmediate(canvas.gameObject);
        }
        
        canvas = null;
        panelZanahorias = null;
        textoZanahorias = null;
        
        CrearUIZanahorias();
        ConfigurarUI();
    }
}