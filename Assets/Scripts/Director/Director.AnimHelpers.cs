using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

// This file is part of the partial Director class.
// It contains generic animation helpers: fade, move, scale.
public partial class Director
{
    // -------------------------------------------------------------------------
    // Fade(): fades a 3D object's material alpha
    // - Works for either Standard shader (_Color) or URP (_BaseColor)
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator Fade(GameObject go, float toAlpha, float seconds)
    {
        var r = go.GetComponentInChildren<Renderer>();
        if (!r) yield break;

        var m = r.material;

        // pick whichever color property exists
        bool isStd = m.HasProperty("_Color");
        bool isUrp = !isStd && m.HasProperty("_BaseColor");
        if (!isStd && !isUrp) yield break;

        Color c = isStd ? m.color : m.GetColor("_BaseColor");
        float from = c.a;

        if (seconds <= 0f) // if set to instant, just set the alpha to the target value.
        {
            c.a = toAlpha;
            if (isStd) m.color = c; else m.SetColor("_BaseColor", c);
            yield break;
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
        {
            c.a = Mathf.Lerp(from, toAlpha, t);
            if (isStd) m.color = c; else m.SetColor("_BaseColor", c);
            yield return null;
        }

        c.a = toAlpha;
        if (isStd) m.color = c; else m.SetColor("_BaseColor", c);
    }

    // -------------------------------------------------------------------------
    // MoveTo(): moves a GameObject to a position over time with smooth easing
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator MoveTo(GameObject go, Vector3 toPos, float seconds)
    {
        if (!go) yield break;

        Vector3 from = go.transform.position;

        if (seconds <= 0f)
        {
            go.transform.position = toPos;
            yield break;
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
        {
            float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI); // ease in/out (sine-ish)
            go.transform.position = Vector3.Lerp(from, toPos, ease);
            yield return null;
        }

        go.transform.position = toPos;
    }

    // -------------------------------------------------------------------------
    // ScaleTo(): scales a GameObject smoothly (used for UI pop-in/pop-out)
    // disableAtEnd=true + scale=0 means we also deactivate the object at the end
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator ScaleTo(GameObject go, Vector3 toScale, float seconds, bool disableAtEnd)
    {
        if (!go) yield break;

        Vector3 from = go.transform.localScale;

        if (seconds <= 0f)
        {
            go.transform.localScale = toScale;
            if (disableAtEnd && toScale == Vector3.zero) go.SetActive(false);
            yield break;
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
        {
            float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
            go.transform.localScale = Vector3.Lerp(from, toScale, ease);
            yield return null;
        }

        go.transform.localScale = toScale;
        if (disableAtEnd && toScale == Vector3.zero) go.SetActive(false);
    }
}
