using UnityEngine;

public class Director : MonoBehaviour
{
    [Header("IK Weights")]
    public GameObject doorL;
    public GameObject doorR;

    [Header("Decision Boxes")]
    public GameObject decisionL;
    public GameObject decisionR;

    [Header("Demo Scene Positions")]
    public Vector3 doorLPosition;
    public Vector3 doorRPosition;

    void Start()
    {

        if (doorL) doorL.SetActive(false);
        if (doorR) doorR.SetActive(false);
        if (decisionL) decisionL.SetActive(false);
        if (decisionR) decisionR.SetActive(false);
    }


    public void DemoScene()
    {

        doorL.SetActive(true);
        doorR.SetActive(true);

        doorL.transform.position = doorLPosition;
        doorR.transform.position = doorRPosition;


        decisionL.transform.position = doorL.transform.position;
        decisionR.transform.position = doorR.transform.position;
    }
}