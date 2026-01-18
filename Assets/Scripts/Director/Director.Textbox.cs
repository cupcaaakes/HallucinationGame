using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

// This file is part of the partial Director class.
// It contains textbox open/close + typing logic.
public partial class Director
{
    // -------------------------------------------------------------------------
    // ToggleTextbox(): small helper to animate open/close + play SFX
    // (calls the coroutine overload below)
    // -------------------------------------------------------------------------
    private void ToggleTextbox(bool open, int? textBoxLine)
    {
        if (!textbox) return;
        PlaySfx(open ? sfxTextboxOpen : sfxTextboxClose);
        StopCoroutine(nameof(ToggleTextbox)); // prevents stacking calls
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
            yield return TypeText(line.Get(UseGerman), 45f);
        }

        textbox.localScale = to;

        // hide after closing so it doesn't block clicks etc.
        if (!open) textbox.gameObject.SetActive(false);

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
}
