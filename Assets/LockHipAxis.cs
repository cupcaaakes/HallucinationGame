using UnityEngine;

public class LockHipAxis : MonoBehaviour
{
    Quaternion startLocalRotation;

    void Start()
    {
        startLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        Vector3 e = transform.localEulerAngles;

        // X und Z gesperrt, Y erlaubt
        transform.localRotation = Quaternion.Euler(
            startLocalRotation.eulerAngles.x,
            e.y,
            startLocalRotation.eulerAngles.z
        );
    }
}
