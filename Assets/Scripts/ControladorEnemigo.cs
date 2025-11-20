using UnityEngine;
using System.Collections;

public class ControladorEnemigo : MonoBehaviour
{
    // --- Variables Configurables ---
    [Header("Estadísticas")]
    public int salud = 30; // Reducido para pruebas más fáciles
    public float velocidadMovimiento = 2.5f;
    
    [Header("Detección del Jugador")]
    public float distanciaDeteccion = 5f; // Distancia para empezar a perseguir
    public float distanciaAtaque = 1.5f;   // Distancia para atacar
    public float distanciaParada = 0.5f;   // Distancia mínima antes de parar
    
    [Header("Animaciones")]
    // Asegúrate de que este Animator esté en el GameObject o un hijo y tiene un Animator Controller
    public Animator animator; 

    // --- Referencias Internas ---
    private Transform jugador; // Para guardar la posición del jugador
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; 

    // --- Estados del Enemigo ---
    private bool estaMirandoDerecha = true;
    private bool jugadorDetectado = false;
    private bool estaAtacando = false;
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
    }
    
    // Update se llama en cada fotograma
    void Update()
    {
        // Si no encontramos al jugador, no hacemos nada
        if (jugador == null) return;

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
        else if (jugadorDetectado && distanciaAlJugador <= distanciaAtaque && !estaAtacando)
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
        // Parar movimiento durante el ataque
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        if (!estaAtacando)
        {
            estaAtacando = true;
            //Debug.Log("¡Enemigo atacando!");
            
            // Enviar parámetro de animación de ataque (si lo tienes)
            // if (animator != null) { animator.SetTrigger("Attack"); }
            
            // Simular duración del ataque
            Invoke("TerminarAtaque", 1f);
        }
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
            float movementNormalizado = velocidadX / velocidadMovimiento; // Normaliza entre 0 y 1
            
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
        // Debug.Log("El enemigo ha muerto.");
        
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
        
        // Dibujar círculo de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
        
        // Dibujar círculo de parada
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaParada);
        
        // Línea al jugador si está detectado
        if (jugador != null && jugadorDetectado)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, jugador.position);
        }
    }
}