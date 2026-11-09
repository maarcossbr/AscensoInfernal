
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerDeathUI : MonoBehaviour
{
    [Header("Interfaz de muerte")]
    [SerializeField] private GameObject panelMuerte;

    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();

        if (panelMuerte != null)
            panelMuerte.SetActive(false);
    }

    private void OnEnable()
    {
        health.Died += MostrarMuerte;
    }

    private void OnDisable()
    {
        if (health != null)
            health.Died -= MostrarMuerte;
    }

    private void MostrarMuerte()
    {
        if (panelMuerte == null)
        {
            Debug.LogError("Falta asignar el panel de muerte.");
            return;
        }

        panelMuerte.SetActive(true);
        Time.timeScale = 0f;

        Debug.Log("Pantalla de muerte activada.");
    }




    public void Reintentar()
    {
        PlayerCheckpoint checkpoint =
            GetComponent<PlayerCheckpoint>();

        if (checkpoint != null && checkpoint.HasCheckpoint)
        {
            CheckpointRespawn.PrepararRespawn(
                SceneManager.GetActiveScene().name,
                checkpoint.LastCheckpointPosition,
                checkpoint.HealthAtCheckpoint,
                checkpoint.HasLavaState,
                checkpoint.LavaPositionAtCheckpoint,
                checkpoint.LavaSpeedAtCheckpoint
            );
        }
        else
        {
            CheckpointRespawn.CancelarRespawn();
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }



    public void VolverAlMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu_Inicio");
    }

}
