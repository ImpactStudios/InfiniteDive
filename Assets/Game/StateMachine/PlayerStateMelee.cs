using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerStateMelee : PlayerBaseState {


    public PlayerStateMelee(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base (currentContext, playerStateFactory) {
        _isMovementState = false;
        name = "melee";
    }

    public override void EnterState()
    {
        // Debug.Log("ENTER MELEE");
        InfluenceAim();
        FindLookingAtTarget();
        InitializeSubStates();
        t = 0f;
        ctx.moveData.attacking = false;
        
        
    }

    public override void UpdateState()
    {

        if (ctx.currentTarget == null) {
            FindLookingAtTarget();
            return;
        }

        if (ctx.currentTarget != null && ctx.moveData.wishFireDown) {
            LungeTargeting();
        }

        if (ctx.currentTarget != null && !ctx.moveData.wishFireDown) {
            ctx.moveData.attacking = true;
            t += Time.deltaTime;

            ctx.bezierCurve.InterpolateAcrossCurveC1(t);
        }

        if (t >= 1f) {
            ctx.moveData.attacking = false;
            ctx.reduceGravityTimer = 0.5f;

            ctx.currentTarget = null;
            ctx.moveData.targetPoint = Vector3.zero;
            ctx.moveData.targetDir = Vector3.zero;
            ctx.moveData.distanceFromTarget = 0f;
            ctx.bezierCurve.Clear();
            ctx.bezierCurve = null;
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        // Debug.Log("EXIT MELEE");
    }

    public override void InitializeSubStates()
    {

    }

    public override void CheckSwitchStates()
    {

        if (ctx.moveData.momentumVelocity.magnitude < 50f && t >= 1f && !ctx.moveData.attacking) {
            SwitchState(_factory.Neutral());
        } else if (ctx.moveData.momentumVelocity.magnitude >= 50f && t >= 1f && !ctx.moveData.attacking) {
            SwitchState(_factory.Dash());
        }
    }

    private void LungeTargeting() {

        if (Vector3.Dot(ctx.avatarLookForward, ctx.moveData.targetDir) < .5f) {
            ctx.currentTarget = null;
            t = 1f;
            return;
        }

        ctx.bezierCurve.AttackArc(Vector3.ProjectOnPlane(ctx.moveData.targetNormal, Vector3.up).normalized, ctx.currentTarget.transform.position);

    }

    private void FindLookingAtTarget() {

        if (ctx.targetLength > 0) {
            
            Vector3 _targetPos = Vector3.zero;
            Vector3 _targetDir = Vector3.zero;

            foreach (Collider target in ctx.moveData.targets) {

                if (target == null) continue;

                _targetPos = target.transform.position;
                _targetDir = (target.transform.position - ctx.avatarLookTransform.position).normalized;

                if (Vector3.Dot(ctx.avatarLookForward, _targetDir) >= .5f) {
                    ctx.moveData.targetPoint = _targetPos;
                    ctx.moveData.targetDir = _targetDir;
                    ctx.moveData.distanceFromTarget = (ctx.moveData.targetPoint - ctx.avatarLookTransform.position).magnitude;
                    ctx.currentTarget = target;
                }
            }
            
        }

        if (ctx.moveData.targetDir == Vector3.zero) {
            ctx.currentTarget = null;
            ctx.moveData.targetPoint = Vector3.zero;
            ctx.moveData.targetDir = Vector3.zero;
            ctx.moveData.distanceFromTarget = 0f;
            ctx.moveData.targetNormal = Vector3.zero;
            return;
        }

        Ray ray = new Ray(ctx.avatarLookTransform.position, ctx.moveData.targetDir);
        RaycastHit hit;

        if (Physics.SphereCast(ray, 1f, out hit, 300f, LayerMask.GetMask (new string[] { "Enemy" }))) {
            ctx.moveData.targetNormal = hit.normal;
            ctx.bezierCurve = new BezierCurve(ctx, ctx.moveData.targetNormal, ctx.moveData.targetPoint);
            ctx.bezierCurve.AttackArc(ctx.moveData.targetNormal, ctx.moveData.targetPoint);
        } else {
            ctx.currentTarget = null;
            ctx.moveData.targetPoint = Vector3.zero;
            ctx.moveData.targetDir = Vector3.zero;
            ctx.moveData.distanceFromTarget = 0f;
            ctx.moveData.targetNormal = Vector3.zero;
            Debug.Log("miss");
        }


        // Collision closestTarget = null;

        // foreach (Collision target in moveData.targets) {

        //     if (closestTarget == null) {
        //         closestTarget = target;
        //     } else {

        //         if (Vector3.Distance(target.transform.position, moveData.origin) > Vector3.Distance(closestTarget.transform.position, moveData.origin)) {
        //             closestTarget = target;
        //         }

        //     }

        // }

    }


    

}