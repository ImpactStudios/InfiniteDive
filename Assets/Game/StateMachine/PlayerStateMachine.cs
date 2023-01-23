using UnityEngine;
using System.Collections.Generic;
using Fragsurf.Movement;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Fragsurf.TraceUtil;
using DynamicMeshCutter;
using UnityEngine.VFX;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

// BanannaRepublic
// user
// wTTg3nspLoYM
// qGWQtm1FcHUB

public enum ColliderType {
    Capsule,
    Box,
    Sphere
}

public class PlayerStateMachine : MonoBehaviour, ISurfControllable {

    [SerializeField] GameObject grappleGun;
    [SerializeField] GameObject smokeObj;
    [SerializeField] GameObject smokeLandObj;
    [SerializeField] GameObject sonicBoomObj;
    [SerializeField] GameObject airHikeObj;
    [SerializeField] GameObject sphereLinesObj;
    public GameObject lookAhead;
    public Volume globalVolume;
    public VisualEffect grappleArc;
    public VisualEffect slash;
    public VisualEffect smoke;
    public VisualEffect smokeLand;
    public VisualEffect sonicBoom;
    public VisualEffect airHike;
    public VisualEffect sphereLines;
    public GameObject _vcam;
    public GameObject _groupCam;
    public GameObject _targetGroup;
    public ParticleSystem speedTrails;
    public CinemachineVirtualCamera virtualCam;
    public CinemachineFramingTransposer framingCam;
    public CinemachineSameAsFollowTarget aimCam;
    public CinemachineFramingTransposer groupcam;
    public CinemachineTargetGroup targetGroup;
    public CinemachineBrain brain;
    
    [SerializeField] LayerMask _groundMask;
    [SerializeField] Slider vMeter;
    [SerializeField] Slider pMeter;

    public Grapple _grapple;
    protected Grapple grapple;

    public BezierCurve bezierCurve;

    public Camera _cam;
    GameObject _groundObject;

    ///// Fields /////

    [Header("View Settings")]
    public Transform avatarLookTransform;



    [Header ("Movement Config")]
    [SerializeField]
    public MovementConfig movementConfig;
    
    private CapsuleCollider _playerCollider;
    private Vector3 _frontSide;
    private Vector3 _leftSide;
    private Vector3 _rightSide;
    private Vector3 _backSide;

    PlayerBaseState _currentState;
    PlayerStateFactory _states;
    public PlayerControls PlayerControls;

    [Header ("Movement Data")]
    [SerializeField]
    private MoveData _moveData = new MoveData();

    ///// Properties /////

    public PlayerBaseState currentState { get {return _currentState; } set { _currentState = value; } }
    public MovementConfig moveConfig { get { return movementConfig; } }
    public MoveData moveData { get { return _moveData; } }

    public Camera cam { get { return _cam; } set { _cam = value; } }
    public Vector3 viewForward { get { return cam.transform.forward; } set { cam.transform.forward = value; } }
    public Vector3 viewRight { get { return cam.transform.right; } }
    public Vector3 viewUp { get { return cam.transform.up; } }
    public Vector3 avatarLookForward { get { return avatarLookTransform.forward; } set { avatarLookTransform.forward = value; } }
    public Vector3 avatarLookRight { get { return avatarLookTransform.right; } }
    public Vector3 avatarLookUp { get { return avatarLookTransform.up; } }
    public Vector3 bodyForward { get { return transform.forward; } }
    public Vector3 bodyRight { get { return transform.right; } }
    public Vector3 bodyUp { get { return transform.up; } }
    public Vector3 velocityForward { get { return velocityRotation * Vector3.forward; } }
    public Vector3 velocityRight { get { return velocityRotation * Vector3.right; } }
    public Vector3 velocityUp { get { return velocityRotation * Vector3.up; } }
    public Vector3 leftSide { get { return _leftSide; } set { _leftSide = value; } }
    public Vector3 rightSide { get { return _rightSide; } set { _rightSide = value; } }
    public Vector3 backSide { get { return _backSide; } set { _backSide = value; } }
    public Vector3 frontSide { get { return _frontSide; } set { _frontSide = value; } }
    public LayerMask groundMask { get { return _groundMask; } set { _groundMask = value; } }
    public Vector3 groundNormal { get { return _groundNormal; } set { _groundNormal = value; } }
    public CapsuleCollider playerCollider { get { return _playerCollider; } set { _playerCollider = value; } }
    public GameObject groundObject { get { return _groundObject; } set { _groundObject = value; } }
    public Quaternion viewRotation { get { return cam.transform.rotation; } set { cam.transform.rotation = value; } }
    public Quaternion avatarLookRotation { get { return avatarLookTransform.rotation; } set { avatarLookTransform.rotation = value; } }
    public Quaternion bodyRotation { get { return transform.rotation; } set { transform.rotation = value; } }
    public VisualEffect _grappleArc { get { return grappleArc; } }

    Vector3 prevPosition;
    Animator animator;
    Vector3 _groundNormal = Vector3.up;
    Vector3 lastContact = Vector3.zero;
    public float wallTouchTimer = 0f;
    public float jumpTimer = 0f;
    public float groundInputTimer = 0f;
    public float boostInputTimer = 0f;
    public float grappleShootTimer = 0f;
    public float grappleZipTimer = 0f;
    public float reduceGravityTimer = 0f;
    public float ignoreGravityTimer = 0f;
    public float inputBufferTimer = 0f;
    public float runTimer = 2f;
    public float lungeTimer = 0f;
    public bool doubleJump = false;
    public Quaternion velocityRotation;
	public float xMovement;
	public float yMovement;
	public LayerMask CamOcclusion;
	public Vector3 displacement;
	private Vector3 camMask = new Vector3(0, 1, -2);
    public Vector3 viewTransformLookAt;
    Vector3 tmpLookAt = Vector3.zero;
    Quaternion tmpLookAtRot;


    public Transform lookAtThis;
    public GameObject slashObj;
    public GameObject trajectory;
    public GameObject cloak;
    private Material cloakMat;
    public GameObject focusCircle;
    private Material circleMat;
    public GameObject cutLine;
    private Material lineMat;
    public GameObject aimDot;
    private Material dotMat;

    

    public int targetLength = 0;
    public Collider currentTarget = null;
    private float mouseDeceleration;
    private Vector3 centeredPoint;
    private float stability;
    

    
    

    private void Awake() {

        playerCollider = transform.GetComponent<CapsuleCollider>();
        animator = transform.GetChild(1).GetComponent<Animator>();
        slashObj = transform.GetChild(2).transform.GetChild(1).gameObject;

        cloakMat = cloak.GetComponent<Renderer>().material;
        circleMat = focusCircle.GetComponent<RawImage>().material;
        lineMat = cutLine.GetComponent<RawImage>().material;
        dotMat = aimDot.GetComponent<RawImage>().material;

        virtualCam =  _vcam.GetComponent<CinemachineVirtualCamera>();
        framingCam = _vcam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        groupcam = _groupCam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        targetGroup = _targetGroup.GetComponent<CinemachineTargetGroup>();
        brain = cam.GetComponent<CinemachineBrain>();
        aimCam = _vcam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineSameAsFollowTarget>();
        
        circleMat.SetFloat("_alpha", 0f);
        mouseDeceleration = 1f;

        moveData.origin = transform.position;
        prevPosition = transform.position;

        PlayerControls = new PlayerControls();

        Cursor.lockState = CursorLockMode.Locked;

        // lr = grappleGun.GetComponent<LineRenderer>();
        grappleArc = grappleGun.GetComponent<VisualEffect>();
        smoke = smokeObj.GetComponent<VisualEffect>();
        smokeLand = smokeLandObj.GetComponent<VisualEffect>();
        sonicBoom = sonicBoomObj.GetComponent<VisualEffect>();
        airHike = airHikeObj.GetComponent<VisualEffect>();
        sphereLines = sphereLinesObj.GetComponent<VisualEffect>();

        sonicBoomObj.SetActive(true);

        sonicBoom.Stop();
        sphereLines.Stop();

        grapple = null;
        viewTransformLookAt = cam.transform.forward;
        lookAtThis.position = Vector3.zero;
        lookAtThis.localPosition = Vector3.zero;
        lookAtThis.localScale = new Vector3(moveConfig.castRadius/2f, moveConfig.castRadius/2f, moveConfig.castRadius/2f);

        avatarLookForward = bodyForward;


    }

    private void OnEnable() {
        PlayerControls.Enable();
    }

    private void OnDisable() {
        PlayerControls.Disable();
    }

    public IEnumerator SlashVfx() {
        // slashObj.SetActive(true);
        slash.Play();
        yield return new WaitForSeconds(0.5f);
        // slashObj.SetActive(false);
        // cutTransform.gameObject.SetActive(false);
    }

    
    private void Start() {

        PlayerControls.Player.Dive.started += context => {
            moveData.wishCtrl = true;
        };

        PlayerControls.Player.Dive.canceled += context => {
            moveData.wishCtrl = false;
        };

        PlayerControls.Player.Q.started += context => {
            moveData.wishQDown = true;
        };

        PlayerControls.Player.Q.canceled += context => {
            moveData.wishQDown = false;
        };

        PlayerControls.Player.Escape.started += context => {
            moveData.wishEscapeDown = true;
        };

        PlayerControls.Player.Escape.canceled += context => {
            moveData.wishEscapeDown = false;
        };

        // PlayerControls.Player.Fire.started += context => {
        //     moveData.wishFireDown = true;
        // };

        PlayerControls.Player.Fire.canceled += context => {
            moveData.wishFireDown = false;
        };

        PlayerControls.Player.Fire2.started += context => {
            moveData.wishFire2Down = true;
        };

        PlayerControls.Player.Fire2.canceled += context => {
            moveData.wishFire2Down = false;
            moveData.wishFire2Up = true;
        };

        PlayerControls.Player.Move.started += context => {
            moveData.horizontalAxis = context.ReadValue<Vector2>().x;
            moveData.verticalAxis = context.ReadValue<Vector2>().y;
        };

        PlayerControls.Player.Move.performed += context => {
            moveData.horizontalAxis = context.ReadValue<Vector2>().x;
            moveData.verticalAxis = context.ReadValue<Vector2>().y;
        };

        PlayerControls.Player.Move.canceled += context => {
            moveData.horizontalAxis = context.ReadValue<Vector2>().x;
            moveData.verticalAxis = context.ReadValue<Vector2>().y;
        };

        PlayerControls.Player.Crouch.started += context => {
            moveData.wishCrouchDown = true;
        };

        PlayerControls.Player.Crouch.canceled += context => {
            moveData.wishCrouchDown = false;
            moveData.wishCrouchUp = true;
        };

        PlayerControls.Player.Shift.started += context => {
            moveData.wishShiftDown = true;
        };

        PlayerControls.Player.Shift.canceled += context => {
            moveData.wishShiftDown = false;
            moveData.wishShiftUp = true;
        };


        PlayerControls.Player.Jump.started += context => {
            moveData.wishJumpDown = true;
        };

        PlayerControls.Player.Jump.canceled += context => {
            moveData.wishJumpDown = false;
            moveData.wishJumpUp = true;
        };

        // 1920 x 1200

        PlayerControls.Player.Look.performed += context => {
            moveData.mouseDelta = context.ReadValue<Vector2>();
            moveData.mousePosition += moveData.mouseDelta;

            moveData.mousePosition.x = moveData.mousePosition.x % 1920f;
            moveData.mousePosition.y = moveData.mousePosition.y % 1200f;
        };

        // PlayerControls.Player.Look2.performed += context => {
        //     moveData.mousePosition = context.ReadValue<Vector2>();
        // };

        // PlayerControls.Player.Dive.performed += context => {
        //     if (context.interaction is HoldInteraction) {
        //         moveType = MoveType.Charge;
        //     }

        // };

        currentState = new PlayerStateAir(this, new PlayerStateFactory(this));
        currentState.InitializeSubStates();
        moveData.targets = new Collider[5];
        bezierCurve = new BezierCurve(this);

    }

    private void Update () {

        // vMeter.value = moveData.vMeter;
        // pMeter.value = moveData.pMeter;

        // moveData.wishGrappleDown = moveData.wishFire2;

        DecrementTimers();
        CollisionCheck();
        UpdateInputData();
        FindTargets();

        Vector3 positionalMovement = transform.position - prevPosition; // TODO: Update
        transform.position = prevPosition;
        moveData.origin += positionalMovement;

        ClampVelocity();
        ResolveCollisions();
        DoVelocityAnimations();
        CheckGrapplePress();
        CameraStuff();

        currentState.UpdateStates();

        transform.position = moveData.origin;
        prevPosition = transform.position;

        moveData.flatMomentumVelocity = Vector3.ProjectOnPlane(moveData.momentumVelocity, groundNormal);

        TransformRotation();

        leftSide = Vector3.zero;
        rightSide = Vector3.zero;
        backSide = Vector3.zero;
        frontSide = Vector3.zero;

        moveData.wishJumpUp = false;
        moveData.wishShiftUp = false;
        moveData.wishCrouchUp = false;
        moveData.wishFireUp = false;
        moveData.wishFire2Up = false;

        if (moveData.wishEscapeDown) {
            Application.Quit();
        }

    }

    private void FindTargets() {

        targetLength = Physics.OverlapSphereNonAlloc(moveData.origin + moveData.momentumVelocity, 20f, moveData.targets, 1<<15);
        lookAhead.transform.position = moveData.origin + moveData.momentumVelocity;

    }

    

    public Quaternion FlatLookRotation(Vector3 forward) {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, groundNormal).normalized, groundNormal);
    }

    public Quaternion FlatLookRotation(Vector3 forward, Vector3 normal) {
        return Quaternion.LookRotation(Vector3.ProjectOnPlane(forward, normal).normalized, normal);
    }

    private void DoVelocityAnimations() {

        // Vector3.ProjectOnPlane(moveData.momentumVelocity, groundNormal);

        var yVel = Vector3.Dot(Vector3.ProjectOnPlane(moveData.momentumVelocity, groundNormal), bodyForward);

        var xVel = Vector3.Dot(Vector3.ProjectOnPlane(moveData.momentumVelocity, groundNormal), bodyRight);

        var isDiveInput = new Vector2(moveData.horizontalAxis, moveData.verticalAxis) != Vector2.zero;

        smoke.SetVector3("position", moveData.origin - groundNormal);
        smoke.SetFloat("force", moveData.momentumVelocity.magnitude / 10f);
        smoke.SetFloat("spawnRate", 32f + moveData.momentumVelocity.magnitude);

        if (moveData.momentumVelocity.magnitude > moveConfig.walkSpeed) {

            if (moveData.grounded) {
                smoke.SetVector3("direction", -moveData.flatWishMove);
            } else {
                smoke.SetVector3("direction", -moveData.wishMove);
            }

        } else {
            smoke.SetVector3("direction", Vector3.zero);
            smoke.SetFloat("force", 0f);
        }


        // if (!moveData.wishFire2) {
            animator.SetFloat("xVel", xVel);
            animator.SetFloat("yVel", yVel);


            animator.SetBool("ChargePress", moveData.wishCrouchDown);
            animator.SetBool("onGround", moveData.grounded);
            
            if (xVel >= 2f) {
                cloakMat.SetFloat("_WaveFrequency", -.25f);
            } else {
                cloakMat.SetFloat("_WaveFrequency", .25f);
            }

            // Mathf.Lerp(-.1f, .3f, moveData.momentumVelocity.magnitude/moveConfig.runSpeed)

            cloakMat.SetVector("_WindDirection2", new Vector3(-xVel/moveConfig.runSpeed, -moveData.momentumVelocity.y/moveConfig.runSpeed + .25f, -yVel/moveConfig.runSpeed));
            cloakMat.SetFloat("_Amplitude", .15f);
            cloakMat.SetFloat("_AmplitudeFloor", Mathf.Lerp(.15f, .35f, moveData.momentumVelocity.magnitude/moveConfig.runSpeed));
        // }

    }

    private void CheckGrapplePress() {

        float distance = 150f * grappleShootTimer * 2f;

        if (moveData.wishFire2Down && !moveData.grappling) {

            grappleShootTimer = Mathf.Min((Time.deltaTime) + grappleShootTimer, .5f);
            circleMat.SetFloat("_size", .5f - grappleShootTimer + .001f);
            circleMat.SetFloat("_alpha", grappleShootTimer*2f);

            Ray ray = new Ray(avatarLookTransform.position + avatarLookForward * 20f, avatarLookForward);
            RaycastHit hit;

            if (Physics.SphereCast(ray, moveConfig.castRadius, out hit, distance, LayerMask.GetMask (new string[] { "Focus", "Ground" }))) {
                circleMat.SetColor("_color", moveConfig.grappleColor);
            } else {
                circleMat.SetColor("_color", Color.grey);
            }

        } else {

            if (grappleShootTimer > 0f) {
                ShootGrapple(distance);
            }


            grappleShootTimer = 0f;
            circleMat.SetFloat("_alpha", 0f);
        }

        // if (moveData.grappling) {
        //     if (moveData.wishCrouchDown || moveData.distanceFromPoint > 150f || moveData.distanceFromPoint < moveConfig.minDistance) {
        //         StopGrapple();
        //     }
        // }


    }

    //     private void CheckGrapplePress() {

    //     float distance = moveConfig.maxDistance * grappleShootTimer * 2f;

    //     if (moveData.wishGrapple) {

    //         grappleShootTimer = Mathf.Min((Time.deltaTime) + grappleShootTimer, .5f);
    //         circleMat.SetFloat("_size", .5f - grappleShootTimer + .001f);
    //         circleMat.SetFloat("_alpha", grappleShootTimer*2f);

    //         Ray ray = new Ray(viewTransform.position + viewForward * 10f, viewForward);
    //         RaycastHit hit;

    //         if (Physics.SphereCast(ray, 2f, out hit, distance, LayerMask.GetMask (new string[] { "Focus", "Ground" }))) {
    //             circleMat.SetColor("_color", grappleColor);
    //         } else {
    //             circleMat.SetColor("_color", Color.grey);
    //         }

    //     } else {

    //         if (grappleShootTimer > 0f) {
    //             ShootGrapple(distance);
    //         }


    //         grappleShootTimer = 0f;
    //         circleMat.SetFloat("_alpha", 0f);
    //     }

    //     if (moveData.grappling) {
    //         if (moveData.wishCrouch || moveData.distanceFromPoint > moveConfig.maxDistance || moveData.distanceFromPoint < moveConfig.minDistance) {
    //             StopGrapple();
    //         }
    //     }


    // }

    private void LateUpdate() {
        DrawRope();
    }

    private void TransformRotation() {

        Vector3 tPoint = cam.WorldToViewportPoint(avatarLookTransform.position);
        tPoint.z = 0f;
        centeredPoint = Vector3.Lerp(centeredPoint, (tPoint - new Vector3(.5f, .5f, 0f)) * 2f, Time.deltaTime * 10f);

        mouseDeceleration = (1f - Mathf.Clamp01(centeredPoint.magnitude));

        xMovement = moveData.mouseDelta.x * moveConfig.horizontalSensitivity * moveConfig.sensitivityMultiplier;
		yMovement = -moveData.mouseDelta.y * moveConfig.verticalSensitivity  * moveConfig.sensitivityMultiplier;

		float response = 100f; // TODO:

        if (moveData.momentumVelocity.magnitude > moveConfig.walkSpeed) {
            avatarLookRotation = Quaternion.Slerp(avatarLookRotation, Quaternion.LookRotation((lookAtThis.position - avatarLookTransform.position).normalized, groundNormal), Time.deltaTime * 20f);
            velocityRotation = Quaternion.LookRotation(moveData.momentumVelocity, groundNormal);
            bodyRotation = Quaternion.Slerp(bodyRotation, FlatLookRotation(viewForward), Time.deltaTime * 5f);
        } else {
            avatarLookRotation = Quaternion.Slerp(avatarLookRotation, Quaternion.LookRotation((lookAtThis.position - avatarLookTransform.position).normalized, groundNormal), Time.deltaTime * 20f);
            bodyRotation = Quaternion.Slerp(bodyRotation, FlatLookRotation(viewForward), Time.deltaTime * 5f);
            velocityRotation = bodyRotation;
        }

        lineMat.SetFloat("_lineAlpha", 0f);

        viewTransformLookAt.x = Mathf.Clamp(viewTransformLookAt.x + yMovement, moveConfig.minYRotation, moveConfig.maxYRotation) % 360f;
        viewTransformLookAt.y = viewTransformLookAt.y + xMovement % 360f;
        
        avatarLookTransform.localPosition = groundNormal;

        viewRotation = 
        Quaternion.AngleAxis(viewTransformLookAt.y, Vector3.up) *
        // Quaternion.AngleAxis(viewTransformLookAt.z, Vector3.forward) *
        Quaternion.AngleAxis(viewTransformLookAt.x, Vector3.right);

        // _targetGroup.transform.rotation = viewRotation;

        _vcam.SetActive(true);
        _groupCam.SetActive(false);

        // _vcam.SetActive(false);
        // _groupCam.SetActive(true);

        string[] mask = new string[] { "Ground" };

        // if (moveData.wishFire2Down) {
        //     mask;
        // } else {
        //     mask = new string[] { "Focus" };
        // }
        
        RaycastHit hit;
        if (Physics.SphereCast (
            ray: cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f)),
            radius: moveConfig.castRadius,
            hitInfo: out hit,
            maxDistance: moveConfig.maxDistance,
            layerMask: LayerMask.GetMask (mask),
            queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            moveData.focusPoint = hit.point;
            moveData.focusNormal = hit.normal;
            moveData.focusDir = (hit.point - moveData.origin).normalized;
        } else {
            moveData.focusPoint = Vector3.zero;
            moveData.focusNormal = Vector3.zero;
            moveData.focusDir = Vector3.zero;
        }

        if (Physics.SphereCast (
            ray: new Ray(moveData.origin, moveData.momentumVelocity.normalized),
            radius: moveConfig.castRadius,
            hitInfo: out hit,
            maxDistance: moveConfig.maxDistance,
            layerMask: LayerMask.GetMask (mask),
            queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            moveData.velocityPoint = hit.point;
            moveData.velocityNormal = hit.normal;
        } else {
            moveData.velocityPoint = Vector3.zero;
            moveData.velocityNormal = Vector3.zero;
        }
            
        lookAtThis.position = cam.transform.position + cam.transform.forward * moveConfig.maxDistance;
    }

    private void CameraStuff() { // TODO:

        framingCam.m_UnlimitedSoftZone = false;

        if (moveData.detectWall) {
            framingCam.m_ScreenX = Mathf.Lerp(framingCam.m_ScreenX, 0.5f + Vector3.Dot(moveData.wallNormal, -viewRight) / 3f, Time.deltaTime * 2f);
        } else {
            framingCam.m_ScreenX = Mathf.Lerp(framingCam.m_ScreenX, 0.5f, Time.deltaTime);
        }

        if (moveData.wishFire2Down || moveData.wishJumpDown) {
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(virtualCam.m_Lens.FieldOfView, 75f, Time.deltaTime * 4f);
        } else {
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(virtualCam.m_Lens.FieldOfView, 90f, Time.deltaTime * 2f);
        }


        if (currentState.currentSubState.name == "neutral") {

            framingCam.m_LookaheadTime = 0f;
            framingCam.m_CameraDistance = Mathf.Lerp(framingCam.m_CameraDistance, 2f, Time.deltaTime * 2f);
            framingCam.m_SoftZoneHeight = Mathf.Lerp(framingCam.m_SoftZoneHeight, .333f, Time.deltaTime * 4f);
            framingCam.m_SoftZoneWidth = Mathf.Lerp(framingCam.m_SoftZoneWidth, .333f, Time.deltaTime * 4f);
            framingCam.m_DeadZoneHeight = 0f;
            framingCam.m_DeadZoneWidth = .333f;
            framingCam.m_XDamping = Mathf.Lerp(framingCam.m_XDamping, .1f, Time.deltaTime * 4f);
            framingCam.m_YDamping = Mathf.Lerp(framingCam.m_YDamping, .1f, Time.deltaTime * 4f);
            framingCam.m_ZDamping = Mathf.Lerp(framingCam.m_ZDamping, .1f, Time.deltaTime * 4f);
            aimCam.m_Damping = Mathf.Lerp(aimCam.m_Damping, 0f, Time.deltaTime * 2f);
            framingCam.m_DeadZoneDepth = Mathf.Lerp(framingCam.m_DeadZoneDepth, 0f, Time.deltaTime * 4f);
        } else if (moveData.wishShiftDown || !moveData.grounded) {
            
            framingCam.m_LookaheadTime = Mathf.Lerp(framingCam.m_LookaheadTime, Mathf.Clamp01(moveData.momentumVelocity.magnitude / moveConfig.runSpeed) / 3f, Time.deltaTime);
            framingCam.m_CameraDistance = Mathf.Lerp(framingCam.m_CameraDistance, Mathf.Max(Vector3.Dot(moveData.momentumVelocity, viewForward) / 2f + 2f, 5f), Time.deltaTime * 2f);

             framingCam.m_XDamping = Mathf.Lerp(framingCam.m_XDamping, .5f, Time.deltaTime * 4f);
            framingCam.m_YDamping = Mathf.Lerp(framingCam.m_YDamping, .5f, Time.deltaTime * 4f);
            framingCam.m_ZDamping = Mathf.Lerp(framingCam.m_ZDamping, .5f, Time.deltaTime * 4f);
            
            framingCam.m_SoftZoneHeight = Mathf.Lerp(framingCam.m_SoftZoneHeight, .8f, Time.deltaTime * 4f);
            framingCam.m_SoftZoneWidth = Mathf.Lerp(framingCam.m_SoftZoneWidth, .8f, Time.deltaTime * 4f);
            framingCam.m_DeadZoneHeight = 0f;
            framingCam.m_DeadZoneWidth = .333f;
            

            float dampingFunction = 0.5f + Mathf.Clamp01(moveData.momentumVelocity.magnitude / moveConfig.runSpeed);

            framingCam.m_XDamping = dampingFunction * .75f;
            framingCam.m_YDamping = dampingFunction * .75f;
            // vcam.m_ZDamping = dampingFunction * .75f;

            if (moveData.grounded) {
                stability = Mathf.Lerp(stability, .5f, Time.deltaTime);
            } else {
                stability = Mathf.Lerp(stability, 1.5f, Time.deltaTime);
            }

            aimCam.m_Damping = Mathf.Lerp(aimCam.m_Damping, Mathf.Clamp01(moveData.momentumVelocity.magnitude / moveConfig.runSpeed) * stability, Time.deltaTime * 2f);

            framingCam.m_DeadZoneDepth = Mathf.Lerp(framingCam.m_DeadZoneDepth, 0f, Time.deltaTime * 4f);
        }

    }

    IEnumerable<Vector3> EvaluateSlerpPoints(Vector3 start, Vector3 end, Vector3 center,int count = 10) {
        var startRelativeCenter = start - center;
        var endRelativeCenter = end - center;

        var f = 1f / count;

        for (var i = 0f; i < 1 + f; i += f) {
            yield return Vector3.Slerp(startRelativeCenter, endRelativeCenter, i) + center;
        }
    }

    private void ConnectGrapple(Vector3 grapplePosition) {

        if (Vector3.Distance(moveData.origin, grapplePosition) < moveConfig.minDistance) {
            return;
        }

        moveData.grapplePoint = grapplePosition;
        moveData.joint = gameObject.AddComponent<SpringJoint>();
        moveData.joint.autoConfigureConnectedAnchor = false;
        moveData.joint.connectedAnchor = moveData.grapplePoint;

        moveData.distanceFromPoint = Vector3.Distance(moveData.origin, moveData.grapplePoint);
        moveData.grappling = true;

        moveData.mousePosition = Vector3.zero;
        
        // bezierCurve = new BezierCurve(this);

    }

    public void StopGrapple() {
        moveData.distanceFromPoint = 0;
        moveData.grapplePoint = Vector3.zero;
        Destroy(moveData.joint);

        // bezierCurve.Clear();
        grappleArc.enabled = false;
        
    }

    void DrawRope() {
        //If not grappling, don't draw rope

        if (!moveData.grappling) return;

        // lr.useWorldSpace = true;
        
        // lr.SetPosition(0, grappleGun.transform.position);
        // lr.SetPosition(1, moveData.grapplePoint);



        

        var _lr = grappleGun.GetComponent<LineRenderer>();

        _lr.positionCount = 2;

        _lr.useWorldSpace = true;

        _lr.SetPosition(0, grappleGun.transform.position);
        _lr.SetPosition(1, moveData.grapplePoint);

        _lr.materials[0].mainTextureOffset += new Vector2(-Time.deltaTime, 0f);

        
        grappleArc.enabled = false;
    }

    public void EraseRope() {
        grappleArc.SetVector3("Pos0", grappleGun.transform.position);
        grappleArc.SetVector3("Pos1", grappleGun.transform.position);
        grappleArc.SetVector3("Pos2", grappleGun.transform.position);
        grappleArc.SetVector3("Pos3", grappleGun.transform.position);
        grappleArc.enabled = false;

    }

    // private void ShootGrapple() {
    //     Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    //     RaycastHit hit;

    //     Vector3 targetPoint;
    //     if (Physics.Raycast(ray, out hit))
    //         targetPoint = hit.point;
    //     else
    //         targetPoint = ray.GetPoint(75);

    //     if (!grapple) {

    //         grapple = Instantiate(_grapple, viewTransform.position + viewForward*2f, Quaternion.identity);

    //     }

    //     grapple.playerCollider = innerCollider;
    //     grapple.rb.useGravity = false;
    //     Physics.IgnoreCollision(grapple.gameObject.GetComponent<SphereCollider>(), grapple.playerCollider, true);
            
    //     grapple.gameObject.SetActive(true);

    //     float launchSpeed = moveConfig.maxDistance * 4f;

    //     // Debug.Log(Vector3.Dot(moveData.velocity, viewForward) * viewForward);
        
    //     grapple.transform.forward = viewForward;
    //     grapple.GetComponent<Rigidbody>().velocity = viewForward * launchSpeed + Mathf.Max(Vector3.Dot(moveData.momentumVelocity, viewForward), 0f) * viewForward;
        // StartCoroutine(GrappleRoutine());
    // }

    private void ShootGrapple(float distance) {
        Ray ray = new Ray(avatarLookTransform.position + avatarLookForward, avatarLookForward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, moveConfig.castRadius, out hit, distance, LayerMask.GetMask (new string[] { "Focus", "Ground" })))
            ConnectGrapple(hit.point); // TODO: Startup animation
            
        moveData.grappleNormal = hit.normal;
    }

    // IEnumerator GrappleRoutine() {

    //     float count = 0f;

    //     while (grapple) {

    //         Vector3 towardsPlayer = transform.position - grapple.transform.position;

    //         count+=Time.deltaTime;

    //         // if (count == Time.deltaTime) {
    //         //     yield return null;
    //         // }

    //         // v*t = 1/2*a*t^2 = d distance of average velocity / acceleration
    //         // d = 1/2*a*t^2 
    //         // 50 = 1/2*a*t^2 
    //         // 100 = a*t^2


    //         float launchSpeed = moveConfig.maxDistance * 4f;
            
    //         grapple.GetComponent<Rigidbody>().velocity += towardsPlayer.normalized * launchSpeed * Time.deltaTime * 2f;
    //         Vector3 tmpVel = grapple.GetComponent<Rigidbody>().velocity;
    //         float speed = grapple.GetComponent<Rigidbody>().velocity.magnitude;

    //         // Debug.Log(Vector3.Dot(tmpVel, towardsPlayer));

    //         if (Vector3.Dot(tmpVel, towardsPlayer) >= 0f && count >= 0.5f) {
    //             grapple.returning = true;
    //             grapple.GetComponent<Rigidbody>().velocity = Vector3.Project(tmpVel, towardsPlayer.normalized * speed);
    //         } 
    //         // else {
    //         //     grapple.GetComponent<Rigidbody>().velocity = Vector3.Project(tmpVel, -towardsPlayer.normalized * speed);
    //         // }

    //         if (count > 1.5f) {
    //             GrappleReturn();
    //         }

    //         yield return null;

    //     }
    // }

    private void CheckGrounded() {

        RaycastHit hit = RaycastTo(-groundNormal);

        if (hit.collider == null) {

            SetGround(null);
            groundNormal = Vector3.Lerp(groundNormal, Vector3.up, Time.deltaTime / 2f);
            moveData.grounded = false;

        } else {

            groundNormal = hit.normal.normalized;
            SetGround(hit.collider.gameObject);
            moveData.grounded = true;
            lastContact = hit.point;

            if (Vector3.Distance(moveData.origin - groundNormal, lastContact) < .49f) {
                moveData.origin += groundNormal * Mathf.Min(Time.deltaTime, .01f); // soft collision resolution?

            }

        }
    }

    private void SetGround (GameObject obj) {

        if (obj != null) {

            groundObject = obj;

        } else
            groundObject = null;

    }

    public RaycastHit RaycastTo (Vector3 groundDir) {

        RaycastHit hit;
        if (Physics.Raycast (
            origin: moveData.origin,
            direction: groundDir,
            hitInfo: out hit,
            maxDistance: 1.5f,
            layerMask: LayerMask.GetMask (new string[] { "Focus", "Ground" }),
            queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {
            
        }

        return hit;
    }

    

    private void ClampVelocity() {

        float yVel = moveData.momentumVelocity.y;
        moveData.momentumVelocity.y = 0f;

        moveData.momentumVelocity = Vector3.ClampMagnitude(moveData.momentumVelocity, moveConfig.maxVelocity);
        moveData.momentumVelocity.y = Mathf.Max(yVel, -moveConfig.terminalVelocity);
        moveData.momentumVelocity.y = Mathf.Min(moveData.momentumVelocity.y, moveConfig.terminalVelocity);

    }

    private void ResolveCollisions() {

        if ((moveData.momentumVelocity.sqrMagnitude) == 0f) {

            // Do collisions while standing still
            SurfPhysics.ResolveCollisions(playerCollider, ref moveData.origin, ref moveData.momentumVelocity);

        } else {

            float maxDistPerFrame = 0.2f;
            Vector3 velocityThisFrame = moveData.momentumVelocity * Time.deltaTime;
            float velocityDistLeft = velocityThisFrame.magnitude;
            float initialVel = velocityDistLeft;
            
            while (velocityDistLeft > 0f) {

                float amountThisLoop = Mathf.Min (maxDistPerFrame, velocityDistLeft);
                velocityDistLeft -= amountThisLoop;

                // increment origin
                Vector3 velThisLoop = velocityThisFrame * (amountThisLoop / initialVel);
                
                moveData.origin += velThisLoop;

                // don't penetrate walls
                SurfPhysics.ResolveCollisions(playerCollider, ref moveData.origin, ref moveData.momentumVelocity);

            }

        }

    }

    private void CollisionCheck() {

        CheckGrounded();
        
        if (jumpTimer > 0f) return;

        RaycastHit hit;
        if (Physics.Raycast(moveData.origin, bodyForward, out hit, 1.1f, groundMask)) {
            frontSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, bodyForward + bodyRight, out hit, 1.1f, groundMask)) {
            frontSide = hit.normal;
            if (frontSide == Vector3.zero) rightSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, bodyRight, out hit, 1.1f, groundMask)) {
            rightSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, bodyRight - bodyForward, out hit, 1.1f, groundMask)) {
            rightSide = hit.normal;
            if (rightSide == Vector3.zero) backSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, -bodyForward, out hit, 1.1f, groundMask)) {
            backSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, -bodyForward - bodyRight, out hit, 1.1f, groundMask)) {
            backSide = hit.normal;
            if (backSide == Vector3.zero) leftSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, -bodyRight, out hit, 1.1f, groundMask)) {
            leftSide = hit.normal;
        }

        if (Physics.Raycast(moveData.origin, -bodyRight + bodyForward, out hit, 1.1f, groundMask)) {
            leftSide = hit.normal;
            if (leftSide == Vector3.zero) frontSide = hit.normal;
        }

        moveData.detectWall = leftSide != Vector3.zero || rightSide != Vector3.zero || backSide != Vector3.zero || frontSide != Vector3.zero;
        moveData.detectWall = moveData.detectWall && wallTouchTimer <= 0f;

        if (moveData.detectWall) {
            moveData.wallNormal = (leftSide + rightSide + backSide + frontSide).normalized;
        } else {
            moveData.wallNormal = Vector3.zero;
        }

    }

    private void DecrementTimers() {
        if (wallTouchTimer > 0f) {
            wallTouchTimer -= Time.deltaTime;
        }

        if (jumpTimer > 0f) {
            jumpTimer -= Time.deltaTime;
        }

        if (groundInputTimer > 0f) {
            groundInputTimer -= Time.deltaTime;
        }

        if (boostInputTimer > 0f) {
            boostInputTimer -= Time.deltaTime;
        }

        // if (grappleShootTimer > 0f) {
        //     grappleShootTimer -= Time.deltaTime;
        // }

        if (grappleZipTimer > 0f) {
            grappleZipTimer -= Time.deltaTime;
        }

        if (reduceGravityTimer > 0f) {
            reduceGravityTimer -= Time.deltaTime;
        }

        if (ignoreGravityTimer > 0f) {
            ignoreGravityTimer -= Time.deltaTime;
        }

        if (inputBufferTimer > 0f) {
            inputBufferTimer -= Time.deltaTime;
        }

        if (lungeTimer > 0f) {
            lungeTimer -= Time.deltaTime;
        }
        
    }

    private void UpdateInputData () {

        moveData.distanceFromPoint = Vector3.Distance(moveData.origin, moveData.grapplePoint);
        moveData.grappleDir = (moveData.grapplePoint - avatarLookTransform.position).normalized;
        moveData.lookingAtPoint = Mathf.Clamp01(Vector3.Dot(avatarLookForward, moveData.grappleDir));

        sonicBoomObj.transform.rotation = avatarLookRotation;
        
    }

    

    private void OnTriggerEnter (Collider other) {


    }

    private void OnTriggerExit (Collider other) {



    }

    private void OnCollisionStay (Collision collision) {


    }

    
  
}