using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class StoryIntroManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI storyText;
    public Button nextButton;
    public TextMeshProUGUI buttonText;

    [Header("Settings")]
    public float typewriterSpeed = 0.05f;
    public string gameSceneName = "GameScene";

    private int currentStoryIndex = 0;
    private bool isTyping = false;

    // Story segments - each element will be shown on button click
    private string[] storySegments = {
        "3:47 AM. Your apartment.",

        "Another null pointer exception crashes through your code. You've been debugging for 14 hours straight, and your coffee mug sits empty—the last drops consumed.",

        "You're WIRED. Maximum caffeine. Maximum focus.",

        "But this bug is different.\n\nIt's not just sitting there—it's learning. Adapting. Evolving.",

        "The null pointer studies your every move, anticipates your debugging strategies, and counters faster than you can think.\n\nIt's reading your mind.",

        "Reality glitches. Your apartment dims.\n\n• Stack traces float in the air\n• Error messages pulse red\n• The bug grows stronger",

        "One chance. One final debugging session before this AI-powered nightmare escapes into the wild.",

        "Time to prove you're the best debugger alive.\n\n// TODO: Save the world. Debug everything."
    };

    private string[] buttonTexts = {
        "NEXT",
        "NEXT",
        "NEXT",
        "NEXT",
        "NEXT",
        "NEXT",
        "NEXT",
        "BEGIN DEBUGGING" // Final button text
    };

    void Start()
    {
        // Initialize
        nextButton.onClick.AddListener(OnNextButtonClicked);

        // Start with first story segment
        ShowCurrentStorySegment();
    }

    void Update()
    {
        // Allow spacebar or enter to advance (like "Press SPACE to continue")
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            OnNextButtonClicked();
        }

        // Skip typewriter effect on click during typing
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            SkipTypewriter();
        }
    }

    void OnNextButtonClicked()
    {
        // If currently typing, skip to end of current text
        if (isTyping)
        {
            SkipTypewriter();
            return;
        }

        // Check if this is the last segment
        if (currentStoryIndex >= storySegments.Length - 1)
        {
            LoadGameScene();
            return;
        }

        // Move to next segment
        currentStoryIndex++;
        ShowCurrentStorySegment();
    }

    void ShowCurrentStorySegment()
    {
        // Update button text
        buttonText.text = buttonTexts[currentStoryIndex];

        // Start typewriter effect
        StartCoroutine(TypewriterEffect(storySegments[currentStoryIndex]));
    }

    IEnumerator TypewriterEffect(string textToShow)
    {
        isTyping = true;
        storyText.text = "";
        nextButton.interactable = false;

        // Type each character
        foreach (char c in textToShow)
        {
            storyText.text += c;

            // Add slight pause for punctuation
            if (c == '.' || c == '!' || c == '?')
            {
                yield return new WaitForSeconds(typewriterSpeed * 3);
            }
            else if (c == ',' || c == ';')
            {
                yield return new WaitForSeconds(typewriterSpeed * 2);
            }
            else
            {
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        // Enable button when done typing
        isTyping = false;
        nextButton.interactable = true;

        // Flash the button briefly to draw attention
        StartCoroutine(FlashButton());
    }

    void SkipTypewriter()
    {
        // Stop all typewriter coroutines and show full text immediately
        StopAllCoroutines();
        storyText.text = storySegments[currentStoryIndex];
        isTyping = false;
        nextButton.interactable = true;
    }

    IEnumerator FlashButton()
    {
        Color originalColor = nextButton.image.color;
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // Quick flash effect
        for (int i = 0; i < 2; i++)
        {
            nextButton.image.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            nextButton.image.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void LoadGameScene()
    {
        // Add loading transition if desired
        StartCoroutine(LoadGameWithTransition());
    }

    IEnumerator LoadGameWithTransition()
    {
        // Optional: Fade out effect
        // CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        // while (canvasGroup.alpha > 0)
        // {
        //     canvasGroup.alpha -= Time.deltaTime * 2f;
        //     yield return null;
        // }

        yield return new WaitForSeconds(0.5f);

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    // Optional: Add dramatic pause after certain segments
    void AddDramaticPause(float pauseTime = 1f)
    {
        StartCoroutine(DramaticPauseCoroutine(pauseTime));
    }

    IEnumerator DramaticPauseCoroutine(float pauseTime)
    {
        nextButton.interactable = false;
        yield return new WaitForSeconds(pauseTime);
        nextButton.interactable = true;
        StartCoroutine(FlashButton());
    }
}
