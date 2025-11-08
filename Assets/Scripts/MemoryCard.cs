using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemoryCard : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int cardValue;
    [HideInInspector] public bool isMatched = false;
    [HideInInspector] public bool isFlipped = false;

    private PairMatchingController gameController;
    private Image cardImage;
    private Sprite frontSprite;
    private Sprite backSprite;

    void Awake()
    {
        cardImage = GetComponent<Image>();
    }

    public void Initialize(int value, Sprite front, Sprite back, PairMatchingController controller)
    {
        cardValue = value;
        frontSprite = front;
        backSprite = back;
        gameController = controller;

        // Start with card back showing
        cardImage.sprite = backSprite;
        isFlipped = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isMatched)
        {
            gameController.OnCardClicked(this);
        }
    }

    public void FlipCard()
    {
        if (!isFlipped && !isMatched)
        {
            // Flip to show front
            StartCoroutine(FlipAnimation(frontSprite));
            isFlipped = true;
        }
        else if (isFlipped && !isMatched)
        {
            // Flip to show back
            StartCoroutine(FlipAnimation(backSprite));
            isFlipped = false;
        }
    }

    private System.Collections.IEnumerator FlipAnimation(Sprite targetSprite)
    {
        float duration = gameController.flipDuration / 2;

        // First half - scale down
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(1f, 0.1f, t / duration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        // Change sprite
        cardImage.sprite = targetSprite;

        // Second half - scale back up
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float scale = Mathf.Lerp(0.1f, 1f, t / duration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    public void PlayMatchAnimation()
    {
        StartCoroutine(MatchAnimation());
    }

    private System.Collections.IEnumerator MatchAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Color originalColor = cardImage.color;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Scale pulse
            float scale = 1f + 0.3f * Mathf.Sin(progress * Mathf.PI * 2);
            transform.localScale = originalScale * scale;

            // Color glow
            Color glowColor = new Color(1f, 1f, 0.8f, 1f);
            cardImage.color = Color.Lerp(originalColor, glowColor, Mathf.PingPong(progress * 2f, 1f));

            yield return null;
        }

        transform.localScale = originalScale;
        cardImage.color = originalColor;
    }

    public void PlayMismatchAnimation()
    {
        StartCoroutine(MismatchAnimation());
    }

    private System.Collections.IEnumerator MismatchAnimation()
    {
        Vector3 originalPosition = transform.localPosition;
        float duration = 0.3f;
        float elapsed = 0f;

        // Gentle shake animation
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 30f) * 5f;
            transform.localPosition = originalPosition + new Vector3(shake, 0, 0);
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}