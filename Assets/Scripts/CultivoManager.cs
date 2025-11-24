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
        
        // 🔧 Detectar escena y optimizar
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esEscena1 = escenaActual.Contains("Escena1") || escenaActual.Contains("1");
        
        if (esEscena1)
        {
            Debug.LogError("🎯 DETECTADA ESCENA1 - Aplicando optimizaciones");
            // Desactivar debugs intensivos
            mostrarDebugResaltado = false;
            mostrarDebugAnimaciones = false;
            mostrarDebugZanahorias = false;
            mostrarDebugColliders = false;
            // Mantener funcionalidad pero sin resaltado para performance
            activarResaltado = false;
        }
        
        mainCamera = Camera.main;
        
        // Verificar configuración básica
        if (!VerificarConfiguracion())
        {
            Debug.LogError("🚨 CULTIVO MANAGER MAL CONFIGURADO - DESACTIVANDO");
            enabled = false;
            return;
        }
        
        // Inicializar sistemas
        if (activarResaltado)
        {
            InicializarSistemaResaltado();
        }
        
        if (activarAnimaciones)
        {
            InicializarSistemaAnimaciones();
        }
        
        Debug.LogError("✅ CULTIVO MANAGER INICIADO CORRECTAMENTE");
    }

    void Update()
    {
        // 🔧 Optimización para Escena1
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esEscena1 = escenaActual.Contains("Escena1") || escenaActual.Contains("1");
        
        if (esEscena1 && Time.frameCount % 3 != 0)
        {
            return; // Procesar solo cada 3 frames en Escena1
        }
        
        // 1. Manejar plantación con clic derecho
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            
            if (activarAnimaciones && !estaAnimandoCultivo && jugadorAnimator != null)
            {
                IniciarAnimacionCultivo(() => {
                    IntentarPlantar(cellPos, "Zanahoria");
                });
            }
            else
            {
                IntentarPlantar(cellPos, "Zanahoria");
            }
        }
        
        // 2. Manejar cosecha con tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            
            if (activarAnimaciones && !estaAnimandoCultivo && jugadorAnimator != null)
            {
                IniciarAnimacionCultivo(() => {
                    CosecharCultivo(cellPos);
                });
            }
            else
            {
                CosecharCultivo(cellPos);
            }
        }

        // 3. Manejar crecimiento
        ManejarCrecimiento();
        
        // 4. Sistema de resaltado (solo si está activado y no es Escena1)
        if (activarResaltado && !esEscena1)
        {
            ManejarResaltadoCultivos();
        }
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
        // Verificaciones de seguridad
        if (!VerificarConfiguracion()) return;

        // A. Obtener el Tile del Tilemap de Piso/Suelo
        TileBase sueloTile = sueloTilemap.GetTile(cellPos);

        // B. Validar si la tierra es cultivable
        bool puedesPlantar = false;
        
        if (tileTierraCultivable == null)
        {
            // Auto-configurar con el tile encontrado
            if (sueloTile != null && sueloTile is Tile)
            {
                tileTierraCultivable = sueloTile as Tile;
                puedesPlantar = true;
            }
        }
        else if (sueloTile != null && (sueloTile.name == tileTierraCultivable.name || sueloTile == tileTierraCultivable))
        {
            puedesPlantar = true;
        }
        else if (sueloTile != null)
        {
            // Auto-actualizar si encontramos un tile similar
            if (sueloTile.name.Contains("Piskel") && tileTierraCultivable.name.Contains("Piskel"))
            {
                tileTierraCultivable = sueloTile as Tile;
                if (tileTierraCultivable != null)
                {
                    puedesPlantar = true;
                }
            }
        }
        
        if (!puedesPlantar)
        {
            if (mostrarDebugAnimaciones) // Usar un debug flag existente
            {
                Debug.LogError("❌ NO SE PUEDE PLANTAR AQUÍ");
            }
            return;
        }

        // C. Validar si la celda ya tiene un cultivo
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            CultivoData existente = cultivosPlantados[cellPos];
            if (mostrarDebugAnimaciones)
            {
                Debug.LogError("❌ Ya hay algo plantado: " + existente.tipoCultivo + " etapa " + existente.etapaActual);
            }
            return;
        }

        // D. Limpiar tile existente si hay inconsistencias
        TileBase cultivoExistente = cultivosTilemap.GetTile(cellPos);
        if (cultivoExistente != null)
        {
            cultivosTilemap.SetTile(cellPos, null);
        }

        // E. ¡PLANTAR!
        CultivoData nuevoCultivo = new CultivoData
        {
            posicionCelda = cellPos,
            tipoCultivo = tipo,
            etapaActual = 0,
            tiempoPlantado = Time.time
        };

        cultivosPlantados.Add(cellPos, nuevoCultivo);

        if (tilesDeCrecimiento[0] != null)
        {
            cultivosTilemap.SetTile(cellPos, tilesDeCrecimiento[0]);
            
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
                }
            }
            
            if (mostrarDebugAnimaciones)
            {
                Debug.LogError("✅ PLANTACIÓN EXITOSA en " + cellPos);
            }
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
        
        bool hayCultivoEnCelda = cultivosPlantados.ContainsKey(cellPos);
        
        if (hayCultivoEnCelda)
        {
            if (cellPos != celdaResaltada)
            {
                QuitarResaltado();
                AplicarResaltado(cellPos);
            }
            
            if (hayResaltado)
            {
                ActualizarParpadeo();
            }
        }
        else
        {
            QuitarResaltado();
        }
    }
    
    private void AplicarResaltado(Vector3Int cellPos)
    {
        if (cultivosTilemap == null) return;
        
        TileBase tileActual = cultivosTilemap.GetTile(cellPos);
        if (tileActual == null) return;
        
        celdaResaltada = cellPos;
        tileOriginal = tileActual;
        hayResaltado = true;
        tiempoParpadeo = 0f;
        
        if (usarOverlay)
        {
            CrearOverlayResaltado(cellPos);
        }
        else
        {
            colorOriginal = cultivosTilemap.GetColor(cellPos);
        }
    }
    
    private void QuitarResaltado()
    {
        if (!hayResaltado) return;
        
        if (usarOverlay)
        {
            DestruirOverlayResaltado();
        }
        else
        {
            if (cultivosTilemap != null)
            {
                cultivosTilemap.SetColor(celdaResaltada, colorOriginal);
            }
        }
        
        hayResaltado = false;
        celdaResaltada = Vector3Int.zero;
    }
    
    private void ActualizarParpadeo()
    {
        if (!hayResaltado) return;
        
        tiempoParpadeo += Time.deltaTime * velocidadParpadeo;
        float intensidad = (Mathf.Sin(tiempoParpadeo) + 1f) / 2f;
        
        if (usarOverlay)
        {
            ActualizarOverlayParpadeo(intensidad);
        }
        else
        {
            if (cultivosTilemap == null) return;
            
            Color colorFinal = intensidad > 0.5f ? 
                colorOriginal + colorResaltado : 
                Color.Lerp(colorOriginal, colorOriginal + colorResaltado * 0.3f, intensidad * 2f);
            
            cultivosTilemap.SetColor(celdaResaltada, colorFinal);
        }
    }
    
    private void CrearOverlayResaltado(Vector3Int cellPos)
    {
        DestruirOverlayResaltado();
        
        overlayResaltado = new GameObject("OverlayResaltado");
        overlayResaltado.transform.SetParent(cultivosTilemap.transform);
        
        Vector3 worldPos = cultivosTilemap.CellToWorld(cellPos);
        Vector3 cellSize = cultivosTilemap.cellSize;
        
        worldPos.x += cellSize.x * 0.5f;
        worldPos.y += cellSize.y * 0.5f;
        worldPos.z = worldPos.z - 0.1f;
        
        overlayResaltado.transform.position = worldPos;
        
        SpriteRenderer sr = overlayResaltado.AddComponent<SpriteRenderer>();
        
        int pixelWidth = Mathf.RoundToInt(cellSize.x * 100);
        int pixelHeight = Mathf.RoundToInt(cellSize.y * 100);
        
        Texture2D texture = new Texture2D(pixelWidth, pixelHeight);
        
        Color[] pixels = new Color[pixelWidth * pixelHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, pixelWidth, pixelHeight), Vector2.one * 0.5f, 100f);
        sr.sprite = sprite;
        sr.color = colorResaltado;
        
        TilemapRenderer tilemapRenderer = cultivosTilemap.GetComponent<TilemapRenderer>();
        if (tilemapRenderer != null)
        {
            sr.sortingLayerName = tilemapRenderer.sortingLayerName;
            sr.sortingOrder = tilemapRenderer.sortingOrder + 1;
        }
    }
    
    private void ActualizarOverlayParpadeo(float intensidad)
    {
        if (overlayResaltado == null) return;
        
        SpriteRenderer sr = overlayResaltado.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = colorResaltado;
            color.a = 0.2f + (intensidad * 0.6f);
            sr.color = color;
            overlayResaltado.transform.localScale = Vector3.one;
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
            
            if (mostrarDebugAnimaciones)
            {
                Debug.LogError("🎬 SISTEMA DE ANIMACIONES:");
                Debug.LogError("  - Jugador: " + (jugadorScript != null ? "✅" : "❌"));
                Debug.LogError("  - Animator: " + (jugadorAnimator != null ? "✅" : "❌"));
            }
        }
    }
    
    private void IniciarAnimacionCultivo(System.Action callbackDespuesAnimacion)
    {
        if (estaAnimandoCultivo)
        {
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        if (jugadorAnimator == null)
        {
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
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        estaAnimandoCultivo = true;
        
        if (jugadorScript != null)
        {
            BloquearMovimientoJugador(true);
        }
        
        try
        {
            jugadorAnimator.SetTrigger(nombreAnimacionCultivando);
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error activando trigger: " + e.Message);
            estaAnimandoCultivo = false;
            BloquearMovimientoJugador(false);
            callbackDespuesAnimacion?.Invoke();
            return;
        }
        
        StartCoroutine(TerminarAnimacionCultivo(callbackDespuesAnimacion));
    }
    
    private System.Collections.IEnumerator TerminarAnimacionCultivo(System.Action callback)
    {
        yield return new WaitForSeconds(duracionAnimacionCultivando);
        
        estaAnimandoCultivo = false;
        
        if (jugadorScript != null)
        {
            BloquearMovimientoJugador(false);
        }
        
        callback?.Invoke();
    }
    
    private void BloquearMovimientoJugador(bool bloquear)
    {
        if (jugadorScript == null) return;
        
        Rigidbody2D jugadorRb = jugadorScript.GetComponent<Rigidbody2D>();
        
        if (bloquear)
        {
            if (jugadorRb != null)
            {
                jugadorRb.linearVelocity = new Vector2(0, jugadorRb.linearVelocity.y);
            }
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
}