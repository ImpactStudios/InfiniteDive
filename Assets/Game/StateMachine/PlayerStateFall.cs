using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFall : PlayerBaseState {

    public PlayerStateFall(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "fall";
    }

    public override void EnterState()
    {
        oldMomentum = Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f));
        // Debug.Log("ENTER FALL");
        ctx.sphereLines.Stop();
    }

    public override void UpdateState()
    {

        OnlyInfluenceAir();

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
            SwitchState(_factory.Dive());
        } else if (ctx.moveData.grappling) {
            SwitchState(_factory.Grapple());
        }

    }

    // private void OnlyInfluenceAir() {

    //     Vector3 neutralMove = viewFlat * ctx.moveData.influenceVelocity * ctx.moveConfig.walkSpeed / 2f;

    //     if (Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f)).magnitude > ctx.moveConfig.walkSpeed) {
    //         oldMomentum = Vector3.Scale(ctx.moveData.momentumVelocity, new Vector3(1f, 0f, 1f));
    //         DiveInfluenceVelocityAir();
    //         Debug.Log(oldMomentum);
    //     } else {
    //         oldMomentum = Vector3.Lerp(oldMomentum, Vector3.zero, Time.deltaTime * 2f);
    //         Debug.Log("boo");

    //         DiveInfluenceVelocityAir();

    //         var yVel = ctx.moveData.momentumVelocity.y;
    //         ctx.moveData.momentumVelocity.y = 0f;
    //         ctx.moveData.momentumVelocity = Vector3.Lerp(ctx.moveData.momentumVelocity, Vector3.ClampMagnitude(neutralMove + oldMomentum, ctx.moveConfig.walkSpeed), Time.deltaTime);
    //         ctx.moveData.momentumVelocity.y = yVel;
    //     }
        
    // }

}