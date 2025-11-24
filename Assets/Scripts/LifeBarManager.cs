using UnityEngine;

public class LifeBarManager : MonoBehaviour
{
    [Header("🎨 Temas de Color Predefinidos")]
    [SerializeField] private TemaColor[] temas;
    [SerializeField] private int temaActual = 0;
    
    [Header("📱 Referencias")]
    [SerializeField] private UILifeBar lifeBar;
    
    [System.Serializable]
    public class TemaColor
    {
        public string nombre;
        public Color colorVidaCompleta;
        public Color colorVidaMedia;
        public Color colorVidaBaja;
    }
    
    void Start()
    {
        if (lifeBar == null)
        {
            lifeBar = FindObjectOfType<UILifeBar>();
        }
        
        InicializarTemas();
        AplicarTema(temaActual);
    }
    
    private void InicializarTemas()
    {
        if (temas == null || temas.Length == 0)
        {
            temas = new TemaColor[]
            {
                new TemaColor 
                { 
                    nombre = "Clásico",
                    colorVidaCompleta = Color.green,
                    colorVidaMedia = Color.yellow,
                    colorVidaBaja = Color.red 
                },
                new TemaColor 
                { 
                    nombre = "Azul",
                    colorVidaCompleta = Color.cyan,
                    colorVidaMedia = Color.blue,
                    colorVidaBaja = Color.magenta 
                },
                new TemaColor 
                { 
                    nombre = "Fuego",
                    colorVidaCompleta = Color.white,
                    colorVidaMedia = new Color(1f, 0.5f, 0f), // Naranja
                    colorVidaBaja = Color.red 
                }
            };
        }
    }
    
    public void CambiarTema()
    {
        temaActual = (temaActual + 1) % temas.Length;
        AplicarTema(temaActual);
        
        Debug.LogError($"🎨 TEMA CAMBIADO A: {temas[temaActual].nombre}");
    }
    
    public void AplicarTema(int indice)
    {
        if (indice >= 0 && indice < temas.Length && lifeBar != null)
        {
            TemaColor tema = temas[indice];
            lifeBar.ConfigurarColores(tema.colorVidaCompleta, tema.colorVidaMedia, tema.colorVidaBaja);
        }
    }
    
    void Update()
    {
        // Test para cambiar temas
        if (Input.GetKeyDown(KeyCode.T))
        {
            CambiarTema();
        }
    }
}
