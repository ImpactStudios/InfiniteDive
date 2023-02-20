using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFall : PlayerBaseState {

    public PlayerStateFall(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isRootState = false;
        name = "fall";
    }

    public override void EnterState()
    {
        // oldMomentum = Vector3.Scale(ctx.moveData.velocity, new Vector3(1f, 0f, 1f));
        Debug.Log("ENTER FALL");
        // ctx.sphereLines.Stop();
        InitializeSubStates();
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
        // if (ctx.moveData.wishFirePress) {
        //     SwitchState(_factory.Melee());
        // }

        // if (ctx.moveData.grounded) {
        //     SwitchState(factory.Neutral());
        // }
        // else if (ctx.moveData.wishShiftDown) {
        //     SwitchState(_factory.Dive());
        // } 
        // else if (ctx.moveData.grappling) {
        //     SwitchState(_factory.Grapple());
        // }

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

}