using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema para reproducir música automáticamente según la escena actual
/// Coloca este script en un GameObject en cada escena o como singleton
/// </summary>
public class SistemaAudioEscena : MonoBehaviour
{
    [Header("🎵 CONFIGURACIÓN DE MÚSICA")]
    [SerializeField] private AudioClip musicaEscena1;
    [SerializeField] private AudioClip musicaEscena2;
    [SerializeField] private AudioClip musicaMenuPrincipal;
    [SerializeField] private AudioClip musicaPorDefecto;
    
    [Header("🔊 CONFIGURACIÓN DE AUDIO")]
    [SerializeField] private float volumenMusica = 0.8f;
    [SerializeField] private bool reproducirEnBucle = true;
    [SerializeField] private bool fadeBetweenTracks = true;
    [SerializeField] private float tiempoFade = 2f;
    [SerializeField] private bool persistirEntreEscenas = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("📱 REFERENCIAS")]
    [SerializeField] private AudioSource audioSource;
    
    // Sistema Singleton para persistencia
    private static SistemaAudioEscena instancia;
    private string escenaActual;
    private AudioClip clipActual;
    private bool estaReproduciendo = false;
    
    void Awake()
    {
        // Configurar Singleton
        if (instancia == null)
        {
            instancia = this;
            
            if (persistirEntreEscenas)
            {
                DontDestroyOnLoad(gameObject);
                Debug.LogError("🎵 SISTEMA DE AUDIO CONFIGURADO COMO PERSISTENTE");
            }
            
            // Suscribirse a eventos de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instancia != this)
        {
            // Ya existe una instancia, destruir esta
            if (mostrarDebug)
            {
                Debug.LogError("🎵 DESTRUYENDO SISTEMA DE AUDIO DUPLICADO");
            }
            Destroy(gameObject);
            return;
        }
        
        // Configurar AudioSource
        ConfigurarAudioSource();
    }
    
    void Start()
    {
        // Reproducir música de la escena actual
        escenaActual = SceneManager.GetActiveScene().name;
        ReproducirMusicaEscena(escenaActual);
    }
    
    private void ConfigurarAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.LogError("🔊 AudioSource creado automáticamente");
            }
        }
        
        // 🔧 CONFIGURACIÓN SIMPLIFICADA - SIN INTERFERENCIAS DE VOLUMEN
        audioSource.volume = volumenMusica; // Solo establecer una vez
        audioSource.loop = reproducirEnBucle;
        audioSource.playOnAwake = false;
        
        // 🔧 NO TOCAR AudioListener.volume - causa problemas
        // AudioListener.volume = 1f; // REMOVIDO
        
        if (mostrarDebug)
        {
            Debug.LogError("🔊 AudioSource configurado:");
            Debug.LogError($"  - Volumen: {volumenMusica}");
            Debug.LogError($"  - Loop: {reproducirEnBucle}");
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instancia != this) return; // Solo procesar en la instancia principal
        
        string nuevaEscena = scene.name;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🎵 NUEVA ESCENA: {nuevaEscena}");
        }
        
        // Cambiar música si es necesario
        if (nuevaEscena != escenaActual)
        {
            escenaActual = nuevaEscena;
            ReproducirMusicaEscena(nuevaEscena);
        }
    }
    
    private void ReproducirMusicaEscena(string nombreEscena)
    {
        AudioClip nuevaMusica = ObtenerMusicaParaEscena(nombreEscena);
        
        if (nuevaMusica != null && nuevaMusica != clipActual)
        {
            if (mostrarDebug)
            {
                Debug.LogError($"🎵 CAMBIANDO MÚSICA: {nuevaMusica.name} para escena {nombreEscena}");
            }
            
            if (fadeBetweenTracks && estaReproduciendo)
            {
                StartCoroutine(CambiarMusicaConFade(nuevaMusica));
            }
            else
            {
                CambiarMusicaDirecto(nuevaMusica);
            }
        }
        else if (nuevaMusica == null)
        {
            if (mostrarDebug)
            {
                Debug.LogError($"❌ NO HAY MÚSICA ASIGNADA PARA ESCENA: {nombreEscena}");
            }
        }
        else
        {
            if (mostrarDebug)
            {
                Debug.LogError($"🎵 MÚSICA YA ESTÁ SONANDO: {clipActual.name}");
            }
        }
    }
    
    private AudioClip ObtenerMusicaParaEscena(string nombreEscena)
    {
        // Verificar escena específica
        if (nombreEscena.Contains("Escena1") || nombreEscena.Contains("1"))
        {
            return musicaEscena1;
        }
        else if (nombreEscena.Contains("Escena2") || nombreEscena.Contains("2"))
        {
            return musicaEscena2;
        }
        else if (nombreEscena.ToLower().Contains("menu"))
        {
            return musicaMenuPrincipal;
        }
        else
        {
            return musicaPorDefecto;
        }
    }
    
    private void CambiarMusicaDirecto(AudioClip nuevaMusica)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        audioSource.clip = nuevaMusica;
        clipActual = nuevaMusica;
        
        // 🔧 REPRODUCIR INMEDIATAMENTE SIN MODIFICAR VOLUMEN
        audioSource.Play();
        estaReproduciendo = true;
        
        if (mostrarDebug)
        {
            Debug.LogError($"▶️ REPRODUCIENDO DIRECTO: {nuevaMusica.name}");
        }
    }
    
    private System.Collections.IEnumerator CambiarMusicaConFade(AudioClip nuevaMusica)
    {
        // Fade Out de la música actual
        if (estaReproduciendo)
        {
            float volumenOriginal = audioSource.volume;
            
            for (float t = 0; t < tiempoFade; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(volumenOriginal, 0f, t / tiempoFade);
                yield return null;
            }
            
            audioSource.Stop();
        }
        
        // Cambiar música
        audioSource.clip = nuevaMusica;
        clipActual = nuevaMusica;
        audioSource.Play();
        
        // Fade In de la nueva música
        for (float t = 0; t < tiempoFade; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, volumenMusica, t / tiempoFade);
            yield return null;
        }
        
        audioSource.volume = volumenMusica;
        estaReproduciendo = true;
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔀 TRANSICIÓN COMPLETADA A: {nuevaMusica.name}");
        }
    }
    
    // MÉTODOS PÚBLICOS PARA CONTROL EXTERNO
    public void DetenerMusica()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            estaReproduciendo = false;
            
            if (mostrarDebug)
            {
                Debug.LogError("⏹️ MÚSICA DETENIDA");
            }
        }
    }
    
    public void PausarMusica()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            
            if (mostrarDebug)
            {
                Debug.LogError("⏸️ MÚSICA PAUSADA");
            }
        }
    }
    
    public void ReanudarMusica()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.UnPause();
            
            if (mostrarDebug)
            {
                Debug.LogError("▶️ MÚSICA REANUDADA");
            }
        }
    }
    
    public void CambiarVolumen(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        
        // 🔧 SOLO CAMBIAR EL VOLUMEN DEL AUDIOSOURCE
        if (audioSource != null)
        {
            audioSource.volume = volumenMusica;
        }
        
        if (mostrarDebug)
        {
            Debug.LogError($"🔊 VOLUMEN CAMBIADO A: {volumenMusica:F2}");
        }
    }
    
    public void ReproducirMusicaEspecifica(AudioClip musica)
    {
        if (musica != null)
        {
            if (fadeBetweenTracks && estaReproduciendo)
            {
                StartCoroutine(CambiarMusicaConFade(musica));
            }
            else
            {
                CambiarMusicaDirecto(musica);
            }
        }
    }
    
    // GETTERS
    public bool EstaReproduciendo() { return estaReproduciendo && audioSource.isPlaying; }
    public AudioClip GetClipActual() { return clipActual; }
    public float GetVolumenActual() { return volumenMusica; }
    public string GetEscenaActual() { return escenaActual; }
    
    // CONFIGURACIÓN EN RUNTIME
    public void SetMusicaEscena1(AudioClip musica) { musicaEscena1 = musica; }
    public void SetMusicaEscena2(AudioClip musica) { musicaEscena2 = musica; }
    public void SetMusicaMenuPrincipal(AudioClip musica) { musicaMenuPrincipal = musica; }
    
    void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    // MÉTODOS DE TESTING
    [ContextMenu("🎵 Test - Reproducir Música Escena1")]
    public void TestMusicaEscena1()
    {
        if (musicaEscena1 != null)
        {
            ReproducirMusicaEspecifica(musicaEscena1);
            Debug.LogError("🧪 TEST: Reproduciendo música de Escena1");
        }
        else
        {
            Debug.LogError("❌ NO HAY MÚSICA ASIGNADA PARA ESCENA1");
        }
    }
    
    [ContextMenu("🎵 Test - Reproducir Música Escena2")]
    public void TestMusicaEscena2()
    {
        if (musicaEscena2 != null)
        {
            ReproducirMusicaEspecifica(musicaEscena2);
            Debug.LogError("🧪 TEST: Reproduciendo música de Escena2");
        }
        else
        {
            Debug.LogError("❌ NO HAY MÚSICA ASIGNADA PARA ESCENA2");
        }
    }
    
    [ContextMenu("📊 Mostrar Estado Audio")]
    public void MostrarEstadoAudio()
    {
        Debug.LogError("🎵 ESTADO DEL SISTEMA DE AUDIO:");
        Debug.LogError("===============================");
        Debug.LogError($"🎵 Escena actual: {escenaActual}");
        Debug.LogError($"🎵 Música actual: {(clipActual != null ? clipActual.name : "NINGUNA")}");
        Debug.LogError($"▶️ Está reproduciendo: {EstaReproduciendo()}");
        Debug.LogError($"🔊 Volumen: {volumenMusica:F2}");
        Debug.LogError($"🔁 Loop activado: {reproducirEnBucle}");
        Debug.LogError($"🎚️ Fade entre pistas: {fadeBetweenTracks}");
        Debug.LogError("");
        Debug.LogError("🎵 MÚSICA ASIGNADA:");
        Debug.LogError($"  - Escena1: {(musicaEscena1 != null ? musicaEscena1.name : "NO ASIGNADA")}");
        Debug.LogError($"  - Escena2: {(musicaEscena2 != null ? musicaEscena2.name : "NO ASIGNADA")}");
        Debug.LogError($"  - Menú: {(musicaMenuPrincipal != null ? musicaMenuPrincipal.name : "NO ASIGNADA")}");
        Debug.LogError($"  - Por defecto: {(musicaPorDefecto != null ? musicaPorDefecto.name : "NO ASIGNADA")}");
        Debug.LogError("===============================");
    }
}
