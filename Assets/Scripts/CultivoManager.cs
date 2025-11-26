using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

public class CultivoManager : MonoBehaviour
{
    // Referencias a Tilemaps y Tiles
    public Tilemap sueloTilemap;       // ¡El Tilemap de tu piso/tierra!
    public Tilemap cultivosTilemap;    // El Tilemap de cultivos
    public Tile tileTierraCultivable; // El Tile que identifica dónde se puede plantar
    
    // Lista de Tiles de Crecimiento (debes asignarlos en el Inspector en orden)
    public Tile[] tilesDeCrecimiento; // Índice 0=Semilla, 1-9=Etapas de crecimiento
    
    [Header("🚶‍♂️ CONFIGURACIÓN DE COLLIDERS")]
    [SerializeField] private bool soloSemillaTieneCollider = true;
    [SerializeField] private bool mostrarDebugColliders = false; // Reducido debug
    
    [Header("✨ SISTEMA DE RESALTADO")]
    [SerializeField] private bool activarResaltado = true;
    [SerializeField] private Color colorResaltado = new Color(1f, 1f, 0f, 0.6f);
    [SerializeField] private float velocidadParpadeo = 2f;
    [SerializeField] private bool mostrarDebugResaltado = false; // Reducido debug
    [SerializeField] private bool usarOverlay = true;
    
    [Header("🥕 SISTEMA DE ZANAHORIAS")]
    [SerializeField] private GameObject prefabZanahoria;
    [SerializeField] private int valorZanahoria = 1;
    [SerializeField] private int cantidadZanahoriasPorCosecha = 1;
    [SerializeField] private float fuerzaLanzamiento = 5f;
    [SerializeField] private float alturaLanzamiento = 2f;
    [SerializeField] private bool efectoLanzamiento = true;
    [SerializeField] private bool mostrarDebugZanahorias = false; // Reducido debug
    
    [Header("🎬 Sistema de Animaciones")]
    [SerializeField] private bool activarAnimaciones = true;
    [SerializeField] private string nombreAnimacionCultivando = "cultivando";
    [SerializeField] private float duracionAnimacionCultivando = 1.5f;
    [SerializeField] private bool mostrarDebugAnimaciones = false; // Reducido debug

    // Variables internas para el resaltado
    private Vector3Int celdaResaltada = Vector3Int.zero;
    private bool hayResaltado = false;
    private TileBase tileOriginal;
    private Color colorOriginal;
    private float tiempoParpadeo = 0f;
    private GameObject overlayResaltado;

    // Diccionario para rastrear los cultivos plantados
    private Dictionary<Vector3Int, CultivoData> cultivosPlantados = new Dictionary<Vector3Int, CultivoData>();
    
    // Referencia de la cámara para la interacción
    private Camera mainCamera;

    // Variables para el sistema de animaciones
    private MovimientoJugador jugadorScript;
    private Animator jugadorAnimator;
    private bool estaAnimandoCultivo = false;

    void Start()
    {
        Debug.LogError("🌱 CULTIVO MANAGER INICIANDO...");
        
        // 🔧 FORZAR RESALTADO ACTIVO SIEMPRE (NO desactivar por escena)
        activarResaltado = true; // FORZAR SIEMPRE ACTIVO
        
        mainCamera = Camera.main;
        
        // Verificar configuración básica
        if (!VerificarConfiguracion())
        {
            Debug.LogError("🚨 CULTIVO MANAGER MAL CONFIGURADO - DESACTIVANDO");
            enabled = false;
            return;
        }
        
        // SIEMPRE inicializar sistemas
        InicializarSistemaResaltado();
        
        if (activarAnimaciones)
        {
            InicializarSistemaAnimaciones();
        }
        
        Debug.LogError("✅ CULTIVO MANAGER INICIADO CORRECTAMENTE");
        Debug.LogError("✨ SISTEMA DE RESALTADO: ACTIVO");
        Debug.LogError("🎮 CONTROLES: Clic derecho = Plantar | C = Cosechar");
    }

    void Update()
    {
        // 🔧 PLANTACIÓN CON ANIMACIÓN - Cambiado para incluir animación
        
        // 1. Manejar plantación con clic derecho (CON ANIMACIÓN)
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            
            Debug.LogError("🖱️ CLIC DERECHO DETECTADO - Iniciando plantación con animación...");
            
            // VERIFICAR SI PUEDE PLANTAR ANTES DE ANIMAR
            if (PuedesPlantar(cellPos))
            {
                if (activarAnimaciones && jugadorAnimator != null && !estaAnimandoCultivo)
                {
                    IniciarAnimacionCultivo(() => IntentarPlantar(cellPos, "Zanahoria"));
                }
                else
                {
                    // Sin animación, plantar directamente
                    IntentarPlantar(cellPos, "Zanahoria");
                }
            }
            else
            {
                Debug.LogError("❌ NO SE PUEDE PLANTAR EN ESTA POSICIÓN");
            }
        }
        
        // 2. Manejar cosecha con tecla C (CON ANIMACIÓN)
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            
            Debug.LogError("⌨️ TECLA C DETECTADA - Iniciando cosecha con animación...");
            
            // VERIFICAR SI HAY CULTIVO PARA COSECHAR
            if (cultivosPlantados.ContainsKey(cellPos) && cultivosPlantados[cellPos].EstaMaduro())
            {
                if (activarAnimaciones && jugadorAnimator != null && !estaAnimandoCultivo)
                {
                    IniciarAnimacionCultivo(() => CosecharCultivo(cellPos));
                }
                else
                {
                    // Sin animación, cosechar directamente
                    CosecharCultivo(cellPos);
                }
            }
            else
            {
                Debug.LogError("❌ NO HAY CULTIVO MADURO PARA COSECHAR");
            }
        }

        // 3. Manejar crecimiento (menos frecuente)
        if (Time.frameCount % 30 == 0) // Solo cada 30 frames
        {
            ManejarCrecimiento();
        }
        
        // 4. Sistema de resaltado (SIEMPRE ACTIVO)
        ManejarResaltadoCultivos();
    }
    
    private bool VerificarConfiguracion()
    {
        if (sueloTilemap == null)
        {
            Debug.LogError("❌ Suelo Tilemap NO ASIGNADO");
            return false;
        }
        
        if (cultivosTilemap == null)
        {
            Debug.LogError("❌ Cultivos Tilemap NO ASIGNADO");
            return false;
        }
        
        if (tileTierraCultivable == null)
        {
            Debug.LogError("❌ Tile Tierra Cultivable NO ASIGNADO");
            return false;
        }
        
        if (tilesDeCrecimiento == null || tilesDeCrecimiento.Length < 10)
        {
            Debug.LogError("❌ Tiles de Crecimiento INCOMPLETOS");
            if (tilesDeCrecimiento != null)
            {
                Debug.LogError("   Tiles actuales: " + tilesDeCrecimiento.Length + "/10 requeridos");
            }
            return false;
        }
        
        return true;
    }
    
    private void IntentarPlantar(Vector3Int cellPos, string tipo)
    {
        Debug.LogError($"🌱 INTENTANDO PLANTAR EN: {cellPos}");
        
        // Verificaciones de seguridad
        if (!VerificarConfiguracion()) 
        {
            Debug.LogError("❌ Configuración inválida");
            return;
        }

        // A. Obtener el Tile del Tilemap de Piso/Suelo
        TileBase sueloTile = sueloTilemap.GetTile(cellPos);
        Debug.LogError($"🔍 TILE DE SUELO: {(sueloTile != null ? sueloTile.name : "NULL")}");

        // B. Validar si la tierra es cultivable (SIMPLIFICADO)
        bool puedesPlantar = false;
        
        if (sueloTile != null)
        {
            puedesPlantar = true; // PERMITIR PLANTAR EN CUALQUIER TILE POR AHORA
            Debug.LogError("✅ PUEDE PLANTAR: Tile encontrado");
        }
        else
        {
            Debug.LogError("❌ NO PUEDE PLANTAR: Sin tile de suelo");
        }
        
        if (!puedesPlantar)
        {
            Debug.LogError("❌ NO SE PUEDE PLANTAR AQUÍ - Sin suelo válido");
            return;
        }

        // C. Validar si la celda ya tiene un cultivo
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            CultivoData existente = cultivosPlantados[cellPos];
            Debug.LogError($"❌ YA HAY CULTIVO: {existente.tipoCultivo} etapa {existente.etapaActual}");
            return;
        }

        // D. Limpiar tile existente si hay inconsistencias
        TileBase cultivoExistente = cultivosTilemap.GetTile(cellPos);
        if (cultivoExistente != null)
        {
            cultivosTilemap.SetTile(cellPos, null);
            Debug.LogError("🧹 TILE ANTERIOR LIMPIADO");
        }

        // E. ¡PLANTAR INMEDIATAMENTE!
        CultivoData nuevoCultivo = new CultivoData
        {
            posicionCelda = cellPos,
            tipoCultivo = tipo,
            etapaActual = 0,
            tiempoPlantado = Time.time
        };

        cultivosPlantados.Add(cellPos, nuevoCultivo);

        if (tilesDeCrecimiento != null && tilesDeCrecimiento.Length > 0 && tilesDeCrecimiento[0] != null)
        {
            cultivosTilemap.SetTile(cellPos, tilesDeCrecimiento[0]);
            
            Debug.LogError($"✅ PLANTACIÓN INSTANTÁNEA EXITOSA en {cellPos}");
            Debug.LogError($"🌱 Tile asignado: {tilesDeCrecimiento[0].name}");
            
            // Configurar collider
            ConfigurarColliderCultivo(cellPos, true);
            
            // Asegurar sorting order
            TilemapRenderer cultivosRenderer = cultivosTilemap.GetComponent<TilemapRenderer>();
            TilemapRenderer sueloRenderer = sueloTilemap.GetComponent<TilemapRenderer>();
            
            if (cultivosRenderer != null && sueloRenderer != null)
            {
                if (cultivosRenderer.sortingOrder <= sueloRenderer.sortingOrder)
                {
                    cultivosRenderer.sortingOrder = sueloRenderer.sortingOrder + 1;
                    Debug.LogError($"🔧 SORTING ORDER AJUSTADO: Cultivos={cultivosRenderer.sortingOrder}, Suelo={sueloRenderer.sortingOrder}");
                }
            }
        }
        else
        {
            Debug.LogError("❌ ERROR: No hay tiles de crecimiento configurados");
        }
    }
    
    private void ManejarCrecimiento()
    {
        // Solo debug ocasional
        bool mostrarDebug = Time.time % 10f < 0.1f && mostrarDebugAnimaciones;
        
        if (mostrarDebug && cultivosPlantados.Count > 0)
        {
            Debug.LogError("🌿 Cultivos plantados: " + cultivosPlantados.Count);
        }
        
        List<Vector3Int> celdasAActualizar = new List<Vector3Int>(cultivosPlantados.Keys);

        foreach (Vector3Int cellPos in celdasAActualizar)
        {
            CultivoData data = cultivosPlantados[cellPos];
            float tiempoTranscurrido = Time.time - data.tiempoPlantado;

            int etapaObjetivo = Mathf.FloorToInt(tiempoTranscurrido / CultivoData.TIEMPO_POR_ETAPA);
            etapaObjetivo = Mathf.Clamp(etapaObjetivo, 0, 9);
            
            if (data.etapaActual < etapaObjetivo && data.etapaActual < 9)
            {
                data.etapaActual++;
                
                if (data.etapaActual < tilesDeCrecimiento.Length && tilesDeCrecimiento[data.etapaActual] != null)
                {
                    cultivosTilemap.SetTile(cellPos, tilesDeCrecimiento[data.etapaActual]);
                    ConfigurarColliderCultivo(cellPos, false); // Etapas de crecimiento sin collider
                    
                    if (mostrarDebug && data.etapaActual == 9)
                    {
                        Debug.LogError("🌾 Cultivo MADURO en " + cellPos);
                    }
                }
            }
        }
    }
    
    public void CosecharCultivo(Vector3Int cellPos)
    {
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            CultivoData data = cultivosPlantados[cellPos];
            
            if (data.EstaMaduro())
            {
                // Lanzar zanahorias
                if (prefabZanahoria != null)
                {
                    LanzarZanahorias(cellPos);
                }
                
                // Eliminar cultivo
                cultivosTilemap.SetTile(cellPos, null);
                cultivosPlantados.Remove(cellPos);
                
                if (mostrarDebugAnimaciones)
                {
                    Debug.LogError("✅ Cosechaste " + data.tipoCultivo);
                }
            }
            else
            {
                if (mostrarDebugAnimaciones)
                {
                    float progreso = data.ObtenerProgreso() * 100f;
                    Debug.LogError("❌ Cultivo no maduro. Progreso: " + progreso.ToString("F1") + "%");
                }
            }
        }
    }
    
    private void LanzarZanahorias(Vector3Int cellPos)
    {
        Vector3 posicionMundo = cultivosTilemap.CellToWorld(cellPos);
        posicionMundo += new Vector3(cultivosTilemap.cellSize.x * 0.5f, cultivosTilemap.cellSize.y * 0.5f, 0);
        
        for (int i = 0; i < cantidadZanahoriasPorCosecha; i++)
        {
            CrearZanahoria(posicionMundo, i);
        }
    }
    
    private void CrearZanahoria(Vector3 posicion, int indice)
    {
        GameObject nuevaZanahoria = Instantiate(prefabZanahoria, posicion, Quaternion.identity);
        
        // Configurar valor
        Zanahoria scriptZanahoria = nuevaZanahoria.GetComponent<Zanahoria>();
        if (scriptZanahoria != null)
        {
            scriptZanahoria.SetValor(valorZanahoria);
        }
        
        // Configurar colliders
        ConfigurarCollidersZanahoria(nuevaZanahoria);
        
        // Aplicar efecto de lanzamiento
        if (efectoLanzamiento)
        {
            Rigidbody2D rb = nuevaZanahoria.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = nuevaZanahoria.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
                rb.linearDamping = 0.3f;
            }
            
            Vector2 direccion = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
            Vector2 fuerzaFinal = direccion * fuerzaLanzamiento + Vector2.up * alturaLanzamiento;
            rb.AddForce(fuerzaFinal, ForceMode2D.Impulse);
            rb.angularVelocity = Random.Range(-180f, 180f);
        }
    }
    
    private void ConfigurarCollidersZanahoria(GameObject zanahoria)
    {
        CircleCollider2D[] collidersExistentes = zanahoria.GetComponents<CircleCollider2D>();
        
        CircleCollider2D triggerCollider = null;
        CircleCollider2D physicsCollider = null;
        
        if (collidersExistentes.Length == 0)
        {
            triggerCollider = zanahoria.AddComponent<CircleCollider2D>();
            physicsCollider = zanahoria.AddComponent<CircleCollider2D>();
        }
        else if (collidersExistentes.Length == 1)
        {
            triggerCollider = collidersExistentes[0];
            physicsCollider = zanahoria.AddComponent<CircleCollider2D>();
        }
        else
        {
            triggerCollider = collidersExistentes[0];
            physicsCollider = collidersExistentes[1];
        }
        
        // Configurar como triggers para evitar daño
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.3f;
        physicsCollider.isTrigger = true;
        physicsCollider.radius = 0.1f;
        
        // Configurar layer seguro
        zanahoria.layer = LayerMask.NameToLayer("Items");
        if (zanahoria.layer == -1)
        {
            zanahoria.layer = 0; // Default layer
        }
        
        // Ignorar colisiones con piso
        Collider2D[] collidersZanahoria = zanahoria.GetComponents<Collider2D>();
        foreach (var colliderZ in collidersZanahoria)
        {
            TilemapCollider2D[] collidersPiso = FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None);
            foreach (var colliderPiso in collidersPiso)
            {
                if (colliderPiso.gameObject.name.ToLower().Contains("piso") || 
                    colliderPiso.gameObject.name.ToLower().Contains("suelo"))
                {
                    Physics2D.IgnoreCollision(colliderZ, colliderPiso, true);
                }
            }
        }
    }
    
    private void ConfigurarColliderCultivo(Vector3Int cellPos, bool tieneCollider)
    {
        TilemapCollider2D tilemapCollider = cultivosTilemap.GetComponent<TilemapCollider2D>();
        
        if (tilemapCollider == null && tieneCollider)
        {
            tilemapCollider = cultivosTilemap.gameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider.isTrigger = false;
        }
        
        if (tilemapCollider != null)
        {
            tilemapCollider.enabled = true;
        }
    }
    
    private void ManejarResaltadoCultivos()
    {
        if (mainCamera == null || cultivosTilemap == null) return;
        
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
        
        // 🔧 VERIFICAR SI HAY SUELO PLANTEABLE EN LA CELDA
        TileBase sueloTile = sueloTilemap.GetTile(cellPos);
        bool haySueloPlanteable = sueloTile != null;
        
        // 🔧 VERIFICAR SI HAY CULTIVO EN LA CELDA
        bool hayCultivoEnCelda = cultivosPlantados.ContainsKey(cellPos);
        
        // RESALTAR SI: hay suelo planteable O hay cultivo
        bool debeResaltar = haySueloPlanteable || hayCultivoEnCelda;
        
        if (debeResaltar)
        {
            if (cellPos != celdaResaltada)
            {
                AplicarResaltado(cellPos, hayCultivoEnCelda ? "cultivo" : "tierra");
            }
            
            if (hayResaltado)
            {
                ActualizarParpadeo();
            }
        }
        
        // Debug del estado del resaltado
        if (Time.frameCount % 60 == 0 && mostrarDebugResaltado)
        {
            Debug.LogError($"✨ RESALTADO: Celda={cellPos} | Suelo={haySueloPlanteable} | Cultivo={hayCultivoEnCelda} | Resaltar={debeResaltar}");
        }
    }
    
    private void AplicarResaltado(Vector3Int cellPos, string tipo = "tierra")
    {
        if (cultivosTilemap == null) return;
        
        celdaResaltada = cellPos;
        hayResaltado = true;
        tiempoParpadeo = 0f;
        
        // SIEMPRE usar overlay para mejor visibilidad
        CrearOverlayResaltado(cellPos, tipo);
        
        if (mostrarDebugResaltado)
        {
            Debug.LogError($"✨ RESALTADO APLICADO: {cellPos} | Tipo: {tipo}");
        }
    }
    
    private void CrearOverlayResaltado(Vector3Int cellPos, string tipo = "tierra")
    {
        DestruirOverlayResaltado();
        
        overlayResaltado = new GameObject("OverlayResaltado_" + tipo);
        overlayResaltado.transform.SetParent(cultivosTilemap.transform);
        
        Vector3 worldPos = cultivosTilemap.CellToWorld(cellPos);
        Vector3 cellSize = cultivosTilemap.cellSize;
        
        worldPos.x += cellSize.x * 0.5f;
        worldPos.y += cellSize.y * 0.5f;
        worldPos.z = worldPos.z - 0.1f; // Delante del tilemap
        
        overlayResaltado.transform.position = worldPos;
        
        SpriteRenderer sr = overlayResaltado.AddComponent<SpriteRenderer>();
        
        int pixelWidth = Mathf.RoundToInt(cellSize.x * 100);
        int pixelHeight = Mathf.RoundToInt(cellSize.y * 100);
        
        Texture2D texture = new Texture2D(pixelWidth, pixelHeight);
        
        // 🔧 COLOR MÁS VISIBLE SEGÚN EL TIPO
        Color colorFondo = tipo == "cultivo" ? 
            new Color(0f, 1f, 0f, 0.8f) : // Verde para cultivos
            new Color(1f, 1f, 0f, 0.8f);   // Amarillo para tierra planteable
        
        Color[] pixels = new Color[pixelWidth * pixelHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = colorFondo;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, pixelWidth, pixelHeight), Vector2.one * 0.5f, 100f);
        sr.sprite = sprite;
        sr.color = colorFondo;
        
        // 🔧 ASEGURAR QUE ESTÉ MUY POR DELANTE
        TilemapRenderer tilemapRenderer = cultivosTilemap.GetComponent<TilemapRenderer>();
        if (tilemapRenderer != null)
        {
            sr.sortingLayerName = tilemapRenderer.sortingLayerName;
            sr.sortingOrder = tilemapRenderer.sortingOrder + 10; // MUY por delante
        }
        else
        {
            sr.sortingOrder = 100; // Muy alto por defecto
        }
        
        Debug.LogError($"✨ OVERLAY CREADO: {tipo} | Color: {colorFondo} | Sorting: {sr.sortingOrder}");
    }
    
    private void ActualizarParpadeo()
    {
        if (!hayResaltado || overlayResaltado == null) return;
        
        tiempoParpadeo += Time.deltaTime * velocidadParpadeo;
        float intensidad = (Mathf.Sin(tiempoParpadeo) + 1f) / 2f;
        
        SpriteRenderer sr = overlayResaltado.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color colorBase = sr.color;
            colorBase.a = 0.5f + (intensidad * 0.4f); // Alpha entre 0.5 y 0.9
            sr.color = colorBase;
            
            // Efecto de escala ligero
            float escala = 1f + (intensidad * 0.1f);
            overlayResaltado.transform.localScale = Vector3.one * escala;
        }
    }
    
    private void DestruirOverlayResaltado()
    {
        if (overlayResaltado != null)
        {
            if (Application.isPlaying)
            {
                Destroy(overlayResaltado);
            }
            else
            {
                DestroyImmediate(overlayResaltado);
            }
            overlayResaltado = null;
        }
    }
    
    private void InicializarSistemaResaltado()
    {
        hayResaltado = false;
        celdaResaltada = Vector3Int.zero;
        tiempoParpadeo = 0f;
    }
    
    private void InicializarSistemaAnimaciones()
    {
        GameObject jugadorObj = GameObject.FindWithTag("Player");
        
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
            jugadorScript = jugadorObj.GetComponent<MovimientoJugador>();
            jugadorAnimator = jugadorObj.GetComponent<Animator>();
            
            if (jugadorAnimator == null)
            {
                jugadorAnimator = jugadorObj.GetComponentInChildren<Animator>();
            }
            
            // 🔧 VERIFICAR PARÁMETROS DEL ANIMATOR MÁS DETALLADO
            if (jugadorAnimator != null)
            {
                bool tieneTriggerCultivo = false;
                
                if (jugadorAnimator.runtimeAnimatorController != null)
                {
                    foreach (AnimatorControllerParameter param in jugadorAnimator.parameters)
                    {
                        if (param.name == nombreAnimacionCultivando && param.type == AnimatorControllerParameterType.Trigger)
                        {
                            tieneTriggerCultivo = true;
                            break;
                        }
                    }
                }
                
                if (mostrarDebugAnimaciones)
                {
                    Debug.LogError("🎬 SISTEMA DE ANIMACIONES:");
                    Debug.LogError("  - Jugador: " + (jugadorScript != null ? "✅" : "❌"));
                    Debug.LogError("  - Animator: " + (jugadorAnimator != null ? "✅" : "❌"));
                    Debug.LogError("  - Controller: " + (jugadorAnimator.runtimeAnimatorController != null ? "✅" : "❌"));
                    Debug.LogError("  - Trigger '" + nombreAnimacionCultivando + "': " + (tieneTriggerCultivo ? "✅" : "❌"));
                    
                    if (jugadorAnimator.runtimeAnimatorController != null)
                    {
                        Debug.LogError("🎯 PARÁMETROS DISPONIBLES:");
                        foreach (AnimatorControllerParameter param in jugadorAnimator.parameters)
                        {
                            Debug.LogError($"    - {param.name} ({param.type})");
                        }
                    }
                }
                
                if (!tieneTriggerCultivo)
                {
                    Debug.LogError($"❌ ANIMATOR NO TIENE TRIGGER '{nombreAnimacionCultivando}' - Animaciones desactivadas");
                    activarAnimaciones = false;
                }
            }
            else
            {
                Debug.LogError("❌ NO SE ENCONTRÓ ANIMATOR EN EL JUGADOR - Animaciones desactivadas");
                activarAnimaciones = false;
            }
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ JUGADOR - Animaciones desactivadas");
            activarAnimaciones = false;
        }
    }
    
    private void IniciarAnimacionCultivo(System.Action callbackDespuesAnimacion)
    {
        if (estaAnimandoCultivo)
        {
            Debug.LogError("⚠️ YA SE ESTÁ EJECUTANDO UNA ANIMACIÓN DE CULTIVO");
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        if (jugadorAnimator == null)
        {
            Debug.LogError("❌ NO HAY ANIMATOR - Ejecutando acción directamente");
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        if (jugadorAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ NO HAY ANIMATOR CONTROLLER - Ejecutando acción directamente");
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        // Verificar que el trigger existe
        bool tieneTrigger = false;
        foreach (AnimatorControllerParameter param in jugadorAnimator.parameters)
        {
            if (param.name == nombreAnimacionCultivando && param.type == AnimatorControllerParameterType.Trigger)
            {
                tieneTrigger = true;
                break;
            }
        }
        
        if (!tieneTrigger)
        {
            Debug.LogError($"❌ NO EXISTE TRIGGER '{nombreAnimacionCultivando}' - Ejecutando acción directamente");
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        estaAnimandoCultivo = true;
        
        Debug.LogError($"🎬 INICIANDO ANIMACIÓN DE CULTIVO: {nombreAnimacionCultivando}");
        Debug.LogError($"⏱️ Duración configurada: {duracionAnimacionCultivando} segundos");
        
        // Bloquear movimiento del jugador
        if (jugadorScript != null)
        {
            BloquearMovimientoJugador(true);
        }
        
        try
        {
            // Activar el trigger de animación
            jugadorAnimator.SetTrigger(nombreAnimacionCultivando);
            Debug.LogError($"✅ TRIGGER '{nombreAnimacionCultivando}' ACTIVADO");
            
            // Iniciar la corrutina para terminar la animación
            StartCoroutine(TerminarAnimacionCultivo(callbackDespuesAnimacion));
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR ACTIVANDO TRIGGER: " + e.Message);
            estaAnimandoCultivo = false;
            BloquearMovimientoJugador(false);
            callbackDespuesAnimacion?.Invoke();
            return;
        }
    }
    
    private System.Collections.IEnumerator TerminarAnimacionCultivo(System.Action callback)
    {
        Debug.LogError($"⏳ ESPERANDO {duracionAnimacionCultivando} segundos para terminar animación...");
        
        yield return new WaitForSeconds(duracionAnimacionCultivando);
        
        estaAnimandoCultivo = false;
        
        if (jugadorScript != null)
        {
            BloquearMovimientoJugador(false);
        }
        
        Debug.LogError("🎬 ANIMACIÓN DE CULTIVO TERMINADA - Ejecutando acción");
        
        // Ejecutar la acción (plantar o cosechar)
        callback?.Invoke();
    }
    
    private void BloquearMovimientoJugador(bool bloquear)
    {
        if (jugadorScript == null) return;
        
        Rigidbody2D jugadorRb = jugadorScript.GetComponent<Rigidbody2D>();
        
        if (bloquear)
        {
            Debug.LogError("🔒 BLOQUEANDO MOVIMIENTO DEL JUGADOR");
            
            if (jugadorRb != null)
            {
                // Detener movimiento horizontal
                jugadorRb.linearVelocity = new Vector2(0, jugadorRb.linearVelocity.y);
            }
            
            // También se podría desactivar temporalmente el input
            // jugadorScript.enabled = false; // Descomentar si quieres bloquear completamente
        }
        else
        {
            Debug.LogError("🔓 DESBLOQUEANDO MOVIMIENTO DEL JUGADOR");
            
            // Reactivar script si se había desactivado
            // jugadorScript.enabled = true; // Descomentar si se había bloqueado completamente
        }
    }

    // Métodos públicos para persistencia
    public Dictionary<Vector3Int, CultivoData> ObtenerTodosCultivos()
    {
        return new Dictionary<Vector3Int, CultivoData>(cultivosPlantados);
    }
    
    public void RestaurarCultivo(Vector3Int posicion, string tipo, int etapa, float tiempoPlantado)
    {
        if (cultivosTilemap == null || tilesDeCrecimiento == null || tilesDeCrecimiento.Length <= etapa)
        {
            return;
        }
        
        CultivoData cultivoRestaurado = new CultivoData
        {
            posicionCelda = posicion,
            tipoCultivo = tipo,
            etapaActual = etapa,
            tiempoPlantado = tiempoPlantado
        };
        
        cultivosPlantados[posicion] = cultivoRestaurado;
        
        if (etapa < tilesDeCrecimiento.Length && tilesDeCrecimiento[etapa] != null)
        {
            cultivosTilemap.SetTile(posicion, tilesDeCrecimiento[etapa]);
            ConfigurarColliderCultivo(posicion, etapa == 0);
        }
    }
    
    public void LimpiarTodosLosCultivos()
    {
        foreach (var posicion in cultivosPlantados.Keys)
        {
            cultivosTilemap.SetTile(posicion, null);
        }
        cultivosPlantados.Clear();
    }
    
    // Métodos de contexto para testing
    [ContextMenu("🌱 Test - Plantar en (0,0)")]
    public void TestPlantar()
    {
        IntentarPlantar(Vector3Int.zero, "Zanahoria");
    }
    
    [ContextMenu("🧹 Limpiar Cultivos")]
    public void TestLimpiar()
    {
        LimpiarTodosLosCultivos();
    }
    
    [ContextMenu("📊 Mostrar Estado")]
    public void MostrarEstado()
    {
        Debug.LogError("📊 ESTADO DEL CULTIVO MANAGER:");
        Debug.LogError($"  - Cultivos plantados: {cultivosPlantados.Count}");
        Debug.LogError($"  - Configuración válida: {VerificarConfiguracion()}");
        Debug.LogError($"  - Animaciones activas: {activarAnimaciones}");
        Debug.LogError($"  - Resaltado activo: {activarResaltado}");
    }
    
    // 🧪 MÉTODO DE TESTING MEJORADO
    [ContextMenu("🧪 Test - Verificar Sistema Completo")]
    public void TestVerificarSistemaCompleto()
    {
        Debug.LogError("🧪 VERIFICANDO SISTEMA COMPLETO DE CULTIVOS:");
        Debug.LogError("===========================================");
        
        // 1. Configuración básica
        bool configValida = VerificarConfiguracion();
        Debug.LogError($"📋 CONFIGURACIÓN VÁLIDA: {(configValida ? "✅" : "❌")}");
        
        if (sueloTilemap != null)
        {
            Debug.LogError($"  - Suelo Tilemap: ✅ {sueloTilemap.name}");
            Debug.LogError($"  - Sorting Order: {sueloTilemap.GetComponent<TilemapRenderer>()?.sortingOrder}");
        }
        else
        {
            Debug.LogError("  - Suelo Tilemap: ❌ NULL");
        }
        
        if (cultivosTilemap != null)
        {
            Debug.LogError($"  - Cultivos Tilemap: ✅ {cultivosTilemap.name}");
            Debug.LogError($"  - Sorting Order: {cultivosTilemap.GetComponent<TilemapRenderer>()?.sortingOrder}");
        }
        else
        {
            Debug.LogError("  - Cultivos Tilemap: ❌ NULL");
        }
        
        // 2. Tiles de crecimiento
        if (tilesDeCrecimiento != null)
        {
            Debug.LogError($"📦 TILES DE CRECIMIENTO: {tilesDeCrecimiento.Length}/10");
            for (int i = 0; i < tilesDeCrecimiento.Length; i++)
            {
                bool tieneSprite = tilesDeCrecimiento[i] != null;
                Debug.LogError($"  - Etapa {i}: {(tieneSprite ? "✅" : "❌")} {(tieneSprite ? tilesDeCrecimiento[i].name : "NULL")}");
            }
        }
        
        // 3. Sistema de resaltado
        Debug.LogError($"✨ SISTEMA RESALTADO:");
        Debug.LogError($"  - Activo: {activarResaltado}");
        Debug.LogError($"  - Hay resaltado: {hayResaltado}");
        Debug.LogError($"  - Overlay existe: {overlayResaltado != null}");
        Debug.LogError($"  - Cámara asignada: {mainCamera != null}");
        
        // 4. Cultivos plantados
        Debug.LogError($"🌱 CULTIVOS PLANTADOS: {cultivosPlantados.Count}");
        foreach (var cultivo in cultivosPlantados)
        {
            Debug.LogError($"  - {cultivo.Value.tipoCultivo} etapa {cultivo.Value.etapaActual} en {cultivo.Key}");
        }
        
        Debug.LogError("===========================================");
    }
    
    [ContextMenu("🧪 Test - Forzar Plantación en (0,0)")]
    public void TestForzarPlantacion()
    {
        Debug.LogError("🧪 FORZANDO PLANTACIÓN DE PRUEBA EN (0,0)...");
        IntentarPlantar(Vector3Int.zero, "Zanahoria");
    }
    
    [ContextMenu("✨ Test - Verificar Resaltado")]
    public void TestVerificarResaltado()
    {
        Debug.LogError("✨ VERIFICANDO SISTEMA DE RESALTADO:");
        Debug.LogError($"  - Activo: {activarResaltado}");
        Debug.LogError($"  - Usando overlay: {usarOverlay}");
        Debug.LogError($"  - Color configurado: {colorResaltado}");
        Debug.LogError($"  - Velocidad parpadeo: {velocidadParpadeo}");
        
        // Forzar crear overlay de prueba en (0,0)
        AplicarResaltado(Vector3Int.zero, "prueba");
        
        Debug.LogError("✅ Resaltado de prueba creado en (0,0)");
    }
    
    // 🧪 MÉTODO DE TESTING PARA ANIMACIONES
    [ContextMenu("🎬 Test - Probar Animación Cultivo")]
    public void TestProbarAnimacionCultivo()
    {
        Debug.LogError("🧪 PROBANDO ANIMACIÓN DE CULTIVO...");
        
        if (activarAnimaciones)
        {
            IniciarAnimacionCultivo(() => {
                Debug.LogError("🎯 CALLBACK DE ANIMACIÓN EJECUTADO");
            });
        }
        else
        {
            Debug.LogError("❌ ANIMACIONES DESACTIVADAS");
        }
    }
    
    [ContextMenu("🔧 Test - Verificar Sistema Animaciones")]
    public void TestVerificarSistemaAnimaciones()
    {
        Debug.LogError("🔍 VERIFICANDO SISTEMA DE ANIMACIONES:");
        Debug.LogError("===========================================");
        
        // Verificar configuración
        Debug.LogError($"🎬 CONFIGURACIÓN:");
        Debug.LogError($"  - Animaciones activadas: {activarAnimaciones}");
        Debug.LogError($"  - Nombre trigger: {nombreAnimacionCultivando}");
        Debug.LogError($"  - Duración: {duracionAnimacionCultivando}s");
        Debug.LogError($"  - Está animando: {estaAnimandoCultivo}");
        
        // Verificar referencias
        Debug.LogError($"🎯 REFERENCIAS:");
        Debug.LogError($"  - Jugador script: {(jugadorScript != null ? "✅" : "❌")}");
        Debug.LogError($"  - Jugador animator: {(jugadorAnimator != null ? "✅" : "❌")}");
        
        if (jugadorAnimator != null)
        {
            Debug.LogError($"  - Controller asignado: {(jugadorAnimator.runtimeAnimatorController != null ? "✅" : "❌")}");
            
            if (jugadorAnimator.runtimeAnimatorController != null)
            {
                bool tieneTrigger = false;
                Debug.LogError($"📋 PARÁMETROS DEL ANIMATOR:");
                
                foreach (AnimatorControllerParameter param in jugadorAnimator.parameters)
                {
                    Debug.LogError($"    - {param.name} ({param.type})");
                    if (param.name == nombreAnimacionCultivando && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        tieneTrigger = true;
                    }
                }
                
                Debug.LogError($"🎯 TRIGGER '{nombreAnimacionCultivando}': {(tieneTrigger ? "✅ ENCONTRADO" : "❌ NO ENCONTRADO")}");
                
                if (!tieneTrigger)
                {
                    Debug.LogError($"💡 SOLUCIÓN: Agrega un parámetro Trigger llamado '{nombreAnimacionCultivando}' en el Animator Controller");
                }
            }
        }
        
        Debug.LogError("===========================================");
    }
    
    // 🆕 MÉTODO PARA VERIFICAR SI SE PUEDE PLANTAR ANTES DE ANIMAR
    private bool PuedesPlantar(Vector3Int cellPos)
    {
        if (!VerificarConfiguracion()) return false;

        // A. Obtener el Tile del Tilemap de Piso/Suelo
        TileBase sueloTile = sueloTilemap.GetTile(cellPos);
        
        // B. Validar si la tierra es cultivable
        bool puedesPlantar = sueloTile != null;
        
        // C. Validar si la celda ya tiene un cultivo
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            return false;
        }
        
        return puedesPlantar;
    }
}