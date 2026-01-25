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
    [SerializeField] private AzureKinectIKDriver ikDriver;

    [SerializeField] private GameObject player;

    // -------------------------------------------------------------------------
    // Scene objects used as "hover zones" / choices
    // (These should have Colliders so hover/trigger detection works.)
    // -------------------------------------------------------------------------
    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;
    [SerializeField] private Transform speechBubble;
    // current scene + routing (left/right)
    Func<System.Collections.IEnumerator> _currentScene;
    SceneRef[] _next = new SceneRef[2];

    [SerializeField] private Transform playerTransform;

    [SerializeField] private bool DisableInactivityTimer;

    /// <summary>
    /// 0-3 are AI, 4-7 are human
    /// </summary>
    private int purityImageValue = 0;

    int _armChoice = -1;        // -1 none, 0 left, 1 right (arm-based preview)
    int _previewChoice = -1;    // which side the choice UI is currently showing
    // -------------------------------------------------------------------------
    // Textbox UI (the dialogue box) + the hover-choice UI text ("choiceText")
    // -------------------------------------------------------------------------
    [Header("Textboxes")]
    public Transform textbox;
    [SerializeField] private TMP_Text textboxText;
    [SerializeField] private GameObject choiceText;
    private GameObject bubble;
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
    [SerializeField] private float defaultTextBoxTime = 7.5f; // default textbox time in seconds
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

    [Header("Idle Return To Title")]
    [SerializeField] private bool idleReturnEnabled = true;
    [SerializeField] private float idleReturnSeconds = 30f;

    // runtime
    private float _idleNoUserTime = 0f;
    private bool _idleReturnInProgress = false;


    // -------------------------------------------------------------------------
    // Audio: SFX + typing sound
    // -------------------------------------------------------------------------
    [Header("Audio")]
    [SerializeField] private AudioSource sfx;      // UI / transitions / confirms (NO pitch jitter)
    [SerializeField] private AudioSource typeSfx;  // typing clicks (pitch jitter)
    [SerializeField] private AudioClip[] sfxTypeChars; // put 20 clips in here in the Inspector
    [SerializeField] private AudioClip[] sfxGlitches; // put 20 clips in here in the Inspector
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

    private float _nextTypeSfxAt;
    private int _typeSfxIdx;
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
    [SerializeField] private AudioSource amb3;
    [SerializeField] private AudioSource amb4;
    [SerializeField] private AudioSource amb5;
    [SerializeField] private AudioSource amb6;
    [SerializeField] private AudioClip ambEnding1;
    [SerializeField] private AudioClip ambEnding2;
    [SerializeField] private AudioClip ambHospital;
    [SerializeField] private AudioClip ambAlley;
    [SerializeField] private AudioClip ambEnding;
    [SerializeField] private AudioClip ambTitle;

    [SerializeField, Range(0f, 1f)] private float ambPreviewVolume = 0.33f;
    [SerializeField] private float ambPreviewFadeSeconds = 0.25f;
    [SerializeField] private float ambCommitFadeSeconds = 1.25f;
    [SerializeField] private float ambStopFadeSeconds = 0.25f;

    Coroutine _ambCo;
    bool _ambPreviewActive;
    bool _ambCommitted;
    int _ambPreviewSide = -1; // -1 none, 0 left, 1 right
    const bool AMB_PREVIEW_ENABLED = false;

    // -------------------------------------------------------------------------
    // Whiteout overlay:
    // An Image that fades from white -> transparent -> white again.
    // It can also block clicks while visible (raycastTarget).
    // -------------------------------------------------------------------------
    [Header("Whiteout Loading Screen")]
    [SerializeField] private Image whiteout;
    [SerializeField] private GameObject glitchTransitionOverlay;
    [SerializeField] private float whiteoutFadeSeconds = 0.5f;
    [SerializeField] private bool whiteoutBlocksInput = true;

    Coroutine _whiteoutCo;
    bool _ending;

    [Header("Glitch Volume (PostProcessVolume or Volume)")]
    [SerializeField] private Component glitchVolume;

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

    [Header("Title Screen")]
    [SerializeField] private GameObject titleScreenParent;
    [SerializeField] private GameObject titleScreenText;
    [SerializeField] private bool isTitleScreenActive = true;
    [SerializeField] private GameObject titleScreenGlitchTransitionOverlay;
    [SerializeField] private float titlePulseSpeed = 0.3f;   // smaller = slower
    [SerializeField] private float titlePulseAmount = 0.03f; // 0.06 = +/-6% scale
    [SerializeField] private GameObject hospitalBackgroundParent;
    [SerializeField] private GameObject demonstrationBackgroundParent;
    [SerializeField] private GameObject aiProtestersBackgroundParent;
    [SerializeField] private GameObject humanProtestersBackgroundParent;
    [SerializeField] private GameObject alleyBackgroundParent;
    [SerializeField] private GameObject usBox;
    [SerializeField] private GameObject deBox;

    // -------------------------------------------------------------------------
    // Language Select Scene: Language doors sliding in.
    // -------------------------------------------------------------------------
    [Header("Language Select Scene")]
    [SerializeField]
    private GameObject languageSceneParent;
    [SerializeField]
    private GameObject leftArrow;
    [SerializeField]
    private GameObject rightArrow;
    private bool _arrowsActive = false;

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

    [Header("Checkup Scene AI")]
    [SerializeField]
    private GameObject checkupSceneAiParent;
    [SerializeField]
    private GameObject checkupAiDoctor;

    [Header("Checkup Scene Human")]
    [SerializeField]
    private GameObject checkupSceneHumanParent;
    [SerializeField]
    private GameObject checkupHumanDoctor;

    // -------------------------------------------------------------------------
    // Demonstration scene
    // -------------------------------------------------------------------------
    [Header("Demonstration scene")]
    [SerializeField] private GameObject demonstrationSceneParent;
    [SerializeField] private GameObject demonstrationSceneFullscreenObj;
    [SerializeField] private GameObject demonstrationSceneAIProtester;
    [SerializeField] private GameObject demonstrationSceneHumanProtester;

    private bool purityTestActive = false;

    [Header("AI Purity scene")]
    [SerializeField] private GameObject aiPuritySceneParent;
    [SerializeField] private GameObject aiPurityTestImage;
    [SerializeField] private GameObject aiPurityAIProtester;
    [SerializeField] private GameObject aiPurityCheckmark;
    [SerializeField] private GameObject aiPurityCross;

    [Header("Human Purity scene")]
    [SerializeField] private GameObject humanPuritySceneParent;
    [SerializeField] private GameObject humanPurityTestImage;
    [SerializeField] private GameObject humanPurityHumanProtester;
    [SerializeField] private GameObject humanPurityCheckmark;
    [SerializeField] private GameObject humanPurityCross;

    [Header("Accepted To AIs scene")]
    [SerializeField] private GameObject acceptedToAIsSceneParent;
    [SerializeField] private GameObject acceptedToAIsSceneAIProtester;

    [Header("Accepted To Humans scene")]
    [SerializeField] private GameObject acceptedToHumansSceneParent;
    [SerializeField] private GameObject acceptedToHumansSceneHumanProtester;

    [Header("Rejected From AIs scene")]
    [SerializeField] private GameObject rejectedFromAIsSceneParent;
    [SerializeField] private GameObject rejectedFromAIsSceneAIProtester;

    [Header("Rejected From Humans scene")]
    [SerializeField] private GameObject rejectedFromHumansSceneParent;
    [SerializeField] private GameObject rejectedFromHumansSceneHumanProtester;

    [Header("AI after Human Rejection scene")]
    [SerializeField] private GameObject aiAfterHumanRejectionSceneParent;
    [SerializeField] private GameObject aiAfterHumanRejectionSceneAIProtester;

    [Header("Human after AI Rejection scene")]
    [SerializeField] private GameObject humanAfterAiRejectionSceneParent;
    [SerializeField] private GameObject humanAfterAiRejectionSceneHumanProtester;

    [Header("Pondering scene")]
    [SerializeField] private GameObject ponderingSceneParent;

    [Header("Ending scene")]
    [SerializeField] private GameObject endingSceneParent;

    [Header("Results screen")]
    [SerializeField] private GameObject resultsScreenParent;
    [SerializeField] private GameObject resultTitle;
    [SerializeField] private GameObject resultRank;
    [SerializeField] private GameObject todayStats;
    [SerializeField] private GameObject totalStats;
    [SerializeField] private GameObject whiteBackground;
    [SerializeField] private GameObject resultsBackground;
    [SerializeField] private Material resultsWhitePreset;

    Coroutine _boatCo;

    [Header("Choices Made")]
    [SerializeField] private bool aiDoctorChosen = false;
    [SerializeField] private bool aiCrowdChosen = false;
    [SerializeField] private bool gotRejectedFromGroup = false;
    [SerializeField] private bool chosenToKeep = false;

    // This is a fixed rotation you want to apply to billboard objects
    private Quaternion defaultBillboardRotation = Quaternion.Euler(90f, 90f, -90f);

    public enum AmbRoute
    {
        None = 0,
        Amb1 = 1, // uses amb1 source / ambEnding1
        Amb2 = 2,  // uses amb2 source / ambEnding2 (protests)
        Hospital = 3,
        Alley = 4,
        Ending = 5,
        Title = 6
    }

    [Serializable]
    public struct SceneRef
    {
        public Func<System.Collections.IEnumerator> routine;
        public GameObject root;

        public AmbRoute amb;          // which ambience this destination wants
        public bool commitAmbOnConfirm; // should we lock ambience when confirming this choice?

        public SceneRef(Func<System.Collections.IEnumerator> routine, GameObject root,
                        AmbRoute amb = AmbRoute.None,
                        bool commitAmbOnConfirm = false)
        {
            this.routine = routine;
            this.root = root;
            this.amb = amb;
            this.commitAmbOnConfirm = commitAmbOnConfirm;
        }

        public bool IsValid => routine != null;
    }

}
