using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

public class KinectAvatarMapper : MonoBehaviour
{
    [Header("Avatar bones (Mixamo)")]
    public Transform head;

    public Transform upperArmL;
    public Transform forearmL;

    public Transform upperArmR;
    public Transform forearmR;

    public Transform sensorOrigin; // Kinect in world

    [Header("Tuning")]
    public bool mirror = false;          // set true if left/right swapped
    public float smooth = 15f;           // higher = snappier
    public Vector3 boneForward = Vector3.right; // Mixamo arms usually point +X in local space
    public Vector3 boneUp = Vector3.up;

    // Call this every frame with the skeleton.
    public void ApplyFromSkeleton(Skeleton skel)
    {
        // --- Get joint positions (in millimeters from SDK) ---
        Vector3 shL = JPos(skel, JointId.ShoulderLeft);
        Vector3 elL = JPos(skel, JointId.ElbowLeft);
        Vector3 wrL = JPos(skel, JointId.WristLeft);

        Vector3 shR = JPos(skel, JointId.ShoulderRight);
        Vector3 elR = JPos(skel, JointId.ElbowRight);
        Vector3 wrR = JPos(skel, JointId.WristRight);

        Vector3 headP = JPos(skel, JointId.Head);
        Vector3 neckP = JPos(skel, JointId.Neck);

        if (mirror)
        {
            (shL, shR) = (shR, shL);
            (elL, elR) = (elR, elL);
            (wrL, wrR) = (wrR, wrL);
        }

        Vector3 pelvis = JPos(skel, JointId.Pelvis);
        Vector3 spine = JPos(skel, JointId.SpineNavel);
        Vector3 bodyUp = (spine - pelvis).normalized;

        AimBone(upperArmL, shL, elL, bodyUp);
        AimBone(forearmL, elL, wrL, bodyUp);

        AimBone(upperArmR, shR, elR, bodyUp);
        AimBone(forearmR, elR, wrR, bodyUp);

        AimBone(head, neckP, headP, bodyUp);
    }

    Vector3 JPos(Skeleton skel, JointId id)
    {
        var p = skel.GetJoint(id).Position; // mm

        // Kinect camera space -> Unity-ish (start here)
        Vector3 v = new Vector3(p.X, -p.Y, p.Z) * 0.001f; // meters

        // Convert from sensor-local to Unity world
        if (sensorOrigin != null)
            return sensorOrigin.TransformPoint(v);

        // fallback (only correct if sensor is at world origin facing +Z)
        return v;
    }


    void AimBone(Transform bone, Vector3 from, Vector3 to, Vector3 up)
    {
        if (bone == null) return;

        Vector3 dir = (to - from);
        if (dir.sqrMagnitude < 1e-6f) return;
        dir.Normalize();

        Quaternion worldRot = Quaternion.LookRotation(dir, up);

        Quaternion axisFix = Quaternion.FromToRotation(Vector3.forward, boneForward);
        Quaternion target = worldRot * axisFix;

        bone.rotation = Quaternion.Slerp(bone.rotation, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
