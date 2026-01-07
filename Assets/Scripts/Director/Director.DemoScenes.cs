using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

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

        if (languageSceneParent) languageSceneParent.SetActive(true);

        StartCoroutine(Fade(doorEnglishL, 0f, 0f));
        StartCoroutine(Fade(doorGermanR, 0f, 0f));
        doorEnglishL.transform.position = new Vector3(decisionL.transform.position.x, 0f, 5f);
        doorGermanR.transform.position = new Vector3(decisionR.transform.position.x, 0f, 5f);
        doorEnglishL.transform.rotation = defaultBillboardRotation;
        doorGermanR.transform.rotation = defaultBillboardRotation;

        float doorTransition = 3f;

        StartCoroutine(Fade(doorEnglishL, 1f, doorTransition));
        StartCoroutine(MoveTo(doorEnglishL, new Vector3(decisionL.transform.position.x, 0f, 0f), doorTransition));
        StartCoroutine(Fade(doorGermanR, 1f, doorTransition));
        StartCoroutine(MoveTo(doorGermanR, new Vector3(decisionR.transform.position.x, 0f, 0f), doorTransition));

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

        if (introSceneParent) introSceneParent.SetActive(true);

        StartCoroutine(Fade(introAiDoctor, 0f, 0f));
        StartCoroutine(Fade(introHumanDoctor, 0f, 0f));
        introAiDoctor.transform.position = new Vector3(decisionL.transform.position.x, 0f, 5f);
        introHumanDoctor.transform.position = new Vector3(decisionR.transform.position.x, 0f, 5f);
        introAiDoctor.transform.rotation = defaultBillboardRotation;
        introHumanDoctor.transform.rotation = defaultBillboardRotation;

        float doctorTransition = 3f;

        StartCoroutine(Fade(introAiDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introAiDoctor, new Vector3(decisionL.transform.position.x, 0f, 0f), doctorTransition));
        StartCoroutine(Fade(introHumanDoctor, 1f, doctorTransition));
        StartCoroutine(MoveTo(introHumanDoctor, new Vector3(decisionR.transform.position.x, 0f, 0f), doctorTransition));

        yield return new WaitForSeconds(doctorTransition); // wait for doctor anims

        SetChoicePair(1);
        ToggleTextbox(true, 1);
        ToggleDecisionBoxes(true);
    }

    public System.Collections.IEnumerator CheckupSceneAi()
    {
        _next[0] = new SceneRef(DemoEnding1, demoEnding1Parent, AmbRoute.None, false);
        _next[1] = new SceneRef(DemoEnding2, demoEnding2Parent, AmbRoute.None, false);

        if (checkupSceneAiParent) checkupSceneAiParent.SetActive(true);
        checkupAiDoctor.transform.position = introAiDoctor.transform.position;
        checkupAiDoctor.transform.rotation = defaultBillboardRotation;

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 2);
        yield return new WaitForSeconds(7.5f);
    }

    public System.Collections.IEnumerator CheckupSceneHuman()
    {
        _next[0] = new SceneRef(DemoEnding1, demoEnding1Parent, AmbRoute.None, false);
        _next[1] = new SceneRef(DemoEnding2, demoEnding2Parent, AmbRoute.None, false);

        if (checkupSceneHumanParent) checkupSceneHumanParent.SetActive(true);
        checkupHumanDoctor.transform.position = introHumanDoctor.transform.position;
        checkupHumanDoctor.transform.rotation = defaultBillboardRotation;

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 2);
        yield return new WaitForSeconds(7.5f);
    }

    // -------------------------------------------------------------------------
    // DemoEnding1():
    // - Activates ending 1 scene
    // - Starts boat drift coroutine
    // - Shows textbox line 1 after reveal timing
    // -------------------------------------------------------------------------
    public System.Collections.IEnumerator DemoEnding1()
    {
        ActivateOnlyScene(demoEnding1Parent);

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
    public System.Collections.IEnumerator DemoEnding2()
    {
        ActivateOnlyScene(demoEnding2Parent);

        if (ending2FullScreenObject)
        {
            ending2FullScreenObject.transform.rotation = defaultBillboardRotation;
            StartCoroutine(Fade(ending2FullScreenObject, 1f, 0f));
        }

        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);

        yield return new WaitForSeconds(scenePrerollSeconds + whiteoutFadeSeconds);
        ToggleTextbox(true, 2);
        yield return new WaitForSeconds(7.5f);
        ToggleTextbox(true, 3);
    }
}
