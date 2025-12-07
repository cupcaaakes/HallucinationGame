using UnityEngine;

public class KinectAvatarMapper : MonoBehaviour
{
    [Header("Head")]
    public Transform head;

    [Header("Left Arm")]
    public Transform shoulderLeft;
    public Transform elbowLeft;
    public Transform wristLeft;

    [Header("Right Arm")]
    public Transform shoulderRight;
    public Transform elbowRight;
    public Transform wristRight;

    public void ApplyJointRotation(Transform bone, Microsoft.Azure.Kinect.BodyTracking.Joint joint)
    {
        if (bone == null) return;

        Quaternion rot = new Quaternion(
            joint.Quaternion.X,
            -joint.Quaternion.Y,
            -joint.Quaternion.Z,
            joint.Quaternion.W
        );

        bone.localRotation = rot;
    }
}
