using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TextboxScripts;

// -----------------------------------------------------------------------------
// Director.cs
//
// What this script does (high level):
// 1) Starts with a full-white screen, while the "DemoScene" starts moving behind it.
// 2) Fades the white away so the player sees two doors (left/right).
// 3) When you hover a door "decision box", it shows a UI text label + a hold-to-confirm ring.
// 4) While hovering, it previews Ambiance for that side at low volume (~33%).
// 5) If you keep hovering long enough, it "chooses" that side, fades to white,
//    ramps Ambiance for the chosen side to 100%, then reveals that ending scene.
//
// Important Unity basics for Tom:
// - Start() runs once when the scene starts.
// - Update() runs every frame.
// - Coroutines (IEnumerator + StartCoroutine) let you do timed sequences with "yield return".
// - GameObject: a thing in the scene (door, UI, etc.).
// - Component: a piece attached to a GameObject (AudioSource, Collider, etc.).
// - "SerializedField" means you can drag stuff into the field in the Unity Inspector.
// -----------------------------------------------------------------------------
public partial class Director : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Start(): one-time setup
    // - Disables scenes
    // - Prepares audio sources
    // - Prepares UI (textbox/choice text/ring)
    // - Starts white screen and starts the main flow coroutine
    // -------------------------------------------------------------------------
    void Start()
    {
        if (sceneParent) sceneParent.SetActive(true);
        if (decisionL && decisionR) ToggleDecisionBoxes(false);
        else Debug.LogError("FATAL ERROR: No decision boxes found!");
        foreach (Transform child in sceneParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        if (!sfx) sfx = GetComponent<AudioSource>();
        if (!typeSfx)
        {
            var sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                // pick the one that's NOT sfx
                typeSfx = (sources[0] == sfx) ? sources[1] : sources[0];
            }
            else
            {
                // create a second one automatically (safe fallback)
                typeSfx = gameObject.AddComponent<AudioSource>();
                typeSfx.playOnAwake = false;
                typeSfx.spatialBlend = 0f; // 2D
            }
        }
        SetupAmbianceSources();

        if (textbox)
        {
            textbox.localScale = Vector3.zero;
            textbox.gameObject.SetActive(false);
        }
        if (choiceText)
        {
            _choiceTmp = choiceText.GetComponentInChildren<TMP_Text>(true);
            _choiceRt = choiceText.GetComponent<RectTransform>();

            if (!canvas) canvas = choiceText.GetComponentInParent<Canvas>();
            // uiCamera stays null for Screen Space Overlay. If your canvas is Screen Space - Camera, assign the camera.

            choiceText.SetActive(false);
            _choiceRt.localScale = Vector3.zero;
        }
        if (choiceRing)
        {
            _ringRt = choiceRing.rectTransform;
            // ring should be positioned in ChoiceText-local space
            if (_choiceRt) _ringRt.SetParent(_choiceRt, worldPositionStays: false);

            // make anchoredPosition behave predictably
            _ringRt.anchorMin = _ringRt.anchorMax = new Vector2(0.5f, 0.5f);
            _ringRt.pivot = new Vector2(0.5f, 0.5f);

            choiceRing.fillAmount = 0f;
            choiceRing.gameObject.SetActive(false);
            _ringRt.localScale = Vector3.zero;
        }
        if (whiteout)
        {
            whiteout.gameObject.SetActive(true);
            glitchTransitionOverlay.SetActive(true);
            if (whiteoutBlocksInput) whiteout.raycastTarget = true;
            SetWhiteoutAlpha(1f); // start fully white
        }

        // Main flow begins here
        StartCoroutine(RunGame());
    }

    // -------------------------------------------------------------------------
    // Update(): every frame
    //
    // This is ONLY used for the "hold to confirm" timer:
    // - While you're hovering a choice, _activeChoice is 0 or 1
    // - We fill the ring over time
    // - Once time is reached, we confirm that choice
    // -------------------------------------------------------------------------
    void Update()
    {
        if (purityTestActive)
        {
            float t = (Mathf.Sin(Time.unscaledTime * titlePulseSpeed) + 1f) * 0.5f; // 0..1
            float s = Mathf.Lerp(0.1f, 0.125f, t); // 0.5 -> 2
            aiPurityCheckmark.transform.localScale = new Vector3(s, s, s);
            aiPurityCross.transform.localScale = new Vector3(s, s, s);
            humanPurityCheckmark.transform.localScale = new Vector3(s, s, s);
            humanPurityCross.transform.localScale = new Vector3(s, s, s);
        }

        if (ikDriver)
        {
            SetArmChoicePreviewFromBools(
                ikDriver.LeftArmRaised,
                ikDriver.RightArmRaised
            );
        }

        UpdateIdleReturnToTitle();

        if (!decisionL || !decisionR) return;

        // don’t progress if decision boxes are disabled
        if (!decisionL.activeInHierarchy || !decisionR.activeInHierarchy) return;

        // no active hover = no hold progress
        if (_activeChoice < 0) return;

        _choiceHold += Time.deltaTime;

        // fill ring from 0..1 over choiceHoldSeconds
        if (choiceRing)
            choiceRing.fillAmount = Mathf.Clamp01(_choiceHold / Mathf.Max(0.0001f, choiceHoldSeconds));

        // reached hold time: confirm choice exactly once
        if (_choiceHold >= choiceHoldSeconds && !_ending)
        {
            _ending = true;

            PlaySfx(sfxChoiceConfirm, confirmVolume);
            StartCoroutine(EndAfterChoice());
        }
    }

    // -------------------------------------------------------------------------
    // RunGame(): master sequence at start
    // 1) Start demo scene logic immediately (doors begin moving "under white")
    // 2) Wait a short preroll time
    // 3) Fade white away so scene is visible
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator RunGame()
    {
        //yield return RevealScene(HumanPurityScene, humanPuritySceneParent);
        yield return RevealScene(TitleScreen, titleScreenParent);
    }

    // -------------------------------------------------------------------------
    // EndAfterChoice(): what happens after you confirm a choice
    // 1) Fade to white
    // 2) Disable decision boxes and UI
    // 3) Stop boat coroutine (if it was running)
    // 4) Reveal the selected ending scene
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator EndAfterChoice()
    {
        int chosen = _activeChoice; // 0 left, 1 right
        // Capture destination
        var next = _next[chosen];
        // Block re-entry
        _ending = true;

        // Ambiance routing based on DESTINATION (works for choice + auto)
        if (next.IsValid)
        {
            if (next.amb == AmbRoute.None)
            {
                _ambCommitted = false;   // unlock so stopping works
                StopAmbiancePreview();   // fades both out and stops them
            }
            else if (next.commitAmbOnConfirm)
            {
                CommitAmbiance(next.amb);
            }
            else
            {
                // optional: if you want non-committed ambience to stop after confirming
                // _ambCommitted = false;
                // StopAmbiancePreview();
            }
        }

        PlaySfx(sfxTransition, transitionVolume);
        yield return FadeWhiteoutTo(1f, whiteoutFadeSeconds);

        ToggleTextbox(false, null);
        ToggleDecisionBoxes(false);

        // hide choice UI
        _activeChoice = -1;
        _choiceHold = 0f;

        if (choiceText) { choiceText.SetActive(false); if (_choiceRt) _choiceRt.localScale = Vector3.zero; }
        if (choiceRing) { choiceRing.fillAmount = 0f; choiceRing.gameObject.SetActive(false); if (_ringRt) _ringRt.localScale = Vector3.zero; }

        if (_boatCo != null) StopCoroutine(_boatCo);
        _boatCo = null;

        if (_currentScene == LanguageSelectScene) UseGerman = (chosen == 1); // left = English, right = German

        // reset confirm gating for next scene
        _ending = false;
        chosen = Mathf.Clamp(chosen, 0, 1);

        _choiceWasOpen = false;

        // clear routing
        _next[0] = default;
        _next[1] = default;

        if (next.IsValid) yield return RevealScene(next.routine, next.root);
    }

    // -------------------------------------------------------------------------
    // RevealScene(sceneRoutine):
    // - Starts the scene routine (activate + setup)
    // - waits preroll
    // - fades white out
    // - waits for scene routine AND fade to finish
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator RevealScene(Func<System.Collections.IEnumerator> sceneRoutine, GameObject sceneRoot)
    {
        if (sceneRoutine == null)
        {
            Debug.LogError("RevealScene: sceneRoutine was null");
            yield break;
        }

        // disable previous root
        if (_currentSceneRoot) _currentSceneRoot.SetActive(false);

        // enable new root
        _currentSceneRoot = sceneRoot;
        if (_currentSceneRoot) _currentSceneRoot.SetActive(true);
        if (whiteout) SetWhiteoutAlpha(whiteout.color.a);

        ToggleDecisionBoxes(false); // we disable the decision boxes until the scene specifically enables them

        _currentScene = sceneRoutine; // lets EndAfterChoice know what choice means

        var sceneCo = StartCoroutine(sceneRoutine());

        // let things move under white before revealing
        if (scenePrerollSeconds > 0f)
            yield return new WaitForSeconds(scenePrerollSeconds);

        // fade white out
        var fadeCo = StartCoroutine(FadeWhiteoutTo(0f, whiteoutFadeSeconds));

        // wait for both
        yield return sceneCo;
        yield return fadeCo;
    }

    private void UpdateIdleReturnToTitle()
    {
        if (!idleReturnEnabled) return;
        if (DisableInactivityTimer) return;

        // Only do this if Kinect exists (otherwise you’ll “idle return” in editor/dev by accident)
        if (ikDriver == null || ikDriver.kinect == null) return;

        // If we're already on the title screen, keep the timer reset
        if (_currentScene == TitleScreen)
        {
            _idleNoUserTime = 0f;
            _idleReturnInProgress = false; // allow future idle returns after leaving title again
            return;
        }

        bool hasUser = ikDriver.kinect.TryGetLatestSkeleton(out var _);

        if (hasUser)
        {
            _idleNoUserTime = 0f;
            _idleReturnInProgress = false;
            return;
        }

        if (_idleReturnInProgress) return;

        _idleNoUserTime += Time.unscaledDeltaTime;

        if (_idleNoUserTime >= idleReturnSeconds)
        {
            ForceReturnToTitle();
        }
    }

    private void ForceReturnToTitle()
    {
        _idleReturnInProgress = true;
        _idleNoUserTime = 0f;

        // Stop everything currently running (scene routines, fades, boat coroutine, etc.)
        StopAllCoroutines();

        // Hard reset state
        _ending = false;
        _activeChoice = -1;
        _choiceHold = 0f;
        _choiceWasOpen = false;

        // Hide choice UI
        if (choiceText) { choiceText.SetActive(false); if (_choiceRt) _choiceRt.localScale = Vector3.zero; }
        if (choiceRing) { choiceRing.fillAmount = 0f; choiceRing.gameObject.SetActive(false); if (_ringRt) _ringRt.localScale = Vector3.zero; }

        // Hide textbox UI
        ToggleTextbox(false, null);
        ToggleDecisionBoxes(false);

        // Stop ambiance immediately (don’t rely on fade coroutines because we just stopped them)
        _ambCommitted = false;
        _ambPreviewActive = false;
        _ambPreviewSide = -1;
        if (amb1) { amb1.Stop(); amb1.volume = 0f; }
        if (amb2) { amb2.Stop(); amb2.volume = 0f; }
        if (amb3) { amb3.Stop(); amb3.volume = 0f; }
        if (amb4) { amb4.Stop(); amb4.volume = 0f; }
        if (amb5) { amb5.Stop(); amb5.volume = 0f; }
        if (amb6) { amb6.Stop(); amb6.volume = 0f; }

        // Force white screen up immediately (so the jump feels intentional)
        if (whiteout)
        {
            whiteout.gameObject.SetActive(true);
            glitchTransitionOverlay.SetActive(true);
            if (whiteoutBlocksInput) whiteout.raycastTarget = true;
            SetWhiteoutAlpha(1f);
        }

        // Make sure title pulse is active again
        isTitleScreenActive = true;

        // Restart the normal flow at the title
        StartCoroutine(RevealScene(TitleScreen, titleScreenParent));
    }

}
