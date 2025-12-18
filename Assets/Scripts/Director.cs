using UnityEngine;

public class Director : MonoBehaviour
{
    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;

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

    private Quaternion defaultBillboardRotation = Quaternion.Euler(90f, 90f, -90f);

    void Start()
    {
        if (sceneParent) sceneParent.SetActive(true);
        foreach (Transform child in sceneParent.transform)
        {
            child.gameObject.SetActive(false);
        }

        DemoScene();
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


    public void DemoScene()
    {
        if (demoSceneParent) demoSceneParent.SetActive(true);
        StartCoroutine(Fade(doorL, 0f, 0f));
        StartCoroutine(Fade(doorR, 0f, 0f));
        doorL.transform.position = new Vector3(decisionL.transform.position.x, 0f, 5f);
        doorR.transform.position = new Vector3(decisionR.transform.position.x, 0f, 5f);
        doorL.transform.rotation = defaultBillboardRotation;
        doorR.transform.rotation = defaultBillboardRotation;
        StartCoroutine(Fade(doorL, 1f, 3f));
        StartCoroutine(MoveTo(doorL, new Vector3(decisionL.transform.position.x, 0f, 0f), 3f));
        StartCoroutine(Fade(doorR, 1f, 3f));
        StartCoroutine(MoveTo(doorR, new Vector3(decisionR.transform.position.x, 0f, 0f), 3f));


        if (decisionL) decisionL.SetActive(true);
        if (decisionR) decisionR.SetActive(true);
    }
}