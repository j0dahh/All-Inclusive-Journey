using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeSorterController : MonoBehaviour
{
    [Header("Game References")]
    public Transform targetBoxesParent;        // Parent for the 4 target boxes
    public Transform draggableShapesParent;    // Parent for draggable shapes
    public GameObject draggableShapePrefab;    // Prefab for shapes
    public GameObject targetBoxPrefab;         // Prefab for target boxes

    [Header("Shape Settings")]
    public Sprite circleSprite;
    public Sprite squareSprite;
    public Sprite triangleSprite;
    public Sprite starSprite;

    [Header("Game Settings")]
    public int shapesPerType = 3;              // How many of each shape
    public float matchDistance = 120f;

    [Header("Cozy Colors")]
    public Color[] shapeColors = new Color[] {
        new Color(0.85f, 0.65f, 0.75f, 1.0f), // Dusty Rose
        new Color(0.65f, 0.85f, 0.65f, 1.0f), // Sage Green
        new Color(0.70f, 0.80f, 0.90f, 1.0f), // Sky Blue
        new Color(0.95f, 0.80f, 0.60f, 1.0f)  // Warm Peach
    };

    [Header("Sound Effects")]
    public AudioSource successSound;  // Sound when match is successful
    public AudioSource failSound;     // Sound when match fails
    public AudioSource winSound;      // Sound when game is won

    [Header("Win Screen")]
    public GameObject winScreen;      // Reference to win screen UI

    private List<TargetBox> targetBoxes = new List<TargetBox>();
    private List<DraggableShape> draggableShapes = new List<DraggableShape>();
    private int matchesFound = 0;

    // Define our shape types
    public enum ShapeType { Circle, Square, Triangle, Star }

    // Pre-defined grid positions - GUARANTEED no overlap
    // Pre-defined grid positions - GUARANTEED no overlap and away from center boxes
    // Screen-aware grid positions - perfectly calibrated for 1080x1920 portrait
    private Vector2[] gridPositions = {
    // TOP ROW - safe from top edge and center
    new Vector2(-300, 700), new Vector2(-150, 700), new Vector2(0, 700), new Vector2(150, 700), new Vector2(300, 700),
    
    // UPPER MIDDLE ROW - good distance from center boxes
    new Vector2(-350, 500), new Vector2(-200, 500), new Vector2(-50, 500),
    new Vector2(50, 500), new Vector2(200, 500), new Vector2(350, 500),
    
    // LOWER MIDDLE ROW - good distance from center boxes  
    new Vector2(-350, -500), new Vector2(-200, -500), new Vector2(-50, -500),
    new Vector2(50, -500), new Vector2(200, -500), new Vector2(350, -500),
    
    // BOTTOM ROW - safe from bottom edge and center
    new Vector2(-300, -700), new Vector2(-150, -700), new Vector2(0, -700), new Vector2(150, -700), new Vector2(300, -700),
    
    // LEFT COLUMN - safe from left edge
    new Vector2(-450, 300), new Vector2(-450, 150), new Vector2(-450, 0), new Vector2(-450, -150), new Vector2(-450, -300),
    
    // RIGHT COLUMN - safe from right edge
    new Vector2(450, 300), new Vector2(450, 150), new Vector2(450, 0), new Vector2(450, -150), new Vector2(450, -300)
};

    void Start()
    {
        CreateGame();
    }

    void CreateGame()
    {
        // Clear existing objects
        foreach (Transform child in targetBoxesParent) Destroy(child.gameObject);
        foreach (Transform child in draggableShapesParent) Destroy(child.gameObject);

        targetBoxes.Clear();
        draggableShapes.Clear();
        matchesFound = 0;

        CreateTargetBoxes();
        CreateDraggableShapes();
    }

    void CreateTargetBoxes()
    {
        // Create 4 target boxes in the middle
        ShapeType[] shapeTypes = { ShapeType.Circle, ShapeType.Square, ShapeType.Triangle, ShapeType.Star };

        for (int i = 0; i < 4; i++)
        {
            GameObject box = Instantiate(targetBoxPrefab, targetBoxesParent);
            TargetBox targetBox = box.GetComponent<TargetBox>();

            // Position boxes in a 2x2 grid in the middle
            RectTransform rt = box.GetComponent<RectTransform>();
            float xPos = (i % 2 == 0) ? -150 : 150;
            float yPos = (i < 2) ? 150 : -150;
            rt.anchoredPosition = new Vector2(xPos, yPos);

            targetBox.Initialize(shapeTypes[i], GetShapeSprite(shapeTypes[i]), this);
            targetBoxes.Add(targetBox);
        }
    }

    void CreateDraggableShapes()
    {
        List<ShapeType> shapesToCreate = new List<ShapeType>();

        // Create multiple of each shape type
        for (int i = 0; i < shapesPerType; i++)
        {
            shapesToCreate.Add(ShapeType.Circle);
            shapesToCreate.Add(ShapeType.Square);
            shapesToCreate.Add(ShapeType.Triangle);
            shapesToCreate.Add(ShapeType.Star);
        }

        // Shuffle the shapes
        ShuffleList(shapesToCreate);

        // Create a shuffled list of grid positions
        List<Vector2> availablePositions = new List<Vector2>(gridPositions);
        ShuffleList(availablePositions);

        // Make sure we don't create more shapes than available positions
        int shapesToCreateCount = Mathf.Min(shapesToCreate.Count, availablePositions.Count);

        Debug.Log($"Creating {shapesToCreateCount} shapes at {availablePositions.Count} available positions");

        for (int i = 0; i < shapesToCreateCount; i++)
        {
            GameObject shape = Instantiate(draggableShapePrefab, draggableShapesParent);
            DraggableShape draggable = shape.GetComponent<DraggableShape>();

            // Use pre-defined grid position - GUARANTEED no overlap
            Vector2 shapePos = availablePositions[i];
            RectTransform rt = shape.GetComponent<RectTransform>();
            rt.anchoredPosition = shapePos;

            // Random color
            Color randomColor = shapeColors[Random.Range(0, shapeColors.Length)];

            draggable.Initialize(shapesToCreate[i], GetShapeSprite(shapesToCreate[i]), randomColor, this);
            draggableShapes.Add(draggable);

            Debug.Log($"Created {shapesToCreate[i]} at grid position: {shapePos}");
        }

        if (shapesToCreate.Count > availablePositions.Count)
        {
            Debug.LogWarning($"Not enough grid positions! Created {shapesToCreateCount} out of {shapesToCreate.Count} shapes");
        }
    }

    Sprite GetShapeSprite(ShapeType shapeType)
    {
        switch (shapeType)
        {
            case ShapeType.Circle: return circleSprite;
            case ShapeType.Square: return squareSprite;
            case ShapeType.Triangle: return triangleSprite;
            case ShapeType.Star: return starSprite;
            default: return circleSprite;
        }
    }

    public void CheckForMatch(DraggableShape draggedShape, Vector2 dropPosition)
    {
        foreach (TargetBox targetBox in targetBoxes)
        {
            float distance = Vector2.Distance(targetBox.transform.position, dropPosition);

            if (distance <= matchDistance)
            {
                // Check if shapes match
                if (targetBox.shapeType == draggedShape.shapeType)
                {
                    // Successful match!
                    // Play success sound
                    if (successSound != null)
                    {
                        successSound.Play();
                    }

                    StartCoroutine(HandleSuccessfulMatch(draggedShape, targetBox));
                    return;
                }
            }
        }

        // No match found - return to start position
        // Play fail sound
        if (failSound != null)
        {
            failSound.Play();
        }

        draggedShape.ReturnToStartPosition();
    }

    IEnumerator HandleSuccessfulMatch(DraggableShape draggedShape, TargetBox targetBox)
    {
        matchesFound++;
        draggedShape.isMatched = true;

        // Disable dragging
        draggedShape.GetComponent<CanvasGroup>().blocksRaycasts = false;

        // Play match animation
        draggedShape.PlayMatchAnimation();
        targetBox.PlayMatchAnimation();

        yield return new WaitForSeconds(0.8f);

        // Make shape invisible but keep it to maintain layout
        Image img = draggedShape.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        // Check if ALL draggable shapes are matched (win condition)
        int totalMatchedShapes = 0;
        foreach (DraggableShape shape in draggableShapes)
        {
            if (shape.isMatched)
            {
                totalMatchedShapes++;
            }
        }

        if (totalMatchedShapes >= draggableShapes.Count)
        {
            yield return new WaitForSeconds(0.5f); // Small delay before win screen
            ShowWinScreen();
        }
    }

    // Add this new method for showing win screen
    private void ShowWinScreen()
    {
        Debug.Log("Shape Sorter Complete! Showing win screen!");

        // Play win sound
        if (winSound != null)
        {
            winSound.Play();
        }

        // Show the win screen
        if (winScreen != null)
        {
            winScreen.SetActive(true);

            // Optional: Add a gentle scale animation
            winScreen.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleWinScreen());
        }
        else
        {
            Debug.LogWarning("Win screen reference is not set!");
        }
    }

    // Smooth animation for win screen appearance
    private System.Collections.IEnumerator ScaleWinScreen()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            winScreen.transform.localScale = Vector3.one * progress;
            yield return null;
        }

        winScreen.transform.localScale = Vector3.one;
    }

    // Add this method for the replay button
    public void ReplayGame()
    {
        Debug.Log("Replaying Shape Sorter!");

        // Hide win screen
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        // Reset and create new game
        CreateGame();
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}