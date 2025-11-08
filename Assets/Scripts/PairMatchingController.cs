using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PairMatchingController : MonoBehaviour
{
    [Header("Game References")]
    public Transform cardGridParent;           // Parent for the card grid
    public GameObject cardPrefab;              // Prefab for cards

    [Header("Card Settings")]
    public Sprite[] cardSprites;               // All available card images
    public Sprite cardBackSprite;              // Back of the card

    [Header("Game Settings")]
    public int gridColumns = 4;                // Cards per row
    public int gridRows = 3;                   // Number of rows
    public float flipDuration = 0.3f;
    public float matchAnimationDuration = 0.8f;

    [Header("Sound Effects")]
    public AudioSource flipSound;
    public AudioSource matchSound;
    public AudioSource mismatchSound;
    public AudioSource winSound;

    [Header("Win Screen")]
    public GameObject winScreen;

    private List<MemoryCard> cards = new List<MemoryCard>();
    private MemoryCard firstSelectedCard;
    private MemoryCard secondSelectedCard;
    private bool canFlip = true;
    private int matchesFound = 0;
    private int totalPairs;

    void Start()
    {
        CreateGame();
    }

    void CreateGame()
    {
        // Clear existing cards
        foreach (Transform child in cardGridParent) Destroy(child.gameObject);
        cards.Clear();

        firstSelectedCard = null;
        secondSelectedCard = null;
        canFlip = true;
        matchesFound = 0;

        // Calculate total pairs
        totalPairs = (gridColumns * gridRows) / 2;

        SetupCards();
        LayoutCards();
    }

    void SetupCards()
    {
        List<int> cardValues = new List<int>();

        // Create pairs of card values
        for (int i = 0; i < totalPairs; i++)
        {
            int spriteIndex = i % cardSprites.Length;
            cardValues.Add(spriteIndex);
            cardValues.Add(spriteIndex);
        }

        // Shuffle the cards
        ShuffleList(cardValues);

        // Create card objects
        for (int i = 0; i < cardValues.Count; i++)
        {
            GameObject cardObject = Instantiate(cardPrefab, cardGridParent);
            MemoryCard card = cardObject.GetComponent<MemoryCard>();

            card.Initialize(cardValues[i], cardSprites[cardValues[i]], cardBackSprite, this);
            cards.Add(card);
        }
    }

    void LayoutCards()
    {
        // Add or get Grid Layout Group
        GridLayoutGroup grid = cardGridParent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = cardGridParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // SET LARGER CARD SIZES HERE
        grid.cellSize = new Vector2(200, 200); // Increased from 150,150 to 200,200
        grid.spacing = new Vector2(25, 25);    // Increased spacing too
        grid.childAlignment = TextAnchor.MiddleCenter;

        // Add Content Size Fitter for responsive layout
        ContentSizeFitter sizeFitter = cardGridParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = cardGridParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public void OnCardClicked(MemoryCard clickedCard)
    {
        if (!canFlip || clickedCard.isMatched || clickedCard == firstSelectedCard)
            return;

        // Play flip sound
        if (flipSound != null) flipSound.Play();

        // Flip the card
        clickedCard.FlipCard();

        if (firstSelectedCard == null)
        {
            // First card selection
            firstSelectedCard = clickedCard;
        }
        else
        {
            // Second card selection
            secondSelectedCard = clickedCard;
            canFlip = false;

            StartCoroutine(CheckForMatch());
        }
    }

    IEnumerator CheckForMatch()
    {
        yield return new WaitForSeconds(0.5f); // Brief pause to see both cards

        if (firstSelectedCard.cardValue == secondSelectedCard.cardValue)
        {
            // Match found!
            if (matchSound != null) matchSound.Play();

            // Play match animation on both cards
            firstSelectedCard.PlayMatchAnimation();
            secondSelectedCard.PlayMatchAnimation();

            matchesFound++;

            yield return new WaitForSeconds(matchAnimationDuration);

            // Mark as matched
            firstSelectedCard.isMatched = true;
            secondSelectedCard.isMatched = true;

            // Check for win
            if (matchesFound >= totalPairs)
            {
                yield return new WaitForSeconds(0.5f);
                ShowWinScreen();
            }
        }
        else
        {
            // No match
            if (mismatchSound != null) mismatchSound.Play();

            // Play mismatch animation
            firstSelectedCard.PlayMismatchAnimation();
            secondSelectedCard.PlayMismatchAnimation();

            yield return new WaitForSeconds(0.5f);

            // Flip cards back
            firstSelectedCard.FlipCard();
            secondSelectedCard.FlipCard();
        }

        // Reset selection
        firstSelectedCard = null;
        secondSelectedCard = null;
        canFlip = true;
    }

    private void ShowWinScreen()
    {
        Debug.Log("Pair Matching Complete! All pairs found!");

        if (winSound != null) winSound.Play();

        if (winScreen != null)
        {
            winScreen.SetActive(true);
            winScreen.transform.localScale = Vector3.zero;
            StartCoroutine(ScaleWinScreen());
        }
    }

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

    public void ReplayGame()
    {
        Debug.Log("Replaying Pair Matching!");

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }

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