using UnityEngine;

public class UIFijoEnCamara : MonoBehaviour
{
    [Header("📷 Configuración")]
    [SerializeField] private Camera camaraObjetivo;
    [SerializeField] private bool seguirCamara = true;
    [SerializeField] private Vector3 offsetPosicion = Vector3.zero;
    [SerializeField] private bool mantenerEscala = true;
    [SerializeField] private bool mostrarDebug = false;
    
    private Canvas canvas;
    
    void Start()
    {
        // Buscar cámara si no está asignada
        if (camaraObjetivo == null)
        {
            camaraObjetivo = Camera.main;
            if (camaraObjetivo == null)
            {
                camaraObjetivo = FindObjectOfType<Camera>();
            }
        }
        
        // Configurar Canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        
        // Configurar para seguir la cámara
        if (seguirCamara && camaraObjetivo != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camaraObjetivo;
        }
        
        if (mostrarDebug)
        {
            Debug.LogError("🎨 UI FIJO EN CÁMARA CONFIGURADO");
        }
    }
    
    void LateUpdate()
    {
        if (seguirCamara && camaraObjetivo != null)
        {
            // Mantener posición relativa a la cámara
            transform.position = camaraObjetivo.transform.position + offsetPosicion;
            
            // Mantener escala original si está activado
            if (mantenerEscala)
            {
                transform.localScale = Vector3.one;
            }
        }
    }
    
    public void ConfigurarOffset(Vector3 nuevoOffset)
    {
        offsetPosicion = nuevoOffset;
    }
}