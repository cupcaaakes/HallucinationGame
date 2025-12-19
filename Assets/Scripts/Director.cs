using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

public class Director : MonoBehaviour
{
    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;

    [Header("Textboxes")]
    public Transform textbox;
    [SerializeField] private TMP_Text textboxText;
    [SerializeField] private GameObject choiceText;
    [SerializeField] private Canvas canvas;              // drag your Canvas here (or auto-find)
    [SerializeField] private Camera uiCamera;            // leave null for Screen Space Overlay
    [SerializeField] private float choiceTowardCenter = 0.25f; // 0..1
    [SerializeField] private Vector2 choiceOffsetPx = new(0f, 120f);
    [SerializeField] private float choiceAnimSeconds = 0.2f;
    [SerializeField] private Image choiceRing;
    [SerializeField] private float choiceHoldSeconds = 1.25f; // how long to hold to confirm
    [SerializeField] private float choiceRingGapPx = 12f;
    [SerializeField] private float sceneStartLeadSeconds = 0.15f; // start scene this much before fade finishes
    [SerializeField] private float scenePrerollSeconds = 0.35f; // doors move under full white before fade-out starts

    RectTransform _ringRt;
    Coroutine _ringScaleCo;

    float _choiceHold;

    TMP_Text _choiceTmp;
    RectTransform _choiceRt;
    Coroutine _choiceMoveCo, _choiceScaleCo;
    int _activeChoice = -1; // -1 none, 0 left, 1 right

    [Header("Audio")]
    [SerializeField] private AudioSource sfx;      // UI / transitions / confirms (NO pitch jitter)
    [SerializeField] private AudioSource typeSfx;  // typing clicks (pitch jitter)
    [SerializeField] private AudioClip sfxTypeChar;
    [SerializeField] private AudioClip sfxTextboxOpen;
    [SerializeField] private AudioClip sfxTextboxClose;
    [SerializeField] private AudioClip sfxChoiceOpen;
    [SerializeField] private AudioClip sfxChoiceConfirm;
    [SerializeField] private AudioClip sfxTransition;

    [SerializeField] private float confirmVolume = 1f;
    [SerializeField] private float transitionVolume = 1f;

    [SerializeField] private float typeMinInterval = 0.03f;   // seconds between type clicks
    [SerializeField] private float typePitchJitter = 0.1f;   // small pitch variation

    float _nextTypeSfxAt;
    bool _choiceWasOpen;

    [Header("Ambience")]
    [SerializeField] private AudioSource amb1;     // ending 1 ambience source
    [SerializeField] private AudioSource amb2;     // ending 2 ambience source
    [SerializeField] private AudioClip ambEnding1;
    [SerializeField] private AudioClip ambEnding2;

    [SerializeField, Range(0f, 1f)] private float ambPreviewVolume = 0.33f;
    [SerializeField] private float ambPreviewFadeSeconds = 0.25f;
    [SerializeField] private float ambCommitFadeSeconds = 1.25f;
    [SerializeField] private float ambStopFadeSeconds = 0.25f;

    Coroutine _ambCo;
    bool _ambPreviewActive;
    bool _ambCommitted;
    int _ambPreviewSide = -1; // -1 none, 0 left, 1 right

    [Header("Whiteout Loading Screen")]
    [SerializeField] private Image whiteout;
    [SerializeField] private float whiteoutFadeSeconds = 0.5f;
    [SerializeField] private bool whiteoutBlocksInput = true;

    Coroutine _whiteoutCo;
    bool _ending;


    [Header("Scene Parent")]
    [SerializeField]
    private GameObject sceneParent;

    [Header("Demo Scene")]
    [SerializeField]
    private GameObject demoSceneParent;
    [SerializeField]
    private GameObject doorL;
    [SerializeField]
    private GameObject doorR;

    [Header("Demo Ending 1")]
    [SerializeField] private GameObject demoEnding1Parent;
    [SerializeField] private GameObject island;
    [SerializeField] private GameObject boat;
    [SerializeField] private GameObject woodenOverpass;
    [SerializeField] private float endingPlaneZ = 0f;
    [SerializeField] private float boatSpeedUnitsPerSec = 0.05f;
    [SerializeField] private float boatRollDegrees = 4f;          // max roll angle
    [SerializeField] private float boatRollHz = 0.20f;            // cycles per second (0.2 = 5s per cycle)
    [SerializeField] private float boatRollEaseOutSeconds = 2.0f; // how long until sway reaches full strength
    [SerializeField] private float boatRollDamping = 0.35f;       // higher = settles faster (smooths jitter)

    [Header("Demo Ending 2")]
    [SerializeField] private GameObject demoEnding2Parent;
    [SerializeField] private GameObject ending2FullScreenObject;

    Coroutine _boatCo;


    private Quaternion defaultBillboardRotation = Quaternion.Euler(90f, 90f, -90f);

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
        SetupAmbienceSources();

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



        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (!decisionL || !decisionR) return;

        // don’t progress if decision boxes are disabled
        if (!decisionL.activeInHierarchy || !decisionR.activeInHierarchy) return;

        if (_activeChoice < 0) return;

        _choiceHold += Time.deltaTime;

        if (choiceRing)
            choiceRing.fillAmount = Mathf.Clamp01(_choiceHold / Mathf.Max(0.0001f, choiceHoldSeconds));

        if (_choiceHold >= choiceHoldSeconds && !_ending)
        {
            _ending = true;

            // lock in ambience to the chosen side (ramps chosen to 100%, other to 0)
            CommitAmbience(_activeChoice);

            PlaySfx(sfxChoiceConfirm, confirmVolume);
            StartCoroutine(EndAfterChoice());

        }
    }

    System.Collections.IEnumerator RunGame()
    {
        // 1) Start the scene immediately (doors start moving right away under white)
        var sceneCo = StartCoroutine(DemoScene());

        // 2) Optional: keep full white for a tiny bit so stuff is already in motion
        if (scenePrerollSeconds > 0f)
            yield return new WaitForSeconds(scenePrerollSeconds);

        // 3) Now fade out to reveal the already-moving scene
        var fadeCo = StartCoroutine(FadeWhiteoutTo(0f, whiteoutFadeSeconds));

        // 4) Wait for both to finish (order doesn't matter)
        yield return sceneCo;
        yield return fadeCo;
    }

    System.Collections.IEnumerator EndAfterChoice()
    {
        int chosen = _activeChoice; // 0 left, 1 right
        PlaySfx(sfxTransition, transitionVolume);
        yield return FadeWhiteoutTo(1f, whiteoutFadeSeconds);

        ToggleDecisionBoxes(false);
        SetDecisionColliders(false);

        // hide choice UI
        _activeChoice = -1;   // now it's safe to reset
        _choiceHold = 0f;

        if (choiceText) { choiceText.SetActive(false); if (_choiceRt) _choiceRt.localScale = Vector3.zero; }
        if (choiceRing) { choiceRing.fillAmount = 0f; choiceRing.gameObject.SetActive(false); if (_ringRt) _ringRt.localScale = Vector3.zero; }

        if (_boatCo != null) StopCoroutine(_boatCo);
        _boatCo = null;

        if (chosen == 0)
            yield return RevealScene(DemoEnding1());
        else
            yield return RevealScene(DemoEnding2());
    }



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

    private void ToggleDecisionBoxes(bool active)
    {
        if (decisionL) decisionL.SetActive(active);
        if (decisionR) decisionR.SetActive(active);
    }

    private void ToggleTextbox(bool open, int? textBoxLine)
    {
        if (!textbox) return;
        PlaySfx(open ? sfxTextboxOpen : sfxTextboxClose);
        StopCoroutine(nameof(ToggleTextbox)); // prevents stacking calls
        StartCoroutine(ToggleTextbox(open, textBoxLine, 0.35f));
    }

    System.Collections.IEnumerator ToggleTextbox(bool open, int? textBoxLine, float seconds)
    {
        if (!textbox) yield break;

        Vector3 from = textbox.localScale;
        Vector3 to = open ? Vector3.one : Vector3.zero;

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

        if (textBoxLine.HasValue)
        {
            var line = TextboxScripts.Lines[textBoxLine.Value];
            textboxText.fontSize = line.fontSize;
            yield return TypeText(line.text, 45f);
        }

        textbox.localScale = to;

        // hide after closing so it doesn't block clicks etc.
        if (!open) textbox.gameObject.SetActive(false);

        // "display whatever Text (TMP) contains" happens automatically once it's visible
    }

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

    void SetupAmbienceSources()
    {
        // Create sources if missing
        if (!amb1)
        {
            amb1 = gameObject.AddComponent<AudioSource>();
            ConfigureAmbSource(amb1);
        }
        else ConfigureAmbSource(amb1);

        if (!amb2)
        {
            amb2 = gameObject.AddComponent<AudioSource>();
            ConfigureAmbSource(amb2);
        }
        else ConfigureAmbSource(amb2);

        amb1.clip = ambEnding1;
        amb2.clip = ambEnding2;

        // start silent
        amb1.volume = 0f;
        amb2.volume = 0f;
    }

    void ConfigureAmbSource(AudioSource a)
    {
        a.playOnAwake = false;
        a.loop = true;
        a.spatialBlend = 0f; // 2D
        a.pitch = 1f;
    }

    void EnsureAmbiencePlaying()
    {
        if (amb1 && amb1.clip && !amb1.isPlaying) amb1.Play();
        if (amb2 && amb2.clip && !amb2.isPlaying) amb2.Play();
    }

    void FadeAmbienceTo(float v1, float v2, float seconds, bool stopWhenSilent)
    {
        if (_ambCo != null) StopCoroutine(_ambCo);
        _ambCo = StartCoroutine(FadeAmbienceRoutine(v1, v2, seconds, stopWhenSilent));
    }

    System.Collections.IEnumerator FadeAmbienceRoutine(float to1, float to2, float seconds, bool stopWhenSilent)
    {
        if (!amb1 && !amb2) yield break;

        float from1 = amb1 ? amb1.volume : 0f;
        float from2 = amb2 ? amb2.volume : 0f;

        if (seconds <= 0f)
        {
            if (amb1) amb1.volume = to1;
            if (amb2) amb2.volume = to2;
        }
        else
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
            {
                float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
                if (amb1) amb1.volume = Mathf.Lerp(from1, to1, ease);
                if (amb2) amb2.volume = Mathf.Lerp(from2, to2, ease);
                yield return null;
            }
            if (amb1) amb1.volume = to1;
            if (amb2) amb2.volume = to2;
        }

        if (stopWhenSilent)
        {
            if (amb1 && amb1.volume <= 0.001f) amb1.Stop();
            if (amb2 && amb2.volume <= 0.001f) amb2.Stop();
        }
    }

    void StartAmbiencePreview(int side) // 0 = left, 1 = right
    {
        if (_ambCommitted) return;

        // already previewing this side
        if (_ambPreviewActive && _ambPreviewSide == side) return;

        _ambPreviewActive = true;
        _ambPreviewSide = side;

        EnsureAmbiencePlaying();

        float v1 = (side == 0) ? ambPreviewVolume : 0f;
        float v2 = (side == 1) ? ambPreviewVolume : 0f;

        FadeAmbienceTo(v1, v2, ambPreviewFadeSeconds, stopWhenSilent: false);
    }

    void StopAmbiencePreview()
    {
        if (_ambCommitted) return;

        _ambPreviewActive = false;
        _ambPreviewSide = -1;

        FadeAmbienceTo(0f, 0f, ambStopFadeSeconds, stopWhenSilent: true);
    }



    void CommitAmbience(int chosenSide) // 0 = left, 1 = right
    {
        if (_ambCommitted) return;
        _ambCommitted = true;

        EnsureAmbiencePlaying();

        float v1 = (chosenSide == 0) ? 1f : 0f;
        float v2 = (chosenSide == 1) ? 1f : 0f;

        FadeAmbienceTo(v1, v2, ambCommitFadeSeconds, stopWhenSilent: true);
    }


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

        // CLOSE
        if (!open)
        {
            if (_activeChoice != side) return; // ignore exit from the other box

            _activeChoice = -1;
            _choiceHold = 0f;
            _choiceWasOpen = false;
            if (_ambPreviewActive)
            {
                _ambPreviewActive = false;
                StopAmbiencePreview();
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

        // OPEN
        if (_activeChoice != side) _choiceHold = 0f; // switching sides resets hold
        _activeChoice = side;
        StartAmbiencePreview(side);

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

        // set the text FIRST
        var line = ChoiceTextScripts.Lines[side];
        _choiceTmp.text = line.text;
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

    void SetWhiteoutAlpha(float a)
    {
        if (!whiteout) return;
        var c = whiteout.color;
        c.a = a;
        whiteout.color = c;
    }

    System.Collections.IEnumerator FadeWhiteoutTo(float toAlpha, float seconds)
    {
        if (!whiteout) yield break;

        // make sure it's visible while fading
        whiteout.gameObject.SetActive(true);
        if (whiteoutBlocksInput) whiteout.raycastTarget = true;

        float fromAlpha = whiteout.color.a;

        if (seconds <= 0f)
        {
            SetWhiteoutAlpha(toAlpha);
        }
        else
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.0001f, seconds))
            {
                float ease = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
                SetWhiteoutAlpha(Mathf.Lerp(fromAlpha, toAlpha, ease));
                yield return null;
            }
            SetWhiteoutAlpha(toAlpha);
        }

        // once fully faded out, you can disable it (and stop blocking input)
        if (toAlpha <= 0.001f)
        {
            if (whiteoutBlocksInput) whiteout.raycastTarget = false;
            whiteout.gameObject.SetActive(false);
        }
        else
        {
            if (whiteoutBlocksInput) whiteout.raycastTarget = true;
            whiteout.gameObject.SetActive(true);
        }
    }

    System.Collections.IEnumerator SceneTransition()
    {
        // fade IN fully (usually you want this to finish before swapping visuals)
        yield return FadeWhiteoutTo(1f, whiteoutFadeSeconds);

        // start fading OUT, but start the next scene slightly before it's fully gone
        var fadeOut = StartCoroutine(FadeWhiteoutTo(0f, whiteoutFadeSeconds));

        float wait = Mathf.Max(0f, whiteoutFadeSeconds - sceneStartLeadSeconds);
        if (wait > 0f) yield return new WaitForSeconds(wait);

        // then your next scene method runs here, e.g.:
        // yield return Scene2();

        yield return fadeOut;
    }

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

    void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (!clip || !sfx) return;
        sfx.pitch = 1f;
        sfx.PlayOneShot(clip, volume);
    }

    void PlayTypeSfx()
    {
        if (!typeSfx || !sfxTypeChar) return;

        if (Time.unscaledTime < _nextTypeSfxAt) return; // throttle
        _nextTypeSfxAt = Time.unscaledTime + typeMinInterval;

        typeSfx.pitch = 1f + Random.Range(-typePitchJitter, typePitchJitter);
        typeSfx.PlayOneShot(sfxTypeChar, 1f);
    }


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

    System.Collections.IEnumerator RevealScene(System.Collections.IEnumerator sceneRoutine)
    {
        var sceneCo = StartCoroutine(sceneRoutine);

        if (scenePrerollSeconds > 0f)
            yield return new WaitForSeconds(scenePrerollSeconds);

        var fadeCo = StartCoroutine(FadeWhiteoutTo(0f, whiteoutFadeSeconds));

        yield return sceneCo;
        yield return fadeCo;
    }


    public System.Collections.IEnumerator DemoScene()
    {
        if (demoSceneParent) demoSceneParent.SetActive(true);

        StartCoroutine(Fade(doorL, 0f, 0f));
        StartCoroutine(Fade(doorR, 0f, 0f));
        doorL.transform.position = new Vector3(decisionL.transform.position.x, 0f, 5f);
        doorR.transform.position = new Vector3(decisionR.transform.position.x, 0f, 5f);
        doorL.transform.rotation = defaultBillboardRotation;
        doorR.transform.rotation = defaultBillboardRotation;

        float doorTransition = 3f;

        StartCoroutine(Fade(doorL, 1f, doorTransition));
        StartCoroutine(MoveTo(doorL, new Vector3(decisionL.transform.position.x, 0f, 0f), doorTransition));
        StartCoroutine(Fade(doorR, 1f, doorTransition));
        StartCoroutine(MoveTo(doorR, new Vector3(decisionR.transform.position.x, 0f, 0f), doorTransition));

        yield return new WaitForSeconds(doorTransition); // wait for door anims

        ToggleTextbox(true, 0);
        ToggleDecisionBoxes(true);
    }

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