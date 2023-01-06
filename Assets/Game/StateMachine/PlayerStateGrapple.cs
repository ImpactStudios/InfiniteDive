using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateGrapple : PlayerBaseState {

    public PlayerStateGrapple(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "grapple";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER GRAPPLE");
        
        InitializeSubStates();
        ctx.inputBufferTimer = 0.5f;
        // ctx._grappleArc.enabled = true;
    }

    public override void UpdateState()
    {


        // if (ctx.moveData.wishShiftDown) {
        //     if (Vector3.Dot(ctx.moveData.momentumVelocity, ctx.moveData.grappleDir) <= 0f) {
        //         CancelVelocityAgainst(ctx.moveData.grappleDir, 20f);
        //     }
        // }

        if ((!ctx.moveData.wishJumpDown || ctx.inputBufferTimer >= 0f) && ctx.moveData.grappling && t == 0f) {
            t = 0f;
            GrappleMoveTargeting();

            if (ctx.moveData.grounded) {
                OnlyInfluence();
            } else {
                if (Vector3.Dot(ctx.moveData.momentumVelocity, ctx.moveData.grappleDir) <= 0f) {
                    CancelVelocityAgainst(ctx.moveData.grappleDir, 25f);
                }
            }

            // if (ctx.moveData.wishShiftDown) {
            //     ctx.reduceGravityTimer = .1f;
            //     ctx.moveData.momentumVelocity += (ctx.moveData.grappleDir * 7f + ctx.avatarLookForward).normalized * ctx.moveData.distanceFromPoint * Time.deltaTime;
            //     Vector3.ClampMagnitude(ctx.moveData.momentumVelocity, 40f);
            // }
            // Vector3 velocityInGrappleDir = Vector3.Dot(ctx.moveData.momentumVelocity, ctx.moveData.grappleDir) * ctx.moveData.grappleDir;
            // Vector3 velocityOrthagonal = ctx.moveData.momentumVelocity - velocityInGrappleDir;


        } 
        else if  (ctx.moveData.wishJumpDown && ctx.inputBufferTimer < 0f && (ctx.moveData.grappling) || t > 0f) {
            t += Time.deltaTime;

            ctx.ignoreGravityTimer = .25f;

            ctx.bezierCurve.InterpolateAcrossCurveC2(t);

            if (t >= ctx.bezierCurve.estimatedTime) {
                ctx.moveData.grappling = false;
            }

        }


        if (ctx.moveData.wishCrouchDown) {
            ctx.moveData.grappling = false;
            ctx.ignoreGravityTimer = .25f;
        }

        CheckSwitchStates();

    }

    public override void ExitState()
    {
        t = 0f;
        // Debug.Log("EXIT GRAPPLE");
    }

    public override void InitializeSubStates()
    {
        // if (ctx.moveData.wishFire) {
        //     SetSubState(factory.Melee());
        // } else if (ctx.moveData.momentumVelocity.magnitude <= 10f) {
        //     SetSubState(factory.Still());
        // } else if (ctx.moveData.momentumVelocity.magnitude >= 10f) {
        //     SetSubState(factory.Dive());
        // }
    }

    public override void CheckSwitchStates()
    {
        // if (ctx.moveData.wishJump) {
        //     SwitchState(factory.Pull());
        // }
        // else if (ctx.moveData.wishCrouch) {
        //     ctx.EraseRope();
        //     SwitchState(factory.Air());
        // } else if (!ctx.moveData.grappling && ctx.moveData.grounded) {
        //     ctx.EraseRope();
        //     SwitchState(_factory.Grounded());
        // } else if (!ctx.moveData.grappling) {
        //     ctx.EraseRope();
        //     SwitchState(_factory.Air());
        // }

        if (!ctx.moveData.grappling && ctx.moveData.grounded ) {
            ctx.StopGrapple();
            SwitchState(_factory.Neutral());
        } 
        // else if (!ctx.moveData.grappling && ctx.moveData.wishShiftDown && ctx.moveData.grounded) {
        //     ctx.StopGrapple();
        //     SwitchState(_factory.Dash());
        //     ctx.moveData.grappling = false;
        // } 
        // else if (!ctx.moveData.grappling && ctx.moveData.wishShiftDown && !ctx.moveData.grounded) {
        //     ctx.StopGrapple();
        //     SwitchState(_factory.Dive());
        //     ctx.moveData.grappling = false;
        // } 
        else if (!ctx.moveData.grappling && !ctx.moveData.grounded) {
            ctx.StopGrapple();
            SwitchState(_factory.Fall());
        } 
    }

    private void GrappleMoveTargeting() {

        // bezierCurve.CreateArcFromVelocity(ctx.moveData.wishShift);
        Vector3 grappleDir = (ctx.moveData.grapplePoint - ctx.moveData.origin).normalized;

        float lookCurve = Mathf.Clamp(Vector3.Dot(ctx.avatarLookForward, grappleDir), 0f, 1f);
        // Vector3 bonus = (Vector3.Dot(viewRotation * moveData.influenceVelocity, velocityRight)) * velocityRight * Time.deltaTime;

        // ctx.bezierCurve.GetSpiralCurve(0.7f, t);
        // ctx.bezierCurve.SpiralCurvePoint();
        // ctx.bezierCurve.CreateSpiralFromVelocity();
        // ctx.bezierCurve.CreateArcFromVelocity();
        ctx.bezierCurve.GrappleArc(ctx.moveData.grappleNormal, ctx.moveData.grapplePoint);

    }

}