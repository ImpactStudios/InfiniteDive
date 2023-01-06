using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateGrounded : PlayerBaseState
{
    public PlayerStateGrounded(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        _isMovementState = true;
        name = "grounded";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER GROUNDED");

        ctx.airHike.SetVector3("origin", ctx.moveData.origin);
        ctx.airHike.SetVector3("lookAt", ctx.groundNormal);
        ctx.airHike.SetFloat("size", 4f);
        ctx.airHike.Play();

        if (Vector3.Dot(ctx.moveData.momentumVelocity, ctx.groundNormal) <= -7.5f) {
            ctx.smokeLand.SetVector3("velocity", Vector3.ProjectOnPlane(ctx.moveData.momentumVelocity / 2f, ctx.groundNormal));
            ctx.smokeLand.SetVector3("position", ctx.moveData.origin - ctx.groundNormal / 2f);
            ctx.smokeLand.SetVector3("eulerAngles", Quaternion.LookRotation(ctx.groundNormal, Vector3.ProjectOnPlane(-ctx.velocityForward, ctx.groundNormal)).eulerAngles);
            ctx.smokeLand.Play();
        }

        ctx.doubleJump = true;
        
        InitializeSubStates();
    }

    public override void UpdateState()
    {

        // Debug.Log(ctx.moveData.momentumVelocity.magnitude);
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // ctx.smokeLand.SetVector3("direction", Vector3.ProjectOnPlane(ctx.moveData.momentumVelocity, ctx.groundNormal));
        // ctx.smokeLand.SetVector3("position", ctx.moveData.origin);
        // Debug.Log("EXIT GROUNDED");
    }

    public override void InitializeSubStates()
    {
        if (ctx.moveData.wishFireDown || ctx.moveData.attacking) {
            SetSubState(factory.Melee());
        } 
        else if (ctx.moveData.grappling) {
            SetSubState(factory.Grapple());
        } else if (!ctx.moveData.wishShiftDown) {
            SetSubState(factory.Neutral());
            _currentSubState.oldMomentum = Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f));
        } else if (ctx.moveData.wishShiftDown) {
            SetSubState(factory.Dash());
        }
    }

    public override void CheckSwitchStates()
    {
        // if (ctx.moveData.wishCrouch) {
        //     SwitchState(factory.Charge());
        // } else 

        if (!ctx.moveData.grounded) {
            SwitchState(factory.Air());
        }
        // else if (ctx.moveData.grappling) {
        //     SwitchState(factory.Grapple());
        // }
    }

}