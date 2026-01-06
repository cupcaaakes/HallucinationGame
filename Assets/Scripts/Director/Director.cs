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
            _choiceTmp = choiceText.GetComponent<TMP_Text>();
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

            // lock in ambiance to the chosen side (ramps chosen to 100%, other to 0)
            CommitAmbiance(_activeChoice);

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
        yield return RevealScene(LanguageSelectScene);
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
        PlaySfx(sfxTransition, transitionVolume);
        yield return FadeWhiteoutTo(1f, whiteoutFadeSeconds);

        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);

        // hide choice UI
        _activeChoice = -1;
        _choiceHold = 0f;

        if (choiceText) { choiceText.SetActive(false); if (_choiceRt) _choiceRt.localScale = Vector3.zero; }
        if (choiceRing) { choiceRing.fillAmount = 0f; choiceRing.gameObject.SetActive(false); if (_ringRt) _ringRt.localScale = Vector3.zero; }

        if (_boatCo != null) StopCoroutine(_boatCo);
        _boatCo = null;

        if (_currentScene == LanguageSelectScene) UseGerman = (chosen == 1); // left = English, right = German

        // route to next scene
        chosen = Mathf.Clamp(chosen, 0, 1);
        var next = _nextScene[chosen];

        // reset confirm gating for next scene
        _ending = false;
        _choiceWasOpen = false;

        // clear routing
        _nextScene[0] = null;
        _nextScene[1] = null;

        if (next != null) yield return RevealScene(next);
    }

    // -------------------------------------------------------------------------
    // RevealScene(sceneRoutine):
    // - Starts the scene routine (activate + setup)
    // - waits preroll
    // - fades white out
    // - waits for scene routine AND fade to finish
    // -------------------------------------------------------------------------
    System.Collections.IEnumerator RevealScene(Func<System.Collections.IEnumerator> sceneRoutine)
    {
        if (sceneRoutine == null)
        {
            Debug.LogError("RevealScene: sceneRoutine was null");
            yield break;
        }
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
}
