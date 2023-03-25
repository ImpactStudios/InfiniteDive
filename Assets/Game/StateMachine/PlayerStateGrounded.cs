using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateGrounded : PlayerBaseState
{
    public PlayerStateGrounded(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
        name = "grounded";
    }

    public override void EnterState()
    {
        Debug.Log("ENTER GROUNDED");

        InitializeSubStates();
    }

    public override void UpdateState()
    {

        Debug.Log(ctx.moveData.velocity.magnitude);

        if (ctx.moveData.wishJumpDown && ctx.energySlider.value > .25f) {
            // ctx.framingCam.m_CameraDistance = Mathf.Lerp(ctx.framingCam.m_CameraDistance, 3f, Time.deltaTime * 4f);
            ctx.sphereLines.SetFloat("Speed", -ctx.moveData.vCharge);
            ctx.sphereLines.Play();
            
            // SubtractVelocityAgainst(ref ctx.moveData.velocity, -ctx.moveData.velocity.normalized, ctx.moveData.velocity.magnitude * 2f);

            BrakeCharge(ctx.avatarLookForward);
        }

        // if (ctx.moveData.wishJumpUp && !ctx.moveData.grappling) {
        //     BoostJump(ctx.avatarLookForward, Mathf.Max(ctx.moveData.velocity.magnitude, 30f));
        // }

        Slide(ctx.moveData.wishCrouchDown);

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // ctx.smokeLand.SetVector3("direction", Vector3.ProjectOnPlane(ctx.moveData.velocity, ctx.groundNormal));
        // ctx.smokeLand.SetVector3("position", ctx.moveData.origin);
        // Debug.Log("EXIT GROUNDED");

        
    }

    public override void InitializeSubStates()
    {
        // if (ctx.moveData.attacking) {
        //     SetSubState(factory.Lunge());
        // } else if (ctx.moveData.wishShiftDown) {
        //     // SetSubState(factory.Dash());
        // } else {
            oldMomentum = Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 1f, 1f));
            SetSubState(factory.Neutral());
        // }

        

    }

    public override void CheckSwitchStates()
    {
        // if (ctx.moveData.wishCrouch) {
        //     SwitchState(factory.Charge());
        // } else 

        if (!ctx.moveData.grounded) {
            SwitchState(factory.Air());
        }
        else if (ctx.moveData.grappling) {
            SwitchState(factory.Grapple());
        }
    }

}