using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CanvasMuerte : MonoBehaviour
{
    [Header("🎨 Referencias UI")]
    [SerializeField] private Canvas canvasMuerte;
    [SerializeField] private GameObject panelMuerte;
    [SerializeField] private Button botonRevivir;
    [SerializeField] private Button botonMenuPrincipal;
    [SerializeField] private TextMeshProUGUI textoMuerte;
    [SerializeField] private TextMeshProUGUI textoRevivir;
    [SerializeField] private TextMeshProUGUI textoZanahorias;
    
    [Header("🥕 Sistema de Revivir")]
    [SerializeField] private int costoRevivir = 10; // Zanahorias necesarias para revivir
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎨 Configuración Visual")]
    [SerializeField] private Color colorSuficientes = Color.green;
    [SerializeField] private Color colorInsuficientes = Color.red;
    
    private static CanvasMuerte instanciaActual;
    private bool panelMostrado = false;
    private int zanahoriasPoseidas = 0;

    void Awake()
    {
        instanciaActual = this;
    }

    void Start()
    {
        if (mostrarDebug)
        {
            Debug.LogError("💀 CANVAS MUERTE INICIALIZADO");
        }

        CrearUICompleta();
        OcultarPanelMuerte();
    }

    private void CrearUICompleta()
    {
        if (canvasMuerte == null)
        {
            GameObject canvasObj = new GameObject("Canvas_Muerte");
            canvasMuerte = canvasObj.AddComponent<Canvas>();
            canvasMuerte.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasMuerte.sortingOrder = 2000; // MUY ALTO
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (panelMuerte == null)
        {
            CrearPanelMuerte();
        }

        ConfigurarBotones();
    }

    private void CrearPanelMuerte()
    {
        // Fondo completo oscuro
        GameObject fondo = new GameObject("Fondo_Muerte");
        fondo.transform.SetParent(canvasMuerte.transform, false);
        
        Image imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0f, 0f, 0f, 0.9f); // Negro casi opaco
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;

        // Panel central
        panelMuerte = new GameObject("Panel_Muerte");
        panelMuerte.transform.SetParent(canvasMuerte.transform, false);
        
        Image imgPanel = panelMuerte.AddComponent<Image>();
        imgPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Gris oscuro
        
        RectTransform rectPanel = panelMuerte.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(600f, 500f);
        rectPanel.anchorMin = new Vector2(0.5f, 0.5f);
        rectPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rectPanel.pivot = new Vector2(0.5f, 0.5f);

        // Título "HAS MUERTO"
        GameObject titulo = new GameObject("Titulo_Muerte");
        titulo.transform.SetParent(panelMuerte.transform, false);
        
        textoMuerte = titulo.AddComponent<TextMeshProUGUI>();
        textoMuerte.text = "💀 HAS MUERTO 💀";
        textoMuerte.fontSize = 48;
        textoMuerte.color = Color.red;
        textoMuerte.fontStyle = FontStyles.Bold;
        textoMuerte.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(550f, 80f);
        rectTitulo.anchoredPosition = new Vector2(0f, 150f);

        // Texto de zanahorias
        GameObject textoZanObj = new GameObject("Texto_Zanahorias");
        textoZanObj.transform.SetParent(panelMuerte.transform, false);
        
        textoZanahorias = textoZanObj.AddComponent<TextMeshProUGUI>();
        textoZanahorias.text = "🥕 Zanahorias: 0/10";
        textoZanahorias.fontSize = 24;
        textoZanahorias.color = colorInsuficientes;
        textoZanahorias.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectZan = textoZanObj.GetComponent<RectTransform>();
        rectZan.sizeDelta = new Vector2(400f, 40f);
        rectZan.anchoredPosition = new Vector2(0f, 50f);

        // Texto de revivir
        GameObject textoRevObj = new GameObject("Texto_Revivir");
        textoRevObj.transform.SetParent(panelMuerte.transform, false);
        
        textoRevivir = textoRevObj.AddComponent<TextMeshProUGUI>();
        textoRevivir.text = "Necesitas 10 🥕 para revivir";
        textoRevivir.fontSize = 20;
        textoRevivir.color = Color.white;
        textoRevivir.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectRev = textoRevObj.GetComponent<RectTransform>();
        rectRev.sizeDelta = new Vector2(450f, 30f);
        rectRev.anchoredPosition = new Vector2(0f, 10f);

        // Botón Revivir
        botonRevivir = CrearBoton(panelMuerte, "💚 REVIVIR", new Vector2(-120f, -80f), colorSuficientes);
        
        // Botón Menú Principal
        botonMenuPrincipal = CrearBoton(panelMuerte, "🏠 MENÚ PRINCIPAL", new Vector2(120f, -80f), Color.gray);

        // Texto de advertencia
        GameObject advertencia = new GameObject("Advertencia");
        advertencia.transform.SetParent(panelMuerte.transform, false);
        
        TextMeshProUGUI textoAdv = advertencia.AddComponent<TextMeshProUGUI>();
        textoAdv.text = "Si no tienes suficientes zanahorias,\nirás al menú con 0 zanahorias";
        textoAdv.fontSize = 16;
        textoAdv.color = Color.yellow;
        textoAdv.alignment = TextAlignmentOptions.Center;
        textoAdv.fontStyle = FontStyles.Italic;
        
        RectTransform rectAdv = advertencia.GetComponent<RectTransform>();
        rectAdv.sizeDelta = new Vector2(500f, 60f);
        rectAdv.anchoredPosition = new Vector2(0f, -150f);
    }

    private Button CrearBoton(GameObject padre, string texto, Vector2 posicion, Color color)
    {
        GameObject botonObj = new GameObject("Boton_" + texto.Replace(" ", "_"));
        botonObj.transform.SetParent(padre.transform, false);

        Button boton = botonObj.AddComponent<Button>();
        Image img = botonObj.AddComponent<Image>();
        img.color = color;

        RectTransform rect = botonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 60f);
        rect.anchoredPosition = posicion;

        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textComp = textoObj.AddComponent<TextMeshProUGUI>();
        textComp.text = texto;
        textComp.fontSize = 18;
        textComp.color = Color.white;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;

        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;

        return boton;
    }

    private void ConfigurarBotones()
    {
        if (botonRevivir != null)
        {
            botonRevivir.onClick.RemoveAllListeners();
            botonRevivir.onClick.AddListener(IntentarRevivir);
        }

        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.onClick.RemoveAllListeners();
            botonMenuPrincipal.onClick.AddListener(IrAlMenuPrincipal);
        }
    }

    public void MostrarPanelMuerte()
    {
        if (panelMostrado) return;

        panelMostrado = true;

        // PAUSAR JUEGO COMPLETAMENTE
        Time.timeScale = 0f;

        // FORZAR cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Actualizar zanahorias
        ActualizarContadorZanahorias();

        // Mostrar UI
        if (canvasMuerte != null) canvasMuerte.gameObject.SetActive(true);
        if (panelMuerte != null) panelMuerte.SetActive(true);

        if (mostrarDebug)
        {
            Debug.LogError("💀 PANEL DE MUERTE MOSTRADO");
        }
    }

    private void ActualizarContadorZanahorias()
    {
        // 🔧 USAR SistemaMonedas COMO FUENTE PRINCIPAL
        // Intentar obtener zanahorias del SistemaMonedas primero
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            zanahoriasPoseidas = sistemaMonedas.GetZanahorias();
            if (mostrarDebug)
            {
                Debug.LogError($"🥕 ZANAHORIAS OBTENIDAS VÍA SistemaMonedas: {zanahoriasPoseidas}");
            }
        }
        else
        {
            // 🔧 FALLBACK MEJORADO: Usar PlayerPrefs directamente
            zanahoriasPoseidas = PlayerPrefs.GetInt("Zanahorias", 0);
            if (mostrarDebug)
            {
                Debug.LogError($"🥕 ZANAHORIAS OBTENIDAS VÍA PlayerPrefs: {zanahoriasPoseidas}");
            }
        }

        // Actualizar texto
        if (textoZanahorias != null)
        {
            textoZanahorias.text = $"🥕 Zanahorias: {zanahoriasPoseidas}/{costoRevivir}";
            
            // Cambiar color según si tiene suficientes
            bool tieneSuficientes = zanahoriasPoseidas >= costoRevivir;
            textoZanahorias.color = tieneSuficientes ? colorSuficientes : colorInsuficientes;
        }

        // Actualizar estado del botón revivir
        if (botonRevivir != null)
        {
            bool puedeRevivir = zanahoriasPoseidas >= costoRevivir;
            botonRevivir.interactable = puedeRevivir;
            
            Image imgBoton = botonRevivir.GetComponent<Image>();
            if (imgBoton != null)
            {
                imgBoton.color = puedeRevivir ? colorSuficientes : Color.gray;
            }
        }

        // Actualizar texto de revivir
        if (textoRevivir != null)
        {
            bool puedeRevivir = zanahoriasPoseidas >= costoRevivir;
            textoRevivir.text = puedeRevivir ? 
                $"✅ Puedes revivir por {costoRevivir} 🥕" : 
                $"❌ Necesitas {costoRevivir - zanahoriasPoseidas} 🥕 más";
            textoRevivir.color = puedeRevivir ? colorSuficientes : colorInsuficientes;
        }

        if (mostrarDebug)
        {
            Debug.LogError($"🥕 ZANAHORIAS: {zanahoriasPoseidas}/{costoRevivir} | Puede revivir: {zanahoriasPoseidas >= costoRevivir}");
        }
    }

    private void IntentarRevivir()
    {
        // Verificar si tiene suficientes zanahorias
        if (zanahoriasPoseidas < costoRevivir)
        {
            Debug.LogError("❌ NO TIENES SUFICIENTES ZANAHORIAS PARA REVIVIR");
            
            // Mostrar mensaje temporal
            if (textoRevivir != null)
            {
                string textoOriginal = textoRevivir.text;
                textoRevivir.text = "❌ ¡NO TIENES SUFICIENTES ZANAHORIAS!";
                textoRevivir.color = Color.red;
                
                // Restaurar texto después de 2 segundos
                Invoke("RestaurarTextoRevivir", 2f);
            }
            return;
        }

        if (mostrarDebug)
        {
            Debug.LogError("💚 REVIVIENDO JUGADOR - Cobrando 10 zanahorias");
        }

        // Cobrar zanahorias
        CobrarZanahorias(costoRevivir);

        // Revivir jugador
        RevivirJugador();

        // Ocultar panel
        OcultarPanelMuerte();
    }

    private void CobrarZanahorias(int cantidad)
    {
        // 🔧 USAR SistemaMonedas COMO SISTEMA PRINCIPAL
        bool exito = false;
        
        // Intentar cobrar vía SistemaMonedas primero
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            exito = sistemaMonedas.GastarZanahorias(cantidad);
            if (exito && mostrarDebug)
            {
                Debug.LogError($"💰 ZANAHORIAS COBRADAS VÍA SistemaMonedas: -{cantidad} | Restantes: {sistemaMonedas.GetZanahorias()}");
            }
        }
        
        // Fallback: PlayerPrefs directamente
        if (!exito)
        {
            int zanahoriasSalvadas = PlayerPrefs.GetInt("Zanahorias", 0);
            if (zanahoriasSalvadas >= cantidad)
            {
                PlayerPrefs.SetInt("Zanahorias", Mathf.Max(0, zanahoriasSalvadas - cantidad));
                PlayerPrefs.Save();
                exito = true;
                
                if (mostrarDebug)
                {
                    Debug.LogError($"💰 ZANAHORIAS COBRADAS VÍA PlayerPrefs: -{cantidad} | Restantes: {PlayerPrefs.GetInt("Zanahorias", 0)}");
                }
            }
        }
        
        if (!exito)
        {
            Debug.LogError($"❌ ERROR CRÍTICO: No se pudieron cobrar {cantidad} zanahorias");
        }
    }

    private void RevivirJugador()
    {
        // Buscar al jugador y revivirlo
        MovimientoJugador jugadorScript = FindObjectOfType<MovimientoJugador>();
        if (jugadorScript != null)
        {
            jugadorScript.CurarCompletamente();
            Debug.LogError("✨ JUGADOR REVIVIDO CON VIDA COMPLETA");
        }
        else
        {
            Debug.LogError("❌ NO SE PUDO ENCONTRAR AL JUGADOR PARA REVIVIR");
        }
    }

    private void RestaurarTextoRevivir()
    {
        ActualizarContadorZanahorias(); // Esto restaurará el texto correcto
    }

    private void IrAlMenuPrincipal()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🏠 REGRESANDO AL MENÚ PRINCIPAL - PONIENDO ZANAHORIAS EN 0");
        }

        // 🔧 RESETEAR ZANAHORIAS COMPLETAMENTE EN TODOS LOS SISTEMAS
        ResetearZanahoriasCompleto();

        // Restaurar tiempo
        Time.timeScale = 1f;

        // Limpiar sistemas
        LimpiarSistemasAntesCambioEscena();

        try
        {
            SceneManager.LoadScene("MenuPrincipal");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR AL CARGAR MENÚ: " + e.Message);
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

    // 🆕 NUEVO MÉTODO: RESETEAR ZANAHORIAS COMPLETAMENTE
    private void ResetearZanahoriasCompleto()
    {
        Debug.LogError("🔄 RESETEANDO ZANAHORIAS EN TODOS LOS SISTEMAS...");
        
        // 1. Resetear en SistemaMonedas
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.SetMonedas(0);
            sistemaMonedas.GuardarConfiguracion(); // Forzar guardado
            Debug.LogError("✅ SistemaMonedas reseteado a 0");
        }
        
        // 2. Resetear en UIManagerZanahorias si existe
        UIManagerZanahorias uiZanahorias = FindObjectOfType<UIManagerZanahorias>();
        if (uiZanahorias != null)
        {
            // Acceder directamente a la variable pública y resetearla
            uiZanahorias.zanahoriasTotales = 0;
            uiZanahorias.GuardarZanahorias(); // Forzar guardado
            Debug.LogError("✅ UIManagerZanahorias reseteado a 0");
        }
        
        // 3. Limpiar PlayerPrefs múltiples veces para asegurar
        PlayerPrefs.SetInt("Zanahorias", 0);
        PlayerPrefs.SetInt("Monedas", 0);
        PlayerPrefs.SetFloat("DineroJugador", 0f);
        PlayerPrefs.Save();
        
        // 4. Verificar que efectivamente se guardó
        int verificacion = PlayerPrefs.GetInt("Zanahorias", -1);
        Debug.LogError($"🔍 VERIFICACIÓN PlayerPrefs: {verificacion}");
        
        // 5. Forzar actualización inmediata de todos los sistemas de UI
        ActualizarTodosLosSistemasUI();
        
        Debug.LogError("✅ RESETEO COMPLETO DE ZANAHORIAS TERMINADO");
    }
    
    // 🆕 MÉTODO PARA ACTUALIZAR TODOS LOS SISTEMAS DE UI
    private void ActualizarTodosLosSistemasUI()
    {
        // Actualizar SistemaMonedas UI
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.ActualizarDisplayCompleto();
        }
        
        // Actualizar UIManagerZanahorias UI
        UIManagerZanahorias uiZanahorias = FindObjectOfType<UIManagerZanahorias>();
        if (uiZanahorias != null)
        {
            uiZanahorias.ActualizarTextoZanahorias();
        }
        
        // Buscar y actualizar otros managers de UI que puedan mostrar monedas
        MonoBehaviour[] todosScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour script in todosScripts)
        {
            // Buscar métodos que puedan actualizar la UI de monedas
            var metodoActualizar = script.GetType().GetMethod("ActualizarUIMonedas");
            if (metodoActualizar != null)
            {
                metodoActualizar.Invoke(script, null);
            }
        }
    }

    private void LimpiarSistemasAntesCambioEscena()
    {
        Debug.LogError("🧹 LIMPIANDO SISTEMAS ANTES DEL CAMBIO...");
        
        StopAllCoroutines();
        CancelInvoke();
        
        // Limpiar zombies
        ControladorZombies[] controladores = FindObjectsByType<ControladorZombies>(FindObjectsSortMode.None);
        foreach (var controlador in controladores)
        {
            if (controlador != null)
            {
                controlador.StopAllCoroutines();
                controlador.CancelInvoke();
                controlador.DestruirTodosLosZombies();
            }
        }
        
        // Limpiar enemigos
        ControladorEnemigo[] enemigos = FindObjectsByType<ControladorEnemigo>(FindObjectsSortMode.None);
        foreach (var enemigo in enemigos)
        {
            if (enemigo != null)
            {
                enemigo.StopAllCoroutines();
                enemigo.CancelInvoke();
                Destroy(enemigo.gameObject);
            }
        }
        
        Debug.LogError("✅ LIMPIEZA COMPLETA");
    }

    public void OcultarPanelMuerte()
    {
        panelMostrado = false;

        if (canvasMuerte != null) canvasMuerte.gameObject.SetActive(false);
        if (panelMuerte != null) panelMuerte.SetActive(false);

        // Restaurar tiempo
        Time.timeScale = 1f;

        // FORZAR cursor visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mostrarDebug)
        {
            Debug.LogError("💀 PANEL DE MUERTE OCULTADO - TIEMPO RESTAURADO");
        }
    }

    // Métodos estáticos para acceso global
    public static void MostrarPanelMuerteEstatico()
    {
        if (instanciaActual != null)
        {
            instanciaActual.MostrarPanelMuerte();
        }
        else
        {
            Debug.LogError("❌ No existe instancia de CanvasMuerte");
        }
    }

    public static void LimpiarInstancia()
    {
        if (instanciaActual != null)
        {
            instanciaActual.StopAllCoroutines();
            instanciaActual.CancelInvoke();
            if (instanciaActual.gameObject != null)
            {
                Destroy(instanciaActual.gameObject);
            }
            instanciaActual = null;
        }
    }

    // Métodos de testing
    [ContextMenu("🧪 Test - Mostrar Panel Muerte")]
    public void TestMostrarPanel()
    {
        MostrarPanelMuerte();
    }

    [ContextMenu("🧪 Test - Simular 15 Zanahorias")]
    public void TestSimularZanahorias()
    {
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.SetZanahorias(15);
            ActualizarContadorZanahorias();
            Debug.LogError("🧪 Simulando 15 zanahorias para test");
        }
    }

    [ContextMenu("🧪 Test - Simular 5 Zanahorias")]
    public void TestSimularPocasZanahorias()
    {
        SistemaMonedas sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.SetZanahorias(5);
            ActualizarContadorZanahorias();
            Debug.LogError("🧪 Simulando 5 zanahorias para test");
        }
    }
}
