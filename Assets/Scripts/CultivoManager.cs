using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CultivoManager : MonoBehaviour
{
    // Referencias a Tilemaps y Tiles
    public Tilemap sueloTilemap;       // ¡El Tilemap de tu piso/tierra!
    public Tilemap cultivosTilemap;    // El Tilemap de cultivos
    public Tile tileTierraCultivable; // El Tile que identifica dónde se puede plantar
    
    // Lista de Tiles de Crecimiento (debes asignarlos en el Inspector en orden)
    public Tile[] tilesDeCrecimiento; // Índice 0=Semilla, 1-9=Etapas de crecimiento

    // Diccionario para rastrear los cultivos plantados
    private Dictionary<Vector3Int, CultivoData> cultivosPlantados = new Dictionary<Vector3Int, CultivoData>();
    
    // Referencia de la cámara para la interacción
    private Camera mainCamera;

    void Start()
    {
        // MENSAJE PRIORITARIO PARA VERIFICAR EJECUCIÓN
        Debug.LogError("🚨 CULTIVO MANAGER START EJECUTÁNDOSE 🚨");
        Debug.LogWarning("Si ves este mensaje, el CultivoManager SÍ está funcionando");
        
        mainCamera = Camera.main;
        
        // 🔍 DEBUG: Verificar referencias
        Debug.LogError("=== CULTIVO MANAGER - START EJECUTADO ===");
        Debug.LogError("Si ves este mensaje, el script SÍ está funcionando");
        Debug.Log("Camera encontrada: " + (mainCamera != null ? "✅" : "❌"));
        Debug.Log("Suelo Tilemap asignado: " + (sueloTilemap != null ? "✅" : "❌"));
        Debug.Log("Cultivos Tilemap asignado: " + (cultivosTilemap != null ? "✅" : "❌"));
        Debug.Log("Tile Tierra Cultivable asignado: " + (tileTierraCultivable != null ? "✅" : "❌"));
        Debug.Log("Tiles de Crecimiento asignados: " + (tilesDeCrecimiento != null && tilesDeCrecimiento.Length > 0 ? tilesDeCrecimiento.Length + " tiles" : "❌ NINGUNO"));
        
        // 🔍 DEBUG ADICIONAL: Posición del jugador vs cámara
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            Debug.Log("📍 Jugador encontrado en posición: " + jugador.transform.position);
            Debug.Log("📷 Cámara en posición: " + mainCamera.transform.position);
            float distancia = Vector3.Distance(jugador.transform.position, mainCamera.transform.position);
            Debug.Log("📏 Distancia cámara-jugador: " + distancia.ToString("F2"));
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró jugador con tag 'Player'");
        }
        
        if (tilesDeCrecimiento != null)
        {
            for (int i = 0; i < tilesDeCrecimiento.Length; i++)
            {
                Debug.Log("  - Tile[" + i + "]: " + (tilesDeCrecimiento[i] != null ? "✅ " + tilesDeCrecimiento[i].name : "❌ NULL"));
            }
        }
        Debug.Log("=== FIN VERIFICACIÓN ===");
        
        // 🚨 VERIFICACIÓN CRÍTICA
        if (sueloTilemap == null || cultivosTilemap == null || tileTierraCultivable == null || 
            tilesDeCrecimiento == null || tilesDeCrecimiento.Length < 9)
        {
            Debug.LogError("🚨 CULTIVO MANAGER MAL CONFIGURADO!");
            Debug.LogError("   Ve al Inspector y asigna TODAS las referencias requeridas:");
            Debug.LogError("   - Suelo Tilemap");
            Debug.LogError("   - Cultivos Tilemap"); 
            Debug.LogError("   - Tile Tierra Cultivable");
            Debug.LogError("   - Tiles de Crecimiento (necesarios: 9 tiles)");
            if (tilesDeCrecimiento != null)
            {
                Debug.LogError("   - Tiles actuales: " + tilesDeCrecimiento.Length + "/9");
            }
            Debug.LogError("   ⚠️ EL SISTEMA NO FUNCIONARÁ HASTA QUE SE CONFIGURE!");
            return;
        }
        
        Debug.Log("✅ Configuración básica correcta, sistema listo con " + tilesDeCrecimiento.Length + " tiles de crecimiento");
    }
    // ... (continúa con el paso 4 y 5)
// ... (continuación de CultivoManager.cs)

    void Update()
    {
        // DEBUG SIMPLE - para verificar que el script funciona
        if (Time.time % 2f < 0.1f) // Cada 2 segundos
        {
            Debug.LogWarning("🌱 CULTIVO MANAGER ACTIVO - " + Time.time.ToString("F1") + "s");
        }
        
        // 1. Manejar la Plantación (Cambié a clic derecho para evitar conflicto con ataque)
        if (Input.GetMouseButtonDown(1)) // Clic derecho para plantar
        {
            Debug.LogError("🖱️ CLIC DERECHO DETECTADO - Iniciando plantación...");
            
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            
            Debug.Log("📍 Posición del mouse:");
            Debug.Log("  - En pantalla: " + mouseScreenPos);
            Debug.Log("  - En mundo (raw): " + mouseWorldPos);
            Debug.Log("  - En mundo (Z=0): " + new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0));
            
            // Convertir la posición mundial a la celda del Tilemap
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            Debug.Log("  - Celda calculada: " + cellPos);
            
            // 🔍 VERIFICAR QUÉ HAY EN ESA POSICIÓN ANTES DE PLANTAR
            TileBase tileEnEsaPosicion = sueloTilemap.GetTile(cellPos);
            if (tileEnEsaPosicion == null)
            {
                Debug.LogWarning("⚠️ No hay tile en la posición del clic. Buscando tiles cercanos...");
                
                // Buscar tiles en un radio pequeño alrededor del clic
                bool encontroTileCercano = false;
                for (int dx = -2; dx <= 2 && !encontroTileCercano; dx++)
                {
                    for (int dy = -2; dy <= 2 && !encontroTileCercano; dy++)
                    {
                        Vector3Int posicionCercana = cellPos + new Vector3Int(dx, dy, 0);
                        TileBase tileCercano = sueloTilemap.GetTile(posicionCercana);
                        
                        if (tileCercano != null)
                        {
                            Debug.Log("✅ Tile cercano encontrado: '" + tileCercano.name + "' en " + posicionCercana);
                            Debug.Log("   Distancia del clic: " + (dx == 0 && dy == 0 ? "mismo lugar" : dx + "," + dy + " celdas"));
                            encontroTileCercano = true;
                        }
                    }
                }
                
                if (!encontroTileCercano)
                {
                    Debug.LogError("❌ No hay tiles en un radio de 2 celdas. ¿Pintaste tiles con Tile Palette?");
                }
            }
            
            // 🔍 VERIFICAR POSICIÓN DEL JUGADOR
            GameObject jugador = GameObject.FindWithTag("Player");
            if (jugador != null)
            {
                float distanciaAlJugador = Vector3.Distance(new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0), jugador.transform.position);
                Debug.Log("  - Distancia al jugador: " + distanciaAlJugador.ToString("F2"));
            }

            IntentarPlantar(cellPos, "Zanahoria"); // Intentar plantar una "Zanahoria"
        }
        
        // Agregar cosechar con tecla C
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.LogError("🧺 TECLA C PRESIONADA - Intentando cosechar...");
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = cultivosTilemap.WorldToCell(mouseWorldPos);
            
            CosecharCultivo(cellPos);
        }

        // 2. Manejar el Crecimiento
        ManejarCrecimiento();
    }
    
    // Función para verificar y plantar
    private void IntentarPlantar(Vector3Int cellPos, string tipo)
    {
        Debug.Log("🌱 INTENTAR PLANTAR en celda: " + cellPos + " | Tipo: " + tipo);
        
        // Verificaciones de seguridad
        if (sueloTilemap == null)
        {
            Debug.LogError("❌ ERROR: sueloTilemap es NULL!");
            return;
        }
        
        if (cultivosTilemap == null)
        {
            Debug.LogError("❌ ERROR: cultivosTilemap es NULL!");
            return;
        }
        
        if (tileTierraCultivable == null)
        {
            Debug.LogError("❌ ERROR: tileTierraCultivable es NULL!");
            return;
        }
        
        if (tilesDeCrecimiento == null || tilesDeCrecimiento.Length < 10)
        {
            Debug.LogError("❌ ERROR: tilesDeCrecimiento no está configurado correctamente!");
            if (tilesDeCrecimiento != null)
            {
                Debug.LogError("   Tiles actuales: " + tilesDeCrecimiento.Length + "/10 requeridos");
            }
            return;
        }

        // A. Obtener el Tile del Tilemap de Piso/Suelo
        TileBase sueloTile = sueloTilemap.GetTile(cellPos);
        Debug.Log("Tile encontrado en suelo: " + (sueloTile != null ? sueloTile.name : "NULL"));
        
        if (tileTierraCultivable != null)
        {
            Debug.Log("Tile esperado (cultivable): " + tileTierraCultivable.name);
        }
        else
        {
            Debug.LogWarning("⚠️ Tile Tierra Cultivable NO ASIGNADO - usa 'Auto-Configurar' del Inspector");
        }

        // B. Validar si la tierra es cultivable
        bool puedesPlantar = false;
        
        if (tileTierraCultivable == null)
        {
            // Si no hay tile asignado, intentar auto-configurar con el tile encontrado
            Debug.LogWarning("🔧 Auto-asignando el tile encontrado...");
            if (sueloTile != null && sueloTile is Tile)
            {
                tileTierraCultivable = sueloTile as Tile;
                Debug.Log("✅ Tile auto-asignado: " + tileTierraCultivable.name);
                puedesPlantar = true;
            }
        }
        else if (sueloTile != null && sueloTile.name == tileTierraCultivable.name)
        {
            // Coincidencia por nombre (para manejar instancias diferentes)
            puedesPlantar = true;
            Debug.Log("✅ Tile válido por nombre: " + sueloTile.name);
        }
        else if (sueloTile == tileTierraCultivable)
        {
            // Coincidencia directa de referencia
            puedesPlantar = true;
            Debug.Log("✅ Tile válido por referencia");
        }
        else if (sueloTile != null)
        {
            // **NUEVA FUNCIONALIDAD**: Auto-actualizar si encontramos un tile similar
            Debug.LogWarning("🔄 Tile diferente detectado, intentando auto-actualizar...");
            Debug.Log("  - Encontrado: '" + sueloTile.name + "'");
            Debug.Log("  - Configurado: '" + tileTierraCultivable.name + "'");
            
            // Si los nombres son similares (contienen palabras clave similares), usar el encontrado
            if (sueloTile.name.Contains("Piskel") && tileTierraCultivable.name.Contains("Piskel"))
            {
                Debug.LogWarning("🎯 Tiles similares detectados, actualizando configuración...");
                tileTierraCultivable = sueloTile as Tile;
                if (tileTierraCultivable != null)
                {
                    puedesPlantar = true;
                    Debug.Log("✅ Configuración actualizada a: " + tileTierraCultivable.name);
                }
            }
        }
        
        if (!puedesPlantar)
        {
            if (sueloTile == null)
            {
                Debug.Log("❌ No hay tile en esta posición - haz clic en el piso pintado");
            }
            else
            {
                Debug.Log("❌ No se puede plantar aquí - Razones:");
                Debug.Log("  - Suelo encontrado: " + sueloTile.name + " (Tipo: " + sueloTile.GetType().Name + ")");
                if (tileTierraCultivable != null)
                {
                    Debug.Log("  - Suelo requerido: " + tileTierraCultivable.name + " (Tipo: " + tileTierraCultivable.GetType().Name + ")");
                }
                Debug.LogWarning("💡 SUGERENCIA: Usa 'Auto-Configurar - Detectar Tile del Suelo' en el Inspector");
            }
            return;
        }

        // C. Validar si la celda ya tiene un cultivo
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            Debug.Log("❌ Ya hay algo plantado aquí en celda: " + cellPos);
            CultivoData existente = cultivosPlantados[cellPos];
            Debug.Log("  - Tipo existente: " + existente.tipoCultivo);
            Debug.Log("  - Etapa actual: " + existente.etapaActual);
            return;
        }

        // D. Verificar que el tilemap de cultivos no tenga nada
        TileBase cultivoExistente = cultivosTilemap.GetTile(cellPos);
        if (cultivoExistente != null)
        {
            Debug.LogWarning("⚠️ ADVERTENCIA: Hay un tile en el tilemap de cultivos pero no en los datos!");
            Debug.Log("  - Tile encontrado: " + cultivoExistente.name);
            Debug.Log("  - Limpiando tile para plantar...");
            cultivosTilemap.SetTile(cellPos, null);
        }

        // E. ¡PLANTAR!
        Debug.Log("✅ PLANTANDO en celda: " + cellPos);
        
        // 1. Crear el objeto de datos del cultivo
        CultivoData nuevoCultivo = new CultivoData
        {
            posicionCelda = cellPos,
            tipoCultivo = tipo,
            etapaActual = 0, // Semilla
            tiempoPlantado = Time.time // Registrar el tiempo actual
        };

        Debug.Log("  - Datos del cultivo creados:");
        Debug.Log("    * Posición: " + nuevoCultivo.posicionCelda);
        Debug.Log("    * Tipo: " + nuevoCultivo.tipoCultivo);
        Debug.Log("    * Etapa: " + nuevoCultivo.etapaActual);
        Debug.Log("    * Tiempo plantado: " + nuevoCultivo.tiempoPlantado);

        // 2. Guardar los datos en el diccionario
        cultivosPlantados.Add(cellPos, nuevoCultivo);
        Debug.Log("  - Cultivo agregado al diccionario. Total cultivos: " + cultivosPlantados.Count);

        // 3. Colocar el Tile de Semilla en el Tilemap de Cultivos
        if (tilesDeCrecimiento[0] != null)
        {
            cultivosTilemap.SetTile(cellPos, tilesDeCrecimiento[0]);
            Debug.Log("  - Tile de semilla colocado: " + tilesDeCrecimiento[0].name);
            
            // 🔧 SOLUCIÓN: Asegurar que el tilemap de cultivos esté encima del suelo
            TilemapRenderer cultivosRenderer = cultivosTilemap.GetComponent<TilemapRenderer>();
            TilemapRenderer sueloRenderer = sueloTilemap.GetComponent<TilemapRenderer>();
            
            if (cultivosRenderer != null && sueloRenderer != null)
            {
                // Asegurar que los cultivos tengan un sorting order mayor
                if (cultivosRenderer.sortingOrder <= sueloRenderer.sortingOrder)
                {
                    cultivosRenderer.sortingOrder = sueloRenderer.sortingOrder + 1;
                    Debug.LogWarning("🔧 Sorting Order ajustado:");
                    Debug.Log("  - Suelo: " + sueloRenderer.sortingOrder);
                    Debug.Log("  - Cultivos: " + cultivosRenderer.sortingOrder);
                }
                
                // Verificar Sorting Layers también
                if (cultivosRenderer.sortingLayerName != sueloRenderer.sortingLayerName)
                {
                    Debug.Log("📋 Sorting Layers:");
                    Debug.Log("  - Suelo: " + sueloRenderer.sortingLayerName);
                    Debug.Log("  - Cultivos: " + cultivosRenderer.sortingLayerName);
                }
            }
            
            // Verificar que se colocó correctamente
            TileBase verificacion = cultivosTilemap.GetTile(cellPos);
            if (verificacion != null)
            {
                Debug.Log("  - ✅ Verificación: Tile colocado correctamente: " + verificacion.name);
                
                // 🔧 Verificar posición Z del tilemap
                Vector3 posSuelo = sueloTilemap.transform.position;
                Vector3 posCultivos = cultivosTilemap.transform.position;
                
                Debug.Log("📍 Posiciones Z:");
                Debug.Log("  - Suelo Z: " + posSuelo.z);
                Debug.Log("  - Cultivos Z: " + posCultivos.z);
                
                // Si los cultivos están detrás, ajustar
                if (posCultivos.z >= posSuelo.z)
                {
                    cultivosTilemap.transform.position = new Vector3(posCultivos.x, posCultivos.y, posSuelo.z - 0.1f);
                    Debug.LogWarning("🔧 Posición Z ajustada para cultivos: " + (posSuelo.z - 0.1f));
                }
            }
            else
            {
                Debug.LogError("  - ❌ ERROR: No se pudo colocar el tile!");
            }
        }
        else
        {
            Debug.LogError("❌ ERROR: tilesDeCrecimiento[0] es NULL!");
        }
        
        Debug.Log("🎉 PLANTACIÓN COMPLETADA!");
    }
    // ... (continúa con el paso 5)// ... (continuación de CultivoManager.cs)
    
    private void ManejarCrecimiento()
    {
        // Solo mostrar debug cada 5 segundos para no saturar la consola
        bool mostrarDebug = Time.time % 5f < 0.1f;
        
        if (mostrarDebug && cultivosPlantados.Count > 0)
        {
            Debug.Log("🌿 REVISANDO CRECIMIENTO - Cultivos plantados: " + cultivosPlantados.Count);
        }
        
        // Crear una lista temporal para evitar errores al modificar el diccionario mientras iteramos
        List<Vector3Int> celdasAActualizar = new List<Vector3Int>(cultivosPlantados.Keys);

        foreach (Vector3Int cellPos in celdasAActualizar)
        {
            CultivoData data = cultivosPlantados[cellPos];
            float tiempoTranscurrido = Time.time - data.tiempoPlantado;
            
            if (mostrarDebug)
            {
                Debug.Log("  - Cultivo en " + cellPos + ":");
                Debug.Log("    * Tipo: " + data.tipoCultivo);
                Debug.Log("    * Etapa actual: " + data.etapaActual + "/9");
                Debug.Log("    * Tiempo transcurrido: " + tiempoTranscurrido.ToString("F1") + "s");
                Debug.Log("    * Tiempo por etapa: " + CultivoData.TIEMPO_POR_ETAPA + "s");
            }

            // Calcular en qué etapa debería estar basado en el tiempo
            int etapaObjetivo = Mathf.FloorToInt(tiempoTranscurrido / CultivoData.TIEMPO_POR_ETAPA);
            etapaObjetivo = Mathf.Clamp(etapaObjetivo, 0, 9); // Máximo 9 (índice del último tile)
            
            // Si necesita avanzar etapas
            if (data.etapaActual < etapaObjetivo && data.etapaActual < 9)
            {
                // Avanzar a la siguiente etapa
                data.etapaActual++;
                
                // Verificar que tenemos el tile para esta etapa
                if (data.etapaActual < tilesDeCrecimiento.Length && tilesDeCrecimiento[data.etapaActual] != null)
                {
                    // Cambiar el Tile en el Tilemap
                    cultivosTilemap.SetTile(cellPos, tilesDeCrecimiento[data.etapaActual]);
                    
                    if (data.etapaActual == 9)
                    {
                        Debug.Log("🌾 Cultivo MADURO en " + cellPos + " (etapa " + data.etapaActual + "/9)");
                    }
                    else
                    {
                        Debug.Log("🌱➡️🌿 Cultivo en " + cellPos + " avanzó a etapa " + data.etapaActual + "/9");
                    }
                }
                else
                {
                    Debug.LogError("❌ ERROR: No hay tile para la etapa " + data.etapaActual + "!");
                }
            }
        }
    }
    
    // Función de ejemplo para Cosechar (puedes llamarla desde una interacción del jugador)
    public void CosecharCultivo(Vector3Int cellPos)
    {
        Debug.Log("🧺 INTENTAR COSECHAR en celda: " + cellPos);
        
        if (cultivosPlantados.ContainsKey(cellPos))
        {
            CultivoData data = cultivosPlantados[cellPos];
            
            Debug.Log("  - Cultivo encontrado:");
            Debug.Log("    * Tipo: " + data.tipoCultivo);
            Debug.Log("    * Etapa: " + data.etapaActual + "/9");
            Debug.Log("    * Progreso: " + (data.ObtenerProgreso() * 100f).ToString("F1") + "%");
            Debug.Log("    * ¿Está maduro? " + data.EstaMaduro());
            
            if (data.EstaMaduro())
            {
                // 1. Eliminar el Tile
                cultivosTilemap.SetTile(cellPos, null); 
                Debug.Log("  - Tile eliminado del tilemap");
                
                // 2. Eliminar de los datos
                cultivosPlantados.Remove(cellPos); 
                Debug.Log("  - Datos eliminados del diccionario");
                
                // 3. ¡Dar recompensa al jugador!
                int recompensa = 9 - data.etapaActual + 1; // Más recompensa si está más maduro
                Debug.Log("✅ Cosechaste un cultivo de " + data.tipoCultivo + " en " + cellPos);
                Debug.Log("🎉 ¡Recompensa obtenida! Puntos: " + recompensa + " (implementar RecompensaManager)");
            }
            else
            {
                Debug.Log("❌ Este cultivo aún no está maduro. Etapa actual: " + data.etapaActual + "/9");
                float tiempoRestante = data.TiempoRestanteParaMadurez(Time.time);
                Debug.Log("  - Tiempo restante: " + tiempoRestante.ToString("F1") + " segundos");
                Debug.Log("  - Progreso: " + (data.ObtenerProgreso() * 100f).ToString("F1") + "%");
            }
        }
        else
        {
            Debug.Log("❌ No hay ningún cultivo en esta celda: " + cellPos);
            
            // Verificar si hay algo visualmente pero no en los datos
            TileBase tileVisual = cultivosTilemap.GetTile(cellPos);
            if (tileVisual != null)
            {
                Debug.LogWarning("⚠️ INCONSISTENCIA: Hay un tile visual (" + tileVisual.name + ") pero no datos!");
            }
        }
    }
    
    // 🔍 MÉTODO DE DEBUG MANUAL - Para probar desde el Inspector o llamar en código
    [ContextMenu("Debug - Mostrar Estado Completo")]
    public void DebugMostrarEstadoCompleto()
    {
        Debug.Log("🔍 === DEBUG CULTIVO MANAGER ===");
        Debug.Log("📊 REFERENCIAS:");
        Debug.Log("  - Camera: " + (mainCamera != null ? "✅" : "❌"));
        Debug.Log("  - Suelo Tilemap: " + (sueloTilemap != null ? "✅ " + sueloTilemap.name : "❌"));
        Debug.Log("  - Cultivos Tilemap: " + (cultivosTilemap != null ? "✅ " + cultivosTilemap.name : "❌"));
        Debug.Log("  - Tile Cultivable: " + (tileTierraCultivable != null ? "✅ " + tileTierraCultivable.name : "❌"));
        Debug.Log("  - Tiles Crecimiento: " + (tilesDeCrecimiento != null ? tilesDeCrecimiento.Length + " tiles" : "❌"));
        
        Debug.Log("🌱 CULTIVOS PLANTADOS: " + cultivosPlantados.Count);
        if (cultivosPlantados.Count > 0)
        {
            foreach (var kvp in cultivosPlantados)
            {
                CultivoData data = kvp.Value;
                float tiempoTranscurrido = Time.time - data.tiempoPlantado;
                Debug.Log("  - " + kvp.Key + ": " + data.tipoCultivo + " | Etapa: " + data.etapaActual + " | Tiempo: " + tiempoTranscurrido.ToString("F1") + "s");
            }
        }
        Debug.Log("=== FIN DEBUG ===");
    }
    
    // 🛠️ MÉTODO PARA CONFIGURAR AUTOMÁTICAMENTE EL TILE CULTIVABLE
    [ContextMenu("Auto-Configurar - Detectar Tile del Suelo")]
    public void AutoConfigurarTileCultivable()
    {
        Debug.LogWarning("🔍 === AUTO-CONFIGURACIÓN INICIADA ===");
        
        if (sueloTilemap == null)
        {
            Debug.LogError("❌ Asigna el Suelo Tilemap primero!");
            return;
        }
        
        // Buscar tiles en un área más amplia
        TileBase tileEncontrado = null;
        Vector3Int posicionEncontrada = Vector3Int.zero;
        
        Debug.Log("🔍 Buscando tiles en el tilemap...");
        
        // Buscar en los límites del tilemap
        BoundsInt bounds = sueloTilemap.cellBounds;
        Debug.Log("📏 Límites del tilemap: " + bounds);
        
        for (int x = bounds.xMin; x < bounds.xMax && tileEncontrado == null; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax && tileEncontrado == null; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = sueloTilemap.GetTile(pos);
                
                if (tile != null)
                {
                    tileEncontrado = tile;
                    posicionEncontrada = pos;
                    Debug.Log("✅ Primer tile encontrado: '" + tile.name + "' en posición " + pos);
                    break;
                }
            }
        }
        
        if (tileEncontrado != null)
        {
            // Auto-asignar el tile encontrado
            if (tileEncontrado is Tile)
            {
                tileTierraCultivable = tileEncontrado as Tile;
                
                Debug.LogWarning("🎉 CONFIGURACIÓN AUTOMÁTICA EXITOSA!");
                Debug.Log("  - Tile Cultivable configurado: " + tileTierraCultivable.name);
                Debug.Log("  - Encontrado en posición: " + posicionEncontrada);
                Debug.Log("  - ¡Ahora puedes plantar en tiles de este tipo!");
                
                // Probar plantación inmediatamente
                Debug.Log("🌱 Probando plantación automática...");
                IntentarPlantar(posicionEncontrada, "AutoTest");
            }
            else
            {
                Debug.LogError("❌ El tile encontrado no es de tipo 'Tile', es: " + tileEncontrado.GetType().Name);
                Debug.LogWarning("   El tile '" + tileEncontrado.name + "' no es compatible");
                Debug.LogWarning("   Intenta usar un tile diferente o convertirlo a tipo 'Tile'");
            }
        }
        else
        {
            Debug.LogError("❌ No se encontraron tiles en el tilemap!");
            Debug.LogError("   Asegúrate de haber pintado tiles con el Tile Palette");
            Debug.LogError("   O verifica que el Suelo Tilemap sea el correcto");
        }
        
        Debug.LogWarning("🔍 === AUTO-CONFIGURACIÓN TERMINADA ===");
    }
    
    // 🗺️ MÉTODO PARA MOSTRAR MAPA DE TILES
    [ContextMenu("Debug - Mostrar Mapa de Tiles")]
    public void MostrarMapaTiles()
    {
        Debug.LogWarning("🗺️ === MAPA DE TILES ===");
        
        if (sueloTilemap == null)
        {
            Debug.LogError("❌ Suelo Tilemap no asignado!");
            return;
        }
        
        // Encontrar límites del tilemap
        BoundsInt bounds = sueloTilemap.cellBounds;
        Debug.Log("🌍 Límites del tilemap: " + bounds);
        
        int tilesEncontrados = 0;
        
        // Recorrer solo el área con tiles
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = sueloTilemap.GetTile(pos);
                
                if (tile != null)
                {
                    tilesEncontrados++;
                    if (tilesEncontrados <= 10) // Solo mostrar los primeros 10
                    {
                        Vector3 worldPos = sueloTilemap.GetCellCenterWorld(pos);
                        Debug.Log("📍 Tile #" + tilesEncontrados + ": '" + tile.name + "' en celda " + pos + " (mundo: " + worldPos + ")");
                    }
                }
            }
        }
        
        Debug.Log("📊 Total de tiles encontrados: " + tilesEncontrados);
        
        if (tilesEncontrados == 0)
        {
            Debug.LogError("❌ No se encontraron tiles! Verifica que:");
            Debug.LogError("  1. Hayas pintado tiles con Tile Palette");
            Debug.LogError("  2. El Suelo Tilemap sea el correcto");
            Debug.LogError("  3. Los tiles estén en el layer correcto");
        }
        else
        {
            Debug.LogWarning("💡 Haz clic derecho cerca de las posiciones mostradas arriba");
        }
        
        Debug.LogWarning("🗺️ === FIN MAPA ===");
    }
    
    // 🔧 MÉTODO PARA CONFIGURAR ORDENAMIENTO DE TILEMAPS
    [ContextMenu("Configurar - Ordenamiento de Tilemaps")]
    public void ConfigurarOrdenamientoTilemaps()
    {
        Debug.LogWarning("🔧 === CONFIGURANDO ORDENAMIENTO ===");
        
        if (sueloTilemap == null || cultivosTilemap == null)
        {
            Debug.LogError("❌ Asigna ambos tilemaps primero!");
            return;
        }
        
        TilemapRenderer sueloRenderer = sueloTilemap.GetComponent<TilemapRenderer>();
        TilemapRenderer cultivosRenderer = cultivosTilemap.GetComponent<TilemapRenderer>();
        
        if (sueloRenderer == null || cultivosRenderer == null)
        {
            Debug.LogError("❌ Falta TilemapRenderer en uno de los tilemaps!");
            return;
        }
        
        Debug.Log("📋 Estado actual:");
        Debug.Log("  - Suelo Sorting Layer: " + sueloRenderer.sortingLayerName);
        Debug.Log("  - Suelo Sorting Order: " + sueloRenderer.sortingOrder);
        Debug.Log("  - Cultivos Sorting Layer: " + cultivosRenderer.sortingLayerName);
        Debug.Log("  - Cultivos Sorting Order: " + cultivosRenderer.sortingOrder);
        
        // Configurar sorting layers iguales
        cultivosRenderer.sortingLayerName = sueloRenderer.sortingLayerName;
        
        // Asegurar que cultivos estén encima
        cultivosRenderer.sortingOrder = sueloRenderer.sortingOrder + 1;
        
        // Configurar posiciones Z correctas
        Vector3 posSuelo = sueloTilemap.transform.position;
        Vector3 posCultivos = cultivosTilemap.transform.position;
        
        // Cultivos más adelante (Z menor para 2D)
        cultivosTilemap.transform.position = new Vector3(posCultivos.x, posCultivos.y, posSuelo.z - 0.1f);
        
        Debug.LogWarning("✅ Configuración aplicada:");
        Debug.Log("  - Suelo Sorting Order: " + sueloRenderer.sortingOrder);
        Debug.Log("  - Cultivos Sorting Order: " + cultivosRenderer.sortingOrder);
        Debug.Log("  - Suelo Z: " + sueloTilemap.transform.position.z);
        Debug.Log("  - Cultivos Z: " + cultivosTilemap.transform.position.z);
        
        Debug.LogWarning("🎉 ¡Los cultivos ahora deberían aparecer encima del suelo!");
        Debug.LogWarning("🔧 === CONFIGURACIÓN TERMINADA ===");
    }
    
    // 🔍 MÉTODO DE DEBUG SIMPLIFICADO - Para ver qué falla
    [ContextMenu("Debug - Test Completo")]
    public void TestCompleto()
    {
        Debug.Log("🔍 === TEST COMPLETO INICIADO ===");
        
        // 1. Verificar referencias básicas
        if (sueloTilemap == null) { Debug.LogError("❌ sueloTilemap es NULL!"); return; }
        if (cultivosTilemap == null) { Debug.LogError("❌ cultivosTilemap es NULL!"); return; }
        if (tileTierraCultivable == null) { Debug.LogError("❌ tileTierraCultivable es NULL!"); return; }
        if (tilesDeCrecimiento == null || tilesDeCrecimiento.Length == 0) { Debug.LogError("❌ tilesDeCrecimiento vacío!"); return; }
        
        Debug.Log("✅ Todas las referencias están asignadas");
        
        // 2. BUSCAR DONDE SÍ HAY TILES CULTIVABLES
        Debug.LogWarning("🔍 Buscando tiles cultivables en el mundo...");
        bool encontradoTileCultivable = false;
        
        // Buscar en un área de 50x50 alrededor del origen
        for (int x = -25; x <= 25; x++)
        {
            for (int y = -25; y <= 25; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = sueloTilemap.GetTile(pos);
                
                if (tile == tileTierraCultivable)
                {
                    Debug.Log("✅ ENCONTRADO TILE CULTIVABLE en posición: " + pos);
                    encontradoTileCultivable = true;
                    
                    // Probar plantar aquí
                    Debug.Log("🌱 Intentando plantar en esta posición...");
                    IntentarPlantar(pos, "TestZanahoria");
                    break;
                }
                else if (tile != null)
                {
                    // Solo mostrar los primeros 5 tiles diferentes encontrados
                    if (x % 10 == 0 && y % 10 == 0)
                    {
                        Debug.Log("Tile diferente en " + pos + ": " + tile.name);
                    }
                }
            }
            if (encontradoTileCultivable) break;
        }
        
        if (!encontradoTileCultivable)
        {
            Debug.LogError("❌ NO SE ENCONTRÓ NINGÚN TILE CULTIVABLE!");
            Debug.LogError("Necesitas colocar tiles '" + tileTierraCultivable.name + "' en el tilemap de suelo");
            Debug.LogError("O cambiar la referencia 'Tile Tierra Cultivable' por un tile que SÍ exista");
        }
        
        Debug.Log("🔍 === TEST COMPLETO TERMINADO ===");
    }
}