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
    
    [Header("💰 SISTEMA DE ZANAHORIAS")]
    [SerializeField] public int zanahoriasTotales = 0; // 🆕 VARIABLE FALTANTE


    private SistemaMonedas sistemaMonedas;
    private int ultimasMonedas = 0;
    
    void Start()
    {
        // Buscar sistema de monedas
        BuscarSistemaMonedas();
        
        // Crear UI si es necesario

        
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
            textoZanahorias.text = $" {cantidad}";
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
    
    // 🔧 REMOVER MÉTODOS - AHORA ESTÁN EN SistemaMonedas
    // Los métodos GetZanahorias(), GastarZanahorias(), SetZanahorias() 
    // se movieron a SistemaMonedas donde pertenecen

    // 🆕 MÉTODO PARA QUE SistemaMonedas PUEDA ACCEDER AL TEXTO
    public TextMeshProUGUI GetTextoMonedas()
    {
        return textoZanahorias; // Retornar el texto de zanahorias como texto de monedas
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
    
    // 🆕 MÉTODO PÚBLICO PARA FORZAR ACTUALIZACIÓN DE TEXTO
    public void ActualizarTextoZanahorias()
    {
        if (textoZanahorias != null)
        {
            textoZanahorias.text = $"🥕 {zanahoriasTotales}";
            
            if (mostrarDebug)
            {
                Debug.LogError($"🥕 TEXTO ACTUALIZADO: {zanahoriasTotales}");
            }
        }
        
        // Forzar actualización del canvas
        if (textoZanahorias != null && textoZanahorias.canvas != null)
        {
            Canvas.ForceUpdateCanvases();
        }
    }

    // 🔧 MÉTODO MEJORADO PARA GUARDAR
    public void GuardarZanahorias()
    {
        PlayerPrefs.SetInt("Zanahorias", zanahoriasTotales);
        PlayerPrefs.SetInt("Monedas", zanahoriasTotales); // También como Monedas para compatibilidad
        PlayerPrefs.Save();
        
        if (mostrarDebug)
        {
            Debug.LogError($"💾 ZANAHORIAS GUARDADAS: {zanahoriasTotales}");
        }
    }

    // 🆕 MÉTODO PARA RESETEAR COMPLETAMENTE
    public void ResetearZanahorias()
    {
        zanahoriasTotales = 0;
        GuardarZanahorias();
        ActualizarTextoZanahorias();
        
        Debug.LogError("🔄 ZANAHORIAS RESETEADAS A 0");
    }

    // 🆕 MÉTODO PARA AGREGAR ZANAHORIAS
    public void AgregarZanahorias(int cantidad)
    {
        if (cantidad <= 0) return;
        
        zanahoriasTotales += cantidad;
        GuardarZanahorias();
        ActualizarTextoZanahorias();
        
        if (mostrarDebug)
        {
            Debug.LogError($"🥕 ZANAHORIAS AGREGADAS: +{cantidad} | Total: {zanahoriasTotales}");
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
        
        ConfigurarUI();
    }
}