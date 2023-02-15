using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateGrapple : PlayerBaseState {

    Vector3 grapplePointDir = Vector3.zero;
    float initialDistance = 0f;
    Vector3 pullTarget = Vector3.zero;
    Vector3 pullDir = Vector3.zero;

    public PlayerStateGrapple(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isRootState = true;
        name = "grapple";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER GRAPPLE");
        InitializeSubStates();
        initialDistance = ctx.moveData.distanceFromGrapple;
        t = 0f;
    }

    public override void UpdateState()
    {
        GrappleMove();

        CheckSwitchStates();
    }

    // public override void UpdateState()
    // {
    //     ctx.reduceGravityTimer = 1f;


    //     // ctx.focusOnThis.position = ctx.moveData.grapplePoint;


    //         // if (Vector3.Dot(ctx.avatarLookForward, ctx.moveData.grappleDir) > 0f) {
    //         //     ctx.moveData.velocity += (ctx.avatarLookForward).normalized * (Vector3.Dot(ctx.avatarLookForward, ctx.moveData.grappleDir)) * ctx.moveData.distanceFromGrapple * Time.deltaTime;
    //         // }

    //     if (currentSuperState.name == "grounded") {
    //         OnlyInfluence();

    //         if (ctx.moveData.wishJumpUp) {
    //             Jump(ctx.groundNormal, ctx.avatarLookForward);
    //         }
    //     }


    //     ctx.bezierCurve.DrawCurve();

    //     CancelVelocityAgainst(ctx.moveData.grappleDir, 15f);


    //     if (!ctx.moveData.wishFire2Down) {

    //         if (t <= 0f) {
                
    //             initialDistance = ctx.moveData.distanceFromGrapple;
    //             ctx.moveData.velocity = Vector3.zero;
    //             float mag = 5f;
    //             pullDir = (ctx.avatarLookForward - ctx.moveData.grappleDir).normalized;
    //             pullTarget = ctx.moveData.grapplePoint + pullDir;

    //             float oah = 1f / (Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(ctx.moveData.grappleDir, ctx.avatarLookForward)));

    //             if (_currentSuperState.name == "grounded") {
    //                 ctx.moveData.velocity += Vector3.ProjectOnPlane(pullDir, ctx.groundNormal).normalized * mag;
    //             } else if (_currentSuperState.name == "air") {
    //                 ctx.moveData.velocity += Vector3.ProjectOnPlane(pullDir, -ctx.avatarLookForward).normalized * mag;
    //             }

    //             GameObject n = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //             n.transform.position = ctx.moveData.grapplePoint + Vector3.Project(pullDir * ctx.moveData.distanceFromGrapple, )

    //         }

    //         pullDir = (pullTarget - ctx.moveData.origin).normalized;
            
    //         // ctx.moveData.xAimDamp = Mathf.Lerp(ctx.moveData.xAimDamp, 0f, Time.deltaTime * 2f);
    //         // ctx.moveData.yAimDamp = Mathf.Lerp(ctx.moveData.yAimDamp, 0f, Time.deltaTime * 2f);


    //         // ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, ctx.avatarLookForward * (flingDistance), Time.deltaTime * 2f);
    //         ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, pullDir * (ctx.moveData.distanceFromGrapple + 5f), Time.deltaTime);

    //         // ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, ctx.avatarLookForward * zipBoost, Time.deltaTime * 4f);
    //         // ctx.moveData.velocity += Vector3.Project(ctx.moveData.grappleDir,ctx.avatarLookForward) * ctx.moveData.distanceFromTarget;

    //         t += Time.deltaTime;

    //     }

    //     if (ctx.moveData.wishFire2Up || Vector3.Dot(ctx.avatarLookForward, ctx.moveData.grappleDir) < 0f || ctx.moveData.wishCrouchDown) {
    //         ctx.moveData.grappling = false;
    //     }
        
    //     CheckSwitchStates();
    // }

    public override void ExitState()
    {
        // Debug.Log("EXIT GRAPPLE");
        // ctx.jumpTimer = 0f;
        ctx.reduceGravityTimer = 0f;
    }

    public override void InitializeSubStates()
    {
        // if (ctx.moveData.attacking) {
        //     SetSubState(factory.Melee());
        // } 
        if (ctx.moveData.grounded && !ctx.moveData.wishShiftDown) {
            SetSubState(factory.Neutral());
        } else if (ctx.moveData.grounded && ctx.moveData.wishShiftDown) {
            SetSubState(factory.Dash());
        } else if (!ctx.moveData.grounded && !ctx.moveData.wishShiftDown) {
            SetSubState(factory.Fall());
        } else if (!ctx.moveData.grounded && ctx.moveData.wishShiftDown) {
            SetSubState(factory.Dive());
        }
    }

    public override void CheckSwitchStates()
    {

        if (!ctx.moveData.grappling && ctx.moveData.grounded ) {
            ctx.StopGrapple();
            SwitchState(_factory.Grounded());
        } 

        else if (!ctx.moveData.grappling && !ctx.moveData.grounded) {
            ctx.StopGrapple();
            SwitchState(_factory.Air());
        }
    }

    private void GrappleMoveTargeting() {

        Vector3 grappleDir = (ctx.moveData.grapplePoint - ctx.moveData.origin).normalized;

        float lookCurve = Mathf.Clamp(Vector3.Dot(ctx.avatarLookForward, grappleDir), 0f, 1f);

        // ctx.bezierCurve.GetSpiralCurve(0.7f, t);
        // ctx.bezierCurve.SpiralCurvePoint();
        // ctx.bezierCurve.CreateSpiralFromVelocity();
        // ctx.bezierCurve.CreateArcFromVelocity();
        // ctx.bezierCurve.GrappleArc(ctx.moveData.grappleNormal, ctx.moveData.grapplePoint);
        

    }

    private void GrappleMove() {

            CancelVelocityAgainst(ctx.moveData.grappleDir, 7.5f);
            

            ctx.moveData.velocity.y -= (ctx.moveConfig.gravity * Time.deltaTime * .5f);

            if (ctx.moveData.wishJumpUp) {
                BoostJump((ctx.avatarLookForward).normalized, Mathf.Max(ctx.moveData.velocity.magnitude, 30f));
                ctx.moveData.grappling = false;
            } 
            
            if (ctx.moveData.wishShiftDown) {
                
                // ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, ctx.moveData.grappleDir * (ctx.moveData.distanceFromGrapple), Time.deltaTime * (1f + t));
                
                ctx.moveData.velocity += ctx.moveData.grappleDir * (initialDistance) * Time.deltaTime * (Mathf.Sqrt(3f));
                // ctx.moveData.velocity = Vector3.ClampMagnitude(ctx.moveData.velocity, initialDistance * 2f);

                ctx.moveConfig.grappleColor = Color.Lerp(ctx.moveConfig.grappleColor, ctx.moveConfig.accelColor, Time.deltaTime * 2f);
                
                t += Mathf.Min(1f, Time.deltaTime * 1f);
                // Debug.Log(t);

            }
            else {
                t = 0f;
                initialDistance = Mathf.Min(ctx.moveData.distanceFromGrapple, 30f);
            }

            if (ctx.moveData.wishCrouchUp || ctx.moveData.distanceFromGrapple < ctx.moveConfig.minDistance / 2f) {
                ctx.moveData.grappling = false;
                releaseVelocity = ctx.moveData.velocity;
                ctx.releaseTimer = 5f;

                releasedPoints[0] = ctx._grappleArc.GetVector3("Pos0");
                releasedPoints[1] = ctx._grappleArc.GetVector3("Pos1");
                releasedPoints[2] = ctx._grappleArc.GetVector3("Pos2");
                releasedPoints[3] = ctx._grappleArc.GetVector3("Pos3");
            }

    }

}