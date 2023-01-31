using UnityEngine;

[System.Serializable]
public class MoveData {

    ///// Fields /////

    // Core Data
    
    public Vector3 origin;
    public Vector3 momentumVelocity;
    public Vector3 flatMomentumVelocity;
    public Vector3 influenceVelocity;
    public Vector3 influenceMouse;
    public Quaternion velocityRot;
    public Vector2 mouseDelta;
    public Vector2 mousePosition;
    public float vMeter = 0f;
    public float pMeter = 0f;

    // Input
    
    public float verticalAxis = 0f;
    public float horizontalAxis = 0f;
    public Vector3 wishMove = Vector3.zero;
    public Vector3 flatWishMove = Vector3.zero;
    public bool wishJumpDown = false;
    public bool wishJumpUp = false;
    public bool wishCtrl = false; 
    public bool wishShiftDown = false;
    public bool wishShiftUp = false;
    public bool wishCrouchDown = false;
    public bool wishCrouchUp = false;
    public bool wishQDown = false;
    public bool wishEscapeDown = false;
    public bool wishFireDown = false;
    public bool wishFireUp = false;
    public bool wishFire2Down = false;
    public bool wishFire2Up = false;
    public bool wishFire2Press = false;
    
    // Player State
    
    public bool grappling = false; // are we currently
    public bool attacking = false;
    public bool grounded = false;
    public bool detectWall = false;
    public bool bladeMode = false;

    // Grapple

    public Vector3 grapplePoint;
    public SpringJoint joint;
    public float distanceFromPoint = 0f;
    public Vector3 grappleNormal;
    public Vector3 grappleDir;
    public float lookingAtPoint;

    // Focus

    public Vector3 focusPoint;
    public Vector3 focusNormal;
    public Vector3 focusDir;
    public Vector3 velocityPoint;
    public Vector3 velocityNormal;
    public Vector3 screenFocusPoint;
    public Vector3 screenLookAtPoint;

    // Other
    public Vector3 targetNormal;
    public Vector3 targetPoint;
    public Vector3 targetDir;
    public float distanceFromTarget = 0f;
    public Vector3 wallNormal;
    public float vCharge;

    public Collider[] targets;

}
