using UnityEngine;

[System.Serializable]
public class MovementConfig {

    [Header ("Jumping and gravity")]
    public float gravity = 20f;
    public float jumpForce = 6.5f;

    [Range(50f, 100f)]
    [SerializeField] 
    float boostJump = 100f;
    
    [Header ("General physics")]
    public float friction = 6f;
    public float maxVelocity = 500f;
    public float terminalVelocity = 100f;
    [Range (30f, 75f)] public float slopeLimit = 45f;

    [Header ("Air movement")]
    public bool clampAirSpeed = true;
    public float airCap = 0.4f;
    public float airAcceleration = 12f;
    public float airFriction = 0.4f;
    public bool flyMode = false;

    [Header ("Ground movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 50f;
    public float acceleration = 10f;
    public float deceleration = 20f;
    public float maxCharge = 2f;

    [Header ("Grappling")]

    public float maxDistance = 200f;
    public float minDistance = 15f;
    public float grappleSpring = 20f;
    [ColorUsage(true, true)]
    public Color grappleColor;
    public float castRadius;

    [Header ("Aiming")]
    public float sensitivityMultiplier = 0.5f;
    public float horizontalSensitivity = 1f;
    public float verticalSensitivity = 1f;
    public float minYRotation = -90f;
    public float maxYRotation = 90f;
    
}

