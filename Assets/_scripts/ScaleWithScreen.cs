using UnityEngine;

public class ScaleWithScreen : MonoBehaviour
{
    private RectTransform rectTransform;
    public float baseScale = 1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Scale based on screen height
        float screenHeightRatio = Screen.height / 1080f; // 1080 is your base resolution
        float newScale = baseScale * screenHeightRatio;
        newScale = Mathf.Clamp(newScale, minScale, maxScale);

        rectTransform.localScale = Vector3.one * newScale;
    }
}