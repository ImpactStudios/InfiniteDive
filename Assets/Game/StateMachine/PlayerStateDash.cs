using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateDash : PlayerBaseState {

    public PlayerStateDash(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "dash";
    }

    public override void EnterState()
    {
        // ctx.sonicBoom.Play();
    }

    public override void UpdateState()
    {

        if (ctx.moveData.momentumVelocity.magnitude < ctx.moveConfig.runSpeed && !ctx.moveData.wishJumpDown) {
            Accelerate();
        }

        if (!ctx.moveData.wishJumpDown) {
            MouseSteer();
        }

        if (ctx.moveData.wishJumpUp) {
            Jump(ctx.groundNormal, ctx.avatarLookForward);
            ctx.sphereLines.Stop();
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {

    }

    public override void InitializeSubStates()
    {

    }

    public override void CheckSwitchStates()
    {
        if (ctx.moveData.wishFireDown) {
            SwitchState(_factory.Melee());
        } else if (!ctx.moveData.wishShiftDown) {
            SwitchState(_factory.Neutral());
        } else if (ctx.moveData.grappling) {
            SwitchState(_factory.Grapple());
        }
    }

    private void MouseSteer() {
        // ctx.moveData.momentumVelocity += (ctx.viewForward * 15f) * Time.deltaTime;
        // CancelVelocityAgainst(ctx.viewRight, 5f);
        DiveInfluenceVelocityMouseFlat(ref ctx.moveData.momentumVelocity);
    }

    private void Accelerate() {

        float vFactor = 1f;

        if (ctx.moveData.momentumVelocity.magnitude < ctx.moveConfig.walkSpeed) vFactor = .1f;

        ctx.moveData.momentumVelocity += ctx.moveData.flatWishMove * Time.deltaTime * ctx.moveConfig.walkSpeed / vFactor;
    }

    private void GroundBoost() {

        if (ctx.boostInputTimer > 0f) return;

        ctx.boostInputTimer = 1f;
        ctx.jumpTimer = 5f * Time.deltaTime;
        ctx.reduceGravityTimer = 1f;

        ctx.sonicBoom.Play();

        Vector3 wishDir = ctx.avatarLookForward;

        if (Vector3.Dot(wishDir, flatForward) >= .98f || Vector3.Dot(wishDir, -ctx.groundNormal) > 0f) {
            wishDir = Vector3.ProjectOnPlane(ctx.avatarLookForward, ctx.groundNormal);
        }

        ImpulseCancelVelocityAgainst(wishDir);
        float forceJump = Mathf.Max(ctx.moveData.vCharge * 15f, ctx.moveConfig.jumpForce);
        ctx.moveData.momentumVelocity = wishDir * forceJump + Vector3.Dot(ctx.moveData.momentumVelocity.normalized, wishDir) * ctx.moveData.momentumVelocity.magnitude * wishDir;
        ctx.moveData.vCharge = 0f;

    }

    
}