using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MovimientoJugador : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float fuerzaSalto = 10f;
    [SerializeField] private Transform detectorSuelo;
    [SerializeField] private float radioDeteccion = 1f; // Aumentado temporalmente para debug
    [SerializeField] private LayerMask capaSuelo = -1; // Layer para todos los suelos (todos por defecto)
    
    // Variables de ataque
    [SerializeField] private float tiempoAtaque = 0.5f; // Duración de la animación de ataque
    [SerializeField] private float daño = 10f;
    [SerializeField] private Transform puntoAtaque; // Punto desde donde sale el ataque
    [SerializeField] private float rangoAtaque = 1f; // Radio del área de ataque
    [SerializeField] private LayerMask capaEnemigos; // Layer de los enemigos
    
    // 🫀 SISTEMA DE VIDA
    [Header("💖 Sistema de Vida")]
    [SerializeField] private int saludMaxima = 100;
    [SerializeField] private int saludActual;
    [SerializeField] private bool estaMuerto = false;
    [SerializeField] private float tiempoInmunidad = 1f; // Tiempo de inmunidad después de recibir daño
    [SerializeField] private bool esInmune = false;
    [SerializeField] private SpriteRenderer jugadorSprite;
    
    // Para el sistema de trigger
    private Collider2D triggerAtaque;
    
    private float entradaHorizontal;
    private bool enSuelo;
    private bool estaAtacando = false;
    [SerializeField] private Animator animator;
    [SerializeField] private bool mostrarDebugColisiones = false;
    private float ultimoSalto = 0f; // Tiempo del último salto
    private float cooldownSalto = 0.2f; // Espera mínima entre saltos

    // Input System Actions
    private InputSystem_Actions inputActions;

    // 🔧 NUEVO: Variable para mantener la gravedad original
    [Header("🎯 Configuración Physics")]
    [SerializeField] private float gravedadOriginal = 3f; // Valor configurable en inspector

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        // 💖 INICIALIZAR SISTEMA DE VIDA
        saludActual = saludMaxima;
        
        // 🔧 GUARDAR GRAVEDAD ORIGINAL DEL INSPECTOR
        if (rb2D != null)
        {
            gravedadOriginal = rb2D.gravityScale;
            Debug.LogError("🎯 GRAVEDAD ORIGINAL GUARDADA: " + gravedadOriginal);
        }
        
        // Obtener SpriteRenderer si no está asignado
        if (jugadorSprite == null)
        {
            jugadorSprite = GetComponent<SpriteRenderer>();
            if (jugadorSprite == null)
            {
                jugadorSprite = GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        // Configurar el trigger de ataque
        ConfigurarTriggerAtaque();
        
        // ⚔️ Asegurar que el área de ataque esté en la posición inicial correcta
        ActualizarPosicionAreaAtaque();
    }

    void Start()
    {
        // 💖 RESETEAR ESTADO AL INICIAR ESCENA (NUEVO)
        ResetearEstadoJugador();
        
        if (mostrarDebugColisiones)
        {
            Debug.LogError("🎮 JUGADOR INICIADO EN ESCENA");
        }
    }
    
    void Update()
    {
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 💀 NO HACER NADA SI EL JUGADOR ESTÁ MUERTO
        if (estaMuerto) 
        {
            // 🔧 ASEGURAR QUE NO HAYA MOVIMIENTO DURANTE LA MUERTE
            if (rb2D != null)
            {
                rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            }
            entradaHorizontal = 0f;
            return;
        }
        
        // 🔧 VERIFICACIONES ADICIONALES PARA DEBUGGING
        if (mostrarDebugColisiones && Time.frameCount % 60 == 0)
        {
            Debug.LogError("🎮 Update Debug:");
            Debug.LogError($"  - Muerto: {estaMuerto}");
            Debug.LogError($"  - Rigidbody activo: {(rb2D != null && rb2D.simulated)}");
            Debug.LogError($"  - Input horizontal: {entradaHorizontal}");
            Debug.LogError($"  - Velocity: {(rb2D != null ? rb2D.linearVelocity.ToString() : "NULL")}");
        }
        
        // Usar Input System cuando esté disponible
        // entradaHorizontal = inputActions.Player.Movement.ReadValue<Vector2>().x;
        
        // Temporalmente usar el Input Manager clásico
        entradaHorizontal = Input.GetAxis("Horizontal");
        
        // 🔧 VERIFICAR QUE EL INPUT SE ESTÉ RECIBIENDO
        if (entradaHorizontal != 0 && mostrarDebugColisiones && Time.frameCount % 30 == 0)
        {
            Debug.LogError("🎮 INPUT DETECTADO: " + entradaHorizontal);
        }
        
        // Ataque con Input Manager clásico (cambiar después)
        if (Input.GetButtonDown("Fire1") && !estaAtacando) // Fire1 = Click izquierdo por defecto
        {
            Atacar();
        }
        
        // Detectar si está en el suelo usando Raycast hacia abajo - IGNORANDO al jugador
        RaycastHit2D hit = Physics2D.Raycast(detectorSuelo.position, Vector2.down, radioDeteccion);
        
        // Buscar el primer hit que NO sea el jugador
        RaycastHit2D[] hits = Physics2D.RaycastAll(detectorSuelo.position, Vector2.down, radioDeteccion);
        hit = new RaycastHit2D(); // Reset hit
        
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider != null && h.collider.gameObject != gameObject)
            {
                hit = h;
                break;
            }
        }
        
        // TEMPORALMENTE: detectar CUALQUIER objeto que no sea el jugador
        bool sueloDetectadoAnterior = enSuelo;
        enSuelo = hit.collider != null;
        
        // Debug cambios en detección de suelo
        if (sueloDetectadoAnterior != enSuelo && mostrarDebugColisiones)
        {
            Debug.LogError("🌍 CAMBIO EN DETECCIÓN DE SUELO: " + (enSuelo ? "ATERRIZÓ" : "SALTÓ/CAYÓ"));
        }
        
        // Saltar con la tecla Espacio - SOLO si está en suelo Y no está subiendo
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo && rb2D.linearVelocity.y <= 0.1f && Time.time > ultimoSalto + cooldownSalto)
        {
            if (mostrarDebugColisiones)
            {
                Debug.LogError("🚀 SALTO EJECUTADO desde suelo");
            }
            
            Saltar();
            ultimoSalto = Time.time;
        }
    }
    
    // 💖 NUEVO MÉTODO: RESETEAR ESTADO DEL JUGADOR
    public void ResetearEstadoJugador()
    {
        // 🔧 DETENER TODAS LAS CORRUTINAS Y INVOKES ACTIVAS
        StopAllCoroutines();
        CancelInvoke();
        
        // Resetear vida completa
        saludActual = saludMaxima;
        estaMuerto = false;
        esInmune = false;
        estaAtacando = false;
        
        // 🔧 RESETEAR MOVIMIENTO COMPLETAMENTE
        entradaHorizontal = 0f;
        enSuelo = true; // Asumir que está en el suelo al revivir
        
        // Restaurar sprite si está alterado
        if (jugadorSprite != null)
        {
            jugadorSprite.color = Color.white; // Color normal
        }
        
        // 🔧 RESTAURAR PHYSICS COMPLETAMENTE
        if (rb2D != null)
        {
            // Detener todo movimiento
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            
            // Restaurar configuración del Rigidbody2D
            rb2D.simulated = true;
            rb2D.isKinematic = false; // Asegurar que no esté en modo kinematic
            rb2D.gravityScale = gravedadOriginal; // Usar gravedad original
            rb2D.linearDamping = 0f; // Sin arrastre
            rb2D.angularDamping = 0f; // Sin arrastre angular
            rb2D.freezeRotation = true; // Evitar rotación no deseada
            
            Debug.LogError("🔧 Physics del jugador restaurados:");
            Debug.LogError("  - Velocity: " + rb2D.linearVelocity);
            Debug.LogError("  - Simulated: " + rb2D.simulated);
            Debug.LogError("  - Kinematic: " + rb2D.isKinematic);
            Debug.LogError("  - Gravity Scale: " + rb2D.gravityScale);
        }
        
        // 🔧 REACTIVAR TODOS LOS COLLIDERS DEL JUGADOR
        ReactivarColliders();
        
        // Resetear animaciones
        if (animator != null)
        {
            // 🔧 RESETEAR ANIMATOR COMPLETAMENTE
            animator.enabled = false; // Desactivar temporalmente
            animator.enabled = true; // Reactivar para reset completo
            
            animator.SetBool("muerto", false);
            animator.SetBool("atacando", false);
            animator.SetFloat("movement", 0f);
            
            // Forzar el estado idle/default
            animator.Play("Idle", 0, 0f); // Layer 0, tiempo 0
        }
        
        // 🔧 RECONFIGURAR SISTEMA DE ATAQUE
        ReconfigurarSistemaAtaque();
        
        // 🔧 FORZAR POSICIÓN Y ESCALA CORRECTAS
        Vector3 escalaOriginal = transform.localScale;
        if (escalaOriginal.x == 0 || escalaOriginal.y == 0)
        {
            transform.localScale = new Vector3(1f, 1f, escalaOriginal.z);
            Debug.LogError("🔧 Escala corregida de cero a (1,1,z)");
        }
        
        // 🔧 ASEGURAR LAYER CORRECTO
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) playerLayer = 0; // Default si no existe
        gameObject.layer = playerLayer;
        
        // 🔧 VERIFICAR Y CORREGIR TRANSFORM
        if (transform.position.y < -100f)
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            Debug.LogError("🔧 Posición Y corregida desde valor extremo");
        }
        
        // 💰 RESTAURAR MONEDAS DESPUÉS DE LA MUERTE
        SistemaMonedas sistemaMonedas = SistemaMonedas.GetInstancia();
        if (sistemaMonedas != null)
        {
            sistemaMonedas.RestaurarMonedasPostMuerte();
            Debug.LogError("💰 MONEDAS RESTAURADAS DESPUÉS DE LA MUERTE");
        }
        
        // FORZAR CURSOR VISIBLE SIEMPRE
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.LogError("✨ ESTADO DEL JUGADOR RESETEADO COMPLETAMENTE:");
        Debug.LogError($"  - Vida: {saludActual}/{saludMaxima}");
        Debug.LogError($"  - Muerto: {estaMuerto}");
        Debug.LogError($"  - Inmune: {esInmune}");
        Debug.LogError($"  - Physics activos: {(rb2D != null && rb2D.simulated)}");
        Debug.LogError($"  - Colliders reactivados: ✅");
        Debug.LogError($"  - Sistema ataque reconfigurado: ✅");
        Debug.LogError($"  - Posición actual: {transform.position}");
        Debug.LogError($"  - Escala actual: {transform.localScale}");
        Debug.LogError($"  - Sistema monedas restaurado: ✅");
        
        // 🔧 FORZAR UN FRAME DE ACTUALIZACIÓN
        StartCoroutine(ForzarActualizacionPostRevivir());
    }
    
    // 🔧 NUEVO: FORZAR ACTUALIZACIÓN DESPUÉS DE REVIVIR
    private System.Collections.IEnumerator ForzarActualizacionPostRevivir()
    {
        // Esperar un frame
        yield return null;
        
        // Verificar que todo esté funcionando
        if (rb2D != null)
        {
            rb2D.WakeUp(); // "Despertar" el Rigidbody2D
            Debug.LogError("🔧 Rigidbody2D despertado");
        }
        
        // Verificar detección de suelo
        VerificarDeteccionSuelo();
        
        // Esperar otro frame y verificar inputs
        yield return null;
        
        if (mostrarDebugColisiones)
        {
            Debug.LogError("🔧 Verificación post-revivir:");
            Debug.LogError($"  - Puede recibir input: {!estaMuerto}");
            Debug.LogError($"  - Rigidbody activo: {rb2D != null && rb2D.simulated}");
            Debug.LogError($"  - En suelo: {enSuelo}");
        }
    }
    
    // 🔧 MÉTODO PARA VERIFICAR DETECCIÓN DE SUELO
    private void VerificarDeteccionSuelo()
    {
        if (detectorSuelo == null)
        {
            Debug.LogError("❌ detectorSuelo es NULL - el jugador no podrá saltar");
            return;
        }
        
        // Forzar detección de suelo
        RaycastHit2D[] hits = Physics2D.RaycastAll(detectorSuelo.position, Vector2.down, radioDeteccion);
        
        bool sueloDetectado = false;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider != null && h.collider.gameObject != gameObject)
            {
                sueloDetectado = true;
                Debug.LogError("🔧 Suelo detectado: " + h.collider.name);
                break;
            }
        }
        
        enSuelo = sueloDetectado;
        Debug.LogError("🔧 Estado de suelo actualizado: " + (enSuelo ? "EN SUELO" : "EN AIRE"));
    }

    // private void Update()
    // {
    //     // FORZAR CURSOR VISIBLE SIEMPRE
    //     Cursor.lockState = CursorLockMode.None;
    //     Cursor.visible = true;
        
    //     // 💀 NO HACER NADA SI EL JUGADOR ESTÁ MUERTO
    //     if (estaMuerto) 
    //     {
    //         // 🔧 ASEGURAR QUE NO HAYA MOVIMIENTO DURANTE LA MUERTE
    //         if (rb2D != null)
    //         {
    //             rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
    //         }
    //         entradaHorizontal = 0f;
    //         return;
    //     }
        
    //     // 🔧 VERIFICACIONES ADICIONALES PARA DEBUGGING
    //     if (mostrarDebugColisiones && Time.frameCount % 60 == 0)
    //     {
    //         Debug.LogError("🎮 Update Debug:");
    //         Debug.LogError($"  - Muerto: {estaMuerto}");
    //         Debug.LogError($"  - Rigidbody activo: {(rb2D != null && rb2D.simulated)}");
    //         Debug.LogError($"  - Input horizontal: {entradaHorizontal}");
    //         Debug.LogError($"  - Velocity: {(rb2D != null ? rb2D.linearVelocity.ToString() : "NULL")}");
    //     }
        
    //     // Usar Input System cuando esté disponible
    //     // entradaHorizontal = inputActions.Player.Movement.ReadValue<Vector2>().x;
        
    //     // Temporalmente usar el Input Manager clásico
    //     entradaHorizontal = Input.GetAxis("Horizontal");
        
    //     // 🔧 VERIFICAR QUE EL INPUT SE ESTÉ RECIBIENDO
    //     if (entradaHorizontal != 0 && mostrarDebugColisiones && Time.frameCount % 30 == 0)
    //     {
    //         Debug.LogError("🎮 INPUT DETECTADO: " + entradaHorizontal);
    //     }
        
    //     // Ataque con Input Manager clásico (cambiar después)
    //     if (Input.GetButtonDown("Fire1") && !estaAtacando) // Fire1 = Click izquierdo por defecto
    //     {
    //         Atacar();
    //     }
        
    //     // Detectar si está en el suelo usando Raycast hacia abajo - IGNORANDO al jugador
    //     RaycastHit2D hit = Physics2D.Raycast(detectorSuelo.position, Vector2.down, radioDeteccion);
        
    //     // Buscar el primer hit que NO sea el jugador
    //     RaycastHit2D[] hits = Physics2D.RaycastAll(detectorSuelo.position, Vector2.down, radioDeteccion);
    //     hit = new RaycastHit2D(); // Reset hit
        
    //     foreach (RaycastHit2D h in hits)
    //     {
    //         if (h.collider != null && h.collider.gameObject != gameObject)
    //         {
    //             hit = h;
    //             break;
    //         }
    //     }
        
    //     // TEMPORALMENTE: detectar CUALQUIER objeto que no sea el jugador
    //     bool sueloDetectadoAnterior = enSuelo;
    //     enSuelo = hit.collider != null;
        
    //     // Debug cambios en detección de suelo
    //     if (sueloDetectadoAnterior != enSuelo && mostrarDebugColisiones)
    //     {
    //         Debug.LogError("🌍 CAMBIO EN DETECCIÓN DE SUELO: " + (enSuelo ? "ATERRIZÓ" : "SALTÓ/CAYÓ"));
    //     }
        
    //     // Saltar con la tecla Espacio - SOLO si está en suelo Y no está subiendo
    //     if (Input.GetKeyDown(KeyCode.Space) && enSuelo && rb2D.linearVelocity.y <= 0.1f && Time.time > ultimoSalto + cooldownSalto)
    //     {
    //         if (mostrarDebugColisiones)
    //         {
    //             Debug.LogError("🚀 SALTO EJECUTADO desde suelo");
    //         }
            
    //         Saltar();
    //         ultimoSalto = Time.time;
    //     }
    // }
    
    private void FixedUpdate()
    {
        // 💀 NO MOVERSE SI ESTÁ MUERTO
        if (estaMuerto) 
        {
            if (rb2D != null)
            {
                rb2D.linearVelocity = new Vector2(0, rb2D.linearVelocity.y);
            }
            return;
        }
        
        // 🔧 VERIFICAR QUE EL RIGIDBODY2D ESTÉ FUNCIONAL
        if (rb2D == null)
        {
            Debug.LogError("❌ ERROR CRÍTICO: rb2D es NULL en FixedUpdate!");
            return;
        }
        
        if (!rb2D.simulated)
        {
            Debug.LogError("❌ ERROR: Rigidbody2D no está simulado!");
            rb2D.simulated = true;
            return;
        }
        
        // No moverse durante el ataque
        if (!estaAtacando)
        {
            // 🔧 APLICAR MOVIMIENTO HORIZONTAL
            Vector2 nuevaVelocidad = new Vector2(entradaHorizontal * velocidadMovimiento, rb2D.linearVelocity.y);
            rb2D.linearVelocity = nuevaVelocidad;
            
            // 🔧 DEBUG OCASIONAL DEL MOVIMIENTO
            if (entradaHorizontal != 0 && mostrarDebugColisiones && Time.fixedTime % 1f < Time.fixedDeltaTime)
            {
                Debug.LogError("🏃 MOVIMIENTO:");
                Debug.LogError($"  - Input: {entradaHorizontal}");
                Debug.LogError($"  - Velocidad objetivo: {entradaHorizontal * velocidadMovimiento}");
                Debug.LogError($"  - Velocidad actual: {rb2D.linearVelocity.x}");
            }

            if ((entradaHorizontal > 0 && !MirandoAlaDerecha()) || (entradaHorizontal < 0 && MirandoAlaDerecha()))
            {
                CambiarDireccion();
            }
        }

        // Actualizar parámetros del Animator
        if (animator != null)
        {
            animator.SetFloat("movement", Mathf.Abs(entradaHorizontal));
            animator.SetBool("atacando", estaAtacando);
            animator.SetBool("muerto", estaMuerto);
        }
    }
    
    private bool MirandoAlaDerecha()
    {
        return transform.localScale.x == 1;
    }
    
    private void CambiarDireccion()
    {
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
        
        // ⚔️ VOLTEAR EL ÁREA DE ATAQUE JUNTO CON EL JUGADOR
        ActualizarPosicionAreaAtaque();
    }
    
    // ⚔️ MÉTODO PARA ACTUALIZAR LA POSICIÓN DE LA ESPADA
    private void ActualizarPosicionAreaAtaque()
    {
        if (triggerAtaque != null)
        {
            GameObject objetoEspada = triggerAtaque.gameObject;
            Vector3 posicionActual = objetoEspada.transform.localPosition;
            
            // Voltear la posición X según la dirección del jugador
            if (MirandoAlaDerecha())
            {
                // Si mira a la derecha y la posición X es negativa, voltearlo
                if (posicionActual.x < 0)
                {
                    objetoEspada.transform.localPosition = new Vector3(-posicionActual.x, posicionActual.y, posicionActual.z);
                    Debug.LogError("⚔️ Espada volteada a la DERECHA: " + objetoEspada.transform.localPosition);
                }
            }
            else
            {
                // Si mira a la izquierda y la posición X es positiva, voltearlo
                if (posicionActual.x > 0)
                {
                    objetoEspada.transform.localPosition = new Vector3(-posicionActual.x, posicionActual.y, posicionActual.z);
                    Debug.LogError("⚔️ Espada volteada a la IZQUIERDA: " + objetoEspada.transform.localPosition);
                }
            }
        }
    }
    
    private void Saltar()
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, fuerzaSalto);
    }
    
    private void Atacar()
    {
        if (estaAtacando) 
        {
            return;
        }
        
        // Backup: Si por alguna razón la corrutina falla, terminar ataque después de 2 segundos
        CancelInvoke("TerminarAtaqueBackup");
        Invoke("TerminarAtaqueBackup", 2f);
        
        StartCoroutine(EjecutarAtaque());
    }
    
    private void TerminarAtaqueBackup()
    {
        if (estaAtacando)
        {
            estaAtacando = false;
        }
    }
    
    private IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;
        
        // ⚔️ ASEGURAR QUE EL ÁREA DE ATAQUE ESTÉ EN LA POSICIÓN CORRECTA
        ActualizarPosicionAreaAtaque();
        
        // Activar el trigger de ataque
        if (triggerAtaque != null)
        {
            triggerAtaque.enabled = true;
            Debug.LogError("⚔️ TRIGGER DE ATAQUE ACTIVADO:");
            Debug.LogError("  - Posición: " + triggerAtaque.transform.position);
            Debug.LogError("  - Posición local: " + triggerAtaque.transform.localPosition);
            Debug.LogError("  - Mirando derecha: " + MirandoAlaDerecha());
            Debug.LogError("  - Trigger enabled: " + triggerAtaque.enabled);
        }
        else
        {
            Debug.LogError("❌ ERROR: triggerAtaque es NULL!");
        }
        
        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Mantener el trigger activo por un tiempo breve
        yield return new WaitForSeconds(0.2f);
        
        // Desactivar el trigger
        if (triggerAtaque != null)
        {
            triggerAtaque.enabled = false;
        }
        
        // Esperar el resto de la animación
        float tiempoEspera = Mathf.Max(tiempoAtaque - 0.2f, 0.1f);
        
        yield return new WaitForSeconds(tiempoEspera);
        
        // Terminar el ataque
        estaAtacando = false;
        CancelInvoke("TerminarAtaqueBackup");
    }
    
    // ⚔️ MÉTODO PARA PROCESAR TRIGGERS DESDE EL DETECTOR
    public void ProcesarTriggerAtaque(Collider2D other)
    {
        if (mostrarDebugColisiones)
        {
            Debug.LogError("🔴 PROCESANDO TRIGGER DE ATAQUE:");
            Debug.LogError("  - Objeto detectado: " + other.name + " | Layer: " + other.gameObject.layer);
            Debug.LogError("  - estaAtacando: " + estaAtacando);
            Debug.LogError("  - triggerAtaque.enabled: " + (triggerAtaque != null ? triggerAtaque.enabled.ToString() : "NULL"));
        }
        
        // Solo dañar durante un ataque activo
        if (!estaAtacando || triggerAtaque == null || !triggerAtaque.enabled)
        {
            if (mostrarDebugColisiones)
            {
                Debug.LogError("❌ Condiciones de ataque no cumplidas");
            }
            return;
        }
        
        // Verificar si es un enemigo por múltiples métodos
        bool esEnemigo = false;
        
        // Método 1: Por tag
        if (other.CompareTag("Enemy"))
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR TAG: " + other.tag);
        }
        
        // Método 2: Por layer
        if (!esEnemigo && ((1 << other.gameObject.layer) & capaEnemigos) != 0)
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR LAYER: " + LayerMask.LayerToName(other.gameObject.layer));
        }
        
        // Método 3: Por script ControladorEnemigo
        if (!esEnemigo && other.GetComponent<ControladorEnemigo>() != null)
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR SCRIPT: ControladorEnemigo");
        }
        
        if (esEnemigo)
        {
            if (mostrarDebugColisiones)
            {
                Debug.LogError("⚔️ ¡ATACANDO ENEMIGO!");
            }
            
            // Aplicar daño usando reflexión para máxima compatibilidad
            bool dañoAplicado = AplicarDañoAEnemigo(other, (int)daño);
            
            if (!dañoAplicado && mostrarDebugColisiones)
            {
                Debug.LogError("❌ No se pudo aplicar daño a: " + other.name);
            }
            
            // Aplicar fuerza de golpe
            AplicarFuerzaGolpe(other);
        }
        else if (mostrarDebugColisiones)
        {
            Debug.LogError("❌ NO es un enemigo válido");
        }
    }
    
    // 🔧 MÉTODO PARA REACTIVAR TODOS LOS COLLIDERS
    private void ReactivarColliders()
    {
        // Reactivar todos los colliders del jugador
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
            Debug.LogError($"✅ Collider reactivado: {collider.GetType().Name}");
        }
        
        // Reactivar colliders de objetos hijos (incluyendo la espada)
        Collider2D[] collidersHijos = GetComponentsInChildren<Collider2D>();
        foreach (var collider in collidersHijos)
        {
            // No reactivar el trigger de ataque inmediatamente (se activa solo durante ataques)
            if (collider.gameObject.CompareTag("espada"))
            {
                collider.enabled = false; // La espada se activa solo durante ataques
                Debug.LogError($"⚔️ Collider de espada configurado: {collider.GetType().Name} (desactivado por defecto)");
            }
            else
            {
                collider.enabled = true;
                Debug.LogError($"✅ Collider hijo reactivado: {collider.GetType().Name} en {collider.gameObject.name}");
            }
        }
        
        // Verificar que el jugador esté en el layer correcto
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) playerLayer = 0; // Default si no existe
        
        if (gameObject.layer != playerLayer)
        {
            gameObject.layer = playerLayer;
            Debug.LogError($"🏷️ Layer del jugador configurado: {LayerMask.LayerToName(gameObject.layer)}");
        }
    }
    
    // 🔧 MÉTODO PARA RECONFIGURAR SISTEMA DE ATAQUE
    private void ReconfigurarSistemaAtaque()
    {
        // Buscar y reconfigurar el trigger de ataque
        ConfigurarTriggerAtaque();
        
        // Asegurar que la posición del área de ataque esté correcta
        ActualizarPosicionAreaAtaque();
        
        Debug.LogError("⚔️ Sistema de ataque reconfigurado");
    }

    // 💥 APLICAR DAÑO A ENEMIGO CON MÚLTIPLES MÉTODOS
    private bool AplicarDañoAEnemigo(Collider2D enemigo, int cantidad)
    {
        bool dañoAplicado = false;
        
        // Método 1: ControladorEnemigo (específico del proyecto)
        ControladorEnemigo controlador = enemigo.GetComponent<ControladorEnemigo>();
        if (controlador != null)
        {
            controlador.TomarDaño(cantidad);
            dañoAplicado = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ Daño aplicado vía ControladorEnemigo: " + cantidad);
        }
        
        // Método 2: Buscar por reflexión métodos comunes de daño
        if (!dañoAplicado)
        {
            MonoBehaviour[] scripts = enemigo.GetComponents<MonoBehaviour>();
            
            foreach (MonoBehaviour script in scripts)
            {
                // Intentar TomarDaño con int
                var metodoTomarDaño = script.GetType().GetMethod("TomarDaño", new System.Type[] { typeof(int) });
                if (metodoTomarDaño != null)
                {
                    metodoTomarDaño.Invoke(script, new object[] { cantidad });
                    dañoAplicado = true;
                    if (mostrarDebugColisiones)
                        Debug.LogError("✅ Daño aplicado vía TomarDaño(int): " + cantidad);
                    break;
                }
                
                // Intentar RecibirDaño con float
                var metodoRecibirDaño = script.GetType().GetMethod("RecibirDaño", new System.Type[] { typeof(float) });
                if (metodoRecibirDaño != null)
                {
                    metodoRecibirDaño.Invoke(script, new object[] { (float)cantidad });
                    dañoAplicado = true;
                    if (mostrarDebugColisiones)
                        Debug.LogError("✅ Daño aplicado vía RecibirDaño(float): " + cantidad);
                    break;
                }
                
                // Intentar TakeDamage (inglés)
                var metodoTakeDamage = script.GetType().GetMethod("TakeDamage");
                if (metodoTakeDamage != null)
                {
                    var parametros = metodoTakeDamage.GetParameters();
                    if (parametros.Length == 1)
                    {
                        if (parametros[0].ParameterType == typeof(int))
                        {
                            metodoTakeDamage.Invoke(script, new object[] { cantidad });
                        }
                        else if (parametros[0].ParameterType == typeof(float))
                        {
                            metodoTakeDamage.Invoke(script, new object[] { (float)cantidad });
                        }
                        dañoAplicado = true;
                        if (mostrarDebugColisiones)
                            Debug.LogError("✅ Daño aplicado vía TakeDamage: " + cantidad);
                        break;
                    }
                }
            }
        }
        
        return dañoAplicado;
    }

    // 🔧 APLICAR FUERZA AL ENEMIGO AL SER GOLPEADO
    private void AplicarFuerzaGolpe(Collider2D other)
    {
        // Intentar aplicar un knockback sencillo al Rigidbody2D del enemigo
        float fuerzaHorizontal = 4f;
        float fuerzaVertical = 2f;
        int direccion = MirandoAlaDerecha() ? 1 : -1;

        Rigidbody2D rbEnemigo = other.attachedRigidbody;
        if (rbEnemigo == null)
        {
            rbEnemigo = other.GetComponent<Rigidbody2D>() ?? other.GetComponentInParent<Rigidbody2D>();
        }

        if (rbEnemigo != null)
        {
            // Reiniciar velocidad vertical para que el impulso sea consistente
            rbEnemigo.linearVelocity = new Vector2(rbEnemigo.linearVelocity.x, 0f);
            rbEnemigo.AddForce(new Vector2(fuerzaHorizontal * direccion, fuerzaVertical), ForceMode2D.Impulse);
            if (mostrarDebugColisiones)
                Debug.LogError("💥 Fuerza aplicada al enemigo: " + other.name + " | Dirección: " + direccion);
            return;
        }

        // Si no hay Rigidbody, intentar llamar a un método de empuje en algún script del objeto
        MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            var metodoEmpujar = script.GetType().GetMethod("Empujar", new System.Type[] { typeof(float), typeof(Vector2) });
            if (metodoEmpujar != null)
            {
                metodoEmpujar.Invoke(script, new object[] { 1.0f, new Vector2(fuerzaHorizontal * direccion, fuerzaVertical) });
                if (mostrarDebugColisiones)
                    Debug.LogError("💥 Método Empujar invocado en: " + other.name + " (script: " + script.GetType().Name + ")");
                break;
            }
        }
    }

    // 🔧 MÉTODO PARA DESACTIVAR COLLIDERS AL MORIR
    private void DesactivarCollidersTemporalmente()
    {
        // Desactivar colliders del jugador para evitar más daño
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var collider in colliders)
        {
            if (!collider.isTrigger) // Solo desactivar colliders físicos, mantener triggers para recolección
            {
                collider.enabled = false;
                Debug.LogError($"❌ Collider físico desactivado: {collider.GetType().Name}");
            }
        }
        
        // Desactivar trigger de ataque si está activo
        if (triggerAtaque != null)
        {
            triggerAtaque.enabled = false;
            Debug.LogError("⚔️ Trigger de ataque desactivado");
        }
    }

    // 🧪 MÉTODO PARA TESTING - VERIFICAR ESTADO DE COLLIDERS
    [ContextMenu("🔍 TEST - Verificar Colliders")]
    public void TestVerificarColliders()
    {
        Debug.LogError("🔍 VERIFICANDO ESTADO DE COLLIDERS:");
        Debug.LogError($"  - Estado jugador: Muerto={estaMuerto}, Inmune={esInmune}");
        Debug.LogError($"  - Rigidbody2D simulated: {(rb2D != null ? rb2D.simulated.ToString() : "NULL")}");
        
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.LogError($"  - Colliders del jugador: {colliders.Length}");
        
        for (int i = 0; i < colliders.Length; i++)
        {
            Debug.LogError($"    [{i}] {colliders[i].GetType().Name}: Enabled={colliders[i].enabled}, IsTrigger={colliders[i].isTrigger}");
        }
        
        Collider2D[] collidersHijos = GetComponentsInChildren<Collider2D>();
        Debug.LogError($"  - Colliders totales (incluyendo hijos): {collidersHijos.Length}");
        
        foreach (var collider in collidersHijos)
        {
            Debug.LogError($"    - {collider.gameObject.name}: {collider.GetType().Name}, Enabled={collider.enabled}, Tag={collider.gameObject.tag}");
        }
        
        if (triggerAtaque != null)
        {
            Debug.LogError($"  - Trigger ataque: Enabled={triggerAtaque.enabled}, GameObject={triggerAtaque.gameObject.name}");
        }
        else
        {
            Debug.LogError("  - Trigger ataque: NULL");
        }
    }

    // NUEVO MÉTODO: VERIFICAR ESTADO COMPLETO DEL JUGADOR
    [ContextMenu("🔧 Verificar Estado Completo")]
    public void VerificarEstadoCompleto()
    {
        Debug.LogError("🔍 VERIFICACIÓN COMPLETA DEL JUGADOR:");
        Debug.LogError("===========================================");
        
        // Estado básico
        Debug.LogError("📊 ESTADO BÁSICO:");
        Debug.LogError($"  - Muerto: {estaMuerto}");
        Debug.LogError($"  - Inmune: {esInmune}");
        Debug.LogError($"  - Atacando: {estaAtacando}");
        Debug.LogError($"  - Vida: {saludActual}/{saludMaxima}");
        
        // Transform
        Debug.LogError("📍 TRANSFORM:");
        Debug.LogError($"  - Posición: {transform.position}");
        Debug.LogError($"  - Escala: {transform.localScale}");
        Debug.LogError($"  - Rotación: {transform.rotation.eulerAngles}");
        
        // Rigidbody2D
        Debug.LogError("🎯 RIGIDBODY2D:");
        if (rb2D != null)
        {
            Debug.LogError($"  - Existe: ✅");
            Debug.LogError($"  - Simulated: {rb2D.simulated}");
            Debug.LogError($"  - Kinematic: {rb2D.isKinematic}");
            Debug.LogError($"  - Velocity: {rb2D.linearVelocity}");
            Debug.LogError($"  - Gravity Scale: {rb2D.gravityScale}");
            Debug.LogError($"  - Mass: {rb2D.mass}");
            Debug.LogError($"  - Drag: {rb2D.linearDamping}");
        }
        else
        {
            Debug.LogError("  - ❌ RIGIDBODY2D ES NULL!");
        }
        
        // Colliders
        Debug.LogError("🔲 COLLIDERS:");
        Collider2D[] colliders = GetComponents<Collider2D>();
        Debug.LogError($"  - Cantidad: {colliders.Length}");
        for (int i = 0; i < colliders.Length; i++)
        {
            Debug.LogError($"    [{i}] {colliders[i].GetType().Name}: Enabled={colliders[i].enabled}, IsTrigger={colliders[i].isTrigger}");
        }
        
        // Input y movimiento
        Debug.LogError("🎮 INPUT Y MOVIMIENTO:");
        Debug.LogError($"  - Input horizontal actual: {entradaHorizontal}");
        Debug.LogError($"  - En suelo: {enSuelo}");
        Debug.LogError($"  - Velocidad configurada: {velocidadMovimiento}");
        Debug.LogError($"  - Mirando a la derecha: {MirandoAlaDerecha()}");
        
        // Detector de suelo
        Debug.LogError("🌍 DETECCIÓN DE SUELO:");
        if (detectorSuelo != null)
        {
            Debug.LogError($"  - Detector existe: ✅");
            Debug.LogError($"  - Posición detector: {detectorSuelo.position}");
            Debug.LogError($"  - Radio detección: {radioDeteccion}");
            
            // Test de detección inmediato
            RaycastHit2D[] hits = Physics2D.RaycastAll(detectorSuelo.position, Vector2.down, radioDeteccion);
            Debug.LogError($"  - Hits detectados: {hits.Length}");
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    Debug.LogError($"    - Suelo: {hit.collider.name} en {hit.point}");
                }
            }
        }
        else
        {
            Debug.LogError("  - ❌ DETECTOR DE SUELO ES NULL!");
        }
        
        // Animator
        Debug.LogError("🎬 ANIMATOR:");
        if (animator != null)
        {
            Debug.LogError($"  - Existe: ✅");
            Debug.LogError($"  - Enabled: {animator.enabled}");
            Debug.LogError($"  - Has Controller: {animator.runtimeAnimatorController != null}");
            if (animator.runtimeAnimatorController != null)
            {
                Debug.LogError($"  - Controller: {animator.runtimeAnimatorController.name}");
            }
        }
        else
        {
            Debug.LogError("  - ❌ ANIMATOR ES NULL!");
        }
        
        Debug.LogError("===========================================");
    }

    [ContextMenu("🔧 Forzar Reactivación Completa")]
    public void ForzarReactivacionCompleta()
    {
        Debug.LogError("🔧 FORZANDO REACTIVACIÓN COMPLETA...");
        
        // Llamar al reseteo completo
        ResetearEstadoJugador();
        
        // Esperar un frame y verificar
        StartCoroutine(VerificarReactivacion());
    }
    
    private System.Collections.IEnumerator VerificarReactivacion()
    {
        yield return new WaitForSeconds(0.1f);
        
        Debug.LogError("🔍 VERIFICACIÓN POST-REACTIVACIÓN:");
        
        // Test de movimiento
        if (rb2D != null && rb2D.simulated)
        {
            Debug.LogError("✅ Physics activos - Aplicando test de movimiento...");
            rb2D.AddForce(Vector2.right * 100f);
            
            yield return new WaitForSeconds(0.1f);
            
            if (rb2D.linearVelocity.magnitude > 0.1f)
            {
                Debug.LogError("✅ TEST DE MOVIMIENTO EXITOSO - El jugador puede moverse");
                rb2D.linearVelocity = Vector2.zero; // Detener test
            }
            else
            {
                Debug.LogError("❌ TEST DE MOVIMIENTO FALLIDO - Hay un problema con el Rigidbody2D");
            }
        }
        else
        {
            Debug.LogError("❌ PHYSICS NO ACTIVOS - El jugador no podrá moverse");
        }
    }

    // 🔧 MÉTODO PARA OBTENER DAÑO DEL ENEMIGO
    private int ObtenerDañoEnemigo(Collider2D other)
    {
        // Método 1: Por script ControladorEnemigo
        ControladorEnemigo controlador = other.GetComponent<ControladorEnemigo>();
        if (controlador != null)
        {
            return controlador.daño;
        }
        
        // Método 2: Por reflexión para otros scripts
        MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Buscar campo 'daño' o 'damage'
            var campoDaño = script.GetType().GetField("daño");
            if (campoDaño != null)
            {
                object valor = campoDaño.GetValue(script);
                if (valor is int)
                {
                    return (int)valor;
                }
                else if (valor is float)
                {
                    return (int)(float)valor;
                }
            }
            
            // Buscar campo 'damage' en inglés
            var campoDamage = script.GetType().GetField("damage");
            if (campoDamage != null)
            {
                object valor = campoDamage.GetValue(script);
                if (valor is int)
                {
                    return (int)valor;
                }
                else if (valor is float)
                {
                    return (int)(float)valor;
                }
            }
        }
        
        // Daño por defecto
        return 10;
    }

    // Corutina que maneja pequeños efectos tras la muerte (pausa para animación/efectos)
    private System.Collections.IEnumerator EfectoMuerte()
    {
        // Efecto de temblor de cámara simulado
        Vector3 posicionOriginal = transform.position;
        float tiempoEfecto = 1f;
        float intensidad = 0.1f;
        
        for (float t = 0; t < tiempoEfecto; t += Time.deltaTime)
        {
            // Pequeño movimiento aleatorio para simular temblor
            Vector3 offset = new Vector3(
                Random.Range(-intensidad, intensidad),
                Random.Range(-intensidad, intensidad),
                0
            );
            
            transform.position = posicionOriginal + offset;
            
            yield return null;
        }
        
        // Restaurar posición original
        transform.position = posicionOriginal;
        
        // Desactivar la simulación física del Rigidbody2D para evitar movimientos posteriores
        if (rb2D != null)
        {
            rb2D.simulated = false;
        }
        
        yield break;
    }

    // Maneja la muerte del jugador: marca estado, desactiva físicas y colliders, y reproduce la animación de muerte.
    private void Morir()
    {
        if (estaMuerto) return; // Evitar múltiples llamadas
        
        estaMuerto = true;
        Debug.LogError("💀 JUGADOR HA MUERTO!");
        
        // Parar todo movimiento
        rb2D.linearVelocity = Vector2.zero;
        entradaHorizontal = 0;
        estaAtacando = false;
        
        // 🔧 DESACTIVAR COLLIDERS TEMPORALMENTE PARA EVITAR MÁS DAÑO
        DesactivarCollidersTemporalmente();
        
        // Animación de muerte si está disponible
        if (animator != null)
        {
            animator.SetBool("muerto", true);
            animator.SetTrigger("muerte");
        }
        
        // Efecto visual de muerte
        StartCoroutine(EfectoMuerte());
        
        // 🔧 MOSTRAR CANVAS DE MUERTE CON DELAY PARA ASEGURAR QUE FUNCIONE
        StartCoroutine(MostrarCanvasMuerteConDelay());
    }

    // 🔧 MÉTODO MEJORADO PARA MOSTRAR CANVAS CON DELAY
    private System.Collections.IEnumerator MostrarCanvasMuerteConDelay()
    {
        // Esperar un frame para que el estado de muerte se establezca
        yield return new WaitForEndOfFrame();
        
        // Buscar canvas de muerte en la escena
        CanvasMuerte canvasMuerte = FindObjectOfType<CanvasMuerte>();
        
        if (canvasMuerte != null)
        {
            Debug.LogError("💀 CANVAS DE MUERTE ENCONTRADO - Mostrando...");
            // Usar la instancia encontrada para mostrar el panel
            canvasMuerte.MostrarPanelMuerte();
            
            // Verificar que efectivamente se mostró
            yield return new WaitForSeconds(0.1f);
            Debug.LogError("💀 ¿Canvas mostrado? Estado del juego pausado: " + (Time.timeScale == 0f ? "SÍ" : "NO"));
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ CanvasMuerte en la escena!");
            
            // Intentar crear uno dinámicamente
            Debug.LogError("🔧 CREANDO CANVAS DE MUERTE DINÁMICAMENTE...");
            GameObject canvasObj = new GameObject("CanvasMuerte_Dinamico");
            CanvasMuerte canvasScript = canvasObj.AddComponent<CanvasMuerte>();
            
            // Esperar que se inicialice
            yield return new WaitForSeconds(0.2f);
            
            // Mostrar el canvas usando la instancia creada
            canvasScript.MostrarPanelMuerte();
            Debug.LogError("💀 CANVAS DINÁMICO CREADO Y MOSTRADO");
        }
    }

    // Detección de colisiones con enemigos (MEJORADO PARA MEJOR DETECCIÓN)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 💀 NO PROCESAR COLISIONES SI ESTÁ MUERTO
        if (estaMuerto)
        {
            if (mostrarDebugColisiones)
                Debug.LogError("💀 JUGADOR MUERTO - Ignorando colisión");
            return;
        }
        
        // 🔧 DEBUG DETALLADO DE COLISIONES
        if (mostrarDebugColisiones)
        {
            Debug.LogError("🔥 TRIGGER DETECTADO:");
            Debug.LogError("  - Objeto: " + other.name);
            Debug.LogError("  - Tag: " + other.tag);
            Debug.LogError("  - Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " (" + other.gameObject.layer + ")");
            Debug.LogError("  - ¿Es trigger?: " + other.isTrigger);
            Debug.LogError("  - Posición: " + other.transform.position);
            Debug.LogError("  - Estado jugador: Muerto=" + estaMuerto + ", Inmune=" + esInmune + ", Atacando=" + estaAtacando);
        }
        
        // 🚫 EXCLUIR ZANAHORIAS Y OBJETOS DE RECOLECCIÓN
        if (other.GetComponent<Zanahoria>() != null)
        {
            if (mostrarDebugColisiones)
                Debug.Log("🥕 Zanahoria detectada - NO causa daño");
            return;
        }
        
        // 🚫 EXCLUIR OBJETOS POR NOMBRE
        string nombreObjeto = other.name.ToLower();
        if (nombreObjeto.Contains("zanahoria") || nombreObjeto.Contains("carrot") || 
            nombreObjeto.Contains("moneda") || nombreObjeto.Contains("coin") ||
            nombreObjeto.Contains("item") || nombreObjeto.Contains("pickup"))
        {
            if (mostrarDebugColisiones)
                Debug.Log("🚫 Objeto de recolección detectado - NO causa daño: " + other.name);
            return;
        }
        
        // Solo recibir daño de enemigos si no está muerto, inmune
        if (estaMuerto || esInmune)
        {
            if (mostrarDebugColisiones)
                Debug.LogError("🛡️ JUGADOR INMUNE O MUERTO - No recibe daño");
            return;
        }
        
        // 🧟 VERIFICAR SI ES ENEMIGO
        bool esEnemigo = false;
        
        // Verificar por tag
        if (other.CompareTag("Enemy"))
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR TAG: " + other.tag);
        }
        
        // Verificar por layer si no es por tag
        if (!esEnemigo && ((1 << other.gameObject.layer) & capaEnemigos) != 0)
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR LAYER: " + LayerMask.LayerToName(other.gameObject.layer));
        }
        
        // Verificar por script ControladorEnemigo
        if (!esEnemigo && other.GetComponent<ControladorEnemigo>() != null)
        {
            esEnemigo = true;
            if (mostrarDebugColisiones)
                Debug.LogError("✅ ENEMIGO DETECTADO POR SCRIPT: ControladorEnemigo");
        }
        
        if (esEnemigo)
        {
            if (mostrarDebugColisiones)
            {
                Debug.LogError("🧟 CONTACTO CON ENEMIGO CONFIRMADO: " + other.name);
            }
            
            // 🔧 VERIFICAR POSICIONES RELATIVAS (MEJORADO)
            float posicionJugadorY = transform.position.y;
            float posicionEnemigoY = other.transform.position.y;
            float diferenciaPosicionY = posicionJugadorY - posicionEnemigoY;
            
            if (mostrarDebugColisiones)
            {
                Debug.LogError($"📍 POSICIONES Y: Jugador: {posicionJugadorY:F2}, Enemigo: {posicionEnemigoY:F2}, Diferencia: {diferenciaPosicionY:F2}");
            }
            
            // 🛡️ CONDICIONES DE INMUNIDAD POR ATAQUE
            if (estaAtacando)
            {
                if (mostrarDebugColisiones)
                    Debug.LogError("⚔️ JUGADOR ATACANDO - No recibe daño por contacto (inmunidad de ataque)");
                return;
            }
            
            // 🔧 NUEVA LÓGICA: RECIBIR DAÑO EN MÁS CASOS
            bool debeRecibirDaño = true;
            
            // Solo NO recibir daño si está CLARAMENTE saltando sobre el enemigo
            if (diferenciaPosicionY > 0.8f && rb2D.linearVelocity.y > 0.1f)
            {
                debeRecibirDaño = false;
                if (mostrarDebugColisiones)
                    Debug.LogError("🦘 JUGADOR SALTANDO SOBRE ENEMIGO - No recibe daño");
            }
            
            if (debeRecibirDaño)
            {
                // Obtener daño del enemigo
                int dañoEnemigo = ObtenerDañoEnemigo(other);
                
                if (dañoEnemigo > 0)
                {
                    // 🔥 APLICAR DAÑO
                    RecibirDaño(dañoEnemigo);
                    
                    if (mostrarDebugColisiones)
                    {
                        Debug.LogError("💔 DAÑO APLICADO: " + dañoEnemigo + " | Vida restante: " + saludActual);
                        Debug.LogError($"📊 Diferencia Y: {diferenciaPosicionY:F2} | Velocidad Y: {rb2D.linearVelocity.y:F2}");
                    }
                }
            }
        }
        else if (mostrarDebugColisiones)
        {
            Debug.LogWarning("❓ OBJETO NO RECONOCIDO COMO ENEMIGO:");
            Debug.LogWarning("  - Nombre: " + other.name);
            Debug.LogWarning("  - Tag: " + other.tag + " (esperado: 'Enemy')");
            Debug.LogWarning("  - Layer: " + LayerMask.LayerToName(other.gameObject.layer) + " (" + other.gameObject.layer + ")");
            Debug.LogWarning("  - Capa enemigos: " + capaEnemigos.value);
            Debug.LogWarning("  - Tiene ControladorEnemigo: " + (other.GetComponent<ControladorEnemigo>() != null));
        }
    }

    // 🫀 SISTEMA DE VIDA Y DAÑO
    public void RecibirDaño(int cantidad)
    {
        // No recibir daño si está muerto o es inmune
        if (estaMuerto || esInmune) return;
        
        saludActual -= cantidad;
        Debug.LogError("💔 JUGADOR RECIBIÓ DAÑO: " + cantidad + " | Salud restante: " + saludActual);
        
        // Activar inmunidad temporal
        StartCoroutine(InmunidadTemporal());
        
        // Efecto visual de daño
        if (jugadorSprite != null)
        {
            StartCoroutine(EfectoVisualDaño());
        }
        
        // Comprobar si murió
        if (saludActual <= 0)
        {
            saludActual = 0;
            Morir();
        }
    }

    // ✅ SOBRECARGA DEL MÉTODO PARA COMPATIBILIDAD CON FLOAT
    public void RecibirDaño(float cantidad)
    {
        RecibirDaño((int)cantidad);
    }

    private IEnumerator InmunidadTemporal()
    {
        esInmune = true;
        yield return new WaitForSeconds(tiempoInmunidad);
        esInmune = false;
    }
    
    private IEnumerator EfectoVisualDaño()
    {
        if (jugadorSprite == null) yield break;
        
        Color colorOriginal = jugadorSprite.color;
        
        // Parpadeo rojo durante la inmunidad
        float tiempoParpadeo = 0f;
        while (esInmune && tiempoParpadeo < tiempoInmunidad)
        {
            jugadorSprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            jugadorSprite.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
            tiempoParpadeo += 0.2f;
        }
        
        // Asegurar color original al final
        jugadorSprite.color = colorOriginal;
    }

    // Método público para obtener información de salud
    public int GetSaludActual() { return saludActual; }
    public int GetSaludMaxima() { return saludMaxima; }
    
    // 🔧 MÉTODOS PARA SISTEMA DE MEJORAS
    public void ActualizarVidaMaxima(int nuevaVidaMaxima)
    {
        // SISTEMA DE CURACIÓN: Ya no aumentamos vida máxima, siempre 100
        saludMaxima = 100; // Mantener vida fija
        
        Debug.LogError("💖 VIDA MÁXIMA FIJA: 100 (Sistema de curación activo)");
        
        // Actualizar UI
        ActualizarUICorazones();
    }
    
    public void ActualizarDaño(float nuevoDaño)
    {
        daño = nuevoDaño;
        Debug.LogError("⚔️ DAÑO ACTUALIZADO: " + daño);
    }
    
    public void CurarCompletamente()
    {
        if (estaMuerto)
        {
            // Si está muerto, también revivir
            estaMuerto = false;
            
            // Reactivar colliders si están desactivados
            ReactivarColliders();
            
            // Restaurar physics
            if (rb2D != null)
            {
                rb2D.simulated = true;
            }
            
            // Restaurar sprite
            if (jugadorSprite != null)
            {
                jugadorSprite.color = Color.white;
            }
            
            // Resetear animaciones
            if (animator != null)
            {
                animator.SetBool("muerto", false);
            }
        }
        
        saludActual = saludMaxima;
        esInmune = false; // Quitar inmunidad si la tenía
        
        Debug.LogError("✨ JUGADOR CURADO COMPLETAMENTE: " + saludActual + "/" + saludMaxima);
        
        // Actualizar UI
        ActualizarUICorazones();
    }
    
    public void Curar(int cantidad)
    {
        if (estaMuerto)
        {
            // Si está muerto, revivirlo primero
            CurarCompletamente();
            return;
        }
        
        saludActual = Mathf.Min(saludActual + cantidad, saludMaxima);
        Debug.LogError("💚 JUGADOR CURADO: +" + cantidad + " | Vida: " + saludActual + "/" + saludMaxima);
        
        // Actualizar UI
        ActualizarUICorazones();
    }
    
    private void ActualizarUICorazones()
    {
        // Buscar UI Manager de corazones si existe (comentado por ahora)
        // UIManagerCorazones uiCorazones = FindObjectOfType<UIManagerCorazones>();
        // if (uiCorazones != null)
        // {
        //     uiCorazones.ActualizarCorazones(saludActual, saludMaxima);
        // }
    }
    
    // ✅ MÉTODOS PÚBLICOS FALTANTES PARA OTROS SCRIPTS
    public float GetDañoActual() { return daño; }
    public bool EstaMuerto() { return estaMuerto; }
    public bool EsInmune() { return esInmune; }
    public bool EstaAtacando() { return estaAtacando; }
    
    // ✅ MÉTODOS ADICIONALES PARA COMPATIBILIDAD
    public bool PuedeRecibirDaño()
    {
        return !estaMuerto && !esInmune;
    }
    
    public bool EstaVivo()
    {
        return !estaMuerto;
    }
    
    public float GetVidaPorcentaje()
    {
        if (saludMaxima <= 0) return 0f;
        return (float)saludActual / (float)saludMaxima;
    }
    
    public void SetSaludMaxima(int nuevaVidaMaxima)
    {
        saludMaxima = Mathf.Max(1, nuevaVidaMaxima); // Mínimo 1 de vida
        saludActual = Mathf.Min(saludActual, saludMaxima); // Ajustar vida actual si es necesario
        Debug.LogError("💖 VIDA MÁXIMA ACTUALIZADA: " + saludMaxima);
    }
    
    public void SetSalud(int nuevaSalud)
    {
        saludActual = Mathf.Clamp(nuevaSalud, 0, saludMaxima);
        
        if (saludActual <= 0 && !estaMuerto)
        {
            Morir();
        }
        else if (saludActual > 0 && estaMuerto)
        {
            // Si tenía vida 0 y ahora tiene vida, revivir
            CurarCompletamente();
        }
        
        Debug.LogError("🔧 SALUD ESTABLECIDA: " + saludActual + "/" + saludMaxima);
    }
    
    public void ModificarDaño(float multiplicador)
    {
        daño = daño * multiplicador;
        Debug.LogError("⚔️ DAÑO MODIFICADO: " + daño + " (x" + multiplicador + ")");
    }
    
    public void RestablecerDaño(float dañoBase)
    {
        daño = dañoBase;
        Debug.LogError("⚔️ DAÑO RESTABLECIDO: " + daño);
    }
    
    // ✅ MÉTODOS PARA EFECTOS Y ESTADOS TEMPORALES
    public void AplicarInmunidad(float duracion)
    {
        if (estaMuerto) return;
        
        esInmune = true;
        CancelInvoke("TerminarInmunidadManual"); // Cancelar inmunidad anterior
        Invoke("TerminarInmunidadManual", duracion);
        
        Debug.LogError("🛡️ INMUNIDAD APLICADA POR " + duracion + " segundos");
    }
    
    private void TerminarInmunidadManual()
    {
        esInmune = false;
        Debug.LogError("🛡️ INMUNIDAD TERMINADA");
    }
    
    public void ForzarDetenerAtaque()
    {
        if (estaAtacando)
        {
            estaAtacando = false;
            CancelInvoke("TerminarAtaqueBackup");
            
            if (triggerAtaque != null)
            {
                triggerAtaque.enabled = false;
            }
            
            Debug.LogError("🛑 ATAQUE FORZADO A DETENERSE");
        }
    }

    private void ConfigurarTriggerAtaque()
    {
        // Buscar el GameObject existente con tag "espada" dentro del jugador
        Transform espadaTransform = transform.Find("espada");
        if (espadaTransform == null)
        {
            // Si no se encuentra por nombre, buscar por tag en los hijos
            Transform[] hijos = GetComponentsInChildren<Transform>();
            foreach (Transform hijo in hijos)
            {
                if (hijo.CompareTag("espada"))
                {
                    espadaTransform = hijo;
                    break;
                }
            }
        }
        
        if (espadaTransform == null)
        {
            Debug.LogError("❌ ERROR: No se encontró GameObject con tag 'espada' en el jugador!");
            return;
        }
        
        GameObject objetoEspada = espadaTransform.gameObject;
        Debug.LogError("⚔️ ESPADA ENCONTRADA: " + objetoEspada.name);
        
        // Verificar si ya tiene un BoxCollider2D, si no, agregarlo
        BoxCollider2D boxCollider = objetoEspada.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = objetoEspada.AddComponent<BoxCollider2D>();
            Debug.LogError("✅ BoxCollider2D agregado a la espada");
        }
        
        triggerAtaque = boxCollider;
        triggerAtaque.isTrigger = true;
        triggerAtaque.enabled = false; // Desactivado por defecto
        
        // Verificar si ya tiene TriggerDetector, si no, agregarlo
        TriggerDetector detector = objetoEspada.GetComponent<TriggerDetector>();
        if (detector == null)
        {
            detector = objetoEspada.AddComponent<TriggerDetector>();
            Debug.LogError("✅ TriggerDetector agregado a la espada");
        }
        detector.jugador = this; // Referencia al script del jugador
        
        // Configurar el tamaño del área de ataque
        boxCollider.size = new Vector2(rangoAtaque * 1.2f, rangoAtaque * 1.8f);
        
        Debug.LogError("⚔️ ESPADA CONFIGURADA:");
        Debug.LogError("  - Nombre: " + objetoEspada.name);
        Debug.LogError("  - Posición actual: " + objetoEspada.transform.localPosition);
        Debug.LogError("  - Tamaño collider: " + boxCollider.size);
        Debug.LogError("  - Es trigger: " + triggerAtaque.isTrigger);
    }

    // 🔧 GETTER PARA LA GRAVEDAD ACTUAL
    public float GetGravedadOriginal()
    {
        return gravedadOriginal;
    }
    
    public float GetGravedadActual()
    {
        return rb2D != null ? rb2D.gravityScale : gravedadOriginal;
    }

    // 🔧 NUEVO MÉTODO PARA AJUSTAR GRAVEDAD EN RUNTIME
    public void SetGravedad(float nuevaGravedad)
    {
        gravedadOriginal = nuevaGravedad;
        if (rb2D != null)
        {
            rb2D.gravityScale = nuevaGravedad;
            Debug.LogError("🎯 GRAVEDAD ACTUALIZADA: " + nuevaGravedad);
        }
    }
}