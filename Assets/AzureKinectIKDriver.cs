using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

[RequireComponent(typeof(Animator))]
public class AzureKinectIKDriver : MonoBehaviour
{
    public KinectManager kinect;

    [Header("Scene alignment")]
    public Transform sensorOrigin;
    public Vector3 rootOffset;

    [Header("IK Weights")]
    public float handPosWeight = 1f;
    public float elbowHintWeight = 1f;
    public float lookWeight = 0.6f;

    [Header("Fixes")]
    public bool mirror = false;
    public bool flipZ = false;
    public bool flipX = true;
    public float rootSmooth = 12f;
    public float yawSmooth = 10f;

    [Header("Root Axis Lock")]
    public bool lockX = false;
    public bool lockY = true;
    public bool lockZ = true;

    float _lockedY;
    float _lockedZ;
    bool _lockInit;


    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        _lockedY = transform.position.y;
        _lockedZ = transform.position.z;
        _lockInit = true;
    }


    void Update()
    {
        if (kinect == null) return;
        if (!kinect.TryGetLatestSkeleton(out var skel)) return;

        Vector3 pelvisLocal = JPosLocal(skel, JointId.Pelvis);
        Vector3 pelvisWorld = (sensorOrigin != null)
            ? sensorOrigin.TransformPoint(pelvisLocal)
            : pelvisLocal;
        pelvisWorld = LockRootAxes(pelvisWorld + rootOffset) - rootOffset;

        Vector3 targetPos = LockRootAxes(pelvisWorld + rootOffset);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-rootSmooth * Time.deltaTime)
        );
    }



    void OnAnimatorIK(int layerIndex)
    {
        if (kinect == null) return;
        if (!kinect.TryGetLatestSkeleton(out var skel)) return;

        // get local pelvis for offsets
        Vector3 pelvisLocal = JPosLocal(skel, JointId.Pelvis);
        Vector3 pelvisWorld = (sensorOrigin != null)
            ? sensorOrigin.TransformPoint(pelvisLocal)
            : pelvisLocal;

        // HAND TARGETS
        Vector3 handL = WorldJointPos(skel, JointId.HandLeft, pelvisLocal, pelvisWorld);
        Vector3 handR = WorldJointPos(skel, JointId.HandRight, pelvisLocal, pelvisWorld);

        if (mirror) (handL, handR) = (handR, handL);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handPosWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, handPosWeight);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, handL);
        anim.SetIKPosition(AvatarIKGoal.RightHand, handR);

        // ELBOW HINTS
        Vector3 elbowL = WorldJointPos(skel, JointId.ElbowLeft, pelvisLocal, pelvisWorld);
        Vector3 elbowR = WorldJointPos(skel, JointId.ElbowRight, pelvisLocal, pelvisWorld);

        if (mirror) (elbowL, elbowR) = (elbowR, elbowL);

        anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowHintWeight);
        anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowHintWeight);

        anim.SetIKHintPosition(AvatarIKHint.LeftElbow, elbowL);
        anim.SetIKHintPosition(AvatarIKHint.RightElbow, elbowR);

        // HEAD LOOK TARGET
        Vector3 head = WorldJointPos(skel, JointId.Head, pelvisLocal, pelvisWorld);
        Vector3 neck = WorldJointPos(skel, JointId.Neck, pelvisLocal, pelvisWorld);

        Vector3 lookDir = (head - neck).normalized;
        Vector3 lookTarget = head + lookDir * 2f;

        anim.SetLookAtWeight(lookWeight);
        anim.SetLookAtPosition(lookTarget);
    }

    Vector3 JPosLocal(Skeleton skel, JointId id)
    {
        var p = skel.GetJoint(id).Position;

        float x = flipX ? -p.X : p.X;
        float y = -p.Y;
        float z = flipZ ? -p.Z : p.Z;

        return new Vector3(x, y, z) * 0.001f;
    }

    Vector3 WorldJointPos(Skeleton skel, JointId id, Vector3 pelvisLocal, Vector3 pelvisWorld)
    {
        Vector3 jLocal = JPosLocal(skel, id);
        Vector3 relLocal = jLocal - pelvisLocal;

        Vector3 relWorld = (sensorOrigin != null)
            ? sensorOrigin.TransformDirection(relLocal)
            : relLocal;

        return pelvisWorld + relWorld;
    }

    Vector3 LockRootAxes(Vector3 p)
    {
        if (!_lockInit)
        {
            _lockedY = transform.position.y;
            _lockedZ = transform.position.z;
            _lockInit = true;
        }

        if (lockY) p.y = _lockedY;
        if (lockZ) p.z = _lockedZ;
        return p;
    }
}
