using TMPro;
using UnityEngine;
using static TextboxScripts;

public class Director : MonoBehaviour
{
    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;

    [Header("Textboxes")]
    public Transform textbox;
    [SerializeField] private TMP_Text textboxText;

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
        if (textbox)
        {
            textbox.localScale = Vector3.zero;
            textbox.gameObject.SetActive(false);
        }


        StartCoroutine(RunGame());
    }

    System.Collections.IEnumerator RunGame()
    {
        yield return DemoScene();
        // later:
        // yield return Scene2();
        // yield return Scene3();
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
            yield return new WaitForSeconds(delay);
        }
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

}