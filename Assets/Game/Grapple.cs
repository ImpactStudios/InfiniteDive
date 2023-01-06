using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grapple : MonoBehaviour
{

    public Rigidbody rb;
    PhysicMaterial physics_mat;
    public Collider hit;
    public Collider playerCollider;
    public bool returning = false;
    public static event Action Hit;
    public static event Action Return;

    void HitAThing() {
        Hit?.Invoke();
    }

    void HitAPlayer() {
        Return?.Invoke();
    }

    void Start()
    {
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), playerCollider, true);
    }

    void Update()
    {

    }

    private void OnTriggerEnter (Collider other) {

        Debug.Log(other.name);

        if (other.name == "Capsule" && returning) {
            HitAPlayer();
        } else if (!(other.name == "Capsule")) {
            hit = other;
            HitAThing();
        }

    }
}
