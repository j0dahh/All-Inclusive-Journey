using UnityEngine;
using UnityEngine.UI;

public class TargetBox : MonoBehaviour
{
    [HideInInspector] public ShapeSorterController.ShapeType shapeType;

    private ShapeSorterController gameController;
    private Vector3 originalScale;
    private int shapesMatched = 0; // Track how many shapes matched this box

    public void Initialize(ShapeSorterController.ShapeType type, Sprite outlineSprite, ShapeSorterController controller)
    {
        shapeType = type;
        gameController = controller;
        originalScale = transform.localScale;

        // Set up the box appearance (outline of the shape)
        Image img = GetComponent<Image>();
        img.sprite = outlineSprite;
        img.color = new Color(1, 1, 1, 0.3f); // Semi-transparent white

        // Add a subtle outline
        Outline outline = gameObject.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, 2);
    }

    public void PlayMatchAnimation()
    {
        shapesMatched++;
        StartCoroutine(SimpleMatchAnimation());
    }

    private System.Collections.IEnumerator SimpleMatchAnimation()
    {
        Image img = GetComponent<Image>();
        Color originalColor = img.color;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Scale pulse
            transform.localScale = originalScale * (1f + 0.3f * Mathf.Sin(progress * Mathf.PI));

            // Make the box more visible with each match
            float targetAlpha = 0.3f + (shapesMatched * 0.2f); // Gets more opaque with more matches
            targetAlpha = Mathf.Min(targetAlpha, 0.9f); // Don't go fully opaque
            Color targetColor = new Color(1, 1, 1, targetAlpha);
            img.color = Color.Lerp(originalColor, targetColor, progress);

            yield return null;
        }

        transform.localScale = originalScale;
    }
}