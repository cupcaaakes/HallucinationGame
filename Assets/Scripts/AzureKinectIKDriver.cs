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

    [Header("Gesture detection")]
    [Tooltip("Upper arm angle from 'down' (0=arm down, 90=straight out sideways, 180=straight up).")]
    public float armRaiseAngle = 90f;

    [Tooltip("Extra degrees required to ENTER the raised state.")]
    public float angleHysteresis = 7f;

    [Tooltip("Hand must be at least this much above the shoulder.")]
    public float minHandAboveShoulder = 0.05f;

    [Tooltip("Hand must be at least this far to the side.")]
    public float minSideOffset = 0.15f;

    [Tooltip("How much the arm is allowed to point forward/back (0 = never, 1 = always). Lower = stricter anti-salute.")]
    [Range(0f, 1f)] public float maxForwardDot = 0.35f;

    [Tooltip("How much the arm must point sideways (dot with body-right axis). Higher = stricter.")]
    [Range(0f, 1f)] public float minSideDot = 0.45f;

    [Tooltip("Minimum time between logs per arm (seconds).")]
    public float gestureCooldown = 0.6f;

    private bool _rightArmRaised;
    private bool _leftArmRaised;
    private float _nextRightLogTime;
    private float _nextLeftLogTime;


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

        DetectArmRaiseGestures(skel);

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
        pelvisWorld = transform.position - rootOffset;

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

        Vector3 shoulderL = WorldJointPos(skel, JointId.ShoulderLeft, pelvisLocal, pelvisWorld);
        Vector3 shoulderR = WorldJointPos(skel, JointId.ShoulderRight, pelvisLocal, pelvisWorld);
        if (mirror) (shoulderL, shoulderR) = (shoulderR, shoulderL);

        Vector3 spine = WorldJointPos(skel, JointId.SpineChest, pelvisLocal, pelvisWorld);

        Vector3 right = (shoulderR - shoulderL).normalized;
        Vector3 up = (head - spine).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;

        // keep it consistent with your character's facing direction
        if (Vector3.Dot(forward, transform.forward) < 0f) forward = -forward;

        Vector3 lookTarget = head + forward * 2f;

        anim.SetLookAtWeight(lookWeight, 0f, 1f, 0f, 0.5f);
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

    private void DetectArmRaiseGestures(Skeleton skel)
    {
        if (!TryGetBodyAxes(skel, out Vector3 bodyRight, out Vector3 bodyUp, out Vector3 bodyForward))
            return;

        float enterAngle = armRaiseAngle + angleHysteresis;
        float exitAngle = armRaiseAngle - angleHysteresis;

        // Hysteresis trick:
        // - If currently NOT raised -> require enterAngle
        // - If currently raised     -> allow staying raised with exitAngle
        bool rightPose = IsArmRaisedToSide(
            skel,
            JointId.ShoulderRight, JointId.ElbowRight, JointId.HandRight,
            bodyRight, bodyUp, bodyForward,
            isRight: true,
            angleThreshold: _rightArmRaised ? exitAngle : enterAngle
        );

        bool leftPose = IsArmRaisedToSide(
            skel,
            JointId.ShoulderLeft, JointId.ElbowLeft, JointId.HandLeft,
            bodyRight, bodyUp, bodyForward,
            isRight: false,
            angleThreshold: _leftArmRaised ? exitAngle : enterAngle
        );

        // Log ONLY on the rising edge (false -> true)
        if (rightPose && !_rightArmRaised && Time.time >= _nextRightLogTime)
        {
            Debug.Log("GESTURE: Right arm raised to the side");
            _nextRightLogTime = Time.time + gestureCooldown;
        }

        if (leftPose && !_leftArmRaised && Time.time >= _nextLeftLogTime)
        {
            Debug.Log("GESTURE: Left arm raised to the side");
            _nextLeftLogTime = Time.time + gestureCooldown;
        }

        _rightArmRaised = rightPose;
        _leftArmRaised = leftPose;
    }

    private bool TryGetBodyAxes(Skeleton skel, out Vector3 bodyRight, out Vector3 bodyUp, out Vector3 bodyForward)
    {
        Vector3 shL = JPosLocal(skel, JointId.ShoulderLeft);
        Vector3 shR = JPosLocal(skel, JointId.ShoulderRight);
        Vector3 spine = JPosLocal(skel, JointId.SpineChest);
        Vector3 head = JPosLocal(skel, JointId.Head);

        bodyRight = (shR - shL);
        bodyUp = (head - spine);

        if (bodyRight.sqrMagnitude < 1e-6f || bodyUp.sqrMagnitude < 1e-6f)
        {
            bodyForward = Vector3.forward;
            return false;
        }

        bodyRight.Normalize();
        bodyUp.Normalize();

        bodyForward = Vector3.Cross(bodyRight, bodyUp);
        if (bodyForward.sqrMagnitude < 1e-6f)
            return false;

        bodyForward.Normalize();

        // Re-orthonormalize up to be safe
        bodyUp = Vector3.Cross(bodyForward, bodyRight).normalized;

        return true;
    }

    private bool IsArmRaisedToSide(
        Skeleton skel,
        JointId shoulderId,
        JointId elbowId,
        JointId handId,
        Vector3 bodyRight,
        Vector3 bodyUp,
        Vector3 bodyForward,
        bool isRight,
        float angleThreshold
    )
    {
        // Optional safety: ignore if tracking confidence is bad (prevents random logs when occluded)
        var shJ = skel.GetJoint(shoulderId);
        var elJ = skel.GetJoint(elbowId);
        var haJ = skel.GetJoint(handId);

        if (shJ.ConfidenceLevel <= JointConfidenceLevel.Low ||
            elJ.ConfidenceLevel <= JointConfidenceLevel.Low ||
            haJ.ConfidenceLevel <= JointConfidenceLevel.Low)
            return false;

        Vector3 shoulder = JPosLocal(skel, shoulderId);
        Vector3 elbow = JPosLocal(skel, elbowId);
        Vector3 hand = JPosLocal(skel, handId);

        Vector3 upperDir = (elbow - shoulder).normalized;

        // 0 = arm down, 90 = arm sideways, 180 = arm straight up
        float angleFromDown = Vector3.Angle(-bodyUp, upperDir);
        if (angleFromDown < angleThreshold)
            return false;

        // Must actually be above shoulder a bit
        if (hand.y < shoulder.y + minHandAboveShoulder)
            return false;

        Vector3 handRel = hand - shoulder;
        Vector3 handDir = handRel.normalized;

        // Anti-salute: forbid arms pointing forward/backward
        float forwardDot = Mathf.Abs(Vector3.Dot(handDir, bodyForward));
        if (forwardDot > maxForwardDot)
            return false;

        // Must be to the side (based on the body's own right axis)
        float sideDot = Vector3.Dot(handDir, bodyRight);
        if (isRight)
        {
            if (sideDot < minSideDot) return false;
        }
        else
        {
            if (sideDot > -minSideDot) return false;
        }

        // Must reach to the side by distance (meters)
        float sideOffset = Mathf.Abs(Vector3.Dot(handRel, bodyRight));
        if (sideOffset < minSideOffset)
            return false;

        return true;
    }
}