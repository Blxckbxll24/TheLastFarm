using UnityEngine;
using System.Collections;

public class ControladorEnemigo : MonoBehaviour
{
    // Variables Configurables
    [Header("Estadísticas")]
    public int salud = 30;
    public float velocidadMovimiento = 2.5f;
    public int daño = 20;
    
    [Header("Detección del Jugador")]
    public float distanciaDeteccion = 5f;
    public float distanciaAtaque = 1.0f; // REDUCIDO de 1.5f a 1.0f
    public float distanciaParada = 0.3f; // REDUCIDO de 0.5f a 0.3f
    
    [Header("⚡ Configuración de Colisiones")]
    [SerializeField] private bool evitarSuperponer = true;
    [SerializeField] private float radioEvitarZombies = 1f;
    [SerializeField] private float fuerzaRepulsion = 3f;
    [SerializeField] private LayerMask capaZombies;
    [SerializeField] private float tiempoUltimoAtaque = 0f;
    [SerializeField] private float cooldownAtaque = 2f; // AUMENTADO de 1.5f a 2f
    [SerializeField] private bool verificarLineaDeVista = true; // NUEVO: Verificar línea de vista
    [SerializeField] private LayerMask capasObstaculos; // NUEVO: Capas que bloquean ataques
    
    [Header("Animaciones")]
    // Asegúrate de que este Animator esté en el GameObject o un hijo y tiene un Animator Controller
    public Animator animator; 

    [Header("⚡ Sistema de Retroceso")]
    public float fuerzaRetroceso = 8f; // Fuerza del retroceso cuando recibe daño
    public float tiempoRetroceso = 0.5f; // Tiempo que dura el retroceso
    public bool puedeRecibirRetroceso = true; // Si puede ser empujado
    
    // --- Referencias Internas ---
    private Transform jugador; // Para guardar la posición del jugador
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // --- Estados del Enemigo ---
    private bool estaMirandoDerecha = true;
    private bool jugadorDetectado = false;
    private bool estaAtacando = false;
    private bool estaEnRetroceso = false; // 🔥 NUEVO: Estado de retroceso
    private float distanciaAlJugador;

    // Start se llama una vez cuando el juego comienza
    void Start()
    {
        // 1. Obtener referencias
        rb = GetComponent<Rigidbody2D>();
        
        // Buscar SpriteRenderer en el GameObject o sus hijos
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Buscar Animator (si no fue asignado en el Inspector)
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Verificar configuración del Animator
        if (animator != null)
        {
            if (animator.runtimeAnimatorController == null)
            {
               // Debug.LogWarning("Animator encontrado pero SIN Animator Controller en " + gameObject.name + ". Asigna uno para que funcionen las animaciones.");
            }
            else
            {
               // Debug.Log("✓ Animator configurado correctamente con controller: " + animator.runtimeAnimatorController.name);
                
                // Verificar parámetros
                bool tieneMovement = false;
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    //Debug.Log("Parámetro encontrado: " + param.name + " (Tipo: " + param.type + ")");
                    if (param.name == "movement") tieneMovement = true;
                }
                
                if (!tieneMovement)
                {
                   // Debug.LogError("¡FALTA el parámetro 'movement' (Float) en el Animator Controller!");
                }
            }
        }
        else
        {
           // Debug.LogWarning("No se encontró componente Animator en " + gameObject.name + ". Agrégalo si quieres animaciones.");
        }
        
        // --- INICIO DE DIAGNÓSTICO FORZADO PARA VISIBILIDAD Y MANEJADORES ---
        
        // A. FORZAR ESCALA POSITIVA: La escala cero oculta el sprite y los manejadores.
        // Solo la forzamos si es muy pequeña o cero.
        if (Mathf.Abs(transform.localScale.x) < 0.1f || Mathf.Abs(transform.localScale.y) < 0.1f)
        {
            // Usamos el valor absoluto de Z para no afectar la posición de la cámara 
            transform.localScale = new Vector3(1f, 1f, Mathf.Abs(transform.localScale.z));
          //  Debug.LogWarning("ESCALA AJUSTADA: La escala del GameObject era cero o muy pequeña. Se forzó a (1, 1, Z).");
        }

        // B. CONFIGURACIÓN DEL SPRITE VISIBLE
        if (spriteRenderer != null)
        {
            // Asegurar que el sprite esté delante de todo
            spriteRenderer.sortingOrder = 100; // Un número alto lo pone al frente
            
            if (spriteRenderer.sprite == null)
            {
               // Debug.LogError("¡ERROR CRÍTICO! El componente SpriteRenderer NO tiene un sprite (imagen) asignado en el Inspector. ¡Asigna uno!");
            }
            // Forzar Color Alpha (Visibilidad)
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        }
        // --- FIN DE DIAGNÓSTICO FORZADO ---

        // 2. Buscar al jugador por su Tag
        GameObject jugadorObjeto = GameObject.FindGameObjectWithTag("Player");

        if (jugadorObjeto != null)
        {
            jugador = jugadorObjeto.transform;
        }
        else
        {
            //Debug.LogError("¡No se encontró al jugador! Asegúrate de que tenga el Tag 'Player'.");
        }
        
        // Advertencia si falta un componente esencial
      //  if (rb == null) Debug.LogError("¡Falta Rigidbody2D en el enemigo!");
        //if (spriteRenderer == null) Debug.LogError("¡Falta SpriteRenderer! El enemigo no se verá.");
        
        // 🔧 CONFIGURAR LAYER DE ZOMBIES
        if (capaZombies.value == 0)
        {
            capaZombies = LayerMask.GetMask("Enemy"); // Auto-configurar si no está asignado
        }
        
        // 🔧 ASEGURAR QUE EL ZOMBIE TENGA EL LAYER CORRECTO
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        if (gameObject.layer == -1)
        {
            gameObject.layer = 0; // Default si no existe Enemy layer
        }
        
        Debug.LogError("🧟 ZOMBIE INICIALIZADO - Layer: " + LayerMask.LayerToName(gameObject.layer));
    }
    
    // Update se llama en cada fotograma
    void Update()
    {
        if (jugador == null) return;

        // 🔥 NO HACER NADA DURANTE EL RETROCESO
        if (estaEnRetroceso) return;

        // 🚫 EVITAR SUPERPOSICIÓN CON OTROS ZOMBIES
        if (evitarSuperponer)
        {
            EvitarOtrosZombies();
        }

        // Calcular distancia al jugador
        distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        
        // Determinar si el jugador está en rango de detección
        jugadorDetectado = distanciaAlJugador <= distanciaDeteccion;
        
        // --- ESTADO: PERSEGUIR JUGADOR ---
        if (jugadorDetectado && distanciaAlJugador > distanciaParada && !estaAtacando)
        {
            PerseguirJugador();
        }
        // --- ESTADO: ATACAR ---
        else if (jugadorDetectado && distanciaAlJugador <= distanciaAtaque && !estaAtacando && PuedeAtacar())
        {
            IniciarAtaque();
        }
        // --- ESTADO: IDLE/PATRULLAR ---
        else if (!jugadorDetectado)
        {
            Idle();
        }
        // --- ESTADO: MUY CERCA (PARAR) ---
        else if (distanciaAlJugador <= distanciaParada)
        {
            Parar();
        }
        
        // Actualizar animaciones
        ActualizarAnimaciones();
    }
    
    // 🚫 MÉTODO PARA EVITAR QUE LOS ZOMBIES SE SUBAN UNOS SOBRE OTROS
    private void EvitarOtrosZombies()
    {
        Collider2D[] zombiesCercanos = Physics2D.OverlapCircleAll(transform.position, radioEvitarZombies, capaZombies);
        
        Vector2 fuerzaRepulsionTotal = Vector2.zero;
        int zombiesDetectados = 0;
        
        foreach (Collider2D otroZombie in zombiesCercanos)
        {
            if (otroZombie.gameObject != gameObject) // No considerarse a sí mismo
            {
                Vector2 direccionRepulsion = (transform.position - otroZombie.transform.position).normalized;
                float distancia = Vector2.Distance(transform.position, otroZombie.transform.position);
                
                // Fuerza inversamente proporcional a la distancia
                float fuerzaMagnitud = fuerzaRepulsion / Mathf.Max(distancia, 0.1f);
                fuerzaRepulsionTotal += direccionRepulsion * fuerzaMagnitud;
                zombiesDetectados++;
            }
        }
        
        // Aplicar fuerza de repulsión si hay zombies cercanos
        if (zombiesDetectados > 0 && rb != null)
        {
            rb.AddForce(fuerzaRepulsionTotal, ForceMode2D.Force);
            
            if (Time.frameCount % 120 == 0) // Debug ocasional
            {
                Debug.LogError("🚫 SEPARANDO ZOMBIES - Fuerza: " + fuerzaRepulsionTotal.magnitude + " | Zombies cercanos: " + zombiesDetectados);
            }
        }
    }
    
    // 🕒 VERIFICAR SI PUEDE ATACAR (COOLDOWN)
    private bool PuedeAtacar()
    {
        return Time.time >= tiempoUltimoAtaque + cooldownAtaque;
    }
    
    private void PerseguirJugador()
    {
        // Calcula la dirección hacia el jugador
        Vector2 direccion = (jugador.position - transform.position).normalized;

        // Mueve el Rigidbody en esa dirección
        rb.linearVelocity = new Vector2(direccion.x * velocidadMovimiento, rb.linearVelocity.y);
        
        // Girar hacia el jugador
        GirarHacia(direccion.x);
    }
    
    private void Idle()
    {
        // Parar movimiento horizontal
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    private void Parar()
    {
        // Parar movimiento horizontal cuando está muy cerca
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    
    private void IniciarAtaque()
    {
        // VERIFICACIÓN ADICIONAL DE DISTANCIA ANTES DEL ATAQUE
        float distanciaActual = Vector2.Distance(transform.position, jugador.position);
        if (distanciaActual > distanciaAtaque)
        {
            Debug.LogError($"❌ ATAQUE CANCELADO - Distancia {distanciaActual:F2} > {distanciaAtaque}");
            return;
        }
        
        // Parar movimiento durante el ataque
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        if (!estaAtacando && PuedeAtacar())
        {
            estaAtacando = true;
            tiempoUltimoAtaque = Time.time;
            
            Debug.LogError($"🧟 ZOMBIE INICIANDO ATAQUE! Distancia: {distanciaActual:F2}");
            
            // Pequeña pausa antes de aplicar daño para hacer el ataque más predecible
            Invoke("EjecutarAtaque", 0.2f);
            
            // Simular duración del ataque
            Invoke("TerminarAtaque", 1f);
        }
    }
    
    // NUEVO MÉTODO: Ejecutar el ataque después de una pequeña pausa
    private void EjecutarAtaque()
    {
        if (!estaAtacando || jugador == null) return;
        
        // Verificar distancia nuevamente por si el jugador se alejó
        float distanciaFinal = Vector2.Distance(transform.position, jugador.position);
        if (distanciaFinal > distanciaAtaque)
        {
            Debug.LogError($"❌ ATAQUE FALLIDO - Jugador se alejó. Distancia: {distanciaFinal:F2}");
            return;
        }
        
        MovimientoJugador jugadorScript = jugador.GetComponent<MovimientoJugador>();
        if (jugadorScript != null)
        {
            bool puedeHacerDaño = VerificarCondicionesDaño(jugadorScript);
            
            if (puedeHacerDaño)
            {
                jugadorScript.RecibirDaño(daño);
                Debug.LogError($"🧟 ¡ZOMBIE ATACÓ AL JUGADOR! Daño: {daño} | Distancia final: {distanciaFinal:F2}");
            }
            else
            {
                Debug.LogError("🛡️ CONDICIONES DE DAÑO NO CUMPLIDAS EN EJECUCIÓN");
            }
        }
    }
    
    // 🔍 VERIFICAR CONDICIONES PARA HACER DAÑO AL JUGADOR
    private bool VerificarCondicionesDaño(MovimientoJugador jugadorScript)
    {
        // 1. No hacer daño si el jugador es inmune o está atacando
        if (jugadorScript.EsInmune() || jugadorScript.EstaAtacando())
        {
            Debug.LogError("🛡️ JUGADOR INMUNE O ATACANDO - No se aplica daño");
            return false;
        }
        
        // 2. VERIFICAR DISTANCIA EXACTA
        float distanciaReal = Vector2.Distance(transform.position, jugador.position);
        if (distanciaReal > distanciaAtaque)
        {
            Debug.LogError($"📏 DEMASIADO LEJOS - Distancia: {distanciaReal:F2} | Máximo: {distanciaAtaque}");
            return false;
        }
        
        // 3. VERIFICAR LÍNEA DE VISTA (NUEVO)
        if (verificarLineaDeVista)
        {
            Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
            float distanciaLineaVista = Vector2.Distance(transform.position, jugador.position);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, distanciaLineaVista, capasObstaculos);
            
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                Debug.LogError($"🚫 LÍNEA DE VISTA BLOQUEADA por: {hit.collider.name}");
                return false;
            }
            
            // Debug visual de la línea de vista
            Debug.DrawRay(transform.position, direccionAlJugador * distanciaLineaVista, Color.red, 0.1f);
        }
        
        // 4. VERIFICAR POSICIÓN RELATIVA DEL JUGADOR (MEJORADO)
        float diferenciaY = Mathf.Abs(jugador.position.y - transform.position.y);
        float diferenciaX = Mathf.Abs(jugador.position.x - transform.position.x);
        
        // Si el jugador está MUY por encima del zombie (saltando sobre él)
        if (jugador.position.y > transform.position.y + 1.2f)
        {
            Debug.LogError($"🦘 JUGADOR SALTANDO SOBRE ZOMBIE - Diferencia Y: {diferenciaY:F2}");
            return false;
        }
        
        // Si está demasiado lejos horizontalmente
        if (diferenciaX > distanciaAtaque * 0.8f)
        {
            Debug.LogError($"↔️ DEMASIADO LEJOS HORIZONTALMENTE - Diferencia X: {diferenciaX:F2}");
            return false;
        }
        
        // 5. VERIFICAR QUE EL ZOMBIE ESTÉ MIRANDO HACIA EL JUGADOR
        bool jugadorALaDerecha = jugador.position.x > transform.position.x;
        if (jugadorALaDerecha != estaMirandoDerecha)
        {
            Debug.LogError("👀 ZOMBIE NO ESTÁ MIRANDO AL JUGADOR");
            return false;
        }
        
        Debug.LogError($"✅ CONDICIONES DE DAÑO VÁLIDAS - Dist: {distanciaReal:F2} | Dif Y: {diferenciaY:F2} | Dif X: {diferenciaX:F2}");
        return true;
    }
    
    private void TerminarAtaque()
    {
        estaAtacando = false;
    }
    
    private void ActualizarAnimaciones()
    {
        if (animator != null)
        {
            // Verificar que el Animator Controller esté asignado
            if (animator.runtimeAnimatorController == null)
            {
              //  Debug.LogWarning("El Animator no tiene un Animator Controller asignado en " + gameObject.name);
                return;
            }
            
            // Parámetro para velocidad de movimiento (normalizado entre 0 y 1)
            float velocidadX = Mathf.Abs(rb.linearVelocity.x);
            float movementNormalizado = estaEnRetroceso ? 0 : velocidadX / velocidadMovimiento; // No animar durante retroceso
            
            // Debug para ver los valores
            //Debug.Log("Velocidad X: " + velocidadX + " | Movement normalizado: " + movementNormalizado + " | Velocidad máxima: " + velocidadMovimiento);
            
            // Verificar que el parámetro "movement" existe
            bool parametroEncontrado = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "movement")
                {
                    animator.SetFloat("movement", movementNormalizado);
                    parametroEncontrado = true;
                   // Debug.Log("✓ Parámetro 'movement' actualizado a: " + movementNormalizado);
                    break;
                }
            }
            
            if (!parametroEncontrado)
            {
               // Debug.LogError("¡NO se encontró el parámetro 'movement' en el Animator Controller! Agrégalo en la ventana Animator.");
            }
        }
        else
        {
            // Debug.LogWarning("No hay Animator asignado en " + gameObject.name + ". Las animaciones no funcionarán.");
        }
    }
    
    private void GirarHacia(float direccionX)
    {
        // Gira el sprite para que mire hacia el jugador
        if (direccionX > 0 && !estaMirandoDerecha)
        {
            Girar();
        }
        else if (direccionX < 0 && estaMirandoDerecha)
        {
            Girar();
        }
    }

    // --- 3. Función para recibir daño ---
    public void TomarDaño(int cantidadDaño)
    {
        salud -= cantidadDaño;
      //  Debug.Log("Enemigo recibió " + cantidadDaño + " de daño. Salud restante: " + salud);

        // 🔥 APLICAR RETROCESO CUANDO RECIBE DAÑO
        if (puedeRecibirRetroceso && jugador != null)
        {
            AplicarRetroceso();
        }

        // Efecto visual de daño (cambiar color temporalmente)
        if (spriteRenderer != null)
        {
            StartCoroutine(EfectoDaño());
        }

        if (salud <= 0)
        {
            Morir();
        }
        else if (animator != null)
        {
            // Enviar animación de recibir daño (si lo tienes)
            // animator.SetTrigger("Hurt");
        }
    }
    
    // 🔥 MÉTODO PARA APLICAR RETROCESO
    private void AplicarRetroceso()
    {
        if (estaEnRetroceso || jugador == null || rb == null) return;
        
        estaEnRetroceso = true;
        
        // Calcular dirección opuesta al jugador
        Vector2 direccionRetroceso = (transform.position - jugador.position).normalized;
        
        // Aplicar fuerza de retroceso
        rb.AddForce(direccionRetroceso * fuerzaRetroceso, ForceMode2D.Impulse);
        
        Debug.LogError("💥 RETROCESO APLICADO: " + direccionRetroceso + " con fuerza " + fuerzaRetroceso);
        
        // Terminar retroceso después del tiempo especificado
        Invoke("TerminarRetroceso", tiempoRetroceso);
    }
    
    private void TerminarRetroceso()
    {
        estaEnRetroceso = false;
        
        // Reducir velocidad gradualmente
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        }
        
        Debug.LogError("✅ RETROCESO TERMINADO");
    }
    
    // Método alternativo para el sistema de ataque del jugador
    public void RecibirDaño(float cantidadDaño)
    {
        TomarDaño((int)cantidadDaño);
    }
    
    // Efecto visual cuando recibe daño
    private System.Collections.IEnumerator EfectoDaño()
    {
        Color colorOriginal = spriteRenderer.color;
        
        // Cambiar a rojo
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        
        // Volver al color original
        spriteRenderer.color = colorOriginal;
    }

    // --- 4. Función para morir ---
    private void Morir()
    {
        Debug.LogError("💀 ZOMBIE MURIENDO...");
        
        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Enviar animación de muerte (si lo tienes)
        if (animator != null) 
        { 
            // animator.SetBool("IsDead", true);
            // animator.SetTrigger("Death");
        }
        
        // Efecto de muerte (opcional)
        if (spriteRenderer != null)
        {
            StartCoroutine(EfectoMuerte());
        }
        else
        {
            // Si no hay sprite renderer, destruir inmediatamente
            Destroy(gameObject);
        }
    }
    
    // Efecto visual de muerte
    private System.Collections.IEnumerator EfectoMuerte()
    {
        Color colorOriginal = spriteRenderer.color;
        float tiempoMuerte = 0.5f;
        float tiempoTranscurrido = 0f;
        
        // Desvanecer gradualmente
        while (tiempoTranscurrido < tiempoMuerte)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / tiempoMuerte);
            spriteRenderer.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
            yield return null;
        }
        
        // Destruir el GameObject
        Destroy(gameObject);
    }

    // --- 5. Función para girar el sprite (usando FlipX, recomendado) ---
    private void Girar()
    {
        estaMirandoDerecha = !estaMirandoDerecha;
        
        if (spriteRenderer != null)
        {
            // Usa el flipX del SpriteRenderer, es más limpio que cambiar la escala
            spriteRenderer.flipX = !estaMirandoDerecha; 
        }
        else
        {
            // Si por alguna razón no tienes SpriteRenderer, usa la escala (como el código original)
            Vector3 laEscala = transform.localScale;
            // Asegura que solo se voltee la X, manteniendo el valor original de Y y Z
            laEscala.x = Mathf.Abs(transform.localScale.x) * (estaMirandoDerecha ? 1f : -1f);
            transform.localScale = laEscala;
        }
    }
    
    // --- 6. Visualización en el editor ---
    private void OnDrawGizmos()
    {
        // Dibujar círculo de detección
        Gizmos.color = jugadorDetectado ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
        
        // Dibujar círculo de ataque (MÁS VISIBLE)
        Gizmos.color = estaAtacando ? Color.red : Color.orange;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
        
        // Dibujar círculo de parada
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaParada);
        
        // NUEVO: Mostrar línea de vista al jugador
        if (jugador != null && verificarLineaDeVista)
        {
            Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
            float distanciaLineaVista = Vector2.Distance(transform.position, jugador.position);
            
            Gizmos.color = jugadorDetectado ? Color.green : Color.gray;
            Gizmos.DrawRay(transform.position, direccionAlJugador * Mathf.Min(distanciaLineaVista, distanciaDeteccion));
        }
        
        // Información de estado en el editor (MEJORADA)
        if (Application.isPlaying && jugador != null)
        {
            float dist = Vector2.Distance(transform.position, jugador.position);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, 
                $"🧟 S:{salud} D:{dist:F1}" + 
                (estaAtacando ? "⚔️" : "") + 
                (estaEnRetroceso ? "💥" : "") + 
                (jugadorDetectado ? "👁️" : "") +
                (PuedeAtacar() ? "" : $"⏱️{(tiempoUltimoAtaque + cooldownAtaque - Time.time):F1}s"));
        }
    }
    
    // --- 7. Nuevos métodos para el sistema de logros ---
    // (Ejemplo: registrar zombie muerto)
    /*
    public void RegistrarZombieMuerto()
    {
        if (sistemaLogros != null)
        {
            sistemaLogros.RegistrarEvento("ZombieMuerto");
        }
    }
    */

    // 🔄 MEJORADO: SISTEMA DE COLISIONES PARA DETECCIÓN DE DAÑO
    private void OnTriggerEnter2D(Collider2D other)
    {
        // DESACTIVAR TEMPORALMENTE EL DAÑO POR TRIGGER PARA EVITAR DOBLE DAÑO
        return; // Comentar esta línea si quieres reactivar el daño por trigger
        
        // Verificar si es el jugador
        if (other.CompareTag("Player") || other.GetComponent<MovimientoJugador>() != null)
        {
            // Solo aplicar daño por trigger si NO está en modo ataque normal
            if (estaAtacando) return; // Evitar doble daño
            
            MovimientoJugador jugadorScript = other.GetComponent<MovimientoJugador>();
            if (jugadorScript != null)
            {
                Debug.LogError("🔥 TRIGGER: ZOMBIE DETECTÓ JUGADOR");
                
                // Verificar condiciones para hacer daño
                bool puedeHacerDaño = VerificarCondicionesDaño(jugadorScript);
                
                if (puedeHacerDaño && PuedeAtacar())
                {
                    tiempoUltimoAtaque = Time.time;
                    jugadorScript.RecibirDaño(daño);
                    Debug.LogError("⚡ DAÑO POR CONTACTO! Daño: " + daño);
                    
                    // Pequeño retroceso del zombie para evitar spam de daño
                    if (rb != null)
                    {
                        Vector2 direccionRetroceso = (transform.position - other.transform.position).normalized;
                        rb.AddForce(direccionRetroceso * 2f, ForceMode2D.Impulse);
                    }
                }
                else
                {
                    Debug.LogError("🛡️ Condiciones de daño no cumplidas");
                }
            }
        }
    }
}