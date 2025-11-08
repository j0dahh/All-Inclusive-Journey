using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ShapeSorterController.ShapeType shapeType;
    [HideInInspector] public bool isMatched = false;

    private ShapeSorterController gameController;
    private Vector2 startPosition;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(ShapeSorterController.ShapeType type, Sprite sprite, Color color, ShapeSorterController controller)
    {
        shapeType = type;
        gameController = controller;

        Image img = GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;

        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint);
        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isMatched) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        gameController.CheckForMatch(this, rectTransform.position);
    }

    public void ReturnToStartPosition()
    {
        if (!isMatched)
        {
            rectTransform.anchoredPosition = startPosition;
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

            // Color brighten
            Color brightColor = new Color(originalColor.r * 2f, originalColor.g * 2f, originalColor.b * 2f);
            float colorProgress = Mathf.PingPong(progress * 3f, 1f);
            img.color = Color.Lerp(originalColor, brightColor, colorProgress);

            // Gentle rotation
            float rotation = Mathf.Sin(progress * Mathf.PI * 4) * 15f;
            transform.rotation = Quaternion.Euler(0, 0, rotation);

            yield return null;
        }

        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        img.color = originalColor;
    }
}