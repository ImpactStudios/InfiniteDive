using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateDive : PlayerBaseState {

    public PlayerStateDive(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "dive";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER DIVE");
        // ctx.reduceGravityTimer = 1f;
        ctx.sphereLines.Stop();
    }

    public override void UpdateState()
    {

        if (!ctx.moveData.wishJumpDown) { 
            if (ctx.moveConfig.flyMode) {
                MouseFly();
            } else {
                MouseSteerAir();
            }
        }

        // if (ctx.moveData.momentumVelocity.magnitude < ctx.moveConfig.runSpeed) {
        //     Accelerate();
        // }

        // if (ctx.moveData.wishJumpDown && ctx.boostInputTimer <= 0f) {
        //     CancelVelocityAgainst(-ctx.moveData.momentumVelocity.normalized, 1f);
        //     CancelVelocityAgainst(-Vector3.up, 5f);
        //     ctx.vcam.m_CameraDistance = Mathf.Lerp(ctx.vcam.m_CameraDistance, 3f, Time.deltaTime * 4f);

        //     BrakeCharge();

        //     ctx.sphereLines.SetFloat("Speed", -ctx.moveData.vCharge);

        //     ctx.sphereLines.Play();
        // }

        // if (ctx.moveData.wishJumpUp && ctx.boostInputTimer <= 0f) {
        //     Jump(Vector3.up);
        //     ctx.sphereLines.Stop();
        // }

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
            SwitchState(_factory.Fall());
        } else if (ctx.moveData.grappling) {
            SwitchState(_factory.Grapple());
        }
    }

    private void MouseFly() {
        ctx.ignoreGravityTimer = 1f;
        DiveInfluenceVelocityMouseFly(ref ctx.moveData.momentumVelocity);

    }

    private void MouseSteerAir() {
        DiveInfluenceVelocityAir();
    }

    private void Accelerate() {
        ctx.moveData.momentumVelocity += (ctx.avatarLookForward * ctx.moveConfig.walkSpeed) * Time.deltaTime;
    }

}