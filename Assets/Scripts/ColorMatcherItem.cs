using UnityEngine;
using UnityEngine.UI;

public class ColorMatcherItem : MonoBehaviour
{
    [HideInInspector] public Color itemColor;
    [HideInInspector] public bool isMatched = false;

    private ColorMatcherController gameController;
    private Vector3 originalScale;

    public void Initialize(Color color, ColorMatcherController controller)
    {
        itemColor = color;
        gameController = controller;
        originalScale = transform.localScale;

        // Make the top item slightly different - add a border or different shape
        GetComponent<Image>().color = color;
    }

    public void PlayMatchAnimation()
    {
        // Play a matching animation without LeanTween
        StartCoroutine(SimpleMatchAnimation());
    }

    private System.Collections.IEnumerator SimpleMatchAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Pulse effect
            transform.localScale = originalScale * (1f + 0.2f * Mathf.Sin(progress * Mathf.PI));

            // Color brighten effect
            Image img = GetComponent<Image>();
            Color brighterColor = new Color(itemColor.r * (1f + 0.3f * progress),
                                          itemColor.g * (1f + 0.3f * progress),
                                          itemColor.b * (1f + 0.3f * progress));
            img.color = brighterColor;

            yield return null;
        }

        // Return to original
        transform.localScale = originalScale;
        GetComponent<Image>().color = itemColor;
    }
}