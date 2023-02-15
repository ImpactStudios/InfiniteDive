using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateLunge : PlayerBaseState {

    public PlayerStateLunge(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isRootState = false;
        name = "lunge";
    }

    public override void EnterState()
    {
        Debug.Log("ENTER LUNGE");
        t = 0f;

    }

    public override void UpdateState()
    {

        Debug.Log("actually here");

        if (ctx.moveData.wishFireDown) {

            Vector3 toTarget = (ctx.moveData.focusPoint - ctx.moveData.origin);

            ctx.moveData.velocity = Vector3.Slerp(ctx.moveData.velocity, toTarget, Time.deltaTime * 2f);

            t += Time.deltaTime;

        }

        if (ctx.moveData.wishFireUp) {
            ctx.moveData.attacking = false;
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        ctx.slashObj.SetActive(true);
        ctx.slash.Play();
    }

    public override void InitializeSubStates()
    {

    }

    public override void CheckSwitchStates()
    {

        if (ctx.moveData.grounded && !ctx.moveData.attacking) {
            SwitchState(factory.Neutral());
        } else if (!ctx.moveData.grounded && !ctx.moveData.attacking) {
            SwitchState(factory.Fall());
        }
    }

}