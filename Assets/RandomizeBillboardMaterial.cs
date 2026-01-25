using UnityEngine;

[DisallowMultipleComponent]
public class RandomizeBillboardMaterial : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Material Pool (size = 8)")]
    [SerializeField] private Material[] options = new Material[8];

    [Header("Auto Fit")]
    [SerializeField] private bool fitToTextureAspectOnStart = true;

    public int LastPickedIndex { get; private set; } = -1;
    public Material LastPickedMaterial { get; private set; }

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    private void Awake()
    {
        ApplyRandom();
        if (fitToTextureAspectOnStart) FitToCurrentTextureAspect();
    }

    [ContextMenu("Apply Random Material")]
    public int ApplyRandom()
    {
        if (!targetRenderer) return -1;

        int count = options != null ? options.Length : 0;
        if (count == 0) return -1;

        int safety = 64;
        int idx = Random.Range(0, count);
        while (options[idx] == null && safety-- > 0)
            idx = Random.Range(0, count);

        if (options[idx] == null) return -1;

        LastPickedIndex = idx;
        LastPickedMaterial = options[idx];

        targetRenderer.material = LastPickedMaterial;

        // quick “tell me what happened”
        Debug.Log($"[{name}] Picked material #{idx}: {LastPickedMaterial.name}");

        return idx;
    }

    [ContextMenu("Fit To Current Texture Aspect")]
    public void FitToCurrentTextureAspect()
    {
        if (!targetRenderer) return;

        var mat = targetRenderer.material;
        if (!mat) return;

        Texture tex = mat.mainTexture;
        if (!tex) tex = mat.GetTexture("_BaseMap");
        if (!tex) tex = mat.GetTexture("_MainTex");
        if (!tex || tex.height == 0) return;

        float aspect = (float)tex.width / tex.height;

        var s = transform.localScale;
        transform.localScale = s;
    }
}
