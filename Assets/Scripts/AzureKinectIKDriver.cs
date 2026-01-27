using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;

[RequireComponent(typeof(Animator))]
public class AzureKinectIKDriver : MonoBehaviour
{
    public KinectManager kinect;

    [Header("Scene alignment")]
    public Transform sensorOrigin;
    public Vector3 rootOffset;

    [Header("IK Weights (0..1)")]
    [Range(0f, 1f)] public float handPosWeight = 1f;
    [Range(0f, 1f)] public float elbowHintWeight = 1f;
    [Range(0f, 1f)] public float lookWeight = 0.6f;
    [Range(0f, 1f)] public float footPosWeight = 1f;
    [Range(0f, 1f)] public float kneeHintWeight = 0.25f;

    [Header("Foot offsets (optional)")]
    public Vector3 leftFootOffset = Vector3.zero;
    public Vector3 rightFootOffset = Vector3.zero;

    public bool enableLegIK = false;

    [Header("Fixes")]
    public bool mirror = false;
    public bool flipZ = false;
    public bool flipX = true;

    [Tooltip("Smoothing for character root position.")]
    public float rootSmooth = 12f;

    [Header("Leg smoothing")]
    [Tooltip("Higher = snappier, lower = smoother.")]
    public float footSmooth = 25f;
    [Tooltip("Higher = snappier, lower = smoother.")]
    public float kneeSmooth = 18f;

    Vector3 _footL, _footR, _kneeL, _kneeR;
    bool _legInit;

    [Header("Root Axis Lock")]
    public bool lockX = false;
    public bool lockY = true;
    public bool lockZ = true;

    [Header("Gesture detection")]
    [Tooltip("Upper arm angle from 'down' (0=arm down, 90=straight out sideways, 180=straight up).")]
    public float armRaiseAngle = 70f;

    [Tooltip("Extra degrees required to ENTER the raised state.")]
    public float angleHysteresis = 5f;

    [Tooltip("Minimum time between logs per arm (seconds).")]
    public float gestureCooldown = 0.3f;

    [Header("Anti-salute filter")]
    [Tooltip("Arm must point within this many degrees of the side direction (left/right). 45° = forward becomes invalid.")]
    public float sideMaxAngle = 45f;

    public bool RightArmRaised { get; private set; }
    public bool LeftArmRaised { get; private set; }
    private float _nextRightLogTime;
    private float _nextLeftLogTime;

    float _lockedX;
    float _lockedY;
    float _lockedZ;
    bool _lockInit;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();

        _lockedX = transform.position.x;
        _lockedY = transform.position.y;
        _lockedZ = transform.position.z;
        _lockInit = true;
    }

    void Update()
    {
        if (kinect == null) return;
        if (!kinect.TryGetLatestSkeleton(out var skel)) return;

        DetectArmRaiseGestures(skel);

        // Root tracking (pelvis drives character position)
        Vector3 pelvisLocal = JPosLocal(skel, JointId.Pelvis);
        Vector3 pelvisWorld = (sensorOrigin != null)
            ? sensorOrigin.TransformPoint(pelvisLocal)
            : pelvisLocal;

        Vector3 raw = pelvisWorld + rootOffset;

        raw.x = _lockedX + (raw.x - _lockedX) * 1.5f;

        Vector3 targetPos = LockRootAxes(raw);


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

        // Pelvis reference for world conversion
        Vector3 pelvisLocal = JPosLocal(skel, JointId.Pelvis);

        // Important: pelvisWorld should match how your avatar is positioned
        Vector3 pelvisWorld = transform.position - rootOffset;

        // ---------------------------
        // HAND TARGETS
        // ---------------------------
        Vector3 handL = WorldJointPos(skel, JointId.HandLeft, pelvisLocal, pelvisWorld);
        Vector3 handR = WorldJointPos(skel, JointId.HandRight, pelvisLocal, pelvisWorld);

        if (mirror) (handL, handR) = (handR, handL);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, handPosWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, handPosWeight);

        anim.SetIKPosition(AvatarIKGoal.LeftHand, handL);
        anim.SetIKPosition(AvatarIKGoal.RightHand, handR);

        // ---------------------------
        // ELBOW HINTS
        // ---------------------------
        Vector3 elbowL = WorldJointPos(skel, JointId.ElbowLeft, pelvisLocal, pelvisWorld);
        Vector3 elbowR = WorldJointPos(skel, JointId.ElbowRight, pelvisLocal, pelvisWorld);

        if (mirror) (elbowL, elbowR) = (elbowR, elbowL);

        anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowHintWeight);
        anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowHintWeight);

        anim.SetIKHintPosition(AvatarIKHint.LeftElbow, elbowL);
        anim.SetIKHintPosition(AvatarIKHint.RightElbow, elbowR);

        // ---------------------------
        // FEET + KNEES (SMOOTHED)
        // ---------------------------
        if (enableLegIK)
        {
            bool footLValid = skel.GetJoint(JointId.FootLeft).ConfidenceLevel != JointConfidenceLevel.None;
            bool footRValid = skel.GetJoint(JointId.FootRight).ConfidenceLevel != JointConfidenceLevel.None;
            bool kneeLValid = skel.GetJoint(JointId.KneeLeft).ConfidenceLevel != JointConfidenceLevel.None;
            bool kneeRValid = skel.GetJoint(JointId.KneeRight).ConfidenceLevel != JointConfidenceLevel.None;

            Vector3 footLRaw = _footL;
            Vector3 footRRaw = _footR;
            Vector3 kneeLRaw = _kneeL;
            Vector3 kneeRRaw = _kneeR;

            if (footLValid) footLRaw = WorldJointPos(skel, JointId.FootLeft, pelvisLocal, pelvisWorld) + leftFootOffset;
            if (footRValid) footRRaw = WorldJointPos(skel, JointId.FootRight, pelvisLocal, pelvisWorld) + rightFootOffset;
            if (kneeLValid) kneeLRaw = WorldJointPos(skel, JointId.KneeLeft, pelvisLocal, pelvisWorld);
            if (kneeRValid) kneeRRaw = WorldJointPos(skel, JointId.KneeRight, pelvisLocal, pelvisWorld);

            if (mirror)
            {
                (footLRaw, footRRaw) = (footRRaw, footLRaw);
                (kneeLRaw, kneeRRaw) = (kneeRRaw, kneeLRaw);
            }

            if (!_legInit)
            {
                _footL = footLRaw;
                _footR = footRRaw;
                _kneeL = kneeLRaw;
                _kneeR = kneeRRaw;
                _legInit = true;
            }
            else
            {
                _footL = SmoothVec(_footL, footLRaw, footSmooth);
                _footR = SmoothVec(_footR, footRRaw, footSmooth);

                _kneeL = SmoothVec(_kneeL, kneeLRaw, kneeSmooth);
                _kneeR = SmoothVec(_kneeR, kneeRRaw, kneeSmooth);
            }

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, footPosWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, footPosWeight);

            anim.SetIKPosition(AvatarIKGoal.LeftFoot, _footL);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, _footR);

            anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, kneeHintWeight);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, kneeHintWeight);

            anim.SetIKHintPosition(AvatarIKHint.LeftKnee, _kneeL);
            anim.SetIKHintPosition(AvatarIKHint.RightKnee, _kneeR);
        }
        else
        {
            // Ensure leg IK does nothing (so animator/base pose wins)
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0f);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0f);

            // Optional: let it re-init smoothly if you re-enable later
            _legInit = false;
        }


        // ---------------------------
        // HEAD LOOK TARGET
        // ---------------------------
        Vector3 head = WorldJointPos(skel, JointId.Head, pelvisLocal, pelvisWorld);

        Vector3 shoulderL = WorldJointPos(skel, JointId.ShoulderLeft, pelvisLocal, pelvisWorld);
        Vector3 shoulderR = WorldJointPos(skel, JointId.ShoulderRight, pelvisLocal, pelvisWorld);
        if (mirror) (shoulderL, shoulderR) = (shoulderR, shoulderL);

        Vector3 spine = WorldJointPos(skel, JointId.SpineChest, pelvisLocal, pelvisWorld);

        Vector3 right = (shoulderR - shoulderL).normalized;
        Vector3 up = (head - spine).normalized;
        Vector3 forward = Vector3.Cross(right, up).normalized;

        if (Vector3.Dot(forward, transform.forward) < 0f)
            forward = -forward;

        Vector3 lookTarget = head + forward * 2f;

        anim.SetLookAtWeight(lookWeight, 0f, 1f, 0f, 0.5f);
        anim.SetLookAtPosition(lookTarget);
    }

    // ---------------------------
    // Joint conversion helpers
    // ---------------------------
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
            _lockedX = transform.position.x;
            _lockedY = transform.position.y;
            _lockedZ = transform.position.z;
            _lockInit = true;
        }

        if (lockX) p.x = _lockedX;
        if (lockY) p.y = _lockedY;
        if (lockZ) p.z = _lockedZ;

        return p;
    }

    Vector3 SmoothVec(Vector3 current, Vector3 target, float smooth)
    {
        float t = 1f - Mathf.Exp(-smooth * Time.deltaTime);
        return Vector3.Lerp(current, target, t);
    }

    // ---------------------------
    // Arm “raised sideways” detection (unchanged)
    // ---------------------------
    private void DetectArmRaiseGestures(Skeleton skel)
    {
        if (!TryGetBodyAxes(skel, out Vector3 bodyRight, out Vector3 bodyUp, out Vector3 bodyForward))
            return;

        float enterAngle = armRaiseAngle + angleHysteresis;
        float exitAngle = armRaiseAngle - angleHysteresis;

        bool rightPose = IsArmRaisedToSide(
            skel,
            JointId.ShoulderRight, JointId.ElbowRight, JointId.HandRight,
            bodyRight, bodyUp, bodyForward,
            isRight: true,
            angleThreshold: RightArmRaised ? exitAngle : enterAngle
        );

        bool leftPose = IsArmRaisedToSide(
            skel,
            JointId.ShoulderLeft, JointId.ElbowLeft, JointId.HandLeft,
            bodyRight, bodyUp, bodyForward,
            isRight: false,
            angleThreshold: LeftArmRaised ? exitAngle : enterAngle
        );

        if (rightPose && !RightArmRaised && Time.time >= _nextRightLogTime)
        {
            Debug.Log("GESTURE: Right arm raised to the side");
            _nextRightLogTime = Time.time + gestureCooldown;
        }

        if (leftPose && !LeftArmRaised && Time.time >= _nextLeftLogTime)
        {
            Debug.Log("GESTURE: Left arm raised to the side");
            _nextLeftLogTime = Time.time + gestureCooldown;
        }

        RightArmRaised = rightPose;
        LeftArmRaised = leftPose;
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

        bodyUp = Vector3.Cross(bodyForward, bodyRight).normalized;

        if (Vector3.Dot(bodyForward, transform.forward) < 0f)
            bodyForward = -bodyForward;

        bodyUp = Vector3.Cross(bodyForward, bodyRight).normalized;

        return true;
    }

    public void RecenterX(float worldX = 0f)
    {
        // Move the avatar root
        var p = transform.position;
        p.x = worldX;
        transform.position = p;

        // Also reset the IK driver's internal X anchor
        _lockedX = worldX;
        _lockInit = true;

        // Optional: if you ever enable leg IK, prevents smoothing from old pose
        _legInit = false;
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
        var shJ = skel.GetJoint(shoulderId);
        var haJ = skel.GetJoint(handId);

        if (shJ.ConfidenceLevel == JointConfidenceLevel.None ||
            haJ.ConfidenceLevel == JointConfidenceLevel.None)
            return false;

        Vector3 shoulder = JPosLocal(skel, shoulderId);
        Vector3 hand = JPosLocal(skel, handId);

        Vector3 handRel = hand - shoulder;
        if (handRel.sqrMagnitude < 0.0001f)
            return false;

        Vector3 handDir = handRel.normalized;

        float angleFromDown = Vector3.Angle(-bodyUp, handDir);
        if (angleFromDown < angleThreshold)
            return false;

        Vector3 flat = Vector3.ProjectOnPlane(handDir, bodyUp);
        if (flat.sqrMagnitude < 0.0001f)
            return false;

        Vector3 flatDir = flat.normalized;
        Vector3 sideAxis = isRight ? bodyRight : -bodyRight;

        float sideYawAngle = Vector3.Angle(flatDir, sideAxis);
        if (sideYawAngle > sideMaxAngle)
            return false;

        return true;
    }
}
