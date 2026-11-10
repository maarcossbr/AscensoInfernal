
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeartsUI : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Corazones")]
    [SerializeField] private Image[] hearts;

    [Header("Sprites opcionales")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged += UpdateHearts;
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            UpdateHearts(
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth
            );
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            bool lleno = i < current;

            Sprite sprite = lleno ? fullHeart : emptyHeart;

            if (sprite != null)
            {
                hearts[i].sprite = sprite;
                hearts[i].color = Color.white;
            }
            else
            {
                hearts[i].color = lleno
                    ? Color.red
                    : Color.gray;
            }
        }
    }
}
