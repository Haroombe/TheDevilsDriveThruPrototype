using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class FadingMessage : MonoBehaviour
{
    // === 1. SINGLETON SETUP ===
    public static FadingMessage Instance { get; private set; }

    [Header("Component & Settings")]
    [Tooltip("The TextMeshPro component for general messages.")]
    [SerializeField] private TMP_Text messageText;
    [Tooltip("The TextMeshPro component for callouts/secondary notifications.")]
    [SerializeField] private TMP_Text CalloutText;

    [Tooltip("How long the text stays solid before fading (Default).")]
    [SerializeField] private float durationVisible = 1.0f;
    [Tooltip("How long it takes to fade to invisible (Default).")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Styling & Colors")]
    [SerializeField] private string errorTextHex = "#FF4046";
    [SerializeField] private string succesTextHex = "#6EFF67";

    [Tooltip("Size reduction factor for success messages (e.g., 0.75 for 75% size).")]
    [SerializeField] private float successScaleFactor = 0.75f;

    private Coroutine currentFadeRoutine;
    private Color successColor;
    private Color errorColor;
    private float defaultFontSize = 0f;

    private void Awake()
    {
        // === Singleton Pattern ===
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (messageText == null)
            messageText = GetComponent<TMP_Text>();

        if (messageText != null)
        {
            defaultFontSize = messageText.fontSize;
        }

        messageText.text = "";
        messageText.alpha = 0;

        // Ensure CalloutText is also initialized
        if (CalloutText != null)
        {
            CalloutText.text = "";
            CalloutText.alpha = 0;
        }

        ColorUtility.TryParseHtmlString(succesTextHex, out successColor);
        ColorUtility.TryParseHtmlString(errorTextHex, out errorColor);
    }

    // --- ShowCallout Implementation ---
    public void ShowCallout(string message,
                            float? customFadeDuration = null,
                            float? customDurationVisible = null)
    {
        float fDuration = customFadeDuration ?? fadeDuration;
        float dVisible = customDurationVisible ?? durationVisible;

        // NEW: Stop and clear the main message if it's running before starting a callout.
        StopAndClearCurrentRoutine(messageText, CalloutText);

        CalloutText.text = message;
        CalloutText.color = Color.white;
        CalloutText.alpha = 1f;

        currentFadeRoutine = StartCoroutine(FadeOutRoutine(CalloutText, fDuration, dVisible));
    }

    // --- ShowMessage Implementation ---
    public void ShowMessage(string message,
                            bool isGreen = false,
                            float? customFadeDuration = null,
                            float? customDurationVisible = null)
    {
        float fDuration = customFadeDuration ?? fadeDuration;
        float dVisible = customDurationVisible ?? durationVisible;

        // NEW: Stop and clear the callout if it's running before starting the main message.
        // This achieves the desired override/priority.
        StopAndClearCurrentRoutine(CalloutText, messageText);

        messageText.text = message;
        messageText.alpha = 1f;

        // --- Conditional Styling ---
        if (isGreen)
        {
            messageText.color = successColor;
            messageText.fontSize = defaultFontSize * successScaleFactor;
            messageText.outlineWidth = 0f;
        }
        else // Error / Default case
        {
            messageText.color = errorColor;
            messageText.fontSize = defaultFontSize;
            messageText.outlineWidth = 0f;
        }
        // --- End Conditional Styling ---

        currentFadeRoutine = StartCoroutine(FadeOutRoutine(messageText, fDuration, dVisible));
    }

    /// <summary>
    /// Stops the currently running coroutine and immediately clears both text fields, 
    /// ensuring a clean slate for the new message.
    /// </summary>
    private void StopAndClearCurrentRoutine(TMP_Text textToClearBeforeStart, TMP_Text textToClearAfterStop)
    {
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
            currentFadeRoutine = null;
        }

        // Clear the target of the *new* message, which might have been mid-fade
        // This is necessary if you use one routine for two fields.
        textToClearBeforeStart.alpha = 0f;
        textToClearBeforeStart.text = "";

        textToClearAfterStop.alpha = 0f;
        textToClearAfterStop.text = "";
    }


    // --- Core Coroutine Logic ---
    private IEnumerator FadeOutRoutine(TMP_Text TextField, float fDuration, float dVisible)
    {
        yield return new WaitForSeconds(dVisible);

        float timer = 0f;
        while (timer < fDuration)
        {
            timer += Time.deltaTime;
            TextField.alpha = Mathf.Lerp(1f, 0f, timer / fDuration);
            yield return null;
        }

        TextField.alpha = 0f;
        TextField.text = "";
        currentFadeRoutine = null; // Mark the routine as fully finished
    }
}