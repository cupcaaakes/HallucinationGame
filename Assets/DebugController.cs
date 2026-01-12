using UnityEngine;

public class DebugController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftArrow)) transform.position = new Vector3(-1f, transform.position.y, transform.position.x);
            else if (Input.GetKey(KeyCode.RightArrow)) transform.position = new Vector3(1f, transform.position.y, transform.position.x);
            else transform.position = new Vector3(0f, transform.position.y, transform.position.x);
#endif
    }
}
