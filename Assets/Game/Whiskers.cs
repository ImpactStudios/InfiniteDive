using System.Collections;
using System.Collections.Generic;
using Fragsurf.Movement;
using UnityEngine;

public class Whiskers : MonoBehaviour
{

    private SphereCollider collider;


    void Awake() {

        collider = gameObject.GetComponent<SphereCollider>();

        if (collider == null) {
            collider = gameObject.AddComponent<SphereCollider>();   
        }

        collider.radius = 1;
        collider.isTrigger = true;

    }
    void Start()
    {
        
    }

    void Update()
    {
        // Vector3 newPos = SurfPhysics.ResolveCollisions(collider, transform.position);

        // Debug.Log(transform.position + " " + newPos);

        // transform.position = newPos;
        
    }

}
