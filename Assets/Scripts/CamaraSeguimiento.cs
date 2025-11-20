using UnityEngine;

public class CamaraSeguimiento : MonoBehaviour
{
    [SerializeField] private Transform objetivo; // El jugador
    [SerializeField] private float suavizado = 0.125f; // Qué tan suave es el seguimiento
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Distancia de la cámara
    
    void LateUpdate()
    {
        if (objetivo != null)
        {
            Vector3 posicionDeseada = objetivo.position + offset;
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
            transform.position = posicionSuavizada;
        }
    }
}