using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;
using System.Collections;

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
        if (!typeSfx || !sfxTypeChar) return;

        if (Time.unscaledTime < _nextTypeSfxAt) return; // throttle
        _nextTypeSfxAt = Time.unscaledTime + typeMinInterval;

        typeSfx.pitch = 1f + Random.Range(-typePitchJitter, typePitchJitter);
        typeSfx.PlayOneShot(sfxTypeChar, 1f);
    }

    // -------------------------------------------------------------------------
    // BoatDriftForever(): moves boat to the right and adds gentle sway forever
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator BoatDriftForever(Transform t)
    {
        if (!t) yield break;

        // remember whatever rotation you spawned the boat with
        Quaternion baseRot = t.rotation;

        float elapsed = 0f;
        float smoothRoll = 0f;

        while (t && t.gameObject.activeInHierarchy)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            // drift right at constant speed
            t.position += Vector3.right * (boatSpeedUnitsPerSec * dt);

            // amplitude eases in (0 -> 1). Clamp so it stays 1 after the ease time.
            float amp = (boatRollEaseOutSeconds <= 0f)
                ? 1f
                : Mathf.Clamp01(elapsed / boatRollEaseOutSeconds);

            // target roll is a sine wave in degrees
            float targetRoll = Mathf.Sin(elapsed * (Mathf.PI * 2f) * boatRollHz) * boatRollDegrees * amp;

            // damp/smooth the roll so it feels gentle
            float k = 1f - Mathf.Exp(-boatRollDamping * dt); // framerate-independent smoothing factor
            smoothRoll = Mathf.Lerp(smoothRoll, targetRoll, k);

            t.rotation = baseRot * Quaternion.Euler(0f, smoothRoll, 0f);

            yield return null;
        }
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
        _next[0] = new SceneRef(IntroScene, introSceneParent, AmbRoute.None, false);
        _next[1] = new SceneRef(IntroScene, introSceneParent, AmbRoute.None, false);

        StartupScene(languageSceneParent);

        StartCoroutine(Fade(doorEnglishL, 0f, 0f));
        StartCoroutine(Fade(doorGermanR, 0f, 0f));
        doorEnglishL.transform.position = new Vector3(decisionL.transform.position.x, 0f, 7.5f);
        doorGermanR.transform.position = new Vector3(decisionR.transform.position.x, 0f, 7.5f);
        doorEnglishL.transform.rotation = defaultBillboardRotation;
        doorGermanR.transform.rotation = defaultBillboardRotation;
        doorEnglishL.transform.localScale = new Vector3(0.2f, 0.1f, 0.1f);
        doorGermanR.transform.localScale = new Vector3(0.2f, 0.1f, 0.1f);

        float doorTransition = 3f;

        StartCoroutine(Fade(doorEnglishL, 1f, doorTransition));
        StartCoroutine(MoveTo(doorEnglishL, new Vector3(decisionL.transform.position.x, 0f, 1f), doorTransition));
        StartCoroutine(Fade(doorGermanR, 1f, doorTransition));
        StartCoroutine(MoveTo(doorGermanR, new Vector3(decisionR.transform.position.x, 0f, 1f), doorTransition));

        yield return new WaitForSeconds(doorTransition); // wait for door anims

        SetChoicePair(0);
        ToggleTextbox(true, 0);
        ToggleDecisionBoxes(true);
    }

    // -------------------------------------------------------------------------
    // IntroScene():
    // - Activates the first real scene
    // - Moves and fades the intro doctors into position
    // - next scene is the same for both choices but with different texts
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator IntroScene()
    {
        _next[0] = new SceneRef(CheckupSceneAi, checkupSceneAiParent, AmbRoute.None, false);
        _next[1] = new SceneRef(CheckupSceneHuman, checkupSceneHumanParent, AmbRoute.None, false);

        StartupScene(introSceneParent);

        StartCoroutine(Fade(introAiDoctor, 0f, 0f));
        StartCoroutine(Fade(introHumanDoctor, 0f, 0f));
        introAiDoctor.transform.position = new Vector3(decisionL.transform.position.x - 5f, 0.4f, 5f);
        introHumanDoctor.transform.position = new Vector3(decisionR.transform.position.x + 5f, 0f, 5f);
        introAiDoctor.transform.rotation = defaultBillboardRotation;
        introHumanDoctor.transform.rotation = defaultBillboardRotation;
        introAiDoctor.transform.localScale = new Vector3(0.2f, introAiDoctor.transform.localScale.y, introAiDoctor.transform.localScale.z);

        float doctorTransition = 3f;

        StartCoroutine(Fade(introAiDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introAiDoctor, new Vector3(decisionL.transform.position.x, 0.4f, 0f), doctorTransition));
        StartCoroutine(Fade(introHumanDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introHumanDoctor, new Vector3(decisionR.transform.position.x, 0f, 0f), doctorTransition));

        yield return new WaitForSeconds(doctorTransition); // wait for doctor anims

        SetChoicePair(1);
        ToggleTextbox(true, 1);
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
        ToggleTextbox(true, 2);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 4);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 6);
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

    // -------------------------------------------------------------------------
    // DemoEnding1():
    // - Activates ending 1 scene
    // - Starts boat drift coroutine
    // - Shows textbox line 1 after reveal timing
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator DemoEnding1()
    {
        StartupScene(demoEnding1Parent);

        // edge positions on the z-plane
        Vector3 rightEdge = ViewportToWorldOnZPlane(1f, 0.5f, endingPlaneZ);
        Vector3 bottomEdge = ViewportToWorldOnZPlane(0.5f, 0f, endingPlaneZ);

        if (boat)
        {
            _boatCo = StartCoroutine(BoatDriftForever(boat.transform));
        }

        // no choices in endings
        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);

        // start textbox AFTER reveal so typing is visible
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 1);
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

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 8);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 9);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 10);

        SetChoicePair(2);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator AiPurityScene()
    {
        StartupScene(aiPuritySceneParent);
        aiCrowdChosen = true;

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
        ToggleTextbox(true, 11);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 13);
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
        ToggleTextbox(true, 12);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 14);
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
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 20);
        else ToggleTextbox(true, 22);
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
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 19);
        else ToggleTextbox(true, 21);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(HumansAfterAiRejectionScene, humanAfterAiRejectionSceneParent, AmbRoute.Amb2, true)
        );
        yield break;
    }

    public System.Collections.IEnumerator AcceptedByHumansScene()
    {
        StartupScene(acceptedToHumansSceneParent);
        gotRejectedFromGroup = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 16);
        else ToggleTextbox(true, 18);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 24);
        yield return new WaitForSeconds(defaultTextBoxTime);
        yield return EndSceneWithNoChoiceMade(
            new SceneRef(VotingBoothScene, votingBoothSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator AcceptedByAIsScene()
    {
        StartupScene(acceptedToAIsSceneParent);
        gotRejectedFromGroup = false;
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (purityImageValue <= 3) ToggleTextbox(true, 15);
        else ToggleTextbox(true, 17);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 23);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(VotingBoothScene, votingBoothSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator AIsAfterHumanRejectionScene()
    {
        StartupScene(aiAfterHumanRejectionSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 25);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 23);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(VotingBoothScene, votingBoothSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator HumansAfterAiRejectionScene()
    {
        StartupScene(humanAfterAiRejectionSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 26);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 24);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(VotingBoothScene, votingBoothSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator VotingBoothScene()
    {
        StartupScene(votingBoothSceneParent);

        _next[0] = new SceneRef(LeavingBoothKeepScene, leavingBoothFlagSceneParent, AmbRoute.Amb2, true);
        _next[1] = new SceneRef(LeavingBoothFlagScene, leavingBoothFlagSceneParent, AmbRoute.Amb2, true);

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (aiCrowdChosen && !gotRejectedFromGroup || !aiCrowdChosen && gotRejectedFromGroup) // if AI crowd
        {
            ToggleTextbox(true, 27);
        }
        else // if human crowd
        {
            ToggleTextbox(true, 28);
        }
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 29);
        SetChoicePair(4);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator LeavingBoothKeepScene()
    {
        StartupScene(leavingBoothKeepSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (aiCrowdChosen && !gotRejectedFromGroup || !aiCrowdChosen && gotRejectedFromGroup) // if AI crowd
        {
            ToggleTextbox(true, 30);
        }
        else // if human crowd
        {
            ToggleTextbox(true, 31);
        }
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator LeavingBoothFlagScene()
    {
        StartupScene(leavingBoothFlagSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        if (aiCrowdChosen && !gotRejectedFromGroup || !aiCrowdChosen && gotRejectedFromGroup) // if AI crowd
        {
            ToggleTextbox(true, 32);
        }
        else // if human crowd
        {
            ToggleTextbox(true, 33);
        }
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(PonderingScene, ponderingSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator PonderingScene()
    {
        StartupScene(ponderingSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 34);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 35);
        yield return new WaitForSeconds(defaultTextBoxTime);
        ToggleTextbox(true, 36);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(EndingScene, endingSceneParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator EndingScene()
    {
        StartupScene(endingSceneParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 37);
        yield return new WaitForSeconds(defaultTextBoxTime);

        yield return EndSceneWithNoChoiceMade(
            new SceneRef(ResultsScreen, resultsScreenParent, AmbRoute.None, false)
        );
        yield break;
    }

    public System.Collections.IEnumerator ResultsScreen()
    {
        StartupScene(resultsScreenParent);
        yield return new WaitForSeconds(10f);

        // both sides go to the same destination
        _next[0] = new SceneRef(TitleScreen, titleScreenParent, AmbRoute.None, false);
        _next[1] = _next[0];
        _activeChoice = 0;
        yield return EndAfterChoice();
    }

    public System.Collections.IEnumerator TitleScreen()
    {
        StartupScene(titleScreenParent);
        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);

        _next[0] = new SceneRef(LanguageSelectScene, languageSceneParent, AmbRoute.None, false);
        _next[1] = new SceneRef(LanguageSelectScene, languageSceneParent, AmbRoute.None, false);
        SetChoicePair(5);
        ToggleDecisionBoxes(true);
    }
}
