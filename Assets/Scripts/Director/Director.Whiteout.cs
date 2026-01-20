using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TextboxScripts;

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

        a = Mathf.Clamp01(a);

        var c = whiteout.color;
        c.a = a;
        whiteout.color = c;

        float targetWeight = _currentSceneRoot == titleScreenParent ? 1f : a;

        // opposite value of whiteout alpha:
        SetGlitchVolumeWeight(targetWeight);
    }


    System.Collections.IEnumerator FadeWhiteoutTo(float toAlpha, float seconds)
    {
        if (!whiteout) yield break;

        // make sure it's visible while fading
        whiteout.gameObject.SetActive(true);
        glitchTransitionOverlay.SetActive(true);
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
            glitchTransitionOverlay.SetActive(false);
        }
        else
        {
            if (whiteoutBlocksInput) whiteout.raycastTarget = true;
            whiteout.gameObject.SetActive(true);
            glitchTransitionOverlay.SetActive(true);
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

    void SetGlitchVolumeWeight(float w)
    {
        if (!glitchVolume) return;

        w = Mathf.Clamp01(w);

        // 1) try setting weight on whatever is currently assigned
        if (TrySetWeightOn(glitchVolume, w))
            return;

        // 2) if that failed, you probably dragged the CAMERA (Transform)
        // so we search the SAME GameObject for the Post-process Volume component
        var go = glitchVolume.gameObject;

        foreach (var c in go.GetComponents<Component>())
        {
            if (!c) continue;

            if (TrySetWeightOn(c, w))
            {
                glitchVolume = c; // auto-replace with the correct component
                return;
            }
        }

        Debug.LogError("glitchVolume is assigned, but no component with 'weight' exists on that GameObject. You need a Post-process Volume component on the Glitch Camera.");
    }

    bool TrySetWeightOn(Component c, float w)
    {
        var t = c.GetType();

        // Weight might be a property...
        var prop = t.GetProperty("weight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite && prop.PropertyType == typeof(float))
        {
            prop.SetValue(c, w);
            return true;
        }

        // ...or a field (PostProcessVolume uses a field)
        var field = t.GetField("weight", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(float))
        {
            field.SetValue(c, w);
            return true;
        }

        return false;
    }

}
