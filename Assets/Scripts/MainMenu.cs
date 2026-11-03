using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelControles;
    [SerializeField] private string escenaNivel = "Prototipo_Level01";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (panelControles != null) panelControles.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Cambiar a la escena con el nivel
    public void CambiarEscena()
    {
        SceneManager.LoadScene(escenaNivel);
    }

    //Abrir panel con los controles
    public void AbrirControles()
    {
        if (panelControles != null) panelControles.SetActive(true);
    }

    //Cerrar panel con los controles
    public void CerrarControles()
    {
        if (panelControles != null) panelControles.SetActive(false);
    }
}
