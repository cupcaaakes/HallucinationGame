using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;
using System.Collections;
using System;

// This file is part of the partial Director class.
// It contains scene activation, decision box toggles, audio helpers, and the demo scene routines.
public partial class Director
{
    // -------------------------------------------------------------------------
    // Decision boxes ON/OFF (these are the hover zones)
    // -------------------------------------------------------------------------
    private void ToggleDecisionBoxes(bool active)
    {
        if (decisionL) decisionL.SetActive(active);
        if (decisionR) decisionR.SetActive(active);
        SetDecisionColliders(active);
    }

    // -------------------------------------------------------------------------
    // ActivateOnlyScene(active):
    // Turns on exactly one child under sceneParent, and turns the others off.
    // -------------------------------------------------------------------------
    void ActivateOnlyScene(GameObject active)
    {
        if (!sceneParent)
        {
            if (active) active.SetActive(true);
            return;
        }

        foreach (Transform child in sceneParent.transform)
            child.gameObject.SetActive(child.gameObject == active);
    }

    // Enables/disables the colliders on the decision boxes
    void SetDecisionColliders(bool enabled)
    {
        if (decisionL)
        {
            var c = decisionL.GetComponent<Collider>();
            if (c) c.enabled = enabled;
        }
        if (decisionR)
        {
            var c = decisionR.GetComponent<Collider>();
            if (c) c.enabled = enabled;
        }
    }

    void StartupScene(GameObject sceneParent)
    {
        ActivateOnlyScene(sceneParent);
        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);
    }

    IEnumerator EndSceneWithNoChoiceMade(SceneRef scene)
    {
        // both sides go to the same destination
        _next[0] = scene;
        _next[1] = _next[0];
        _activeChoice = 0;
        yield return EndAfterChoice();
    }

    // -------------------------------------------------------------------------
    // ViewportToWorldOnZPlane():
    // Converts a screen-space viewport coordinate (0..1) to a 3D world position on a Z plane.
    // Example: vx=1 means right edge of screen, vy=0.5 means middle vertically.
    // -------------------------------------------------------------------------
    Vector3 ViewportToWorldOnZPlane(float vx, float vy, float z)
    {
        var cam = Camera.main;
        if (!cam) return new Vector3(0f, 0f, z);

        var ray = cam.ViewportPointToRay(new Vector3(vx, vy, 0f));
        var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, z)); // z = constant plane

        return plane.Raycast(ray, out var enter)
            ? ray.GetPoint(enter)
            : new Vector3(0f, 0f, z);
    }

    // -------------------------------------------------------------------------
    // PlaySfx(): plays a one-shot sound (simple UI sounds)
    // -------------------------------------------------------------------------
    void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (!clip || !sfx) return;
        sfx.pitch = 1f;
        sfx.PlayOneShot(clip, volume);
    }

    // -------------------------------------------------------------------------
    // PlayTypeSfx(): plays typing click sound with slight pitch randomness
    // - throttled by typeMinInterval
    // -------------------------------------------------------------------------
    void PlayTypeSfx()
    {
        if (!typeSfx) return;
        if (sfxTypeChars == null || sfxTypeChars.Length == 0) return;

        if (Time.unscaledTime < _nextTypeSfxAt) return; // throttle
        _nextTypeSfxAt = Time.unscaledTime + typeMinInterval;

        // pick next clip (cycles)
        var clip = sfxTypeChars[_typeSfxIdx];
        _typeSfxIdx = (_typeSfxIdx + 1) % sfxTypeChars.Length;

        if (!clip) return;

        typeSfx.pitch = 1f + UnityEngine.Random.Range(-typePitchJitter, typePitchJitter);
        typeSfx.PlayOneShot(clip, 1f);
    }

    private void PurityTestSlide(GameObject purityTest, float slideTransition)
    {
        StartCoroutine(Fade(purityTest, 1f, slideTransition));
        StartCoroutine(MoveTo(purityTest, Vector3.zero, slideTransition));

        var cam = uiCamera ? uiCamera : Camera.main;
        if (!cam || !purityTest) return;

        var r = purityTest.GetComponent<Renderer>();
        if (!r) return;

        var mat = r.material;
        if (!mat) return;

        // grab the actual texture used by common shaders
        Texture tex = mat.mainTexture;
        if (!tex) tex = mat.GetTexture("_BaseMap");
        if (!tex) tex = mat.GetTexture("_MainTex");
        if (!tex || tex.height == 0) return;

        float imgAspect = (float)tex.width / tex.height;

        // We scale for the FINAL position (0,0,0), not the current animated position.
        float viewH, viewW;
        if (cam.orthographic)
        {
            viewH = cam.orthographicSize * 2f;
            viewW = viewH * cam.aspect;
        }
        else
        {
            // depth of (0,0,0) along camera forward
            float dist = Mathf.Abs(Vector3.Dot(-cam.transform.position, cam.transform.forward));
            if (dist < 0.0001f) dist = 0.0001f;

            viewH = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            viewW = viewH * cam.aspect;
        }

        // Fit image inside camera view (no crop)
        float targetW, targetH;
        if (viewW / viewH > imgAspect)
        {
            targetH = viewH;
            targetW = targetH * imgAspect;
        }
        else
        {
            targetW = viewW;
            targetH = targetW / imgAspect;
        }

        // If parent is scaled non-uniformly, compensate so WORLD size is correct.
        var parentScale = purityTest.transform.parent ? purityTest.transform.parent.lossyScale : Vector3.one;
        if (Mathf.Abs(parentScale.x) < 0.0001f) parentScale.x = 1f;
        if (Mathf.Abs(parentScale.z) < 0.0001f) parentScale.z = 1f;

        // Unity Plane mesh is 10x10 in local X/Z
        var s = purityTest.transform.localScale;
        s.x = (targetW / 10f) / parentScale.x;
        s.z = (targetH / 10f) / parentScale.z;
        purityTest.transform.localScale = s;
    }

    // -------------------------------------------------------------------------
    // LanguageSelectScene():
    // - Moves and fades language flags into position
    // - Then shows textbox line 0 and enables decision boxes
    // - The player choice determines the UseGerman flag and sets the language for the rest of the game
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator LanguageSelectScene()
    {
        _next[0] = new SceneRef(IntroScene, introSceneParent, AmbRoute.Hospital, true);
        _next[1] = new SceneRef(IntroScene, introSceneParent, AmbRoute.Hospital, true);

        StartupScene(languageSceneParent);
        usBox.SetActive(false);
        deBox.SetActive(false);

        
        StartCoroutine(Fade(leftArrow, 0f, 0f));
        StartCoroutine(Fade(rightArrow, 0f, 0f));
        leftArrow.transform.position = new Vector3(decisionL.transform.position.x - 2f, 0.25f, 0f);
        rightArrow.transform.position = new Vector3(decisionR.transform.position.x + 2f, 0.25f, 0f);
        leftArrow.transform.rotation = defaultBillboardRotation;
        rightArrow.transform.rotation = defaultBillboardRotation;
        leftArrow.transform.localScale = new Vector3(-0.1f, leftArrow.transform.localScale.y, 0.1f);
        rightArrow.transform.localScale = new Vector3(0.1f, rightArrow.transform.localScale.y, 0.1f);

        float demonstrationTransition = 1.5f;
        StartCoroutine(Fade(leftArrow, 1f, demonstrationTransition));
        StartCoroutine(MoveTo(leftArrow, new Vector3(decisionL.transform.position.x, 0.25f, 0f), demonstrationTransition));
        StartCoroutine(Fade(rightArrow, 1f, demonstrationTransition));
        StartCoroutine(MoveTo(rightArrow, new Vector3(-decisionL.transform.position.x, 0.25f, 0f), demonstrationTransition));
        _arrowsActive = true;

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        SetChoicePair(0);
        ToggleTextbox(true, 0);
        ToggleDecisionBoxes(true);
        yield break;
    }

    // -------------------------------------------------------------------------
    // IntroScene():
    // - Activates the first real scene
    // - Moves and fades the intro doctors into position
    // - next scene is the same for both choices but with different texts
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator IntroScene()
    {
        _next[0] = new SceneRef(CheckupSceneAi, checkupSceneAiParent, AmbRoute.Hospital, true);
        _next[1] = new SceneRef(CheckupSceneHuman, checkupSceneHumanParent, AmbRoute.Hospital, true);

        StartupScene(introSceneParent);
        _arrowsActive = false;
        StartCoroutine(Fade(introAiDoctor, 0f, 0f));
        StartCoroutine(Fade(introHumanDoctor, 0f, 0f));
        introAiDoctor.transform.position = new Vector3(decisionL.transform.position.x - 5f, 0f, 0f);
        introHumanDoctor.transform.position = new Vector3(decisionR.transform.position.x + 5f, 0f, 0.5f);
        introAiDoctor.transform.rotation = defaultBillboardRotation;
        introHumanDoctor.transform.rotation = defaultBillboardRotation;
        introAiDoctor.transform.localScale = new Vector3(0.225f, introAiDoctor.transform.localScale.y, introAiDoctor.transform.localScale.z);

        float doctorTransition = 3f;

        StartCoroutine(Fade(introAiDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introAiDoctor, new Vector3(decisionL.transform.position.x, 0f, 0f), doctorTransition));
        StartCoroutine(Fade(introHumanDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introHumanDoctor, new Vector3(decisionR.transform.position.x + 0.25f, 0f, 0.5f), doctorTransition));

        yield return new WaitForSeconds(doctorTransition); // wait for doctor anims

        ToggleTextbox(true, 1);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 2);

        SetChoicePair(1);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator CheckupSceneAi()
    {
        StartupScene(checkupSceneAiParent);
        checkupAiDoctor.transform.position = introAiDoctor.transform.position;
        checkupAiDoctor.transform.rotation = defaultBillboardRotation;
        checkupAiDoctor.transform.localScale = introAiDoctor.transform.localScale;
        aiDoctorChosen = true;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 3);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 5);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 7);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(DemonstrationScene, demonstrationSceneParent, AmbRoute.Amb2, true)
        );
        yield break;

    }

    public System.Collections.IEnumerator CheckupSceneHuman()
    {
        StartupScene(checkupSceneHumanParent);
        checkupHumanDoctor.transform.position = introHumanDoctor.transform.position;
        checkupHumanDoctor.transform.rotation = defaultBillboardRotation;
        aiDoctorChosen = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 4);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 6);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 8);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(DemonstrationScene, demonstrationSceneParent, AmbRoute.Amb2, true)
        );
        yield break;

    }

    // -------------------------------------------------------------------------
    // DemoEnding2():
    // - Activates ending 2 scene
    // - Shows textbox line 2, then line 3 later
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator DemonstrationScene()
    {
        _next[0] = new SceneRef(AiPurityScene, aiPuritySceneParent, AmbRoute.Amb2, true);
        _next[1] = new SceneRef(HumanPurityScene, humanPuritySceneParent, AmbRoute.Amb2, true);

        StartupScene(demonstrationSceneParent);

        if (demonstrationSceneFullscreenObj)
        {
            demonstrationSceneFullscreenObj.transform.rotation = defaultBillboardRotation;
            StartCoroutine(Fade(demonstrationSceneFullscreenObj, 1f, 0f));
        }

        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);

        StartCoroutine(Fade(demonstrationSceneAIProtester, 0f, 0f));
        StartCoroutine(Fade(demonstrationSceneHumanProtester, 0f, 0f));
        demonstrationSceneAIProtester.transform.position = new Vector3(decisionL.transform.position.x - 2f, 0.25f, 0f);
        demonstrationSceneHumanProtester.transform.position = new Vector3(decisionR.transform.position.x + 2f, 0.25f, 0.5f);
        demonstrationSceneAIProtester.transform.rotation = defaultBillboardRotation;
        demonstrationSceneHumanProtester.transform.rotation = defaultBillboardRotation;
        demonstrationSceneAIProtester.transform.localScale = new Vector3(0.35f, demonstrationSceneAIProtester.transform.localScale.y, demonstrationSceneAIProtester.transform.localScale.z);
        demonstrationSceneHumanProtester.transform.localScale = new Vector3(0.45f, demonstrationSceneHumanProtester.transform.localScale.y, 0.35f);

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 9);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 10);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 11);

        float demonstrationTransition = 1.5f;
        StartCoroutine(Fade(demonstrationSceneAIProtester, 1f, demonstrationTransition));
        StartCoroutine(MoveTo(demonstrationSceneAIProtester, new Vector3(decisionL.transform.position.x, 0.25f, 0f), demonstrationTransition));
        StartCoroutine(Fade(demonstrationSceneHumanProtester, 1f, demonstrationTransition));
        StartCoroutine(MoveTo(demonstrationSceneHumanProtester, new Vector3(decisionR.transform.position.x + 0.25f, 0.25f, 0.5f), demonstrationTransition));

        SetChoicePair(2);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator AiPurityScene()
    {
        StartupScene(aiPuritySceneParent);
        aiCrowdChosen = true;
        aiPurityCheckmark.transform.position = new Vector3(decisionL.transform.position.x - 5f, 0f, -0.423f);
        aiPurityCross.transform.position = new Vector3(decisionR.transform.position.x + 5f, 0f, -0.423f);
        aiPurityCheckmark.transform.rotation = defaultBillboardRotation;
        aiPurityCross.transform.rotation = defaultBillboardRotation;
        aiPurityCheckmark.transform.localScale = new Vector3(0.15f, aiPurityCheckmark.transform.localScale.y, 0.1f);
        aiPurityCross.transform.localScale = new Vector3(0.15f, aiPurityCross.transform.localScale.y, 0.1f);

        float puritySymbolTransition = 3f;
        
        aiPurityTestImage.transform.SetPositionAndRotation(new Vector3(0f, -10f, 0f), defaultBillboardRotation);
        purityImageValue = aiPurityTestImage.GetComponent<RandomizeBillboardMaterial>().LastPickedIndex; // 0-3 are AI, 4-7 are human
        Debug.Log("Value of purity image: " + purityImageValue);
        if (purityImageValue <= 3)
        {
            _next[0] = new SceneRef(AcceptedByAIsScene, aiPuritySceneParent, AmbRoute.Amb2, true);
            _next[1] = new SceneRef(RejectedFromAIsScene, humanPuritySceneParent, AmbRoute.Amb2, true);
        }
        else
        {
            _next[0] = new SceneRef(RejectedFromAIsScene, aiPuritySceneParent, AmbRoute.Amb2, true);
            _next[1] = new SceneRef(AcceptedByAIsScene, humanPuritySceneParent, AmbRoute.Amb2, true);
        }

        // start textbox AFTER reveal so typing is visible
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 12);
        yield return new WaitForSeconds(defaultTextBoxTime);


        StartCoroutine(Fade(aiPurityCheckmark, 1f, puritySymbolTransition));
        StartCoroutine(MoveTo(aiPurityCheckmark, new Vector3(-1.059f, 0f, -0.423f), puritySymbolTransition));
        StartCoroutine(Fade(aiPurityCross, 1f, puritySymbolTransition));
        StartCoroutine(MoveTo(aiPurityCross, new Vector3(1.059f, 0f, -0.423f), puritySymbolTransition));

        purityTestActive = true;
        ToggleTextbox(true, 14);
        float slideTransition = 3f;
        PurityTestSlide(aiPurityTestImage, slideTransition);
        yield return new WaitForSeconds(slideTransition);
        SetChoicePair(3);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator HumanPurityScene()
    {
        StartupScene(humanPuritySceneParent);
        aiCrowdChosen = false;
        humanPurityCheckmark.transform.position = new Vector3(decisionL.transform.position.x - 5f, 0f, -0.423f);
        humanPurityCross.transform.position = new Vector3(decisionR.transform.position.x + 5f, 0f, -0.423f);
        humanPurityCheckmark.transform.rotation = defaultBillboardRotation;
        humanPurityCross.transform.rotation = defaultBillboardRotation;
        humanPurityCheckmark.transform.localScale = new Vector3(0.15f, humanPurityCheckmark.transform.localScale.y, 0.1f);
        humanPurityCross.transform.localScale = new Vector3(0.15f, humanPurityCross.transform.localScale.y, 0.1f);

        float puritySymbolTransition = 3f;

        humanPurityTestImage.transform.SetPositionAndRotation(new Vector3(0f, -10f, 0f), defaultBillboardRotation);
        purityImageValue = humanPurityTestImage.GetComponent<RandomizeBillboardMaterial>().LastPickedIndex; // 0-3 are AI, 4-7 are human
        Debug.Log("Value of purity image: " + purityImageValue);
        if (purityImageValue <= 3) 
        {
            _next[0] = new SceneRef(RejectedFromHumansScene, aiPuritySceneParent, AmbRoute.Amb2, true);
            _next[1] = new SceneRef(AcceptedByHumansScene, humanPuritySceneParent, AmbRoute.Amb2, true);
            Debug.Log("BLOCK A");
        }
        else
        {
            _next[0] = new SceneRef(AcceptedByHumansScene, humanPuritySceneParent, AmbRoute.Amb2, true);
            _next[1] = new SceneRef(RejectedFromHumansScene, aiPuritySceneParent, AmbRoute.Amb2, true);
            Debug.Log("BLOCK B");
        }

        // start textbox AFTER reveal so typing is visible
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 13);
        yield return new WaitForSeconds(defaultTextBoxTime);

        StartCoroutine(Fade(humanPurityCheckmark, 1f, puritySymbolTransition));
        StartCoroutine(MoveTo(humanPurityCheckmark, new Vector3(-1.059f, 0f, -0.423f), puritySymbolTransition));
        StartCoroutine(Fade(humanPurityCross, 1f, puritySymbolTransition));
        StartCoroutine(MoveTo(humanPurityCross, new Vector3(1.059f, 0f, -0.423f), puritySymbolTransition));

        purityTestActive = true;
        ToggleTextbox(true, 15);
        float slideTransition = 3f;
        PurityTestSlide(humanPurityTestImage, slideTransition);
        yield return new WaitForSeconds(slideTransition);
        SetChoicePair(3);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator RejectedFromHumansScene()
    {
        StartupScene(rejectedFromHumansSceneParent);
        gotRejectedFromGroup = true;
        purityTestActive = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 21); //AI chosen
        else ToggleTextbox(true, 24);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(AIsAfterHumanRejectionScene, aiAfterHumanRejectionSceneParent, AmbRoute.Amb2, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator RejectedFromAIsScene()
    {
        StartupScene(rejectedFromAIsSceneParent);
        gotRejectedFromGroup = true;
        purityTestActive = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 20);
        else
        {
            ToggleTextbox(true, 22);
            yield return new WaitForSeconds(defaultTextBoxTime);
            ToggleTextbox(true, 23);
            yield return new WaitForSeconds(defaultTextBoxTime);
        }
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(HumansAfterAiRejectionScene, humanAfterAiRejectionSceneParent, AmbRoute.Amb2, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator AcceptedByHumansScene()
    {
        StartupScene(acceptedToHumansSceneParent);
        gotRejectedFromGroup = false;
        purityTestActive = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 17);
        else ToggleTextbox(true, 19);
        yield return new WaitForSeconds(defaultTextBoxTime);
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.Alley, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator AcceptedByAIsScene()
    {
        StartupScene(acceptedToAIsSceneParent);
        gotRejectedFromGroup = false;
        purityTestActive = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 16);
        else ToggleTextbox(true, 18);
        yield return new WaitForSeconds(defaultTextBoxTime);
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.Alley, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator AIsAfterHumanRejectionScene()
    {
        StartupScene(aiAfterHumanRejectionSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 25);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 27);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 29);
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.Alley, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator HumansAfterAiRejectionScene()
    {
        StartupScene(humanAfterAiRejectionSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 26);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 28);
        yield return new WaitForSeconds(defaultTextBoxTime);
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.Alley, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator PonderingScene()
    {
        StartupScene(ponderingSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 30);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 31);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 32);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(EndingScene, endingSceneParent, AmbRoute.Ending, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator EndingScene()
    {
        StartupScene(endingSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 33);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(ResultsScreen, resultsScreenParent, AmbRoute.Title, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator ResultsScreen()
    {
        StartupScene(resultsScreenParent);

        // Title label
        if (UseGerman) resultTitle.GetComponent<TextMeshPro>().text = "Dein GAIA-Rank:";
        else resultTitle.GetComponent<TextMeshPro>().text = "Your GAIA Rank:";

        // Rank ID (language-independent) with localized label
        var rankId = GetRankId(aiDoctorChosen, aiCrowdChosen, gotRejectedFromGroup);
        string rankingText = RankLabel(rankId, UseGerman);

        // Record and show stats
        if (GAIAStats.I != null)
        {
            GAIAStats.I.RecordRank(rankId);

            int nthOverall = GAIAStats.I.GetTotal(rankId);
            int nthToday = GAIAStats.I.GetToday(rankId);

            float pctOverall = GAIAStats.I.GetPercentOverall(rankId);
            float pctToday = GAIAStats.I.GetPercentToday(rankId);

            string todayStatsLine = UseGerman
                ? $"Du bist heute die #{nthToday} Person mit diesem Rank.\nHeute haben {pctToday:0}% aller Spieler diesen Rank erhalten!"
                : $"You are the #{nthToday} person today to get this rank.\nToday, {pctToday:0}% of all players got this rank!";

            string totalStatsLine = UseGerman
                ? $"Du bist insgesamt die #{nthOverall} Person mit diesem Rank.\nInsgesamt haben {pctOverall:0}% aller Spieler diesen Rank erhalten!"
                : $"You are the #{nthOverall} person overall to get this rank.\nOverall, {pctOverall:0}% of all players got this rank!";

            resultRank.GetComponent<TextMeshPro>().text = rankingText;
            todayStats.GetComponent<TextMeshPro>().text = todayStatsLine;
            totalStats.GetComponent<TextMeshPro>().text = totalStatsLine;
        }
        else
        {
            resultRank.GetComponent<TextMeshPro>().text = "ERROR";
            todayStats.GetComponent<TextMeshPro>().text = "GAIAStats";
            totalStats.GetComponent<TextMeshPro>().text = "Is null!";
        }

        yield return new WaitForSeconds(10f);
        
        // both sides go to the same destination
        _next[0] = new SceneRef(TitleScreen, titleScreenParent, AmbRoute.None, false);
        _next[1] = _next[0];
        _activeChoice = 0;
        yield return EndAfterChoice();
    }

    RankId GetRankId(bool aiDoctorChosen, bool aiCrowdChosen, bool gotRejectedFromGroup)
    {
        // AI Doc, AI Crowd
        if (aiDoctorChosen && aiCrowdChosen) return gotRejectedFromGroup ? RankId.Thinker : RankId.TechEnthusiast;

        // AI Doc, Human Crowd
        if (aiDoctorChosen && !aiCrowdChosen) return gotRejectedFromGroup ? RankId.Revolutionary : RankId.BridgeBuilder;

        // Human Doc, AI Crowd
        if (!aiDoctorChosen && aiCrowdChosen) return gotRejectedFromGroup ? RankId.Doubter : RankId.OpinionShaper;

        // Human Doc, Human Crowd
        return gotRejectedFromGroup ? RankId.Individualist : RankId.Humanist;
    }

    string RankLabel(RankId r, bool de) => r switch
    {
        RankId.Thinker => de ? "Denker" : "Thinker",
        RankId.TechEnthusiast => de ? "Technikenthusiast" : "Tech Enthusiast",
        RankId.Revolutionary => de ? "Revoluzer" : "Revolutionary",
        RankId.BridgeBuilder => de ? "Brückenbauer" : "Bridge Builder",
        RankId.Doubter => de ? "Zweifler" : "Doubter",
        RankId.OpinionShaper => de ? "Meinungsbildner" : "Opinion Shaper",
        RankId.Individualist => de ? "Individuist" : "Individualist",
        RankId.Humanist => "Humanist",
        _ => "Error"
    };

    public System.Collections.IEnumerator TitleScreen()
    {
        StartupScene(titleScreenParent);
        usBox.SetActive(true);
        deBox.SetActive(true);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);

        _next[0] = new SceneRef(LanguageSelectScene, languageSceneParent, AmbRoute.None, false);
        _next[1] = new SceneRef(LanguageSelectScene, languageSceneParent, AmbRoute.None, false);
        SetChoicePair(5);
        ToggleDecisionBoxes(true);
    }
}
