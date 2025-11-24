using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControladorZombies : MonoBehaviour
{
    [Header("🧟 CONFIGURACIÓN DE SPAWN")]
    [SerializeField] private GameObject prefabZombie; // Prefab del zombie
    [SerializeField] private int cantidadZombies = 10; // Cantidad de zombies a generar
    [SerializeField] private float rangoSpawn = 10f; // Radio de spawn alrededor del controlador
    [SerializeField] private float tiempoEntreSpawns = 0.2f; // Tiempo entre cada spawn
    [SerializeField] private bool spawnearAlInicio = true; // Spawear automáticamente al comenzar
    [SerializeField] private LayerMask capaSuelo = -1; // Para verificar que los zombies spaween en el suelo (por defecto todos los layers)
    [SerializeField] private float alturaRaycast = 10f; // Altura desde donde buscar el suelo
    [SerializeField] private float distanciaRaycast = 20f; // Distancia del raycast hacia abajo
    [SerializeField] private float offsetSuelo = 1f; // Distancia sobre el suelo para spawner
    
    [Header("🎯 CONFIGURACIÓN DE ZOMBIES")]
    [SerializeField] private int saludZombieBase = 30; // Valor base
    [SerializeField] private float velocidadZombieBase = 2.5f; // Valor base
    [SerializeField] private int dañoZombieBase = 20; // Valor base
    
    // Variables calculadas según dificultad
    private int saludZombieActual;
    private float velocidadZombieActual;
    private int dañoZombieActual;
    
    [Header("🔄 SISTEMA DE RESPAWN")]
    [SerializeField] private bool respawnActivado = false;
    [SerializeField] private float tiempoRespawn = 5f; // Tiempo para respawnear zombies muertos
    [SerializeField] private int maximoZombiesVivos = 10; // Máximo de zombies vivos al mismo tiempo
    
    // Variables internas
    private List<GameObject> zombiesVivos = new List<GameObject>();
    private Transform jugador;
    private int zombiesGenerados = 0;
    private bool estaSpawneando = false;
    
    void Start()
    {
        Debug.LogError("🧟 INICIANDO CONTROLADOR DE ZOMBIES");
        
        // 🔧 VERIFICAR ESCENA PARA CONFIGURACIÓN ESPECIAL
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esEscena1 = escenaActual.Contains("Escena1") || escenaActual.Contains("1");
        
        if (esEscena1)
        {
            Debug.LogError("🎯 ESCENA1 DETECTADA - Reduciendo zombies para evitar lag");
            
            // Reducir cantidad de zombies en Escena1
            cantidadZombies = Mathf.Min(cantidadZombies, 5); // Máximo 5 zombies
            tiempoEntreSpawns = 0.5f; // Más tiempo entre spawns
            
            Debug.LogError($"🔧 CONFIGURACIÓN AJUSTADA: {cantidadZombies} zombies | Intervalo: {tiempoEntreSpawns}s");
        }
        
        // Usar valores base directamente (SIN configuración de dificultad)
        saludZombieActual = saludZombieBase;
        velocidadZombieActual = velocidadZombieBase;
        dañoZombieActual = dañoZombieBase;
        
        Debug.LogError("🎯 VALORES DE ZOMBIES (FIJOS):");
        Debug.LogError($"  - Vida zombie: {saludZombieActual}");
        Debug.LogError($"  - Velocidad zombie: {velocidadZombieActual:F1}");
        Debug.LogError($"  - Daño zombie: {dañoZombieActual}");
        
        // Verificar configuración inicial
        VerificarConfiguracion();
        
        // Buscar al jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
            Debug.LogError("✅ Jugador encontrado: " + jugadorObj.name);
        }
        else
        {
            Debug.LogError("❌ ControladorZombies: No se encontró jugador con tag 'Player'");
        }
        
        // Verificar que tenemos el prefab
        if (prefabZombie == null)
        {
            Debug.LogError("❌ ControladorZombies: No se ha asignado el prefab del zombie!");
            return;
        }
        
        Debug.LogError("📋 Configuración: " + cantidadZombies + " zombies, rango " + rangoSpawn + ", capa suelo: " + capaSuelo.value);
        
        // Spawear zombies al inicio si está activado
        if (spawnearAlInicio)
        {
            // Pausa más larga en Escena1
            float pausaInicial = esEscena1 ? 2f : 0.1f;
            Invoke("SpawnearZombies", pausaInicial);
        }
        
        // Iniciar sistema de respawn si está activado
        if (respawnActivado)
        {
            InvokeRepeating(nameof(VerificarRespawn), tiempoRespawn, tiempoRespawn);
        }
    }
    
    private void VerificarConfiguracion()
    {
        Debug.LogError("🔍 VERIFICANDO CONFIGURACIÓN:");
        Debug.LogError("  - Prefab Zombie: " + (prefabZombie != null ? prefabZombie.name : "NULL"));
        Debug.LogError("  - Cantidad: " + cantidadZombies);
        Debug.LogError("  - Rango spawn: " + rangoSpawn);
        Debug.LogError("  - Capa suelo: " + capaSuelo.value);
        Debug.LogError("  - Spawn al inicio: " + spawnearAlInicio);
        
        if (capaSuelo.value == 0)
        {
            Debug.LogWarning("⚠️ ADVERTENCIA: capaSuelo está en 0 (Nothing). Esto puede causar problemas de spawn.");
        }
    }
    
    void Update()
    {
        // 🔧 REDUCIR FRECUENCIA DE UPDATE EN ESCENA1
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esEscena1 = escenaActual.Contains("Escena1") || escenaActual.Contains("1");
        
        // En Escena1, procesar cada 5 frames
        if (esEscena1 && Time.frameCount % 5 != 0)
        {
            return;
        }
        
        // Limpiar la lista de zombies muertos
        LimpiarZombiesMuertos();
        
        // Debug info
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MostrarInfoZombies();
        }
        
        // Spawear más zombies con tecla (para pruebas)
        if (Input.GetKeyDown(KeyCode.X))
        {
            SpawnearZombies();
        }
        
        // Forzar spawn completo con tecla C (para pruebas)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ForzarSpawnCompleto();
        }
    }
    
    // 🧟 MÉTODO PRINCIPAL PARA SPAWEAR ZOMBIES
    public void SpawnearZombies()
    {
        if (estaSpawneando)
        {
            Debug.LogWarning("⚠️ Ya se están spawneando zombies, cancelando petición duplicada...");
            return;
        }
        
        if (prefabZombie == null)
        {
            Debug.LogError("❌ No se puede spawnear: prefab es null!");
            return;
        }
        
        // Detener cualquier spawn anterior
        StopAllCoroutines();
        
        Debug.LogError("🧟 INICIANDO NUEVO CICLO DE SPAWN - " + cantidadZombies + " zombies con " + tiempoEntreSpawns + "s entre cada uno");
        StartCoroutine(SpawnearZombiesCorrutina());
    }
    
    private IEnumerator SpawnearZombiesCorrutina()
    {
        estaSpawneando = true;
        int zombiesSpawneadosEnEsteIntento = 0;
        
        Debug.LogError("🧟 INICIANDO SPAWN DE " + cantidadZombies + " ZOMBIES...");
        Debug.LogError("⏱️ Tiempo entre spawns: " + tiempoEntreSpawns + " segundos");
        
        for (int i = 0; i < cantidadZombies; i++)
        {
            Debug.LogError("🔄 Spawneando zombie " + (i + 1) + "/" + cantidadZombies);
            
            bool spawned = SpawnearZombieIndividual();
            if (spawned)
            {
                zombiesSpawneadosEnEsteIntento++;
                Debug.LogError("✅ Zombie " + (i + 1) + " spawneado exitosamente!");
            }
            else
            {
                Debug.LogError("❌ Fallo al spawnear zombie " + (i + 1));
            }
            
            // Solo esperar si no es el último zombie
            if (i < cantidadZombies - 1)
            {
                Debug.LogError("⏳ Esperando " + tiempoEntreSpawns + " segundos...");
                yield return new WaitForSeconds(tiempoEntreSpawns);
            }
        }
        
        estaSpawneando = false;
        Debug.LogError("✅ SPAWN COMPLETADO! " + zombiesSpawneadosEnEsteIntento + "/" + cantidadZombies + " zombies spawneados en este intento.");
        Debug.LogError("📊 Total zombies vivos: " + zombiesVivos.Count);
    }
    
    private bool SpawnearZombieIndividual()
    {
        Vector3 posicionSpawn = CalcularPosicionSpawn();
        
        if (posicionSpawn == Vector3.zero)
        {
            Debug.LogWarning("⚠️ No se pudo encontrar posición válida para spawner zombie #" + (zombiesGenerados + 1));
            return false;
        }
        
        Debug.LogError("🧟 Intentando spawnear zombie #" + (zombiesGenerados + 1) + " en: " + posicionSpawn);
        
        GameObject nuevoZombie = Instantiate(prefabZombie, posicionSpawn, Quaternion.identity);
        
        if (nuevoZombie == null)
        {
            Debug.LogError("❌ Error al instanciar el prefab del zombie!");
            return false;
        }
        
        ConfigurarZombie(nuevoZombie);
        zombiesVivos.Add(nuevoZombie);
        zombiesGenerados++;
        
        Debug.LogError("✅ Zombie #" + zombiesGenerados + " spawneado exitosamente en: " + nuevoZombie.transform.position);
        return true;
    }
    
    // 🔄 MÉTODO PARA FORZAR SPAWN DE CANTIDAD EXACTA
    public void ForzarSpawnCompleto()
    {
        Debug.LogError("💪 FORZANDO SPAWN COMPLETO DE " + cantidadZombies + " ZOMBIES...");
        
        // Detener cualquier proceso anterior
        StopAllCoroutines();
        estaSpawneando = false;
        
        StartCoroutine(ForzarSpawnCompletoCorrutina());
    }
    
    private IEnumerator ForzarSpawnCompletoCorrutina()
    {
        estaSpawneando = true;
        int intentosMaximos = cantidadZombies * 3; // Más intentos para asegurar todos los spawns
        int zombiesSpawneados = 0;
        int intentos = 0;
        
        while (zombiesSpawneados < cantidadZombies && intentos < intentosMaximos)
        {
            bool spawned = SpawnearZombieIndividual();
            if (spawned)
            {
                zombiesSpawneados++;
                Debug.LogError("✅ Progreso: " + zombiesSpawneados + "/" + cantidadZombies);
                
                // Pausa más corta para spawn forzado
                yield return new WaitForSeconds(Mathf.Min(tiempoEntreSpawns, 0.1f));
            }
            else
            {
                // Pausa mínima en caso de fallo
                yield return new WaitForSeconds(0.05f);
            }
            
            intentos++;
        }
        
        estaSpawneando = false;
        
        if (zombiesSpawneados >= cantidadZombies)
        {
            Debug.LogError("🎉 ¡SPAWN FORZADO EXITOSO! Todos los " + cantidadZombies + " zombies spawneados.");
        }
        else
        {
            Debug.LogWarning("⚠️ Spawn forzado incompleto: " + zombiesSpawneados + "/" + cantidadZombies + " después de " + intentos + " intentos.");
        }
    }
    
    private Vector3 CalcularPosicionSpawn()
    {
        int intentos = 0;
        int maxIntentos = 50; // Más intentos
        
        while (intentos < maxIntentos)
        {
            // Generar posición aleatoria alrededor del controlador en 2D
            float anguloAleatorio = Random.Range(0f, 360f);
            float distanciaAleatoria = Random.Range(3f, rangoSpawn);
            
            // Cálculo correcto para 2D (X, Y en lugar de X, Z)
            Vector2 direccion = new Vector2(
                Mathf.Cos(anguloAleatorio * Mathf.Deg2Rad),
                Mathf.Sin(anguloAleatorio * Mathf.Deg2Rad)
            ).normalized;
            
            Vector3 posicionCandidato = transform.position + new Vector3(direccion.x * distanciaAleatoria, 0, 0);
            
            // Raycast hacia abajo desde más arriba para encontrar el suelo
            Vector3 puntoRaycast = new Vector3(posicionCandidato.x, transform.position.y + alturaRaycast, posicionCandidato.z);
            
            // Debug para ver dónde estamos buscando
            Debug.DrawRay(puntoRaycast, Vector2.down * distanciaRaycast, Color.red, 1f);
            
            RaycastHit2D hit = Physics2D.Raycast(puntoRaycast, Vector2.down, distanciaRaycast, capaSuelo);
            
            if (hit.collider != null)
            {
                // Ajustar Y para que esté encima del suelo
                posicionCandidato.y = hit.point.y + offsetSuelo; // Configurable para evitar enterrarse
                
                Debug.LogError("🎯 SUELO ENCONTRADO en Y: " + hit.point.y + " | Zombie en Y: " + posicionCandidato.y + " | Collider: " + hit.collider.name);
                
                // Verificar que no esté demasiado cerca de otros zombies
                bool posicionValida = true;
                foreach (GameObject zombie in zombiesVivos)
                {
                    if (zombie != null && Vector2.Distance(new Vector2(posicionCandidato.x, posicionCandidato.y), 
                                                          new Vector2(zombie.transform.position.x, zombie.transform.position.y)) < 2f)
                    {
                        posicionValida = false;
                        break;
                    }
                }
                
                if (posicionValida)
                {
                    return posicionCandidato;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró suelo en intento " + intentos + " en posición: " + puntoRaycast);
            }
            
            intentos++;
        }
        
        // Si no encontramos posición válida, usar posición del controlador + offset
        Vector3 posicionFallback = transform.position + new Vector3(Random.Range(-2f, 2f), 2f, 0);
        Debug.LogWarning("⚠️ Usando posición fallback: " + posicionFallback);
        return posicionFallback;
    }
    
    private void ConfigurarZombie(GameObject zombie)
    {
        if (zombie == null)
        {
            Debug.LogError("❌ ERROR: zombie es null en ConfigurarZombie");
            return;
        }
        
        // Configurar el componente ControladorEnemigo con valores según dificultad
        ControladorEnemigo controlador = zombie.GetComponent<ControladorEnemigo>();
        if (controlador != null)
        {
            controlador.salud = saludZombieActual;
            controlador.velocidadMovimiento = velocidadZombieActual;
            controlador.daño = dañoZombieActual;
            
            Debug.LogError("⚙️ Zombie configurado según dificultad:");
            Debug.LogError($"  - Salud: {saludZombieActual}");
            Debug.LogError($"  - Velocidad: {velocidadZombieActual:F1}");
            Debug.LogError($"  - Daño: {dañoZombieActual}");
        }
        else
        {
            Debug.LogError("❌ ERROR CRÍTICO: El prefab del zombie no tiene componente ControladorEnemigo!");
        }
        
        // Asegurar que tenga el tag correcto
        if (!zombie.CompareTag("Enemy"))
        {
            zombie.tag = "Enemy";
            Debug.LogError("🏷️ Tag 'Enemy' asignado al zombie");
        }
        
        // Configurar layer si es necesario
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            zombie.layer = enemyLayer;
            Debug.LogError("📋 Layer 'Enemy' asignado al zombie");
        }
        else
        {
            zombie.layer = 0; // Default layer si no existe Enemy
            Debug.LogWarning("⚠️ Layer 'Enemy' no existe, usando default layer");
        }
        
        // Verificar que tenga Rigidbody2D para physics
        Rigidbody2D rb = zombie.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("⚠️ El zombie no tiene Rigidbody2D, puede que no se mueva correctamente");
        }
        else
        {
            // Configurar Rigidbody2D para evitar que se entierren
            rb.freezeRotation = true; // Evitar rotación
            rb.gravityScale = 1f; // Asegurar gravedad
        }
    }
    
    private void LimpiarZombiesMuertos()
    {
        // Eliminar referencias de zombies que ya no existen
        zombiesVivos.RemoveAll(zombie => zombie == null);
    }
    
    private void VerificarRespawn()
    {
        if (zombiesVivos.Count < maximoZombiesVivos && !estaSpawneando)
        {
            int zombiesASpawnear = maximoZombiesVivos - zombiesVivos.Count;
            Debug.LogError("🔄 RESPAWN: Spawneando " + zombiesASpawnear + " zombies para mantener " + maximoZombiesVivos);
            
            StartCoroutine(RespawnZombies(zombiesASpawnear));
        }
    }
    
    private IEnumerator RespawnZombies(int cantidad)
    {
        estaSpawneando = true;
        
        for (int i = 0; i < cantidad; i++)
        {
            SpawnearZombieIndividual();
            yield return new WaitForSeconds(tiempoEntreSpawns);
        }
        
        estaSpawneando = false;
    }
    
    // 📊 MÉTODOS DE INFORMACIÓN Y DEBUG
    public void MostrarInfoZombies()
    {
        Debug.LogError("📊 INFO ZOMBIES:");
        Debug.LogError("  - Zombies vivos: " + zombiesVivos.Count + "/" + cantidadZombies);
        Debug.LogError("  - Zombies generados total: " + zombiesGenerados);
        Debug.LogError("  - Está spawneando: " + estaSpawneando);
        Debug.LogError("  - Respawn activado: " + respawnActivado);
        Debug.LogError("  - Prefab asignado: " + (prefabZombie != null ? "✅" : "❌"));
        Debug.LogError("  - Capa suelo configurada: " + (capaSuelo.value != 0 ? "✅" : "❌"));
        
        // Mostrar posiciones de zombies vivos
        for (int i = 0; i < zombiesVivos.Count; i++)
        {
            if (zombiesVivos[i] != null)
            {
                Debug.LogError("    Zombie " + (i+1) + ": " + zombiesVivos[i].transform.position);
            }
        }
    }
    
    public void DestruirTodosLosZombies()
    {
        foreach (GameObject zombie in zombiesVivos)
        {
            if (zombie != null)
            {
                Destroy(zombie);
            }
        }
        zombiesVivos.Clear();
        Debug.LogError("🧹 Todos los zombies han sido destruidos.");
    }
    
    // 🎮 MÉTODOS PÚBLICOS PARA CONTROL EXTERNO
    public void ActivarRespawn() { respawnActivado = true; }
    public void DesactivarRespawn() { respawnActivado = false; }
    public int GetZombiesVivos() { return zombiesVivos.Count; }
    public bool EstaSpawneando() { return estaSpawneando; }
    
    // 🎨 VISUALIZACIÓN EN EL EDITOR
    private void OnDrawGizmos()
    {
        // Dibujar área de spawn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoSpawn);
        
        // Dibujar área mínima de spawn
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
        
        // Dibujar líneas a cada zombie vivo
        Gizmos.color = Color.green;
        foreach (GameObject zombie in zombiesVivos)
        {
            if (zombie != null)
            {
                Gizmos.DrawLine(transform.position, zombie.transform.position);
                // Pequeño cubo en la posición del zombie
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(zombie.transform.position, Vector3.one * 0.5f);
                Gizmos.color = Color.green;
            }
        }
        
        // Información en el editor
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3, 
                $"🧟 Spawner\\n" +
                $"Vivos: {zombiesVivos.Count}/{cantidadZombies}\\n" +
                $"Total: {zombiesGenerados}\\n" +
                $"Spawneando: {(estaSpawneando ? "SÍ" : "NO")}\\n" +
                $"Prefab: {(prefabZombie != null ? "✅" : "❌")}\\n" +
                $"Suelo: {(capaSuelo.value != 0 ? "✅" : "❌")}");
        }
        
        // Dibujar raycasts de prueba para debug
        if (Application.isPlaying && Input.GetKey(KeyCode.G))
        {
            // Mostrar varios raycasts de prueba
            for (int i = 0; i < 8; i++)
            {
                float angulo = (360f / 8f) * i;
                Vector2 direccion = new Vector2(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad));
                Vector3 inicio = transform.position + new Vector3(direccion.x * 5f, 10f, 0);
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(inicio, Vector2.down * 20f);
            }
        }
    }
    
    // 🔧 MÉTODOS PARA CONFIGURAR EN RUNTIME
    public void SetCantidadZombies(int cantidad) { cantidadZombies = cantidad; }
    public void SetRangoSpawn(float rango) { rangoSpawn = rango; }
    public void SetSaludZombie(int salud) { saludZombieBase = salud; }
    public void SetVelocidadZombie(float velocidad) { velocidadZombieBase = velocidad; }
}