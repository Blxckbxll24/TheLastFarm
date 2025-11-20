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
    
    // Para el sistema de trigger
    private Collider2D triggerAtaque;
    
    private float entradaHorizontal;
    private bool enSuelo;
    private bool estaAtacando = false;
    [SerializeField] private Animator animator;
    private float ultimoSalto = 0f; // Tiempo del último salto
    private float cooldownSalto = 0.2f; // Espera mínima entre saltos

    // Input System Actions
    // Descomenta esto DESPUÉS de generar la clase C# del InputSystem_Actions:
     private InputSystem_Actions inputActions;

    private void Awake()
    {
        // Cuando generes la clase C#, descomenta esto:
        inputActions = new InputSystem_Actions();
        
        // Configurar el trigger de ataque
        ConfigurarTriggerAtaque();
    }
    
    private void ConfigurarTriggerAtaque()
    {
        // Crear un GameObject hijo para el área de ataque
        GameObject objetoAtaque = new GameObject("AreaAtaque");
        objetoAtaque.transform.SetParent(transform);
        objetoAtaque.transform.localPosition = Vector3.zero;
        
        // Agregar el collider como trigger
        triggerAtaque = objetoAtaque.AddComponent<CircleCollider2D>();
        triggerAtaque.isTrigger = true;
        triggerAtaque.enabled = false; // Desactivado por defecto
        
        // Configurar el radio
        CircleCollider2D circulo = triggerAtaque as CircleCollider2D;
        circulo.radius = rangoAtaque;
    }
    
    private void OnEnable()
    {
        inputActions?.Enable();
        inputActions.Player.Attack.performed += OnAttackPerformed;
         inputActions.Player.Jump.performed += OnJumpPerformed;
    }
    
    private void OnDisable()
    {
         inputActions?.Disable();
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Jump.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        // Usar Input System cuando esté disponible
        // entradaHorizontal = inputActions.Player.Movement.ReadValue<Vector2>().x;
        
        // Temporalmente usar el Input Manager clásico
        entradaHorizontal = Input.GetAxis("Horizontal");
        
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
            if (h.collider != null && h.collider.gameObject != gameObject) // Ignorar al propio jugador
            {
                hit = h;
                break;
            }
        }
        
        // TEMPORALMENTE: detectar CUALQUIER objeto que no sea el jugador
        enSuelo = hit.collider != null;
        
        // Debug detallado (comentado para no saturar consola)
        /*if (hit.collider != null)
        {
            Debug.Log("Detectando objeto: '" + hit.collider.gameObject.name + "' | Es el jugador: " + (hit.collider.gameObject == gameObject));
        }
        else
        {
            Debug.Log("No detecta nada (ignorando jugador) | Posición detector: " + detectorSuelo.position + " | Radio: " + radioDeteccion);
        }*/
        
        // Debug cuando presiona Jump (comentado para no saturar consola)
        /*if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("¡PRESIONÓ JUMP!");
            Debug.Log("EnSuelo: " + enSuelo);
            Debug.Log("Velocidad Y: " + rb2D.linearVelocity.y);
            Debug.Log("Tiempo actual: " + Time.time + " | Último salto: " + ultimoSalto);
            Debug.Log("Puede saltar: " + (Time.time > ultimoSalto + cooldownSalto));
        }*/
        
        // Saltar con la tecla Espacio - SOLO si está en suelo Y no está subiendo
        if (Input.GetButtonDown("Jump") && enSuelo && rb2D.linearVelocity.y <= 0.1f)
        {
            // Debug.Log("¡SALTANDO AHORA!"); // Comentado
            Saltar();
        }
        
        // PRUEBA TEMPORAL - comentada
        /*if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("¡SALTANDO SIN CONDICIONES!");
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, fuerzaSalto);
        }*/
    }
    private void FixedUpdate()
    {
        // No moverse durante el ataque
        if (!estaAtacando)
        {
            rb2D.linearVelocity = new Vector2(entradaHorizontal * velocidadMovimiento, rb2D.linearVelocity.y);

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
    }
    
    private void Saltar()
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, fuerzaSalto);
    }
    
    private void Atacar()
    {
        // Debug.Log("Método Atacar llamado. estaAtacando actual: " + estaAtacando); // Comentado
        
        if (estaAtacando) 
        {
            // Debug.Log("Ya está atacando, ignorando nuevo ataque"); // Comentado
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
            // Debug.LogWarning("Terminando ataque por backup (la corrutina no terminó)"); // Comentado
            estaAtacando = false;
        }
    }
    
    private IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;
        
        // Debug.Log("¡COMENZANDO ATAQUE! estaAtacando = " + estaAtacando); // Comentado
        
        // Activar el trigger de ataque
        if (triggerAtaque != null)
        {
            triggerAtaque.enabled = true;
            // Debug.Log("Trigger de ataque activado"); // Comentado
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
            // Debug.Log("Trigger de ataque desactivado"); // Comentado
        }
        
        // Esperar el resto de la animación
        float tiempoEspera = Mathf.Max(tiempoAtaque - 0.2f, 0.1f);
        // Debug.Log("Esperando " + tiempoEspera + " segundos..."); // Comentado
        
        yield return new WaitForSeconds(tiempoEspera);
        
        // Terminar el ataque
        estaAtacando = false;
        CancelInvoke("TerminarAtaqueBackup"); // Cancelar el backup ya que terminó correctamente
        // Debug.Log("¡ATAQUE TERMINADO! estaAtacando = " + estaAtacando); // Comentado
    }
    
    // Método para cuando un enemigo entra en el trigger de ataque
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo dañar durante un ataque activo
        if (!estaAtacando || triggerAtaque == null || !triggerAtaque.enabled)
        {
            // Debug.Log("Trigger detectado pero no hay ataque activo o trigger deshabilitado"); // Comentado
            return;
        }
            
        // Debug.Log("Trigger detectó: " + other.name + " en layer: " + other.gameObject.layer); // Comentado
        
        // Verificar si es un enemigo
        if (((1 << other.gameObject.layer) & capaEnemigos) != 0)
        {
            // Debug.Log("¡Es un enemigo! Aplicando daño..."); // Comentado
            
            // Buscar el componente de daño en el enemigo
            MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
            bool dañoAplicado = false;
            
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().GetMethod("RecibirDaño") != null)
                {
                    // Debug.Log("Aplicando " + daño + " de daño a " + other.name + " (RecibirDaño)"); // Comentado
                    script.GetType().GetMethod("RecibirDaño").Invoke(script, new object[] { daño });
                    dañoAplicado = true;
                    break;
                }
                else if (script.GetType().GetMethod("TomarDaño") != null)
                {
                    // Debug.Log("Aplicando " + (int)daño + " de daño a " + other.name + " (TomarDaño)"); // Comentado
                    script.GetType().GetMethod("TomarDaño").Invoke(script, new object[] { (int)daño });
                    dañoAplicado = true;
                    break;
                }
            }
            
            if (!dañoAplicado)
            {
                // Debug.LogWarning("No se encontró método de daño en " + other.name); // Comentado
            }
            
            // Aplicar empuje al enemigo
            var rbEnemigo = other.GetComponent<Rigidbody2D>();
            if (rbEnemigo != null)
            {
                Vector2 direccionEmpuje = (other.transform.position - transform.position).normalized;
                rbEnemigo.AddForce(direccionEmpuje * 300f);
                // Debug.Log("Empuje aplicado a " + other.name); // Comentado
            }
        }
        else
        {
            // Debug.Log("No es un enemigo (layer incorrecto). Layer del objeto: " + other.gameObject.layer + ", LayerMask esperado: " + capaEnemigos.value); // Comentado
        }
    }
    
    // Método para visualizar el área de detección del suelo y ataque en el editor
    private void OnDrawGizmos()
    {
        // Detector de suelo
        if (detectorSuelo != null)
        {
            Gizmos.color = enSuelo ? Color.green : Color.red;
            // Dibujar una línea hacia abajo para mostrar el raycast
            Gizmos.DrawLine(detectorSuelo.position, detectorSuelo.position + Vector3.down * radioDeteccion);
            // Dibujar un punto al final del raycast
            Gizmos.DrawWireSphere(detectorSuelo.position + Vector3.down * radioDeteccion, 0.1f);
        }
        
        // Área de ataque (usando el trigger)
        if (triggerAtaque != null)
        {
            Gizmos.color = estaAtacando && triggerAtaque.enabled ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(triggerAtaque.transform.position, rangoAtaque);
        }
        // Fallback si no hay trigger
        else if (puntoAtaque != null)
        {
            Gizmos.color = estaAtacando ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
        }
    }
    
    // Métodos callback para Input System (descomenta cuando generes la clase C#)
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!estaAtacando)
        {
            Atacar();
        }
    }
    
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (enSuelo && rb2D.linearVelocity.y <= 0.1f)
        {
            Saltar();
        }
    }
    
}
