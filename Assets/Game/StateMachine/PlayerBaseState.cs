using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
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
    protected Vector3 releaseVelocity = Vector3.zero;
    protected Vector3[] releasedPoints = new Vector3[4];
    
    protected Quaternion avatarLookFlat;
    protected Quaternion avatarLookFlatSide;
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

        flatForward = avatarLookFlat * Vector3.forward;
        flatForwardSide = avatarLookFlatSide * Vector3.forward;

        if (_isRootState) {

            if (!ctx.moveData.wishShiftDown) {
                ctx.moveConfig.grappleColor = Color.Lerp(ctx.moveConfig.grappleColor, ctx.moveConfig.normalColor, Time.deltaTime);
            }

            if (ctx.moveData.grappling) {
                ctx.bezierCurve.DrawCurve();
            }

            

            if (ctx.releaseTimer > 0f && !ctx.moveData.grappling) {

                ctx.moveConfig.grappleColor = Color.Lerp(ctx.moveConfig.grappleColor, Color.clear, Time.deltaTime);

                Vector3 releaseDir = releaseVelocity.normalized;

                releasedPoints[0] += releaseVelocity * Time.deltaTime;
                releasedPoints[1] += releaseVelocity * Time.deltaTime;
                releasedPoints[2] += releaseVelocity * Time.deltaTime;
                releasedPoints[3] += releaseVelocity * Time.deltaTime;

                ctx._grappleArc.SetVector3("Pos0", releasedPoints[0]);
                ctx._grappleArc.SetVector3("Pos1", releasedPoints[1]);
                ctx._grappleArc.SetVector3("Pos2", releasedPoints[2]);
                ctx._grappleArc.SetVector3("Pos3", releasedPoints[3]);
                ctx._grappleArc.SetVector4("Color", ctx.moveConfig.grappleColor);

                ctx.bezierCurve.DrawCurve();

            }

            if (ctx.moveData.wishJumpUp && !ctx.moveData.grounded && !ctx.moveData.grappling && !ctx.moveData.detectWall) {
                BoostJump(ctx.avatarLookForward, Mathf.Max(ctx.moveData.velocity.magnitude, 30f));
            } 

        }

        InfluenceMove();

        UpdateState();
        if (_currentSubState != null) {
            _currentSubState.UpdateStates();
        }
    }

    protected void SwitchState(PlayerBaseState newState) {
        ExitState();

        newState.EnterState();

        if (_isRootState) {
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

    private void InfluenceMove() {
        float forwardMove = ctx.moveData.verticalAxis;
        float rightMove = ctx.moveData.horizontalAxis;

        Vector3 wishDir = (forwardMove * Vector3.forward + rightMove * Vector3.right).normalized;

        ctx.moveData.inputDir = Vector3.Lerp(ctx.moveData.inputDir, wishDir, Time.deltaTime * 10f);
        ctx.moveData.inputDir.y = 0f;

        avatarLookFlat = ctx.FlatLookRotation(ctx.avatarLookForward);
        avatarLookFlatSide = ctx.FlatLookRotation(ctx.avatarLookForward, ctx.moveData.wallNormal);
        ctx.moveData.wishMove = ctx.avatarLookRotation * ctx.moveData.inputDir;
        ctx.moveData.flatWishMove = ctx.FlatLookRotation(ctx.avatarLookForward) * ctx.moveData.inputDir;
    }

    protected void BrakeCharge(Vector3 wishDir) {
        float velocityFactor = Mathf.Max(ctx.moveData.velocity.magnitude / (ctx.moveConfig.runSpeed / 2f), 1f);
        // ctx.moveData.vCharge = Mathf.Min(ctx.moveData.vCharge + Time.deltaTime * velocityFactor, ctx.moveConfig.maxCharge);
        ctx.moveData.vCharge = Mathf.Min(ctx.moveData.vCharge + Time.deltaTime, ctx.moveConfig.maxCharge);

        Vector3 launchVel = ImpulseCancelVelocityAgainst(wishDir, ctx.moveData.velocity);
        float forceJump = Mathf.Max(ctx.moveData.vCharge * 20f, ctx.moveConfig.jumpForce + 10f);
        launchVel = wishDir * forceJump + Vector3.Dot(launchVel.normalized, wishDir) * launchVel.magnitude * wishDir;
        // ctx.bezierCurve.PredictGravityArc(ctx.moveData.origin, ctx.moveConfig.gravity, launchVel);
        // ctx.bezierCurve.DrawProjection();
    }

    protected void Jump(Vector3 normal, Vector3 wishDir) {

        ctx.boostInputTimer = .2f;
        ctx.jumpTimer = Time.deltaTime * 2f;
        ctx.ignoreGravityTimer = Time.deltaTime * 2f;


        float forceJump = ctx.moveConfig.jumpForce * (ctx.moveData.vCharge + 1f);
        ctx.moveData.velocity += (ctx.groundNormal + wishDir).normalized * forceJump;

        ctx.moveData.vCharge = 0f;
    }

    protected void BoostJump(Vector3 wishDir, float magnitude = 0f) {

        if (ctx.boostInputTimer > 0f) return;

        ctx.boostInputTimer = .2f;
        ctx.jumpTimer = Time.deltaTime * 2f;
        ctx.ignoreGravityTimer = Time.deltaTime * 2f;

        ctx.sonicBoom.Play();

        float forceJump = ctx.moveConfig.jumpForce * (ctx.moveData.vCharge + 2f);
        if (magnitude > 0f) {
            ctx.moveData.velocity = wishDir * magnitude + Vector3.Dot(ctx.moveData.velocity.normalized, wishDir) * ctx.moveData.velocity.magnitude * wishDir;
        } else {
            ImpulseCancelVelocityAgainst(wishDir);
            ctx.moveData.velocity = wishDir * forceJump + Vector3.Dot(ctx.moveData.velocity.normalized, wishDir) * ctx.moveData.velocity.magnitude * wishDir;
        }

        ctx.moveData.vCharge = 0f;
    }

    protected void Dash(Vector3 wishDir, float magnitude) {

        ctx.moveData.velocity = wishDir * magnitude;

    }

    protected void OnlyInfluence() {

        if (ctx.jumpTimer > 0f) return;

        Vector3 neutralMove = avatarLookFlat * ctx.moveData.inputDir * ctx.moveConfig.walkSpeed;

        bool sliding = ctx.moveData.grounded ? ctx.moveData.velocity.magnitude > ctx.moveConfig.walkSpeed : Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 0f, 1f)).magnitude > ctx.moveConfig.walkSpeed;

        if (sliding) {
            oldMomentum = ctx.moveData.grounded ? ctx.moveData.velocity : Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 0f, 1f));
            if (ctx.moveData.grounded) SubtractVelocityAgainst(ref oldMomentum, -oldMomentum.normalized, Mathf.Max(oldMomentum.magnitude / 2f, 5f));
            DiveInfluenceVelocity(ref oldMomentum);
            ctx.moveData.velocity = oldMomentum;
        } else {
            oldMomentum = Vector3.Lerp(oldMomentum, Vector3.zero, Time.deltaTime * 2f);
            
            var yVel = ctx.moveData.velocity.y;
            ctx.moveData.velocity.y = 0f;
            ctx.moveData.velocity = Vector3.Lerp(ctx.moveData.velocity, Vector3.ClampMagnitude(neutralMove + oldMomentum, ctx.moveConfig.walkSpeed), Time.deltaTime * 8f);
            ctx.moveData.velocity.y = yVel;
            
        }
        
    }

    private void OnlyInfluenceAir() {

        Vector3 neutralMove = avatarLookFlat * ctx.moveData.inputDir * ctx.moveConfig.walkSpeed / 2f;

        if (Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 0f, 1f)).magnitude > ctx.moveConfig.walkSpeed) {
            oldMomentum = Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 0f, 1f));
            DiveInfluenceVelocityAir();
        } else {
            oldMomentum = Vector3.Lerp(oldMomentum, Vector3.zero, Time.deltaTime * 2f);

            DiveInfluenceVelocityAir();

            var yVel = ctx.moveData.velocity.y;
            ctx.moveData.velocity.y = 0f;
            ctx.moveData.velocity = Vector3.Lerp(ctx.moveData.velocity, Vector3.ClampMagnitude(neutralMove + oldMomentum, ctx.moveConfig.walkSpeed), Time.deltaTime);
            ctx.moveData.velocity.y = yVel;
        }
        
    }

    protected void DiveInfluenceVelocityAir() {

        Vector3 influence = ctx.moveData.wishMove * ctx.moveData.velocity.magnitude * Mathf.Pow(3f, .5f) / 1.5f;
        
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        ctx.moveData.velocity += angularAcceleration * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocity(ref Vector3 influencedV) {

        Vector3 influence = ctx.moveData.flatWishMove * ctx.moveData.velocity.magnitude * Mathf.Pow(3f, .5f);
        
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        influencedV += angularAcceleration * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocity() {

        Vector3 influence = ctx.moveData.flatWishMove * ctx.moveData.velocity.magnitude * Mathf.Pow(3f, .5f);
        
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 4f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        ctx.moveData.velocity += angularAcceleration * (Time.deltaTime);
        
    }

    protected void DiveInfluenceVelocityMouseFly(ref Vector3 influencedV) {
        
        Vector3 influence = ctx.moveData.wishMove * ctx.moveData.velocity.magnitude * Mathf.Pow(3f, .5f);

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

        Vector3 influence = ctx.moveData.flatWishMove * ctx.moveData.velocity.magnitude * Mathf.Pow(3f, .5f);
        float influenceOrthagonalToVelocity = Vector3.Dot(influence, ctx.velocityRight);
        Vector3 angularAcceleration = influenceOrthagonalToVelocity * ctx.velocityRight;

        float influenceOppositeToVelocity = Mathf.Clamp(Vector3.Dot(influence, -ctx.velocityForward), 0f, influence.magnitude / 8f);
        Vector3 deceleration = influenceOppositeToVelocity * -ctx.velocityForward;

        influencedV += angularAcceleration * (Time.deltaTime) + deceleration * (Time.deltaTime);
        
        // Vector3 keyInfluence = ctx.velocityRotation * ctx.moveData.influenceVelocity;
        // keyInfluence = Vector3.Dot(keyInfluence, ctx.velocityRight) * ctx.velocityRight;
        
        // influencedV += keyInfluence * Time.deltaTime * ctx.moveData.velocity.magnitude / 4f;
        
    }

    public void ImpulseCancelVelocityAgainst(Vector3 wishDir, float k = 1f) {

        if (Vector3.Dot(ctx.moveData.velocity, -wishDir.normalized) > 0f) {
            ctx.moveData.velocity += Vector3.Dot(ctx.moveData.velocity, -wishDir.normalized) * wishDir * k;
        }


    }

    public Vector3 ImpulseCancelVelocityAgainst(Vector3 wishDir, Vector3 influencedV) {

        if (Vector3.Dot(ctx.moveData.velocity, -wishDir.normalized) > 0f) {

            return influencedV + Vector3.Dot(influencedV, -wishDir.normalized) * wishDir;

        }

        return influencedV;

    }

    public void OnlyAngularVelocity(Vector3 wishDir, float response) {

        Vector3 velocityOrthagonal = ctx.moveData.velocity + Vector3.Dot(ctx.moveData.velocity, -wishDir) * wishDir;

        ctx.moveData.velocity = Vector3.Lerp(ctx.moveData.velocity, velocityOrthagonal, Time.deltaTime * response);

    }

    public void CancelVelocityAgainst(Vector3 wishDir, float response) {

        if (Vector3.Dot(ctx.moveData.velocity, -wishDir.normalized) > 0f) {

            ctx.moveData.velocity += Vector3.Dot(ctx.moveData.velocity, -wishDir) * wishDir * Time.deltaTime * response;

        }
        
    }

    // public void OnlyAngularVelocity(float response, float pull) {

    //     float speed = ctx.moveData.velocity.magnitude;
    //     Vector3 result = Vector3.zero;
    //     Vector3 velocityRadial = Vector3.Dot(ctx.moveData.velocity, ctx.moveData.targetDir) * ctx.moveData.targetDir;
        
    //     Vector3 targetUp = Vector3.Cross(ctx.moveData.targetDir, ctx.velocityForward);

    //     // if (Vector3.Dot(ctx.moveData.velocity, ctx.moveData.targetDir) < 0f) {
    //     //     velocityRadial = Vector3.Dot(ctx.moveData.velocity, ctx.moveData.targetDir) * ctx.moveData.targetDir;
    //     // }

    //     Vector3 velocityOrthagonal = ctx.moveData.velocity - velocityRadial;

    //     Vector3 velocityOrthagonalUp = Vector3.Project(velocityOrthagonal, Vector3.up);
    //     Vector3 velocityOrthagonalRight = velocityOrthagonal - velocityOrthagonalUp;

    //     Vector3 velocityOrthagonalUpDampen = Vector3.Dot(velocityOrthagonal, -Vector3.up) * Vector3.up;

    //     result = (velocityOrthagonalRight + velocityOrthagonalUp + velocityOrthagonalUpDampen * Time.deltaTime + pull * ctx.moveData.targetDir * Time.deltaTime).normalized * speed;
        
    //     // ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, result, Time.deltaTime * response);
    //     ctx.moveData.velocity = velocityOrthagonal;

    // }

    // public void ShiftAngularVelocity(Vector3 wishDir, float response) {

    //     float speed = ctx.moveData.velocity.magnitude;
    //     Vector3 velocityRadial = Vector3.Dot(ctx.moveData.velocity, wishDir) * wishDir;

    //     // if (Vector3.Dot(ctx.moveData.velocity, ctx.moveData.targetDir) < 0f) {
    //     //     velocityRadial = Vector3.Dot(ctx.moveData.velocity, wishDir) * wishDir;
    //     // }

    //     Vector3 velocityOrthagonal = (ctx.moveData.velocity - velocityRadial).normalized * speed;

    //     ctx.moveData.velocity = Vector3.Lerp(ctx.moveData.velocity, velocityOrthagonal, Time.deltaTime * response);
    //     // ctx.moveData.velocity = velocityOrthagonal;

    // }


    protected void SubtractVelocityAgainst(Vector3 wishDir, float amount) {

        ctx.moveData.velocity += Vector3.Dot(ctx.moveData.velocity.normalized, -wishDir.normalized) * wishDir.normalized * amount * Time.deltaTime; 

    }

    protected void SubtractVelocityAgainst(ref Vector3 originalV, Vector3 wishDir, float amount) {
        originalV += Vector3.Dot(ctx.moveData.velocity.normalized, -wishDir.normalized) * wishDir.normalized * amount * Time.deltaTime; 
    }

    protected void AddVelocityTo(Vector3 wishDir, float amount) {
        ctx.moveData.velocity += wishDir.normalized * amount * Time.deltaTime; 
    }

}
