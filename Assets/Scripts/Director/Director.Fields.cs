using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TextboxScripts;

// This file is part of the partial Director class.
// It contains ALL Inspector fields + runtime state variables.
public partial class Director
{
    // -------------------------------------------------------------------------
    // Scene objects used as "hover zones" / choices
    // (These should have Colliders so hover/trigger detection works.)
    // -------------------------------------------------------------------------
    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;
    // current scene + routing (left/right)
    Func<System.Collections.IEnumerator> _currentScene; 
    SceneRef[] _next = new SceneRef[2];

    // -------------------------------------------------------------------------
    // Textbox UI (the dialogue box) + the hover-choice UI text ("choiceText")
    // -------------------------------------------------------------------------
    [Header("Textboxes")]
    public Transform textbox;
    [SerializeField] private TMP_Text textboxText;
    [SerializeField] private GameObject choiceText;
    public bool UseGerman { get; private set; } = false; // default to English

    // Canvas + UI camera:
    // - If Canvas is Screen Space Overlay: uiCamera can be null.
    // - If Canvas is Screen Space Camera: uiCamera should be set to the UI camera.
    [SerializeField] private Canvas canvas;              // drag your Canvas here (or auto-find)
    [SerializeField] private Camera uiCamera;            // leave null for Screen Space Overlay

    // How the choice UI text is positioned (between the door and center)
    [SerializeField] private float choiceTowardCenter = 0.25f; // 0..1
    [SerializeField] private Vector2 choiceOffsetPx = new(0f, 120f);
    [SerializeField] private float choiceAnimSeconds = 0.2f;

    // Hold-to-confirm ring around/near the choice text
    [SerializeField] private Image choiceRing;
    [SerializeField] private float choiceHoldSeconds = 1.25f; // how long to hold to confirm
    [SerializeField] private float choiceRingGapPx = 12f;

    // Transition timing between white fade + scene start
    [SerializeField] private float sceneStartLeadSeconds = 0.15f; // start scene this much before fade finishes
    [SerializeField] private float scenePrerollSeconds = 0.35f; // doors move under full white before fade-out starts

    // Cached UI transforms + coroutines so we can stop animations mid-way
    RectTransform _ringRt;
    Coroutine _ringScaleCo;

    float _choiceHold;

    TMP_Text _choiceTmp;
    RectTransform _choiceRt;
    Coroutine _choiceMoveCo, _choiceScaleCo;

    // Active hovered choice:
    // left = base, right = base+1
    int _activeChoice = -1;

    int _choiceBaseIndex = 0; // left = base, right = base+1

    // -------------------------------------------------------------------------
    // Audio: SFX + typing sound
    // -------------------------------------------------------------------------
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

    // Typing sound: throttle so it doesn’t spam too many clicks
    [SerializeField] private float typeMinInterval = 0.03f;   // seconds between type clicks
    [SerializeField] private float typePitchJitter = 0.1f;   // small pitch variation

    float _nextTypeSfxAt;
    bool _choiceWasOpen;

    // -------------------------------------------------------------------------
    // Ambiance:
    // We have TWO Ambiance tracks: one for ending 1 and one for ending 2.
    //
    // While hovering a choice:
    // - we play ONLY that side at low volume (ambPreviewVolume).
    //
    // Once selection is confirmed:
    // - chosen side ramps to 100%
    // - other side ramps to 0 and stops
    // -------------------------------------------------------------------------
    [Header("Ambiance")]
    [SerializeField] private AudioSource amb1;     // ending 1 Ambiance source
    [SerializeField] private AudioSource amb2;     // ending 2 Ambiance source
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

    // -------------------------------------------------------------------------
    // Whiteout overlay:
    // An Image that fades from white -> transparent -> white again.
    // It can also block clicks while visible (raycastTarget).
    // -------------------------------------------------------------------------
    [Header("Whiteout Loading Screen")]
    [SerializeField] private Image whiteout;
    [SerializeField] private float whiteoutFadeSeconds = 0.5f;
    [SerializeField] private bool whiteoutBlocksInput = true;

    Coroutine _whiteoutCo;
    bool _ending;

    // -------------------------------------------------------------------------
    // Scene parenting:
    // You have a "sceneParent" which contains children scenes.
    // ActivateOnlyScene() turns on only the child scene you want.
    // The current scene root is storing the active scene objects in its child objects.
    // -------------------------------------------------------------------------
    [Header("Scene Parent")]
    [SerializeField]
    private GameObject sceneParent;
    GameObject _currentSceneRoot;

    // -------------------------------------------------------------------------
    // Language Select Scene: Language doors sliding in.
    // -------------------------------------------------------------------------
    [Header("Language Select Scene")]
    [SerializeField]
    private GameObject languageSceneParent;
    [SerializeField]
    private GameObject doorEnglishL;
    [SerializeField]
    private GameObject doorGermanR;

    // -------------------------------------------------------------------------
    // Intro Scene: AI vs Human doctor after waking up.
    // -------------------------------------------------------------------------
    [Header("Intro Scene")]
    [SerializeField]
    private GameObject introSceneParent;
    [SerializeField]
    private GameObject introAiDoctor;
    [SerializeField]
    private GameObject introHumanDoctor;

    // -------------------------------------------------------------------------
    // Ending 1: island + boat drift
    // -------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------
    // Ending 2: full-screen object fade
    // -------------------------------------------------------------------------
    [Header("Demo Ending 2")]
    [SerializeField] private GameObject demoEnding2Parent;
    [SerializeField] private GameObject ending2FullScreenObject;

    Coroutine _boatCo;

    // This is a fixed rotation you want to apply to billboard objects
    private Quaternion defaultBillboardRotation = Quaternion.Euler(90f, 90f, -90f);

    [Serializable]
    public struct SceneRef
    {
        public Func<System.Collections.IEnumerator> routine;
        public GameObject root;

        public SceneRef(Func<System.Collections.IEnumerator> routine, GameObject root)
        {
            this.routine = routine;
            this.root = root;
        }

        public bool IsValid => routine != null;
    }
}
