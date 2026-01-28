using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;
using System.Collections;

// This file is part of the partial Director class.
// It contains textbox open/close + typing logic.
public partial class Director
{
    Coroutine _typeTextCo;

    // -------------------------------------------------------------------------
    // ToggleTextbox(): small helper to animate open/close + play SFX
    // (calls the coroutine overload below)
    // -------------------------------------------------------------------------
    private void ToggleTextbox(bool open, int? textBoxLine)
    {
        if (!textbox) return;
        PlaySfx(open ? sfxTextboxOpen : sfxTextboxClose);
        StopTextboxTyping();
        StartCoroutine(ToggleTextbox(open, textBoxLine, 0.35f));
    }

    // -------------------------------------------------------------------------
    // ToggleTextbox coroutine:
    // - scales textbox from 0 to 1 (open) or 1 to 0 (close)
    // - then optionally types out a line of text from TextboxScripts.Lines[]
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator ToggleTextbox(bool open, int? textBoxLine, float seconds)
    {
        if (!textbox) yield break;

        Vector3 from = textbox.localScale;
        Vector3 to = open ? new Vector3(2f, 2f, 1f) : Vector3.zero;

        if (seconds <= 0f)
        {
            textbox.localScale = to;
            if (open) textbox.gameObject.SetActive(true);
            else textbox.gameObject.SetActive(false);
            yield break;
        }

        textboxText.text = "";

        // make sure it's active while animating open
        if (open) textbox.gameObject.SetActive(true);
        SyncDotsBlink();


        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
        {
            float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
            textbox.localScale = Vector3.Lerp(from, to, ease);
            yield return null;
        }

        // After opening, type the requested line
        if (textBoxLine.HasValue)
        {
            var line = TextboxScripts.Lines[textBoxLine.Value];
            textboxText.fontSize = line.fontSize;
            _typeTextCo = StartCoroutine(TypeText(line.Get(UseGerman), 45f));
            yield return _typeTextCo;
            _typeTextCo = null;
        }

        textbox.localScale = to;

        // hide after closing so it doesn't block clicks etc.
        if (!open) textbox.gameObject.SetActive(false);

        SyncDotsBlink();

        // "display whatever Text (TMP) contains" happens automatically once it's visible
    }

    // -------------------------------------------------------------------------
    // TypeText(): types one character at a time into textboxText
    // - Also plays typing clicks (not for spaces)
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator TypeText(string s, float cps = 40f)
    {
        textboxText.text = "";
        if (string.IsNullOrEmpty(s)) yield break;

        float delay = 1f / Mathf.Max(1f, cps);
        for (int i = 0; i <= s.Length; i++)
        {
            if (_ending) yield break;
            textboxText.text = s.Substring(0, i);

            if (i > 0) // don't play for the 0th frame
            {
                char ch = s[i - 1];
                if (!char.IsWhiteSpace(ch))
                    PlayTypeSfx();
            }

            yield return new WaitForSeconds(delay);
        }
    }

    void StopTextboxTyping()
    {
        StopCoroutine(nameof(ToggleTextbox));

        if (_typeTextCo != null)
        {
            StopCoroutine(_typeTextCo);
            _typeTextCo = null;
        }

        if (typeSfx) typeSfx.Stop();
    }


    Coroutine _dotsBlinkCo;

    bool ShouldPulseDots()
    {
        if (_ending) return false;
        if (!dotsActive) return false;

        // you said you still control GO on/off manually
        if (!dots || !dots.activeInHierarchy) return false;

        bool textboxActive = textbox && textbox.gameObject.activeInHierarchy;

        bool decisionBoxesActive =
            (decisionL && decisionL.activeInHierarchy) ||
            (decisionR && decisionR.activeInHierarchy);

        return textboxActive && !decisionBoxesActive;
    }

    void SyncDotsBlink()
    {
        if (!dots) return;

        bool shouldPulse = ShouldPulseDots();

        if (shouldPulse)
        {
            if (_dotsBlinkCo == null)
                _dotsBlinkCo = StartCoroutine(DotsPulseLoop());
        }
        else
        {
            StopDotsPulseImmediate(resetToMax: true);
        }
    }

    void SetDotsActive(bool active)
    {
        dotsActive = active;
        SyncDotsBlink();
    }

    IEnumerator DotsPulseLoop()
    {
        var img = dots.GetComponent<RawImage>();
        if (!img)
        {
            _dotsBlinkCo = null;
            yield break;
        }

        // start visible immediately
        SetDotsAlpha(img, dotsAlphaMax);

        while (ShouldPulseDots())
        {
            // fade out over dotsBlinkInterval
            yield return FadeDots(img, dotsAlphaMax, dotsAlphaMin, dotsBlinkInterval);
            if (!ShouldPulseDots()) break;

            // fade in over dotsBlinkInterval
            yield return FadeDots(img, dotsAlphaMin, dotsAlphaMax, dotsBlinkInterval);
        }

        // reset
        if (img) SetDotsAlpha(img, dotsAlphaMax);
        _dotsBlinkCo = null;
    }

    IEnumerator FadeDots(RawImage img, float fromA, float toA, float seconds)
    {
        seconds = Mathf.Max(0.0001f, seconds);
        float t = 0f;

        while (t < 1f)
        {
            if (!ShouldPulseDots()) yield break;
            if (!img) yield break;

            t += Time.unscaledDeltaTime / seconds;
            float u = Mathf.Clamp01(t);

            // smooth ease-in-out
            float ease = 0.5f - 0.5f * Mathf.Cos(u * Mathf.PI);

            SetDotsAlpha(img, Mathf.Lerp(fromA, toA, ease));
            yield return null;
        }
    }

    void SetDotsAlpha(RawImage img, float a)
    {
        var c = img.color;
        c.a = a;
        img.color = c;
    }

    void StopDotsPulseImmediate(bool resetToMax)
    {
        if (_dotsBlinkCo != null)
        {
            StopCoroutine(_dotsBlinkCo);
            _dotsBlinkCo = null;
        }

        var img = dots ? dots.GetComponent<RawImage>() : null;
        if (img && resetToMax) SetDotsAlpha(img, dotsAlphaMin);
    }
}
