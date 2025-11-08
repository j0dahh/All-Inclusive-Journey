using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableColorItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Color itemColor;
    private ColorMatcherController gameController;
    private Vector2 startPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 layoutPosition;

    public bool isMatched = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(Color color, ColorMatcherController controller)
    {
        itemColor = color;
        gameController = controller;
        GetComponent<Image>().color = color;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Store the exact starting position in world space
        startPosition = rectTransform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        // Convert screen position to world position for smooth dragging
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint);
        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check for match at drop position
        gameController.CheckForMatch(this, rectTransform.position);
    }

    public void ReturnToStartPosition()
    {
        if (!isMatched)
        {
            // Return to the exact original world position
            rectTransform.position = startPosition;
        }
    }

    public void PlayMatchAnimation()
    {
        StartCoroutine(SimpleMatchAnimation());
    }

    private System.Collections.IEnumerator SimpleMatchAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Image img = GetComponent<Image>();
        Color originalColor = img.color;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Scale pulse
            float scale = 1f + 0.5f * Mathf.Sin(progress * Mathf.PI * 2);
            transform.localScale = originalScale * scale;

            // Color brighten and fade
            Color brightColor = new Color(originalColor.r * 2f, originalColor.g * 2f, originalColor.b * 2f);
            float colorProgress = Mathf.PingPong(progress * 3f, 1f);
            img.color = Color.Lerp(originalColor, brightColor, colorProgress);

            // Gentle rotation during animation
            float rotation = Mathf.Sin(progress * Mathf.PI * 4) * 15f;
            transform.rotation = Quaternion.Euler(0, 0, rotation);

            yield return null;
        }

        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        img.color = originalColor;
    }

    // Remove the StoreLayoutPosition method - we don't need it anymore
}