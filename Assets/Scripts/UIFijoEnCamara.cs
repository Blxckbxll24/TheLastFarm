using UnityEngine;

public class UIFijoEnCamara : MonoBehaviour
{
    [SerializeField] private Vector3 offsetPantalla = new Vector3(50, 50, 0); // Posición en pantalla
    
    private Camera cam;
    
    void Start()
    {
        cam = Camera.main;
    }
    
    void Update()
    {
        if (cam != null)
        {
            // Convertir posición de pantalla a mundo
            Vector3 posicionPantalla = new Vector3(offsetPantalla.x, Screen.height - offsetPantalla.y, offsetPantalla.z);
            Vector3 posicionMundo = cam.ScreenToWorldPoint(posicionPantalla);
            transform.position = posicionMundo;
        }
    }
}