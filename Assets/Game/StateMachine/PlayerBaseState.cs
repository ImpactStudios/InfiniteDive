using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected bool _isMovementState = false;
    protected bool _isTransitionState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateMachine ctx { get { return _ctx; } set { _ctx = value; } }
    protected PlayerStateFactory _factory;
    protected PlayerStateFactory factory { get { return _factory; } set { _factory = value; } }
    protected PlayerBaseState _currentSubState;
    public PlayerBaseState currentSubState { get { return _currentSubState; } }
    protected PlayerBaseState _currentSuperState;
    public PlayerBaseState currentSuperState { get { return _currentSuperState; } }
    public string name = "";
    public float t = 0f;
    protected Quaternion viewFlat;
    protected Quaternion viewFlatSide;
    public Vector3 oldMomentum;
    protected Vector3 flatForward = Vector3.zero;
    protected Vector3 flatForwardSide = Vector3.zero;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubStates();

    public void UpdateStates() {

        flatForward = viewFlat * Vector3.forward;

        InfluenceMove();
        InfluenceAim();
        
        UpdateState();
        if (_currentSubState != null) {
            _currentSubState.UpdateStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState) {
        ExitState();

        newState.EnterState();

        if (_isMovementState) {
            _ctx.currentState = newState;
        } else if (_currentSuperState != null) {
            _currentSuperState.SetSubState(newState);
        }

    }

    protected void SetSuperState(PlayerBaseState newSuperState) {
        _currentSuperState = newSuperState;
    }

    protected void SwitchSuperState(PlayerBaseState newSuperState) {
        _currentSuperState.ExitState();
        newSuperState.EnterState();
        _currentSuperState = newSuperState;
    }

    protected void SetSubState(PlayerBaseState newSubState) {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

    protected void EnterSubState(PlayerBaseState newSubState) {
        _currentSubState = newSubState;
        newSubState.EnterState();
        newSubState.SetSuperState(this);
    }

    protected void SwitchSubState(PlayerBaseState newSubState) {
        _currentSubState.ExitState();
        newSubState.EnterState();
        
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

    private void InfluenceMove() {
        float forwardMove = ctx.moveData.verticalAxis;
        float rightMove = ctx.moveData.horizontalAxis;

        Vector3 wishDir = (forwardMove * Vector3.forward + rightMove * Vector3.right).normalized;

        ctx.moveData.influenceVelocity = Vector3.Lerp(ctx.moveData.influenceVelocity, wishDir, Time.deltaTime * 10f);
        ctx.moveData.influenceVelocity.y = 0f;

        viewFlat = ctx.FlatLookRotation(ctx.avatarLookForward);
        viewFlatSide = ctx.FlatLookRotation(ctx.avatarLookForward, ctx.moveData.wallNormal);
        ctx.moveData.wishMove = (ctx.avatarLookRotation * ctx.moveData.influenceVelocity);
        ctx.moveData.flatWishMove = ctx.FlatLookRotation(ctx.avatarLookForward) * ctx.moveData.influenceVelocity;
    }

    public void InfluenceAim() {

        ctx.moveData.influenceMouse = Vector3.zero;
        ctx.moveData.influenceMouse.y = ctx.moveData.mousePosition.y / 100f;
        ctx.moveData.influenceMouse.x = ctx.moveData.mousePosition.x / 100f;

        ctx.moveData.influenceMouse = Vector3.ClampMagnitude(ctx.moveData.influenceMouse, 1f);

        // Debug.Log(ctx.moveData.influenceMouse);

    }

    public void InfluenceAim2(Vector3 lookAt) {

        ctx.moveData.influenceMouse = Vector3.Project(ctx.avatarLookForward, lookAt);
        ctx.moveData.influenceMouse.y = ctx.moveData.mousePosition.y / 100f;
        ctx.moveData.influenceMouse.x = ctx.moveData.mousePosition.x / 100f;

        ctx.moveData.influenceMouse = Vector3.ClampMagnitude(ctx.moveData.influenceMouse, 1f);

        // (Vector3.ProjectOnPlane(Vector3.ProjectOnPlane(initialVelocityDir * initialVelocityMag, hyp.normalized), contactNormal));

        // Debug.Log(ctx.moveData.influenceMouse);

    }

    protected void BrakeCharge() {
        float velocityFactor = Mathf.Max(ctx.moveData.momentumVelocity.magnitude / (ctx.moveConfig.runSpeed / 2f), 1f);
        ctx.moveData.vCharge = Mathf.Min(ctx.moveData.vCharge + Time.deltaTime * velocityFactor, ctx.moveConfig.maxCharge);
    }

    protected void Jump(Vector3 normal) {

        if (ctx.boostInputTimer > 0f) return;

        ctx.boostInputTimer = 1f;
        ctx.jumpTimer = .5f;
        ctx.reduceGravityTimer = Time.deltaTime * 2f;

        if (name == "neutral") {
            // Debug.Log(ctx.moveData.vCharge);
            if (ctx.moveData.vCharge < .12f) {
                ctx.moveData.momentumVelocity += ctx.groundNormal * ctx.moveConfig.jumpForce / Mathf.Sqrt(2f);
            } else {
                ctx.moveData.momentumVelocity += ctx.groundNormal * ctx.moveConfig.jumpForce;
            }
            ctx.moveData.vCharge = 0f;
        }
        else {
            ctx.sonicBoom.Play();

            if (Vector3.Dot(ctx.avatarLookForward, flatForward) >= .99f || Vector3.Dot(ctx.avatarLookForward, -normal) > 0f) {
                ctx.avatarLookForward = Vector3.ProjectOnPlane(ctx.avatarLookForward, normal);
            }

            ImpulseCancelVelocityAgainst(ctx.avatarLookForward);
            float forceJump = Mathf.Max(ctx.moveData.vCharge * 20f, ctx.moveConfig.jumpForce + 10f);
            ctx.moveData.momentumVelocity = ctx.avatarLookForward * forceJump + Vector3.Dot(ctx.moveData.momentumVelocity.normalized, ctx.avatarLookForward) * ctx.moveData.momentumVelocity.magnitude * ctx.avatarLookForward;
            ctx.moveData.vCharge = 0f;

        }

    }

    protected void BoostJump() {
        
    }

    protected void WallJump() {

        if (ctx.boostInputTimer > 0f) return;

        ctx.boostInputTimer = .5f;
        ctx.jumpTimer = .25f;

        Vector3 wishDir = (ctx.moveData.wallNormal + ctx.groundNormal).normalized;

        ImpulseCancelVelocityAgainst(wishDir);

        ctx.moveData.momentumVelocity += wishDir * ctx.moveConfig.jumpForce * 2f;
    }

    protected void OnlyInfluence() {

        Vector3 neutralMove = viewFlat * ctx.moveData.influenceVelocity * ctx.moveConfig.walkSpeed;

        if (oldMomentum.magnitude > ctx.moveConfig.walkSpeed) {
            SubtractVelocityAgainst(ref oldMomentum, -oldMomentum.normalized, Mathf.Max(oldMomentum.magnitude / 2f, 5f));
            DiveInfluenceVelocity(ref oldMomentum);
            ctx.moveData.momentumVelocity = oldMomentum;
        } else {
            oldMomentum = Vector3.Lerp(oldMomentum, Vector3.zero, Time.deltaTime * 2f);
            ctx.moveData.momentumVelocity = Vector3.Lerp(ctx.moveData.momentumVelocity, Vector3.ClampMagnitude(neutralMove + oldMomentum, ctx.moveConfig.walkSpeed), Time.deltaTime * 8f);
        }
        
    }

    protected void OnlyInfluenceAir() {

        Vector3 neutralMove = viewFlat * ctx.moveData.influenceVelocity * ctx.moveConfig.walkSpeed;

        if (Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f)).magnitude > ctx.moveConfig.walkSpeed / 2f) {
            oldMomentum = Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f));
            DiveInfluenceVelocityAir();
        } else {
            oldMomentum = Vector3.Lerp(oldMomentum, Vector3.zero, Time.deltaTime * 2f);

            var yVel = ctx.moveData.momentumVelocity.y;
            ctx.moveData.momentumVelocity.y = 0f;
            ctx.moveData.momentumVelocity = Vector3.Lerp(ctx.moveData.momentumVelocity, Vector3.ClampMagnitude(neutralMove + oldMomentum, ctx.moveConfig.walkSpeed), Time.deltaTime * 8f);
            ctx.moveData.momentumVelocity.y = yVel;
        }
        
    }

    protected void DiveInfluenceVelocityAir() {

        Vector3 influence = ctx.moveData.wishMove * ctx.moveData.momentumVelocity.magnitude * Mathf.Pow(3f, .5f) / 5f;
        
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        ctx.moveData.momentumVelocity += angularAcceleration * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocity(ref Vector3 influencedV) {

        Vector3 influence = ctx.moveData.flatWishMove * ctx.moveData.momentumVelocity.magnitude * Mathf.Pow(3f, .5f);
        
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        influencedV += angularAcceleration * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocityMouseFly(ref Vector3 influencedV) {
        
        Vector3 influence = ctx.moveData.wishMove * ctx.moveData.momentumVelocity.magnitude * Mathf.Pow(3f, .5f);

        float influenceOrthagonalToVelocityRight = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAccelerationY = influenceOrthagonalToVelocityRight * ctx.velocityRight;

        float influenceOrthagonalToVelocityUp = Vector3.Dot(influence, ctx.velocityUp);
        Vector3 angularAccelerationX = influenceOrthagonalToVelocityUp * ctx.velocityUp;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;


        float lookingRightY = Vector3.Dot(influence.normalized, ctx.velocityRight);
        float lookingBackY = Vector3.Dot(influence.normalized, -ctx.velocityForward);

        influencedV += angularAccelerationY * (Time.deltaTime) + angularAccelerationX * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocityMouseFlat(ref Vector3 influencedV) {

        Vector3 influence = ctx.moveData.flatWishMove * ctx.moveData.momentumVelocity.magnitude * Mathf.Pow(3f, .5f);
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 8f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        influencedV += angularAcceleration * (Time.deltaTime) + deceleration * (Time.deltaTime);
        
        // Vector3 keyInfluence = ctx.velocityRotation * ctx.moveData.influenceVelocity;
        // keyInfluence = Vector3.Dot(keyInfluence, ctx.velocityRight) * ctx.velocityRight;
        
        // influencedV += keyInfluence * Time.deltaTime * ctx.moveData.momentumVelocity.magnitude / 4f;
        
    }

    public void ImpulseCancelVelocityAgainst(Vector3 wishDir) {

        ctx.moveData.momentumVelocity += Vector3.Dot(ctx.moveData.momentumVelocity, -wishDir.normalized) * wishDir;

    }

    public Vector3 ImpulseCancelVelocityAgainst(Vector3 wishDir, Vector3 influencedV) {

        return influencedV + Vector3.Dot(influencedV, -wishDir.normalized) * wishDir;

    }

    public void OnlyAngularVelocity(Vector3 wishDir, float response) {

        Vector3 velocityOrthagonal = ctx.moveData.momentumVelocity + Vector3.Dot(ctx.moveData.momentumVelocity, -wishDir) * wishDir;

        ctx.moveData.momentumVelocity = Vector3.Lerp(ctx.moveData.momentumVelocity, velocityOrthagonal, Time.deltaTime * response);

    }

    public void CancelVelocityAgainst(Vector3 wishDir, float response) {

        ctx.moveData.momentumVelocity += Vector3.Dot(ctx.moveData.momentumVelocity, -wishDir) * wishDir * Time.deltaTime * response;
        
    }

    protected void SubtractVelocityAgainst(Vector3 wishDir, float amount) {

        if (Vector3.Dot(ctx.moveData.momentumVelocity.normalized, -wishDir.normalized) > 0f) {
            ctx.moveData.momentumVelocity += Vector3.Dot(ctx.moveData.momentumVelocity.normalized, -wishDir.normalized) * wishDir.normalized * amount * Time.deltaTime; 
        }

    }

    protected void SubtractVelocityAgainst(ref Vector3 originalV, Vector3 wishDir, float amount) {
        originalV += Vector3.Dot(ctx.moveData.momentumVelocity.normalized, -wishDir.normalized) * wishDir.normalized * amount * Time.deltaTime; 
    }

    protected void AddVelocityTo(Vector3 wishDir, float amount) {
        ctx.moveData.momentumVelocity += wishDir.normalized * amount * Time.deltaTime; 
    }

}
