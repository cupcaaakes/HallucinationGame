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
        if (decisionL) decisionL.SetActive(true);
        if (decisionR) decisionR.SetActive(true);
        if (sceneParent) sceneParent.SetActive(true);
        foreach (Transform child in sceneParent.transform)
        {
            child.gameObject.SetActive(false);
        }

        DemoScene();
    }

    public void DemoScene()
    {
        if (demoSceneParent) demoSceneParent.SetActive(true);
        doorL.transform.position = new Vector3(decisionL.transform.position.x, 0f, 0f);
        doorR.transform.position = new Vector3(decisionR.transform.position.x, 0f, 0f);
        doorL.transform.rotation = defaultBillboardRotation;
        doorR.transform.rotation = defaultBillboardRotation;
        Debug.Log("Wurstbrot");
    }
}