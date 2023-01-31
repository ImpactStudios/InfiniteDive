using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateAir : PlayerBaseState {

    public PlayerStateAir(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = true;
        name = "air";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER AIR");
        InitializeSubStates();
    }

    public override void UpdateState()
    {
        AirMovement();
        // Debug.Log(ctx.moveData.momentumVelocity.magnitude);
        Vector3.ClampMagnitude(ctx.moveData.momentumVelocity, 50f);
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Debug.Log("EXIT AIR");
    }

    public override void InitializeSubStates()
    {
        if (!ctx.moveData.grappling && ctx.moveData.wishFireDown || ctx.moveData.attacking) {
            SetSubState(factory.Melee());
        } else if (!ctx.moveData.wishShiftDown) {
            SetSubState(factory.Fall());
        } else if (ctx.moveData.wishShiftDown) {
            SetSubState(factory.Dive());
        }
    }

    public override void CheckSwitchStates()
    {

        // if (ctx.moveData.wishCrouch) {
        //     SwitchState(factory.Charge());
        // } else 
        if (_ctx.moveData.grounded) {
            SwitchState(factory.Grounded());
        } 
        // else if (ctx.moveData.grappling) {
        //     SwitchState(factory.Grapple());
        // } 

    }

    

    private void AirMovement() {

        if (ctx.ignoreGravityTimer > 0f) { } 
        else if (ctx.reduceGravityTimer > 0f) {
            ctx.moveData.momentumVelocity.y -= (ctx.moveConfig.gravity * Time.deltaTime * (1f - ctx.reduceGravityTimer));
        } else if (!ctx.moveData.grappling) {
            ctx.moveData.momentumVelocity.y -= (ctx.moveConfig.gravity * Time.deltaTime);
        }
            
        if (ctx.moveData.detectWall && !ctx.moveData.grappling) { // wall run state

            if (ctx.doubleJump) {
                ctx.airHike.SetVector3("origin", ctx.moveData.origin);
                ctx.airHike.SetVector3("lookAt", ctx.moveData.wallNormal / 2f);
                ctx.airHike.SetFloat("size", 4f);
                ctx.airHike.Play();
            }

            ctx.doubleJump = false;

            if (Vector3.Dot(ctx.moveData.momentumVelocity, ctx.moveData.wallNormal) <= -7.5f) {
                
                ctx.smokeLand.SetVector3("velocity", Vector3.ProjectOnPlane(ctx.moveData.momentumVelocity / 2f, ctx.moveData.wallNormal));
                ctx.smokeLand.SetVector3("position", ctx.moveData.origin - ctx.moveData.wallNormal / 2f);
                ctx.smokeLand.SetVector3("eulerAngles", Quaternion.LookRotation(ctx.moveData.wallNormal, Vector3.ProjectOnPlane(-ctx.velocityForward, ctx.moveData.wallNormal)).eulerAngles);
                ctx.smokeLand.Play();
            }
            
            // ctx.moveData.momentumVelocity.y = Mathf.Lerp(ctx.moveData.momentumVelocity.y, 0f, Time.deltaTime / 4f);

            // if (ctx.moveData.momentumVelocity.magnitude > ctx.moveConfig.runSpeed + 10f) {
            //     SubtractVelocityAgainst(Vector3.ProjectOnPlane(ctx.moveData.momentumVelocity.normalized, ctx.moveData.wallNormal), ctx.moveData.momentumVelocity.magnitude / 2f);
            // } 
            // else 
            // if (ctx.moveData.momentumVelocity.magnitude < ctx.moveConfig.walkSpeed && ctx.moveData.wishShiftDown) {
            //     AddVelocityTo(Vector3.ProjectOnPlane(ctx.moveData.momentumVelocity.normalized, ctx.moveData.wallNormal), ctx.moveConfig.walkSpeed + 5f);
            // }


            // if (ctx.jumpTimer <= 0f) ctx.moveData.momentumVelocity += -ctx.moveData.wallNormal;

            ctx.smoke.SetVector3("position", ctx.moveData.origin - ctx.moveData.wallNormal / 2f);
            ctx.smoke.SetVector3("direction", -ctx.moveData.momentumVelocity.normalized);

            if (ctx.moveData.wishJumpDown) {
                // ctx.framingCam.m_CameraDistance = Mathf.Lerp(ctx.framingCam.m_CameraDistance, 3f, Time.deltaTime * 4f);
                ctx.sphereLines.SetFloat("Speed", -ctx.moveData.vCharge);
                ctx.sphereLines.Play();
                
                SubtractVelocityAgainst(ref ctx.moveData.momentumVelocity, -ctx.moveData.momentumVelocity.normalized, ctx.moveData.momentumVelocity.magnitude / 2f);

                BrakeCharge(ctx.avatarLookForward);
            }

            if (ctx.moveData.wishJumpUp) {
                Jump(ctx.groundNormal, ctx.avatarLookForward);
                ctx.sphereLines.Stop();
            }

        } else { // falling state

            // ctx.bezierCurve.PredictGravityArc(ctx.moveData.origin, ctx.moveConfig.gravity, ctx.moveData.momentumVelocity);
            // ctx.bezierCurve.DrawProjection();



            // if (ctx.moveData.wishJumpDown && !ctx.doubleJump) {
            //     SubtractVelocityAgainst(ref ctx.moveData.momentumVelocity, -ctx.moveData.momentumVelocity.normalized, ctx.moveData.momentumVelocity.magnitude / 2f);
            //     ctx.vcam.m_CameraDistance = Mathf.Lerp(ctx.vcam.m_CameraDistance, 3f, Time.deltaTime * 4f);

            //     BrakeCharge();

            //     ctx.sphereLines.SetFloat("Speed", -ctx.moveData.vCharge);

            //     ctx.sphereLines.Play();
            // }

            // if (ctx.moveData.wishJumpUp) {
            //     AirJump(Vector3.up);
            //     ctx.sphereLines.Stop();
            // }

        }
    }

}