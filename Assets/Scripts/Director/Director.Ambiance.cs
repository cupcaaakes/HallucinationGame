using TMPro;
using UnityEngine;
using static TextboxScripts;
using UnityEngine.UI;

// This file is part of the partial Director class.
// It contains Ambiance preview + commit + crossfade logic.
public partial class Director
{
    // -------------------------------------------------------------------------
    // Ambiance setup:
    // - Ensures amb1/amb2 AudioSources exist, loop, and are 2D
    // - Assigns ambEnding1/2 clips to them
    // - Starts volumes at 0 (silent)
    // -------------------------------------------------------------------------
    void SetupAmbianceSources()
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

    // Applies "good defaults" for Ambiance sources
    void ConfigureAmbSource(AudioSource a)
    {
        a.playOnAwake = false;
        a.loop = true;
        a.spatialBlend = 0f; // 2D
        a.pitch = 1f;
    }

    // Ensures they are actually playing (so fades work immediately)
    void EnsureAmbiancePlaying()
    {
        if (amb1 && amb1.clip && !amb1.isPlaying) amb1.Play();
        if (amb2 && amb2.clip && !amb2.isPlaying) amb2.Play();
    }

    // Starts a crossfade coroutine to set amb1/amb2 target volumes
    void FadeAmbianceTo(float v1, float v2, float seconds, bool stopWhenSilent)
    {
        if (_ambCo != null) StopCoroutine(_ambCo);
        _ambCo = StartCoroutine(FadeAmbianceRoutine(v1, v2, seconds, stopWhenSilent));
    }

    // Actually performs the crossfade (frame-by-frame volume changes)
    System.Collections.IEnumerator FadeAmbianceRoutine(float to1, float to2, float seconds, bool stopWhenSilent)
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

        // Optional: stop the AudioSource when it’s basically silent
        if (stopWhenSilent)
        {
            if (amb1 && amb1.volume <= 0.001f) amb1.Stop();
            if (amb2 && amb2.volume <= 0.001f) amb2.Stop();
        }
    }

    // -------------------------------------------------------------------------
    // StartAmbiancePreview(side):
    // - side=0 -> play amb1 at preview volume, amb2 at 0
    // - side=1 -> play amb2 at preview volume, amb1 at 0
    // -------------------------------------------------------------------------
    void StartAmbiancePreview(int side) // 0 = left, 1 = right
    {
        if (_ambCommitted) return;

        if ((uint)side >= 2u) return;
        var dest = _next[side];

        // If this choice doesn't define ambience, stop any preview.
        if (dest.amb == AmbRoute.None)
        {
            StopAmbiancePreview();
            return;
        }

        // already previewing this side
        if (_ambPreviewActive && _ambPreviewSide == side) return;

        _ambPreviewActive = true;
        _ambPreviewSide = side;

        EnsureAmbiancePlaying();

        float v1 = (side == 0) ? ambPreviewVolume : 0f;
        float v2 = (side == 1) ? ambPreviewVolume : 0f;

        FadeAmbianceTo(v1, v2, ambPreviewFadeSeconds, stopWhenSilent: false);
    }

    // Stops any preview (fades both to 0 and stops them)
    void StopAmbiancePreview()
    {
        if (_ambCommitted) return;

        _ambPreviewActive = false;
        _ambPreviewSide = -1;

        FadeAmbianceTo(0f, 0f, ambStopFadeSeconds, stopWhenSilent: true);
    }

    // -------------------------------------------------------------------------
    // CommitAmbiance(chosenSide):
    // After the player confirms the choice:
    // - chosen side fades to 1.0 volume
    // - the other fades to 0 and stops
    // -------------------------------------------------------------------------
    void CommitAmbiance(AmbRoute chosenSide)
    {
        if (chosenSide == AmbRoute.None) return;
        if (_ambCommitted) return;

        _ambCommitted = true;
        EnsureAmbiancePlaying();

        float v1 = (chosenSide == AmbRoute.Amb1) ? 1f : 0f;
        float v2 = (chosenSide == AmbRoute.Amb2) ? 1f : 0f;

        FadeAmbianceTo(v1, v2, ambCommitFadeSeconds, stopWhenSilent: true);
    }

}
