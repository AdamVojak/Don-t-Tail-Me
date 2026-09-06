using System.Collections;
using UnityEngine;

public class DeckSlider : MonoBehaviour
{
    [SerializeField] private RectTransform decksContainer;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine slideCoroutine;

    // Kad želimo Main Menu -> Kontejner ide na Y = 0
    public void GoToMainMenu()
    {
        StartSlide(0);
    }

    // Kad želimo Player Select -> Kontejner ide GORE za točno visinu ekrana
    public void GoToPlayerSelect()
    {
        float screenHeight = canvasRect.rect.height;
        StartSlide(screenHeight);
    }

    private void StartSlide(float targetY)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(targetY));
    }

    private IEnumerator SlideRoutine(float targetY)
    {
        Vector2 startPos = decksContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(0, targetY);
        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = easeCurve.Evaluate(time / slideDuration);
            decksContainer.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
            yield return null;
        }

        decksContainer.anchoredPosition = targetPos;
    }
}