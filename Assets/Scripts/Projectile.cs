using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Color trailColor; //17

    float speed = 10;
    public LayerMask collisionMask;

    float damage = 1;

    float lifeTime = 3;
    float skinWidth = .1f;

    public void Start (){ // destruir o tiro depois de X tempo

        Destroy (gameObject, lifeTime);

        Collider[] initialCollisions = Physics.OverlapSphere (transform.position, .1f, collisionMask);
            if (initialCollisions.Length > 0) {
                OnHitObject(initialCollisions[0], transform.position); //15
            }

            GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColor); //17
    }
    
    public void SetSpeed(float newSpeed){
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions (moveDistance);
       transform.Translate (Vector3.forward * Time.deltaTime * speed); 
    }

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray (transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance = skinWidth /*/ para checar a colisao antecipada /*/, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint ) { //15
        iDamage damageObject = c.GetComponent<iDamage>();
        if (damageObject != null) {
            damageObject.TakeHit(damage, hitPoint, transform.forward); //15
        }

        GameObject.Destroy(gameObject);        
    }
}