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

            // Ring must always close on hover exit
            SetRingVisible(false, isLeft);

            // BUT keep choice text open if arm preview wants something
            RefreshChoicePreviewUI();
            return;
        }

        // OPEN (hover enter / hover stay)
        if (_activeChoice != side) _choiceHold = 0f; // switching sides resets hold
        _activeChoice = side;

        RefreshChoicePreviewUI();
    }

    public void SetArmChoicePreviewFromBools(bool leftArmRaised, bool rightArmRaised)
    {
        int desired = -1;

        // If both arms are up, ignore (avoids flicker / ambiguity)
        if (leftArmRaised ^ rightArmRaised)
            desired = leftArmRaised ? 0 : 1;

        if (_armChoice == desired) return;
        _armChoice = desired;

        RefreshChoicePreviewUI();
    }

    void RefreshChoicePreviewUI()
    {
        if (!decisionL || !decisionR || !choiceText || !canvas) return;

        // Hover takes priority over arm preview (because hover is the real selectable state)
        int desiredSide = (_activeChoice != -1) ? _activeChoice : _armChoice;

        // No preview at all -> close everything
        if (desiredSide == -1)
        {
            CloseChoiceTextUI();
            if (_ambPreviewActive)
            {
                _ambPreviewActive = false;
                StopAmbiancePreview();
            }
            return;
        }

        bool isLeft = desiredSide == 0;

        // Ambience preview should follow whichever side we're previewing
        StartAmbiancePreview(desiredSide);

        // If we're already showing the correct side, just update ring visibility and bail
        if (_previewChoice == desiredSide && choiceText.activeInHierarchy)
        {
            // Ring only shows while actually hovering inside the collider
            SetRingVisible(_activeChoice != -1, isLeft);
            return;
        }

        // Otherwise show/move/scale the choice UI to this side
        OpenChoiceTextUI(desiredSide, isLeft);

        // Ring only shows while actually hovering inside the collider
        SetRingVisible(_activeChoice != -1, isLeft);
    }

    void OpenChoiceTextUI(int side, bool isLeft)
    {
        if (!decisionL || !decisionR || !choiceText || !canvas) return;
        if (!_choiceTmp || !_choiceRt) return;

        // play once when UI becomes visible (arm OR hover)
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

        _previewChoice = side;
    }

    void CloseChoiceTextUI()
    {
        if (!choiceText) return;

        // Already closed
        if (_previewChoice == -1 && !choiceText.activeInHierarchy)
            return;

        _previewChoice = -1;
        _choiceWasOpen = false;

        if (_choiceMoveCo != null) StopCoroutine(_choiceMoveCo);
        if (_choiceScaleCo != null) StopCoroutine(_choiceScaleCo);
        _choiceScaleCo = StartCoroutine(ScaleTo(choiceText, Vector3.zero, choiceAnimSeconds, true));

        // Always close ring too (even though we *usually* don't show it via arms)
        SetRingVisible(false, true);
        SetRingVisible(false, false);
    }

    void SetRingVisible(bool visible, bool isLeft)
    {
        if (!choiceRing || !_ringRt || !_choiceTmp || !_choiceRt) return;

        if (!visible)
        {
            if (_ringScaleCo != null) StopCoroutine(_ringScaleCo);
            choiceRing.fillAmount = 0f;
            _ringScaleCo = StartCoroutine(ScaleTo(choiceRing.gameObject, Vector3.zero, choiceAnimSeconds, true));
            return;
        }

        // visible = true (hover is active)
        choiceRing.gameObject.SetActive(true);
        choiceRing.fillAmount = 0f;

        _choiceTmp.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();

        // Put ring under the text (bottom-center)
        _ringRt.anchorMin = _ringRt.anchorMax = new Vector2(0.5f, 0f); // bottom-center of the text rect
        _ringRt.pivot = new Vector2(0.5f, 1f);                        // top-center of the ring

        // distance below the text rect (use your existing field)
        _ringRt.anchoredPosition = new Vector2(0f, -choiceRingGapPx);

        if (_ringScaleCo != null) StopCoroutine(_ringScaleCo);
        _ringScaleCo = StartCoroutine(ScaleTo(choiceRing.gameObject, Vector3.one, choiceAnimSeconds, false));
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
