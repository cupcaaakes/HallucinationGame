using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

// This file is part of the partial Director class.
// It contains the white overlay fade helpers.
public partial class Director
{
    // -------------------------------------------------------------------------
    // Whiteout helpers:
    // SetWhiteoutAlpha() directly sets the Image alpha.
    // FadeWhiteoutTo() smoothly fades it over time.
    // -------------------------------------------------------------------------
    void SetWhiteoutAlpha(float a)
    {
        if (!whiteout) return;
        var c = whiteout.color;
        c.a = a;
        whiteout.color = c;
    }

    System.Collections.IEnumerator FadeWhiteoutTo(float toAlpha, float seconds)
    {
        if (!whiteout) yield break;

        // make sure it's visible while fading
        whiteout.gameObject.SetActive(true);
        if (whiteoutBlocksInput) whiteout.raycastTarget = true;

        float fromAlpha = whiteout.color.a;

        if (seconds <= 0f)
        {
            SetWhiteoutAlpha(toAlpha);
        }
        else
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
            {
                float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
                SetWhiteoutAlpha(Mathf.Lerp(fromAlpha, toAlpha, ease));
                yield return null;
            }
            SetWhiteoutAlpha(toAlpha);
        }

        // once fully faded out, you can disable it (and stop blocking input)
        if (toAlpha <= 0.001f)
        {
            if (whiteoutBlocksInput) whiteout.raycastTarget = false;
            whiteout.gameObject.SetActive(false);
        }
        else
        {
            if (whiteoutBlocksInput) whiteout.raycastTarget = true;
            whiteout.gameObject.SetActive(true);
        }
    }

    // -------------------------------------------------------------------------
    // SceneTransition(): generic helper for "fade to white, then fade out"
    // (Not currently used for the demo flow, but we'll prolly use it later.)
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator SceneTransition()
    {
        // fade IN fully (usually you want this to finish before swapping visuals)
        yield return FadeWhiteoutTo(1f, whiteoutFadeSeconds);

        // start fading OUT, but start the next scene slightly before it's fully gone
        var fadeOut = StartCoroutine(FadeWhiteoutTo(0f, whiteoutFadeSeconds));

        float wait = Mathf.Max(0f, whiteoutFadeSeconds - sceneStartLeadSeconds);
        if (wait > 0f) yield return new WaitForSeconds(wait);

        // then your next scene method runs here, e.g.:
        // yield return Scene2();

        yield return fadeOut;
    }
}
