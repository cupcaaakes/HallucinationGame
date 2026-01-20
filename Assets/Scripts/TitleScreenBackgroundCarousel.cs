using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenBackgroundCarousel : MonoBehaviour
{
    [Serializable]
    public class PlaneSettings
    {
        [Header("Plane")]
        public GameObject root;                 // your BackgroundParent GO
        public Renderer targetRenderer;         // MeshRenderer (auto-filled if empty)
        [HideInInspector] public Material runtimeMat;

        [Header("Pan bounds (WORLD offsets from original position)")]
        public Vector2 panMin = new Vector2(-0.25f, -0.15f);
        public Vector2 panMax = new Vector2(0.25f, 0.15f);

        [Header("Zoom factor (multiplies original scale X/Z)")]
        public float zoomMin = 1.02f;
        public float zoomMax = 1.08f;

        [Header("Gentle motion speed (bigger = slower)")]
        public float segmentDurationMin = 9.0f;   // try 9+
        public float segmentDurationMax = 14.0f;  // try 14+

        [Header("Optional: prevents tiny barely-visible moves")]
        public float minPanDistance = 0.08f;      // world units
        public float minZoomDelta = 0.015f;       // zoom factor difference

        // cached
        [HideInInspector] public Vector3 originalPos;
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public Color baseColor;
        [HideInInspector] public string colorProp;
        [HideInInspector] public MaterialPropertyBlock mpb;
    }

    [Header("Your 5 background planes")]
    public List<PlaneSettings> planes = new();

    [Header("Switch planes")]
    public float switchEverySeconds = 7.5f;
    public float crossFadeSeconds = 1.25f;

    int _current = -1;
    Coroutine _loopCo;
    Coroutine _animCo;

    void Awake()
    {
        foreach (var p in planes)
        {
            if (p.root == null) continue;

            p.originalPos = p.root.transform.position;
            p.originalScale = p.root.transform.localScale;

            if (!p.targetRenderer)
                p.targetRenderer = p.root.GetComponentInChildren<Renderer>(true);

            if (!p.targetRenderer)
            {
                Debug.LogWarning($"[Carousel] No Renderer found under: {p.root.name}");
                continue;
            }

            // unique material per plane (no shared-material problems)
            var src = p.targetRenderer.sharedMaterial;
            if (src)
            {
                p.runtimeMat = new Material(src);
                p.targetRenderer.material = p.runtimeMat; // assign instance
            }

            // find the correct color property for fading
            p.colorProp = "";
            if (p.runtimeMat)
            {
                if (p.runtimeMat.HasProperty("_BaseColor")) p.colorProp = "_BaseColor";   // URP
                else if (p.runtimeMat.HasProperty("_Color")) p.colorProp = "_Color";      // Standard/Legacy
                else if (p.runtimeMat.HasProperty("_TintColor")) p.colorProp = "_TintColor"; // some unlit shaders

                if (!string.IsNullOrEmpty(p.colorProp))
                    p.baseColor = p.runtimeMat.GetColor(p.colorProp);
                else
                    Debug.LogWarning($"[Carousel] No color property found for fading on: {p.root.name} (shader: {p.runtimeMat.shader.name})");
            }

            // Start hidden
            SetAlpha(p, 0f);
            p.root.SetActive(false);
        }
    }


    void OnEnable() => StartLoop();

    void OnDisable() => StopLoop();

    public void StartLoop()
    {
        StopLoop();
        _loopCo = StartCoroutine(Loop());
    }

    public void StopLoop()
    {
        if (_loopCo != null) StopCoroutine(_loopCo);
        if (_animCo != null) StopCoroutine(_animCo);
        _loopCo = null;
        _animCo = null;
    }

    IEnumerator Loop()
    {
        if (planes.Count == 0) yield break;

        int start = UnityEngine.Random.Range(0, planes.Count);
        yield return ShowPlane(start, immediate: true);

        while (true)
        {
            yield return new WaitForSecondsRealtime(switchEverySeconds);

            int next = PickNext();
            yield return ShowPlane(next, immediate: false);
        }
    }

    int PickNext()
    {
        if (planes.Count <= 1) return _current;

        int n = _current;
        while (n == _current)
            n = UnityEngine.Random.Range(0, planes.Count);

        return n;
    }

    IEnumerator ShowPlane(int index, bool immediate)
    {
        if (index < 0 || index >= planes.Count) yield break;

        PlaneSettings incoming = planes[index];
        PlaneSettings outgoing = (_current >= 0) ? planes[_current] : null;

        // stop the movement coroutine of the currently active plane (the outgoing one)
        if (_animCo != null) StopCoroutine(_animCo);
        _animCo = null;

        // make sure incoming is enabled and reset to its original base transform
        incoming.root.SetActive(true);
        incoming.root.transform.position = incoming.originalPos;
        incoming.root.transform.localScale = incoming.originalScale;

        // ensure both are active during fade (outgoing is already active)
        if (outgoing != null) outgoing.root.SetActive(true);

        // avoid transparency sorting fights
        if (incoming.targetRenderer) incoming.targetRenderer.sortingOrder = 10;
        if (outgoing != null && outgoing.targetRenderer) outgoing.targetRenderer.sortingOrder = 0;

        // set starting alpha state
        if (immediate || outgoing == null)
        {
            SetAlpha(incoming, 1f);
            if (outgoing != null)
            {
                SetAlpha(outgoing, 0f);
                outgoing.root.SetActive(false);
            }

            // start gentle motion for incoming
            _animCo = StartCoroutine(AnimatePlane(incoming));
            _current = index;
            yield break;
        }

        // NORMAL FADE:
        // incoming starts invisible, outgoing starts fully visible
        SetAlpha(incoming, 0f);
        SetAlpha(outgoing, 1f);

        // start motion immediately on the incoming (it moves while fading in)
        _animCo = StartCoroutine(AnimatePlane(incoming));

        float dur = Mathf.Max(0.01f, crossFadeSeconds);
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / dur);

            SetAlpha(incoming, a);
            SetAlpha(outgoing, 1f - a);

            yield return null;
        }

        // finalize
        SetAlpha(incoming, 1f);
        SetAlpha(outgoing, 0f);
        outgoing.root.SetActive(false);

        _current = index;
    }


    IEnumerator AnimatePlane(PlaneSettings p)
    {
        if (p.root == null) yield break;

        Transform tr = p.root.transform;

        // pick a fresh segment immediately (so we can start zoomed-in OR zoomed-out)
        Vector3 startPos, endPos;
        Vector3 startScale, endScale;
        float dur, t;

        PickNewSegment(p, out startPos, out endPos, out startScale, out endScale, out dur);

        // IMPORTANT: set initial state instantly (this is where zoom-in vs zoom-out "starts")
        tr.position = startPos;
        tr.localScale = startScale;

        t = 0f;

        while (p.root.activeInHierarchy)
        {
            float dt = Time.unscaledDeltaTime;
            t += dt;

            float u = Mathf.Clamp01(t / dur);
            // super gentle ease-in-out curve
            float eased = u * u * (3f - 2f * u);

            tr.position = Vector3.LerpUnclamped(startPos, endPos, eased);
            tr.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);

            if (u >= 1f)
            {
                // start the next slow drift from wherever we ended
                startPos = endPos;
                startScale = endScale;

                Vector3 newStartPos, newEndPos;
                Vector3 newStartScale, newEndScale;

                PickNewSegment(p, out newStartPos, out newEndPos, out newStartScale, out newEndScale, out dur);

                // force continuity: we keep our current end pose as the new start pose
                endPos = newEndPos;
                endScale = newEndScale;

                t = 0f;
            }

            yield return null;
        }
    }

    void PickNewSegment(
        PlaneSettings p,
        out Vector3 startPos,
        out Vector3 endPos,
        out Vector3 startScale,
        out Vector3 endScale,
        out float duration)
    {
        duration = UnityEngine.Random.Range(p.segmentDurationMin, p.segmentDurationMax);

        // --- PAN ---
        Vector2 aOff = RandomPanOffset(p);
        Vector2 bOff = RandomPanOffset(p);

        // ensure pan isn't microscopic
        int panGuards = 0;
        while (Vector2.Distance(aOff, bOff) < p.minPanDistance && panGuards < 10)
        {
            bOff = RandomPanOffset(p);
            panGuards++;
        }

        startPos = p.originalPos + new Vector3(aOff.x, aOff.y, 0f);
        endPos = p.originalPos + new Vector3(bOff.x, bOff.y, 0f);

        // --- ZOOM (random zoom-in OR zoom-out) ---
        bool zoomIn = UnityEngine.Random.value > 0.5f;

        float za = UnityEngine.Random.Range(p.zoomMin, p.zoomMax);
        float zb = UnityEngine.Random.Range(p.zoomMin, p.zoomMax);

        // enforce direction
        if (zoomIn && zb < za) (za, zb) = (zb, za);
        if (!zoomIn && zb > za) (za, zb) = (zb, za);

        // ensure zoom delta isn't microscopic
        int zoomGuards = 0;
        while (Mathf.Abs(zb - za) < p.minZoomDelta && zoomGuards < 10)
        {
            zb = UnityEngine.Random.Range(p.zoomMin, p.zoomMax);
            if (zoomIn && zb < za) (za, zb) = (zb, za);
            if (!zoomIn && zb > za) (za, zb) = (zb, za);
            zoomGuards++;
        }

        startScale = new Vector3(
            p.originalScale.x * za,
            p.originalScale.y,
            p.originalScale.z * za
        );

        endScale = new Vector3(
            p.originalScale.x * zb,
            p.originalScale.y,
            p.originalScale.z * zb
        );
    }

    Vector2 RandomPanOffset(PlaneSettings p)
    {
        float x = UnityEngine.Random.Range(p.panMin.x, p.panMax.x);
        float y = UnityEngine.Random.Range(p.panMin.y, p.panMax.y);
        return new Vector2(x, y);
    }

    void SetAlpha(PlaneSettings p, float a)
    {
        if (!p.targetRenderer) return;

        // If we can't fade via color, fallback to renderer on/off
        if (p.runtimeMat == null || string.IsNullOrEmpty(p.colorProp))
        {
            p.targetRenderer.enabled = a > 0.001f;
            return;
        }

        // Force full alpha range
        Color c = p.baseColor;
        c.a = a;

        p.runtimeMat.SetColor(p.colorProp, c);
    }

}
