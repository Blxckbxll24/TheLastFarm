using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfiguradorMenus : MonoBehaviour
{
    [Header("🔧 CONFIGURADOR AUTOMÁTICO DE MENÚS")]
    [SerializeField] private bool configurarAlInicio = true;
    [SerializeField] private bool recrearSiExiste = true;
    [SerializeField] private bool mostrarDebug = true;
    
    [Header("🎮 Referencias")]
    [SerializeField] private MenuPausa menuPausa;
    [SerializeField] private MenuOpciones menuOpciones;
    
    void Start()
    {
        if (configurarAlInicio)
        {
            Invoke("ConfigurarTodoAutomaticamente", 0.5f);
        }
    }
    
    [ContextMenu("🚀 CONFIGURAR TODO AUTOMÁTICAMENTE")]
    public void ConfigurarTodoAutomaticamente()
    {
        Debug.LogError("🚀 INICIANDO CONFIGURACIÓN AUTOMÁTICA DE MENÚS");
        
        // 1. Buscar o crear MenuPausa
        ConfigurarMenuPausa();
        
        // 2. Buscar o crear MenuOpciones
        ConfigurarMenuOpciones();
        
        // 3. Conectar ambos sistemas
        ConectarSistemas();
        
        Debug.LogError("✅ CONFIGURACIÓN AUTOMÁTICA COMPLETADA");
    }
    
    private void ConfigurarMenuPausa()
    {
        if (menuPausa == null)
        {
            menuPausa = FindAnyObjectByType<MenuPausa>();
        }
        
        if (menuPausa == null)
        {
            Debug.LogError("📱 Creando MenuPausa desde cero...");
            GameObject menuPausaObj = new GameObject("MenuPausa");
            menuPausa = menuPausaObj.AddComponent<MenuPausa>();
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ MenuPausa configurado: " + menuPausa.gameObject.name);
        }
    }
    
    private void ConfigurarMenuOpciones()
    {
        if (menuOpciones == null)
        {
            if (menuPausa != null)
            {
                menuOpciones = menuPausa.GetComponent<MenuOpciones>();
            }
            
            if (menuOpciones == null)
            {
                menuOpciones = FindAnyObjectByType<MenuOpciones>();
            }
        }
        
        if (menuOpciones == null)
        {
            Debug.LogError("⚙️ Creando MenuOpciones desde cero...");
            if (menuPausa != null)
            {
                menuOpciones = menuPausa.gameObject.AddComponent<MenuOpciones>();
            }
            else
            {
                GameObject menuOpcionesObj = new GameObject("MenuOpciones");
                menuOpciones = menuOpcionesObj.AddComponent<MenuOpciones>();
            }
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("✅ MenuOpciones configurado: " + menuOpciones.gameObject.name);
        }
    }
    
    private void ConectarSistemas()
    {
        if (menuPausa != null && menuOpciones != null)
        {
            if (recrearSiExiste)
            {
                Debug.LogError("🔧 Forzando recreación de UI mejorada...");
                menuPausa.CrearUICompleta();
            }
            
            Debug.LogError("🔗 Sistemas conectados correctamente");
        }
        else
        {
            Debug.LogError("❌ Error conectando sistemas - MenuPausa: " + (menuPausa != null) + " | MenuOpciones: " + (menuOpciones != null));
        }
    }
    
    [ContextMenu("🔧 REPARAR MENÚ DE OPCIONES")]
    public void RepararMenuOpciones()
    {
        Debug.LogError("🔧 INICIANDO REPARACIÓN DEL MENÚ DE OPCIONES");
        
        // Buscar MenuPausa activo
        MenuPausa[] menusPausa = FindObjectsByType<MenuPausa>(FindObjectsSortMode.None);
        
        if (menusPausa.Length == 0)
        {
            Debug.LogError("❌ No se encontró MenuPausa en la escena!");
            return;
        }
        
        MenuPausa menuPausaActivo = menusPausa[0];
        Debug.LogError("✅ MenuPausa encontrado: " + menuPausaActivo.gameObject.name);
        
        // Verificar si tiene Canvas
        Canvas canvas = menuPausaActivo.GetComponentInChildren<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("📱 Canvas no encontrado, creando UI completa...");
            menuPausaActivo.CrearUICompleta();
            canvas = menuPausaActivo.GetComponentInChildren<Canvas>();
        }
        
        if (canvas != null)
        {
            Debug.LogError("✅ Canvas encontrado: " + canvas.gameObject.name);
            
            // Buscar panel de opciones
            Transform panelOpciones = BuscarEnHijos(canvas.transform, "Panel_Opciones");
            
            if (panelOpciones == null)
            {
                Debug.LogError("❌ Panel de opciones no encontrado, recreando...");
                menuPausaActivo.CrearUICompleta();
            }
            else
            {
                Debug.LogError("✅ Panel de opciones encontrado: " + panelOpciones.name);
                
                // Verificar MenuOpciones
                MenuOpciones menuOpc = menuPausaActivo.GetComponent<MenuOpciones>();
                if (menuOpc == null)
                {
                    Debug.LogError("⚙️ Agregando componente MenuOpciones...");
                    menuOpc = menuPausaActivo.gameObject.AddComponent<MenuOpciones>();
                }
                
                Debug.LogError("✅ Menú de opciones reparado correctamente");
            }
        }
        else
        {
            Debug.LogError("❌ No se pudo crear o encontrar Canvas!");
        }
    }
    
    private Transform BuscarEnHijos(Transform padre, string nombre)
    {
        if (padre.name.Contains(nombre))
        {
            return padre;
        }
        
        for (int i = 0; i < padre.childCount; i++)
        {
            Transform resultado = BuscarEnHijos(padre.GetChild(i), nombre);
            if (resultado != null)
            {
                return resultado;
            }
        }
        
        return null;
    }
    
    [ContextMenu("🧪 PROBAR MENÚ DE OPCIONES")]
    public void ProbarMenuOpciones()
    {
        Debug.LogError("🧪 PROBANDO MENÚ DE OPCIONES");
        
        MenuPausa menuP = FindAnyObjectByType<MenuPausa>();
        if (menuP == null)
        {
            Debug.LogError("❌ No hay MenuPausa en la escena!");
            return;
        }
        
        Debug.LogError("1. Pausando juego...");
        menuP.PausarJuego();
        
        Invoke("ProbarAbrirOpciones", 1f);
    }
    
    private void ProbarAbrirOpciones()
    {
        MenuPausa menuP = FindAnyObjectByType<MenuPausa>();
        if (menuP != null)
        {
            Debug.LogError("2. Abriendo opciones...");
            menuP.AbrirOpciones();
        }
    }
    
    [ContextMenu("🔍 DIAGNOSTICAR PROBLEMA")]
    public void DiagnosticarProblema()
    {
        Debug.LogError("🔍 INICIANDO DIAGNÓSTICO COMPLETO");
        
        // 1. Buscar MenuPausa
        MenuPausa[] menusPausa = FindObjectsByType<MenuPausa>(FindObjectsSortMode.None);
        Debug.LogError("📱 MenuPausa encontrados: " + menusPausa.Length);
        
        if (menusPausa.Length > 0)
        {
            MenuPausa menu = menusPausa[0];
            Debug.LogError("  - Nombre: " + menu.gameObject.name);
            Debug.LogError("  - Activo: " + menu.gameObject.activeInHierarchy);
            Debug.LogError("  - Enabled: " + menu.enabled);
            
            // 2. Verificar Canvas
            Canvas[] canvases = menu.GetComponentsInChildren<Canvas>(true);
            Debug.LogError("📺 Canvas encontrados: " + canvases.Length);
            
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas c = canvases[i];
                Debug.LogError("  Canvas " + (i+1) + ": " + c.gameObject.name + " | Activo: " + c.gameObject.activeInHierarchy);
                
                // 3. Verificar hijos del Canvas
                Transform[] hijos = c.GetComponentsInChildren<Transform>(true);
                Debug.LogError("    - Hijos totales: " + hijos.Length);
                
                foreach (Transform hijo in hijos)
                {
                    if (hijo.name.Contains("Panel") || hijo.name.Contains("Opciones"))
                    {
                        Debug.LogError("      🔍 " + hijo.name + " | Activo: " + hijo.gameObject.activeInHierarchy);
                    }
                }
            }
            
            // 4. Verificar MenuOpciones
            MenuOpciones menuOpc = menu.GetComponent<MenuOpciones>();
            Debug.LogError("⚙️ MenuOpciones: " + (menuOpc != null ? "✅ SÍ" : "❌ NO"));
            
            if (menuOpc != null)
            {
                Debug.LogError("  - Enabled: " + menuOpc.enabled);
                Debug.LogError("  - UI Creada: " + menuOpc.EstaUICreada());
            }
        }
        
        Debug.LogError("🔍 DIAGNÓSTICO TERMINADO");
    }
    
    [ContextMenu("💪 FORZAR SOLUCIÓN COMPLETA")]
    public void ForzarSolucionCompleta()
    {
        Debug.LogError("💪 FORZANDO SOLUCIÓN COMPLETA DEL PROBLEMA");
        
        // 1. Destruir MenuPausa existentes
        MenuPausa[] menusExistentes = FindObjectsByType<MenuPausa>(FindObjectsSortMode.None);
        foreach (MenuPausa menu in menusExistentes)
        {
            Debug.LogError("🗑️ Destruyendo MenuPausa existente: " + menu.gameObject.name);
            DestroyImmediate(menu.gameObject);
        }
        
        // 2. Crear nuevo GameObject con ambos componentes
        GameObject nuevoMenu = new GameObject("MenuPausa_Completo");
        DontDestroyOnLoad(nuevoMenu); // Para que persista entre escenas
        
        // 3. Agregar MenuPausa
        MenuPausa nuevoPausa = nuevoMenu.AddComponent<MenuPausa>();
        
        // 4. Agregar MenuOpciones
        MenuOpciones nuevasOpciones = nuevoMenu.AddComponent<MenuOpciones>();
        
        // 5. Configurar automáticamente
        Debug.LogError("🔧 Creando UI automáticamente...");
        nuevoPausa.CrearUICompleta();
        
        Debug.LogError("✅ SOLUCIÓN COMPLETA APLICADA");
        Debug.LogError("🎮 Ahora presiona ESC para probar el menú");
    }
}
