using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 velocity;
    Rigidbody myRigidBody;



    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 _velocity){
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookPoint) {
        transform.rotation = Quaternion.Euler(Vector3.up * (90 - Mathf.Rad2Deg * Mathf.Atan2(lookPoint.z, lookPoint.x)));
    }

    public void FixedUpdate(){
        myRigidBody.MovePosition (myRigidBody.position + velocity * Time.fixedDeltaTime);
    }


}
