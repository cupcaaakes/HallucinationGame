using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

// This file is part of the partial Director class.
// It contains the hover-choice UI positioning + hover enter/exit handling.
public partial class Director
{
    // -------------------------------------------------------------------------
    // WorldToCanvasLocal():
    // Converts a 3D world position into a 2D position inside the UI Canvas
    // so the choiceText can "follow" something in the 3D world.
    // -------------------------------------------------------------------------
    Vector2 WorldToCanvasLocal(Vector3 world)
    {
        var cam = Camera.main; // the camera that sees your world
        Vector2 screen = cam.WorldToScreenPoint(world);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screen,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera,
            out var local);

        return local;
    }

    // Moves a UI RectTransform smoothly to a target anchoredPosition
    System.Collections.IEnumerator MoveToUI(RectTransform rt, Vector2 toPos, float seconds)
    {
        if (!rt) yield break;

        Vector2 from = rt.anchoredPosition;

        if (seconds <= 0f)
        {
            rt.anchoredPosition = toPos;
            yield break;
        }

        for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
        {
            float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
            rt.anchoredPosition = Vector2.Lerp(from, toPos, ease);
            yield return null;
        }

        rt.anchoredPosition = toPos;
    }

    // -------------------------------------------------------------------------
    // SetChoiceHover(isLeft, open)
    //
    // This is meant to be called by some trigger/hover script on the door colliders.
    // - open=true: cursor entered / started hovering this decision collider
    // - open=false: cursor exited / stopped hovering
    //
    // When open=true:
    // - sets _activeChoice to 0 or 1
    // - starts Ambiance preview for that side
    // - shows choiceText and ring, and positions them nicely
    //
    // When open=false:
    // - resets hold timer and hides UI
    // - stops Ambiance preview
    // -------------------------------------------------------------------------
    public void SetChoiceHover(bool isLeft, bool open)
    {
        if (!decisionL || !decisionR || !choiceText || !canvas) return;
        if (!decisionL.activeInHierarchy || !decisionR.activeInHierarchy) return;

        var go = isLeft ? decisionL : decisionR;
        var col = go ? go.GetComponent<Collider>() : null;
        if (!go || !go.activeInHierarchy || !col || !col.enabled) return;

        int side = isLeft ? 0 : 1;

        // CLOSE (hover exit)
        if (!open)
        {
            if (_activeChoice != side) return; // ignore exit from the other box

            _activeChoice = -1;
            _choiceHold = 0f;
            _choiceWasOpen = false;
            if (_ambPreviewActive)
            {
                _ambPreviewActive = false;
                StopAmbiancePreview();
            }

            if (_choiceMoveCo != null) StopCoroutine(_choiceMoveCo);
            if (_choiceScaleCo != null) StopCoroutine(_choiceScaleCo);
            _choiceScaleCo = StartCoroutine(ScaleTo(choiceText, Vector3.zero, choiceAnimSeconds, true));

            if (choiceRing)
            {
                if (_ringScaleCo != null) StopCoroutine(_ringScaleCo);
                choiceRing.fillAmount = 0f;
                _ringScaleCo = StartCoroutine(ScaleTo(choiceRing.gameObject, Vector3.zero, choiceAnimSeconds, true));
            }

            return;
        }

        // OPEN (hover enter / hover stay)
        if (_activeChoice != side) _choiceHold = 0f; // switching sides resets hold
        _activeChoice = side;

        // Start / switch Ambiance preview
        StartAmbiancePreview(side);

        // play the "choice open" sfx once when we first show the UI
        if (!_choiceWasOpen)
        {
            PlaySfx(sfxChoiceOpen);
            _choiceWasOpen = true;
        }

        // compute target for ChoiceText (canvas space)
        Vector3 leftW = decisionL.transform.position;
        Vector3 rightW = decisionR.transform.position;
        Vector3 centerW = (leftW + rightW) * 0.5f;

        Vector3 pickW = isLeft
            ? Vector3.Lerp(leftW, centerW, choiceTowardCenter)
            : Vector3.Lerp(rightW, centerW, choiceTowardCenter);

        Vector2 targetCanvas = WorldToCanvasLocal(pickW) + choiceOffsetPx;
        targetCanvas.y = 0f;
        int idx = _choiceBaseIndex + side; // left=base, right=base+1

        if ((uint)idx >= (uint)ChoiceTextScripts.Lines.Count)
        {
            Debug.LogError($"ChoiceTextScripts index out of range: idx={idx}, base={_choiceBaseIndex}, side={side}");
            return;
        }

        var line = ChoiceTextScripts.Lines[idx];

        _choiceTmp.text = line.Get(UseGerman);
        _choiceTmp.fontSize = line.fontSize;

        choiceText.SetActive(true);

        if (_choiceMoveCo != null) StopCoroutine(_choiceMoveCo);
        if (_choiceScaleCo != null) StopCoroutine(_choiceScaleCo);
        _choiceMoveCo = StartCoroutine(MoveToUI(_choiceRt, targetCanvas, choiceAnimSeconds));
        _choiceScaleCo = StartCoroutine(ScaleTo(choiceText, Vector3.one, choiceAnimSeconds, false));

        // place ring in ChoiceText-local space (because it’s a child)
        if (choiceRing && _ringRt)
        {
            choiceRing.gameObject.SetActive(true);
            choiceRing.fillAmount = 0f;

            _choiceTmp.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();

            float textH = _choiceTmp.preferredHeight;
            float ringH = _ringRt.rect.height;
            float ringW = _ringRt.rect.width;

            // inside edge toward the screen center
            float inside = (_choiceRt.rect.width * 0.5f) - (ringW * 0.5f) - 8f;
            float xLocal = isLeft ? +inside : -inside;
            float yLocalOffset = 50f; // it just looks better slightly higher up. Sue me.
            // directly below the rendered text
            float yLocal = -(textH * 0.5f + ringH * 0.5f + choiceRingGapPx) + yLocalOffset;

            _ringRt.anchoredPosition = new Vector2(xLocal, yLocal);

            if (_ringScaleCo != null) StopCoroutine(_ringScaleCo);
            _ringScaleCo = StartCoroutine(ScaleTo(choiceRing.gameObject, Vector3.one, choiceAnimSeconds, false));
        }
    }

    void SetChoicePair(int leftIndexEven)
    {
        // Each "scene" gets exactly 2 choice lines in ChoiceTextScripts:
        // scene 0 -> indices 0,1
        // scene 1 -> indices 2,3
        // scene 2 -> indices 4,5
        _choiceBaseIndex = leftIndexEven * 2;
    }

}
