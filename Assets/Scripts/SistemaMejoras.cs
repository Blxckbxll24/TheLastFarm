using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SistemaMejoras : MonoBehaviour
{
    [Header("💪 CONFIGURACIÓN DE MEJORAS")]
    [SerializeField] private bool crearUIAutomaticamente = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("💰 COSTOS DE MEJORAS")]
    [SerializeField] private int costoMejoraVida = 50;
    [SerializeField] private int costoMejoraDaño = 75;
    [SerializeField] private int costoMejoraVelocidad = 100;
    [SerializeField] private int costoMejoraRecoleccion = 125;
    
    [Header("📈 INCREMENTOS DE MEJORAS")]
    [SerializeField] private int incrementoVida = 20;
    [SerializeField] private float incrementoDaño = 5f;
    [SerializeField] private float incrementoVelocidad = 0.5f;
    [SerializeField] private float multiplicadorRecoleccion = 1.2f;
    
    [Header("📱 REFERENCIAS UI")]
    [SerializeField] private Canvas canvasMejoras;
    [SerializeField] private GameObject panelMejoras;
    [SerializeField] private Button botonMejorarVida;
    [SerializeField] private Button botonMejorarDaño;
    [SerializeField] private Button botonMejorarVelocidad;
    [SerializeField] private Button botonMejorarRecoleccion;
    [SerializeField] private Button botonCerrar;
    
    // Textos informativos
    private TextMeshProUGUI textoZanahoriasActuales;
    private TextMeshProUGUI textoNivelVida;
    private TextMeshProUGUI textoNivelDaño;
    private TextMeshProUGUI textoNivelVelocidad;
    private TextMeshProUGUI textoNivelRecoleccion;
    
    // Niveles actuales
    private int nivelVida = 0;
    private int nivelDaño = 0;
    private int nivelVelocidad = 0;
    private int nivelRecoleccion = 0;
    
    // Referencias de sistemas
    private MovimientoJugador jugador;
    private SistemaMonedas sistemaMonedas;
    
    // Estado del menú
    private bool menuAbierto = false;
    
    void Start()
    {
        if (crearUIAutomaticamente && canvasMejoras == null)
        {
            CrearUICompleta();
        }
        
        BuscarReferencias();
        ConfigurarBotones();
        CargarNiveles();
        
        // Asegurar que está oculto al inicio
        if (canvasMejoras != null)
        {
            canvasMejoras.gameObject.SetActive(false);
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("💪 SISTEMA DE MEJORAS INICIADO");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) // Cambiado de Q a U para evitar conflictos
        {
            if (mostrarDebug)
            {
                Debug.LogError("🎯 TECLA U PRESIONADA - Toggleando menú mejoras");
            }
            
            if (menuAbierto)
            {
                CerrarMenu();
            }
            else
            {
                AbrirMenu();
            }
        }
        
        // Actualizar UI si está abierto
        if (menuAbierto)
        {
            ActualizarUI();
        }
    }
    
    private void BuscarReferencias()
    {
        // Buscar sistema de monedas
        sistemaMonedas = SistemaMonedas.GetInstancia();
        if (sistemaMonedas == null)
        {
            sistemaMonedas = FindObjectOfType<SistemaMonedas>();
        }
        
        // Buscar jugador
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindWithTag("Player");
            if (jugadorObj != null)
            {
                jugador = jugadorObj.GetComponent<MovimientoJugador>();
            }
            
            if (jugador == null)
            {
                jugador = FindObjectOfType<MovimientoJugador>();
            }
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🔍 REFERENCIAS ENCONTRADAS:");
            Debug.LogError($"  - Sistema monedas: {(sistemaMonedas != null ? "✅" : "❌")}");
            Debug.LogError($"  - Jugador: {(jugador != null ? "✅" : "❌")}");
        }
    }
    
    private void CrearUICompleta()
    {
        if (mostrarDebug)
        {
            Debug.LogError("🎨 CREANDO UI COMPLETA DE MEJORAS...");
        }
        
        // Crear Canvas
        GameObject canvasObj = new GameObject("Canvas_Mejoras");
        canvasMejoras = canvasObj.AddComponent<Canvas>();
        canvasMejoras.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasMejoras.sortingOrder = 500;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Fondo
        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(canvasMejoras.transform, false);
        
        Image imagenFondo = fondo.AddComponent<Image>();
        imagenFondo.color = new Color(0f, 0f, 0f, 0.8f);
        
        RectTransform rectFondo = fondo.GetComponent<RectTransform>();
        rectFondo.anchorMin = Vector2.zero;
        rectFondo.anchorMax = Vector2.one;
        rectFondo.offsetMin = Vector2.zero;
        rectFondo.offsetMax = Vector2.zero;
        
        // Panel principal
        panelMejoras = new GameObject("Panel_Mejoras");
        panelMejoras.transform.SetParent(canvasMejoras.transform, false);
        
        Image fondoPanel = panelMejoras.AddComponent<Image>();
        fondoPanel.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform rectPanel = panelMejoras.GetComponent<RectTransform>();
        rectPanel.sizeDelta = new Vector2(800f, 600f);
        rectPanel.anchorMin = new Vector2(0.5f, 0.5f);
        rectPanel.anchorMax = new Vector2(0.5f, 0.5f);
        rectPanel.pivot = new Vector2(0.5f, 0.5f);
        rectPanel.anchoredPosition = Vector2.zero;
        
        CrearTitulo();
        CrearInfoZanahorias();
        CrearBotonesMejoras();
        CrearBotonCerrar();
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ UI DE MEJORAS CREADA");
        }
    }
    
    private void CrearTitulo()
    {
        GameObject titulo = new GameObject("Titulo");
        titulo.transform.SetParent(panelMejoras.transform, false);
        
        TextMeshProUGUI textoTitulo = titulo.AddComponent<TextMeshProUGUI>();
        textoTitulo.text = "💪 MEJORAS DEL JUGADOR";
        textoTitulo.fontSize = 36;
        textoTitulo.color = Color.yellow;
        textoTitulo.fontStyle = FontStyles.Bold;
        textoTitulo.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTitulo = titulo.GetComponent<RectTransform>();
        rectTitulo.sizeDelta = new Vector2(750f, 50f);
        rectTitulo.anchoredPosition = new Vector2(0f, 250f);
    }
    
    private void CrearInfoZanahorias()
    {
        GameObject infoZanahorias = new GameObject("Info_Zanahorias");
        infoZanahorias.transform.SetParent(panelMejoras.transform, false);
        
        textoZanahoriasActuales = infoZanahorias.AddComponent<TextMeshProUGUI>();
        textoZanahoriasActuales.text = "💰 Monedas: 0";
        textoZanahoriasActuales.fontSize = 24;
        textoZanahoriasActuales.color = Color.cyan;
        textoZanahoriasActuales.fontStyle = FontStyles.Bold;
        textoZanahoriasActuales.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectInfo = infoZanahorias.GetComponent<RectTransform>();
        rectInfo.sizeDelta = new Vector2(400f, 40f);
        rectInfo.anchoredPosition = new Vector2(0f, 180f);
    }
    
    private void CrearBotonesMejoras()
    {
        // Mejorar Vida
        botonMejorarVida = CrearBotonMejora("Vida", "💖 MEJORAR VIDA", new Vector2(0f, 80f), Color.red, out textoNivelVida);
        
        // Mejorar Daño
        botonMejorarDaño = CrearBotonMejora("Daño", "⚔️ MEJORAR DAÑO", new Vector2(0f, 20f), Color.orange, out textoNivelDaño);
        
        // Mejorar Velocidad
        botonMejorarVelocidad = CrearBotonMejora("Velocidad", "🏃 MEJORAR VELOCIDAD", new Vector2(0f, -40f), Color.blue, out textoNivelVelocidad);
        
        // Mejorar Recolección
        botonMejorarRecoleccion = CrearBotonMejora("Recolección", "🥕 MEJORAR RECOLECCIÓN", new Vector2(0f, -100f), Color.green, out textoNivelRecoleccion);
    }
    
    private Button CrearBotonMejora(string nombre, string texto, Vector2 posicion, Color color, out TextMeshProUGUI textoNivel)
    {
        GameObject botonObj = new GameObject("Boton_" + nombre);
        botonObj.transform.SetParent(panelMejoras.transform, false);
        
        Button boton = botonObj.AddComponent<Button>();
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = color;
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(400f, 50f);
        rectBoton.anchoredPosition = posicion;
        
        // Texto del botón
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 20;
        textoBoton.color = Color.white;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        // Texto de nivel/información
        GameObject nivelObj = new GameObject("Nivel_" + nombre);
        nivelObj.transform.SetParent(panelMejoras.transform, false);
        
        textoNivel = nivelObj.AddComponent<TextMeshProUGUI>();
        textoNivel.text = $"Nivel: 0 | Costo: 50";
        textoNivel.fontSize = 16;
        textoNivel.color = Color.white;
        textoNivel.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectNivel = nivelObj.GetComponent<RectTransform>();
        rectNivel.sizeDelta = new Vector2(400f, 30f);
        rectNivel.anchoredPosition = new Vector2(0f, posicion.y - 35f);
        
        return boton;
    }
    
    private void CrearBotonCerrar()
    {
        botonCerrar = CrearBotonSimple("Cerrar", "❌ CERRAR", new Vector2(0f, -200f), Color.gray);
    }
    
    private Button CrearBotonSimple(string nombre, string texto, Vector2 posicion, Color color)
    {
        GameObject botonObj = new GameObject("Boton_" + nombre);
        botonObj.transform.SetParent(panelMejoras.transform, false);
        
        Button boton = botonObj.AddComponent<Button>();
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = color;
        
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(200f, 40f);
        rectBoton.anchoredPosition = posicion;
        
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(botonObj.transform, false);
        
        TextMeshProUGUI textoBoton = textoObj.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 18;
        textoBoton.color = Color.white;
        textoBoton.fontStyle = FontStyles.Bold;
        textoBoton.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;
        
        return boton;
    }
    
    private void ConfigurarBotones()
    {
        if (botonMejorarVida != null)
            botonMejorarVida.onClick.AddListener(() => MejorarStat("vida"));
        
        if (botonMejorarDaño != null)
            botonMejorarDaño.onClick.AddListener(() => MejorarStat("daño"));
        
        if (botonMejorarVelocidad != null)
            botonMejorarVelocidad.onClick.AddListener(() => MejorarStat("velocidad"));
        
        if (botonMejorarRecoleccion != null)
            botonMejorarRecoleccion.onClick.AddListener(() => MejorarStat("recoleccion"));
        
        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarMenu);
    }
    
    public void AbrirMenu()
    {
        if (menuAbierto) return;
        
        BuscarReferencias(); // Buscar referencias antes de abrir
        
        if (canvasMejoras != null)
        {
            canvasMejoras.gameObject.SetActive(true);
        }
        
        if (panelMejoras != null)
        {
            panelMejoras.SetActive(true);
        }
        
        menuAbierto = true;
        
        // NUNCA pausar el tiempo ni ocultar el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        ActualizarUI();
        
        if (mostrarDebug)
        {
            Debug.LogError("💪 MENÚ DE MEJORAS ABIERTO");
        }
    }
    
    public void CerrarMenu()
    {
        if (!menuAbierto) return;
        
        if (canvasMejoras != null)
        {
            canvasMejoras.gameObject.SetActive(false);
        }
        
        if (panelMejoras != null)
        {
            panelMejoras.SetActive(false);
        }
        
        menuAbierto = false;
        
        // MANTENER CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (mostrarDebug)
        {
            Debug.LogError("❌ MENÚ DE MEJORAS CERRADO");
        }
    }

    private void CargarNiveles()
    {
        // Cargar niveles desde PlayerPrefs
        nivelVida = PlayerPrefs.GetInt("NivelVida", 0);
        nivelDaño = PlayerPrefs.GetInt("NivelDaño", 0);
        nivelVelocidad = PlayerPrefs.GetInt("NivelVelocidad", 0);
        nivelRecoleccion = PlayerPrefs.GetInt("NivelRecoleccion", 0);
    }

    private void ActualizarUI()
    {
        if (textoZanahoriasActuales != null && sistemaMonedas != null)
        {
            textoZanahoriasActuales.text = $"💰 Monedas: {sistemaMonedas.GetCantidadMonedas()}";
        }
        
        ActualizarTextoMejora(textoNivelVida, nivelVida, costoMejoraVida);
        ActualizarTextoMejora(textoNivelDaño, nivelDaño, costoMejoraDaño);
        ActualizarTextoMejora(textoNivelVelocidad, nivelVelocidad, costoMejoraVelocidad);
        ActualizarTextoMejora(textoNivelRecoleccion, nivelRecoleccion, costoMejoraRecoleccion);
    }

    private void ActualizarTextoMejora(TextMeshProUGUI texto, int nivel, int costo)
    {
        if (texto != null)
        {
            texto.text = $"Nivel: {nivel} | Costo: {costo}";
        }
    }

    private void MejorarStat(string stat)
    {
        int costo = 0;
        int nivelActual = 0;
        
        switch (stat)
        {
            case "vida":
                costo = costoMejoraVida;
                nivelActual = nivelVida;
                break;
            case "daño":
                costo = costoMejoraDaño;
                nivelActual = nivelDaño;
                break;
            case "velocidad":
                costo = costoMejoraVelocidad;
                nivelActual = nivelVelocidad;
                break;
            case "recoleccion":
                costo = costoMejoraRecoleccion;
                nivelActual = nivelRecoleccion;
                break;
        }
        
        if (sistemaMonedas.GetCantidadMonedas() < costo)
        {
            Debug.LogError("❌ No tienes suficientes monedas");
            return;
        }
        
        // Aplicar mejora
        switch (stat)
        {
            case "vida":
                nivelVida++;
                if (jugador != null)
                {
                    var metodo = jugador.GetType().GetMethod("AgregarVida");
                    if (metodo != null)
                    {
                        metodo.Invoke(jugador, new object[] { incrementoVida });
                    }
                    else
                    {
                        Debug.LogWarning("MovimientoJugador no tiene método 'AgregarVida'; se incrementó el nivel pero no se aplicó la salud.");
                    }
                }
                else
                {
                    Debug.LogWarning("Jugador no encontrado al aplicar mejora de vida.");
                }
                break;
            case "daño":
                nivelDaño++;
                if (jugador != null)
                {
                    // Intentamos usar un método SetDañoBase si existe (nombre con ñ)
                    var tipo = jugador.GetType();
                    
                    // Intentar obtener el valor actual del daño usando distintos getters, propiedades o campos
                    float currentDamage = 0f;
                    var getter = tipo.GetMethod("GetDañoBase")
                                 ?? tipo.GetMethod("GetDanoBase")
                                 ?? tipo.GetMethod("GetDamageBase")
                                 ?? tipo.GetMethod("GetBaseDamage")
                                 ?? tipo.GetMethod("GetDamage")
                                 ?? tipo.GetMethod("GetDaño")
                                 ?? tipo.GetMethod("GetDano");
                    
                    if (getter != null)
                    {
                        var val = getter.Invoke(jugador, null);
                        try
                        {
                            currentDamage = System.Convert.ToSingle(val);
                        }
                        catch
                        {
                            Debug.LogWarning("No se pudo convertir el valor devuelto por el getter de daño a float.");
                        }
                    }
                    else
                    {
                        var prop = tipo.GetProperty("DañoBase")
                                ?? tipo.GetProperty("DanoBase")
                                ?? tipo.GetProperty("DamageBase")
                                ?? tipo.GetProperty("BaseDamage")
                                ?? tipo.GetProperty("Damage");
                    
                        if (prop != null)
                        {
                            var val = prop.GetValue(jugador);
                            try
                            {
                                currentDamage = System.Convert.ToSingle(val);
                            }
                            catch
                            {
                                Debug.LogWarning("No se pudo convertir la propiedad de daño a float.");
                            }
                        }
                        else
                        {
                            var field = tipo.GetField("dañoBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("danoBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("damageBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("baseDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    
                            if (field != null)
                            {
                                var val = field.GetValue(jugador);
                                try
                                {
                                    currentDamage = System.Convert.ToSingle(val);
                                }
                                catch
                                {
                                    Debug.LogWarning("No se pudo convertir el campo de daño a float.");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("No se encontró getter/propiedad/campo para obtener el daño; se usará 0 como referencia.");
                                currentDamage = 0f;
                            }
                        }
                    }
                    
                    float nuevoValor = currentDamage + incrementoDaño;
                    
                    // Buscar distintos nombres de método comunes como fallback
                    var metodo = tipo.GetMethod("SetDañoBase")
                                 ?? tipo.GetMethod("SetDanoBase")
                                 ?? tipo.GetMethod("SetDamageBase")
                                 ?? tipo.GetMethod("SetBaseDamage")
                                 ?? tipo.GetMethod("SetDamage");

                    if (metodo != null)
                    {
                        metodo.Invoke(jugador, new object[] { nuevoValor });
                    }
                    else
                    {
                        // Intentar propiedad con distintos nombres
                        var prop = tipo.GetProperty("DañoBase")
                                ?? tipo.GetProperty("DanoBase")
                                ?? tipo.GetProperty("DamageBase")
                                ?? tipo.GetProperty("BaseDamage")
                                ?? tipo.GetProperty("Damage");

                        if (prop != null && prop.CanWrite)
                        {
                            prop.SetValue(jugador, System.Convert.ChangeType(nuevoValor, prop.PropertyType));
                        }
                        else
                        {
                            // Intentar campo/field con distintos nombres
                            var field = tipo.GetField("dañoBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("danoBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("damageBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                     ?? tipo.GetField("baseDamage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                            if (field != null)
                            {
                                field.SetValue(jugador, System.Convert.ChangeType(nuevoValor, field.FieldType));
                            }
                            else
                            {
                                Debug.LogWarning("MovimientoJugador no expone un setter para el daño (SetDañoBase/propiedad/field). Se incrementó el nivel pero no se aplicó el daño.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Jugador no encontrado al aplicar mejora de daño.");
                }
                break;
            case "velocidad":
                nivelVelocidad++;
                if (jugador != null)
                {
                    var tipoV = jugador.GetType();

                    // Obtener valor actual de velocidad mediante distintos getters/propiedades/campos
                    float currentSpeed = 0f;
                    var getterV = tipoV.GetMethod("GetVelocidad")
                                 ?? tipoV.GetMethod("GetVeloc")
                                 ?? tipoV.GetMethod("GetSpeed")
                                 ?? tipoV.GetMethod("GetVelocity");
                    if (getterV != null)
                    {
                        var val = getterV.Invoke(jugador, null);
                        try
                        {
                            currentSpeed = System.Convert.ToSingle(val);
                        }
                        catch
                        {
                            Debug.LogWarning("No se pudo convertir el valor devuelto por el getter de velocidad a float.");
                        }
                    }
                    else
                    {
                        var propV = tipoV.GetProperty("Velocidad")
                                  ?? tipoV.GetProperty("velocidad")
                                  ?? tipoV.GetProperty("Speed")
                                  ?? tipoV.GetProperty("speed");
                        if (propV != null)
                        {
                            var val = propV.GetValue(jugador);
                            try
                            {
                                currentSpeed = System.Convert.ToSingle(val);
                            }
                            catch
                            {
                                Debug.LogWarning("No se pudo convertir la propiedad de velocidad a float.");
                            }
                        }
                        else
                        {
                            var fieldV = tipoV.GetField("velocidad", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                       ?? tipoV.GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (fieldV != null)
                            {
                                var val = fieldV.GetValue(jugador);
                                try
                                {
                                    currentSpeed = System.Convert.ToSingle(val);
                                }
                                catch
                                {
                                    Debug.LogWarning("No se pudo convertir el campo de velocidad a float.");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("No se encontró getter/propiedad/campo para velocidad; se usará 0 como referencia.");
                            }
                        }
                    }

                    float nuevoSpeed = currentSpeed + incrementoVelocidad;

                    // Intentar aplicar nuevo valor mediante distintos setters/propiedades/campos
                    var setterV = tipoV.GetMethod("SetVelocidad")
                                 ?? tipoV.GetMethod("SetSpeed")
                                 ?? tipoV.GetMethod("SetVelocity");
                    if (setterV != null)
                    {
                        setterV.Invoke(jugador, new object[] { nuevoSpeed });
                    }
                    else
                    {
                        var propVSet = tipoV.GetProperty("Velocidad")
                                    ?? tipoV.GetProperty("velocidad")
                                    ?? tipoV.GetProperty("Speed")
                                    ?? tipoV.GetProperty("speed");
                        if (propVSet != null && propVSet.CanWrite)
                        {
                            propVSet.SetValue(jugador, System.Convert.ChangeType(nuevoSpeed, propVSet.PropertyType));
                        }
                        else
                        {
                            var fieldVSet = tipoV.GetField("velocidad", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                           ?? tipoV.GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (fieldVSet != null)
                            {
                                fieldVSet.SetValue(jugador, System.Convert.ChangeType(nuevoSpeed, fieldVSet.FieldType));
                            }
                            else
                            {
                                Debug.LogWarning("MovimientoJugador no expone un setter para la velocidad; se incrementó el nivel pero no se aplicó velocidad.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Jugador no encontrado al aplicar mejora de velocidad.");
                }
                break;
            case "recoleccion":
                nivelRecoleccion++;
                if (jugador != null)
                {
                    var tipoR = jugador.GetType();

                    // Obtener valor actual de multiplicador de recolección mediante distintos getters/propiedades/campos
                    float currentMult = 1f;
                    var getterR = tipoR.GetMethod("GetMultiplicadorRecoleccion")
                                 ?? tipoR.GetMethod("GetRecoleccionMultiplicador")
                                 ?? tipoR.GetMethod("GetRecoleccionMultiplier")
                                 ?? tipoR.GetMethod("GetCollectionMultiplier");
                    if (getterR != null)
                    {
                        var val = getterR.Invoke(jugador, null);
                        try
                        {
                            currentMult = System.Convert.ToSingle(val);
                        }
                        catch
                        {
                            Debug.LogWarning("No se pudo convertir el valor devuelto por el getter de recolección a float.");
                        }
                    }
                    else
                    {
                        var propR = tipoR.GetProperty("MultiplicadorRecoleccion")
                                  ?? tipoR.GetProperty("multiplicadorRecoleccion")
                                  ?? tipoR.GetProperty("RecoleccionMultiplier")
                                  ?? tipoR.GetProperty("CollectionMultiplier");
                        if (propR != null)
                        {
                            var val = propR.GetValue(jugador);
                            try
                            {
                                currentMult = System.Convert.ToSingle(val);
                            }
                            catch
                            {
                                Debug.LogWarning("No se pudo convertir la propiedad de recolección a float.");
                            }
                        }
                        else
                        {
                            var fieldR = tipoR.GetField("multiplicadorRecoleccion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                       ?? tipoR.GetField("collectionMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (fieldR != null)
                            {
                                var val = fieldR.GetValue(jugador);
                                try
                                {
                                    currentMult = System.Convert.ToSingle(val);
                                }
                                catch
                                {
                                    Debug.LogWarning("No se pudo convertir el campo de recolección a float.");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("No se encontró getter/propiedad/campo para recolección; se usará 1 como referencia.");
                            }
                        }
                    }

                    float nuevoMult = currentMult * multiplicadorRecoleccion;

                    // Intentar aplicar nuevo multiplicador mediante distintos setters/propiedades/campos
                    var setterR = tipoR.GetMethod("SetMultiplicadorRecoleccion")
                                 ?? tipoR.GetMethod("SetRecoleccionMultiplicador")
                                 ?? tipoR.GetMethod("SetCollectionMultiplier");
                    if (setterR != null)
                    {
                        setterR.Invoke(jugador, new object[] { nuevoMult });
                    }
                    else
                    {
                        var propRSet = tipoR.GetProperty("MultiplicadorRecoleccion")
                                       ?? tipoR.GetProperty("multiplicadorRecoleccion")
                                       ?? tipoR.GetProperty("RecoleccionMultiplier")
                                       ?? tipoR.GetProperty("CollectionMultiplier");
                        if (propRSet != null && propRSet.CanWrite)
                        {
                            propRSet.SetValue(jugador, System.Convert.ChangeType(nuevoMult, propRSet.PropertyType));
                        }
                        else
                        {
                            var fieldRSet = tipoR.GetField("multiplicadorRecoleccion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                            ?? tipoR.GetField("collectionMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            if (fieldRSet != null)
                            {
                                fieldRSet.SetValue(jugador, System.Convert.ChangeType(nuevoMult, fieldRSet.FieldType));
                            }
                            else
                            {
                                Debug.LogWarning("MovimientoJugador no expone un setter para el multiplicador de recolección; se incrementó el nivel pero no se aplicó el multiplicador.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Jugador no encontrado al aplicar mejora de recolección.");
                }
                break;
        }
        
        // Quitar monedas
        sistemaMonedas.QuitarMonedas(costo);
        
        if (mostrarDebug)
        {
            Debug.LogError($"✅ {stat.ToUpper()} MEJORADO A NIVEL {nivelActual + 1}");
        }
        
        // Guardar niveles
        GuardarNiveles();
        
        // Actualizar UI
        ActualizarUI();
    }

    private void GuardarNiveles()
    {
        PlayerPrefs.SetInt("NivelVida", nivelVida);
        PlayerPrefs.SetInt("NivelDaño", nivelDaño);
        PlayerPrefs.SetInt("NivelVelocidad", nivelVelocidad);
        PlayerPrefs.SetInt("NivelRecoleccion", nivelRecoleccion);
        PlayerPrefs.Save();
    }
}