using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateNeutral : PlayerBaseState {

    public PlayerStateNeutral(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "neutral";
    }

    public override void EnterState()
    {
        oldMomentum = Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f));
        Debug.Log("ENTER NEUTRAL");
    }

    public override void UpdateState()
    {
        OnlyInfluence();

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
        } else if (ctx.moveData.wishShiftDown) {
            oldMomentum = Vector3.zero;
            SwitchState(_factory.Dash());
        } else if (ctx.moveData.grappling) {
            oldMomentum = Vector3.zero;
            SwitchState(_factory.Grapple());
        }

    }

}