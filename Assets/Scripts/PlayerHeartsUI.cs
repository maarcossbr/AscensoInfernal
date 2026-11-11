
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

    private Sprite generatedEmptyHeart;
    private Texture2D generatedEmptyTexture;

    private void Awake()
    {
        Sprite original = fullHeart;

        // Si no hay sprite asignado, usar el primer corazon.
        if (original == null && hearts != null)
        {
            foreach (Image heart in hearts)
            {
                if (heart != null && heart.sprite != null)
                {
                    original = heart.sprite;
                    break;
                }
            }
        }

        if (original != null && emptyHeart == null)
        {
            Rect rect = original.rect;

            int width = (int)rect.width;
            int height = (int)rect.height;

            Color[] pixels = original.texture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                width,
                height
            );

            // Convertir los colores a gris oscuro,
            // conservando el contorno y la transparencia.
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];

                float brillo = Mathf.Max(
                    pixel.r,
                    pixel.g,
                    pixel.b
                );

                float gris = brillo * 0.42f;

                pixels[i] = new Color(
                    gris,
                    gris,
                    gris,
                    pixel.a
                );
            }

            generatedEmptyTexture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );

            generatedEmptyTexture.filterMode = FilterMode.Point;
            generatedEmptyTexture.wrapMode = TextureWrapMode.Clamp;
            generatedEmptyTexture.SetPixels(pixels);
            generatedEmptyTexture.Apply();

            Vector2 pivot = new Vector2(
                original.pivot.x / width,
                original.pivot.y / height
            );

            generatedEmptyHeart = Sprite.Create(
                generatedEmptyTexture,
                new Rect(0, 0, width, height),
                pivot,
                original.pixelsPerUnit
            );
        }
    }

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

    private void OnDestroy()
    {
        if (generatedEmptyHeart != null)
            Destroy(generatedEmptyHeart);

        if (generatedEmptyTexture != null)
            Destroy(generatedEmptyTexture);
    }

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
                continue;

            bool lleno = i < current;

            Sprite sprite = lleno
                ? fullHeart
                : (emptyHeart != null ? emptyHeart : generatedEmptyHeart);

            if (sprite != null)
                hearts[i].sprite = sprite;

            hearts[i].color = Color.white;
        }
    }
}
