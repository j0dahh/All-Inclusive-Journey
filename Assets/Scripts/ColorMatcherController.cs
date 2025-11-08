using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorMatcherController : MonoBehaviour
{
    [Header("Game References")]
    public Transform topRowParent;    // Empty GameObject to hold top row items
    public Transform bottomRowParent; // Empty GameObject to hold bottom row items
    public GameObject draggableItemPrefab; // Prefab for the bottom row items

    [Header("Cozy Colors")]
    public Color[] cozyColors = new Color[] {
        new Color(0.85f, 0.65f, 0.75f, 1.0f), // Dusty Rose
        new Color(0.65f, 0.85f, 0.65f, 1.0f), // Sage Green
        new Color(0.70f, 0.80f, 0.90f, 1.0f), // Sky Blue
        new Color(0.95f, 0.80f, 0.60f, 1.0f)  // Warm Peach
    };

    [Header("Sound Effects")]
    public AudioSource successSound;  // Sound when match is successful
    public AudioSource failSound;     // Sound when match fails

    [Header("Game Settings")]
    public int topRowCount = 4;          // Number of target squares at top
    public int bottomRowCount = 8;       // Number of draggable squares at bottom
    public float matchDistance = 150f;   // How close items need to be to match

    private List<ColorMatcherItem> topRowItems = new List<ColorMatcherItem>();
    private List<DraggableColorItem> bottomRowItems = new List<DraggableColorItem>();
    private int matchesFound = 0;

    [Header("Win Screen")]
    public GameObject winScreen; // Reference to the WinScreen panel

    [Header("Sound Effects")]
    public AudioSource winSound; // Optional: Add win sound

    private Dictionary<Color, int> colorMatchesNeeded = new Dictionary<Color, int>();
    void Start()
    {
        Debug.Log("=== COLOR MATCHER STARTING ===");
        CreateGame();
    }

    void CreateGame()
    {
        // Clear any existing items
        foreach (Transform child in topRowParent) Destroy(child.gameObject);
        foreach (Transform child in bottomRowParent) Destroy(child.gameObject);

        topRowItems.Clear();
        bottomRowItems.Clear();
        matchesFound = 0;
        colorMatchesNeeded.Clear(); // Clear the dictionary

        // Create colors for top row (4 unique colors)
        List<Color> topRowColors = new List<Color>();
        for (int i = 0; i < topRowCount; i++)
        {
            topRowColors.Add(cozyColors[i % cozyColors.Length]);
        }

        // Calculate how many matches we need for each color
        foreach (Color color in topRowColors)
        {
            // Count how many bottom items have this color
            int matchesForThisColor = 0;
            for (int i = 0; i < bottomRowCount; i++)
            {
                if (topRowColors[i % topRowColors.Count] == color)
                {
                    matchesForThisColor++;
                }
            }
            colorMatchesNeeded[color] = matchesForThisColor;
        }

        // Create colors for bottom row (includes duplicates of top row colors)
        List<Color> bottomRowColors = new List<Color>();
        for (int i = 0; i < bottomRowCount; i++)
        {
            Color color = topRowColors[i % topRowColors.Count];
            bottomRowColors.Add(color);
        }

        // Shuffle the bottom row colors
        ShuffleList(bottomRowColors);

        Debug.Log("Creating top row items...");
        // Create top row items (non-draggable targets)
        foreach (Color color in topRowColors)
        {
            CreateTopRowItem(color);
        }

        Debug.Log("Creating bottom row items...");
        // Create bottom row items (draggable)
        foreach (Color color in bottomRowColors)
        {
            CreateBottomRowItem(color);
        }

        Debug.Log("Game creation complete! Top: " + topRowItems.Count + ", Bottom: " + bottomRowItems.Count);

        // Log how many matches are needed for each color
        foreach (var pair in colorMatchesNeeded)
        {
            Debug.Log("Need " + pair.Value + " matches for color: " + pair.Key);
        }
    }

    void CreateTopRowItem(Color color)
    {
        // Create a simple Image for the top row
        GameObject item = new GameObject("TopItem", typeof(RectTransform), typeof(Image));
        item.transform.SetParent(topRowParent, false);

        // Force the size before Layout Group tries to control it
        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 180);

        // Add Layout Element
        LayoutElement layoutElement = item.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 180;
        layoutElement.preferredHeight = 180;
        layoutElement.minWidth = 180;
        layoutElement.minHeight = 180;

        Image img = item.GetComponent<Image>();
        img.color = color;
        img.sprite = Resources.GetBuiltinResource<Sprite>("UI/Sprite");

        // Make top items look different - add a border effect
        Outline outline = item.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, 3);

        // Add the ColorMatcherItem component to identify this target
        ColorMatcherItem colorItem = item.AddComponent<ColorMatcherItem>();
        colorItem.Initialize(color, this);

        topRowItems.Add(colorItem);
    }

    void CreateBottomRowItem(Color color)
    {
        if (draggableItemPrefab == null)
        {
            Debug.LogError("Draggable Item Prefab is not assigned!");
            return;
        }

        GameObject item = Instantiate(draggableItemPrefab, bottomRowParent);

        // Add Layout Element
        LayoutElement layoutElement = item.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = item.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = 160;
        layoutElement.preferredHeight = 160;
        layoutElement.minWidth = 160;
        layoutElement.minHeight = 160;

        DraggableColorItem draggable = item.GetComponent<DraggableColorItem>();

        if (draggable != null)
        {
            draggable.Initialize(color, this);
            bottomRowItems.Add(draggable);

            // Store the original position after layout is calculated
        
        }
    }

    // Helper coroutine to store position after layout is calculated


    // Called when a draggable item is released
    // Called when a draggable item is released
    // Called when a draggable item is released
    // Called when a draggable item is released
    public void CheckForMatch(DraggableColorItem draggedItem, Vector2 dropPosition)
    {
        bool foundMatch = false;

        foreach (ColorMatcherItem target in topRowItems)
        {
            float distance = Vector2.Distance(target.transform.position, dropPosition);

            if (distance <= matchDistance)
            {
                // Check if colors match
                if (ColorsMatch(target.itemColor, draggedItem.itemColor))
                {
                    // Successful match!
                    foundMatch = true;

                    // Play success sound
                    if (successSound != null)
                    {
                        successSound.Play();
                    }

                    StartCoroutine(HandleSuccessfulMatch(draggedItem, target));
                    return;
                }
            }
        }

        // Only return to start position if NO match was found
        if (!foundMatch)
        {
            // Play fail sound
            if (failSound != null)
            {
                failSound.Play();
            }

            draggedItem.ReturnToStartPosition();
        }
    }

    IEnumerator HandleSuccessfulMatch(DraggableColorItem draggedItem, ColorMatcherItem target)
    {
        matchesFound++;
        draggedItem.isMatched = true;

        // Disable dragging
        CanvasGroup cg = draggedItem.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Play match animation at the current position
        draggedItem.PlayMatchAnimation();
        target.PlayMatchAnimation();

        yield return new WaitForSeconds(0.8f);

        // Make it invisible but keep it in the scene to maintain grid space
        Image img = draggedItem.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        Outline outline = draggedItem.GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        Shadow shadow = draggedItem.GetComponent<Shadow>();
        if (shadow != null) shadow.enabled = false;

        // Check if ALL colors have been completely matched
        if (CheckAllColorsMatched())
        {
            yield return new WaitForSeconds(0.5f); // Small delay before win screen
            ShowWinScreen();
        }
    }

    // Add this new method to check if all colors are completely matched
    private bool CheckAllColorsMatched()
    {
        // Count how many matches we have for each color
        Dictionary<Color, int> currentMatches = new Dictionary<Color, int>();

        foreach (DraggableColorItem bottomItem in bottomRowItems)
        {
            if (bottomItem.isMatched)
            {
                Color itemColor = bottomItem.itemColor;
                if (currentMatches.ContainsKey(itemColor))
                {
                    currentMatches[itemColor]++;
                }
                else
                {
                    currentMatches[itemColor] = 1;
                }
            }
        }

        // Check if we have enough matches for each color
        foreach (var pair in colorMatchesNeeded)
        {
            Color color = pair.Key;
            int needed = pair.Value;
            int current = currentMatches.ContainsKey(color) ? currentMatches[color] : 0;

            if (current < needed)
            {
                return false; // This color doesn't have enough matches yet
            }
        }

        return true; // All colors have enough matches!
    }

    // Add this new method for showing win screen
    private void ShowWinScreen()
    {
        Debug.Log("Game Complete! Showing win screen!");

        // Play win sound if available
        if (winSound != null)
        {
            winSound.Play();
        }

        // Show the win screen
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        // Optional: Add a gentle scale animation to the win screen
        if (winScreen != null)
        {
            winScreen.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleWinScreen());
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
        Debug.Log("Replaying game!");

        // Hide win screen
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

        // Reset and create new game
        CreateGame();
    }

    bool ColorsMatch(Color a, Color b)
    {
        return ColorUtility.ToHtmlStringRGBA(a) == ColorUtility.ToHtmlStringRGBA(b);
    }

    // Helper method to shuffle lists
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