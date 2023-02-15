using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateDash : PlayerBaseState {

    public PlayerStateDash(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isRootState = false;
        name = "dash";
    }

    public override void EnterState()
    {
        // ctx.sonicBoom.Play();
    }

    public override void UpdateState()
    {

        if (ctx.moveData.velocity.magnitude < ctx.moveConfig.runSpeed && !ctx.moveData.wishJumpDown) {
            Accelerate();
        }

        if (!ctx.moveData.wishJumpDown) {
            DiveInfluenceVelocityMouseFlat(ref ctx.moveData.velocity);
        }

        if (ctx.moveData.wishJumpUp) {

            Vector3 wishDir = ctx.avatarLookForward;

            if (Vector3.Dot(ctx.avatarLookForward, -ctx.groundNormal) >= 0f) {
                wishDir = flatForward;
            }

            BoostJump(wishDir);
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
        if (!ctx.moveData.wishShiftDown) {
            SwitchState(_factory.Neutral());
        }
    }

    private void Accelerate() {

        float vFactor = 1f;

        if (ctx.moveData.velocity.magnitude < ctx.moveConfig.walkSpeed) vFactor = .1f;

        ctx.moveData.velocity += ctx.moveData.flatWishMove * Time.deltaTime * ctx.moveConfig.walkSpeed / vFactor;
    }

}