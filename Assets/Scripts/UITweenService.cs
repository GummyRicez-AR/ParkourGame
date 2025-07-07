using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public enum TweenStyle
{
    None,
    Linear,
    Quadratic,
    Cubic,
    Quartic,
    Quintic,
    Back
}

public class UITweenService
{
    private readonly List<RectTransform> elementsCurrentlyPosTweening = new();
    private readonly List<RectTransform> elementsCurrentlySizeTweening = new();
    private readonly List<RectTransform> elementsCurrentlyShaking = new();
    private readonly List<Image> imagesColorTweening = new();

    public IEnumerator TweenPosition(RectTransform uiElement, Vector3 deltaPos, float time, TweenStyle tweenStyle, bool useFixedTime = false)
    {
        if (elementsCurrentlyPosTweening.Contains(uiElement)) { yield break; }

        elementsCurrentlyPosTweening.Add(uiElement);
        Vector3 origPos = uiElement.localPosition;
        Vector3 newPos = origPos + deltaPos;
        float progress = 0;

        while (progress < 1)
        {
            if (useFixedTime)
                progress += Time.fixedDeltaTime / time;
            else
                progress += Time.deltaTime / time;

            if (progress >= 1) { break; }
            switch (tweenStyle)
            {
                case TweenStyle.Linear:
                    uiElement.localPosition = Vector3.Lerp(origPos, newPos, progress);
                    break;
                case TweenStyle.Quadratic:
                    uiElement.localPosition = Vector3.Lerp(origPos, newPos, Mathf.Pow(progress, 2));
                    break;
                case TweenStyle.Cubic:
                    uiElement.localPosition = Vector3.Lerp(origPos, newPos, Mathf.Pow(progress, 3));
                    break;
                case TweenStyle.Quartic:
                    uiElement.localPosition = Vector3.Lerp(origPos, newPos, Mathf.Pow(progress, 4));
                    break;
                case TweenStyle.Quintic:
                    uiElement.localPosition = Vector3.Lerp(origPos, newPos, Mathf.Pow(progress, 5));
                    break;
                case TweenStyle.Back:
                    uiElement.localPosition = Vector3.LerpUnclamped(origPos, newPos, -Mathf.Pow(progress, 3) + (progress * 2));
                    break;
                default:
                    break;
            }
            yield return null;
        }

        uiElement.localPosition = newPos;
        elementsCurrentlyPosTweening.Remove(uiElement);
    }

    public IEnumerator TweenSize(RectTransform uiElement, float deltaWidth, float deltaHeight, float time, TweenStyle tweenStyle, bool useFixedTime = false)
    {
        if (elementsCurrentlySizeTweening.Contains(uiElement)) { yield break; }

        elementsCurrentlySizeTweening.Add(uiElement);
        Vector2 oldSize = uiElement.sizeDelta;
        Vector2 newSize = oldSize + new Vector2(deltaWidth, deltaHeight);

        float progress = 0;

        while (progress < 1)
        {
            if (useFixedTime)
                progress += Time.fixedDeltaTime / time;
            else
                progress += Time.deltaTime / time;

            if (progress >= 1) { break; }
            switch (tweenStyle)
            {
                case TweenStyle.Linear:
                    uiElement.sizeDelta = Vector2.Lerp(oldSize, newSize, progress);
                    break;
                case TweenStyle.Quadratic:
                    uiElement.sizeDelta = Vector2.Lerp(oldSize, newSize, Mathf.Pow(progress, 2));
                    break;
                case TweenStyle.Cubic:
                    uiElement.sizeDelta = Vector2.Lerp(oldSize, newSize, Mathf.Pow(progress, 3));
                    break;
                case TweenStyle.Quartic:
                    uiElement.sizeDelta = Vector2.Lerp(oldSize, newSize, Mathf.Pow(progress, 4));
                    break;
                case TweenStyle.Quintic:
                    uiElement.sizeDelta = Vector2.Lerp(oldSize, newSize, Mathf.Pow(progress, 5));
                    break;
                case TweenStyle.Back:
                    uiElement.sizeDelta = Vector2.LerpUnclamped(oldSize, newSize, -Mathf.Pow(progress, 3) + (progress * 2));
                    break;
                default:
                    break;
            }
            yield return null;
        }

        uiElement.sizeDelta = newSize;
        elementsCurrentlySizeTweening.Remove(uiElement);
    }

    public IEnumerator TweenImageColor(Image image, Color newColor, float time, TweenStyle tweenStyle, bool useFixedTime = false)
    {
        if (imagesColorTweening.Contains(image)) { yield break; }

        imagesColorTweening.Add(image);
        Color oldColor = image.color;
        float progress = 0;

        while (progress < 1)
        {
            if (useFixedTime)
                progress += Time.fixedDeltaTime / time;
            else
                progress += Time.deltaTime / time;

            switch (tweenStyle)
            {
                case TweenStyle.Linear:
                    image.color = Color.Lerp(oldColor, newColor, progress);
                    break;
                case TweenStyle.Quadratic:
                    image.color = Color.Lerp(oldColor, newColor, Mathf.Pow(progress, 2));
                    break;
                case TweenStyle.Cubic:
                    image.color = Color.Lerp(oldColor, newColor, Mathf.Pow(progress, 3));
                    break;
                case TweenStyle.Quartic:
                    image.color = Color.Lerp(oldColor, newColor, Mathf.Pow(progress, 4));
                    break;
                case TweenStyle.Quintic:
                    image.color = Color.Lerp(oldColor, newColor, Mathf.Pow(progress, 5));
                    break;
                case TweenStyle.Back:
                    image.color = Color.LerpUnclamped(oldColor, newColor, -Mathf.Pow(progress, 3) + (progress * 2));
                    break;
                default:
                    break;
            }
            yield return null;
        }

        image.color = newColor;
        imagesColorTweening.Remove(image);
    }

    public IEnumerator ShakeEffect(RectTransform uiElement, float shakeStrength, float time, bool useFixedTime = false)
    {
        if (elementsCurrentlyShaking.Contains(uiElement)) { yield break; }
        
        elementsCurrentlyShaking.Add(uiElement);
        Vector3 initPos = uiElement.localPosition;

        if (time < 0)
        {
            while (elementsCurrentlyShaking.Contains(uiElement))
            {
                Vector3 offset = Random.insideUnitCircle * shakeStrength;
                uiElement.localPosition = initPos + offset;
                yield return null;
            }
        }
        else
        {
            float progress = 0;
            while (progress < 1)
            {
                if (useFixedTime)
                    progress += Time.fixedDeltaTime / time;
                else
                    progress += Time.deltaTime / time;
                
                Vector3 offset = Random.insideUnitCircle * shakeStrength;
                uiElement.localPosition = initPos + offset;
                yield return null;
            }
            
            elementsCurrentlyShaking.Remove(uiElement);
        }
    }

    // only meant to be used for UI elements set to permanently shake
    public void StopShakeElement(RectTransform uiElement)
    {
        if (elementsCurrentlyShaking.Contains(uiElement))
        {
            elementsCurrentlyShaking.Remove(uiElement);
        }
    }
}
