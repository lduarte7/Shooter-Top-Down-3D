using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof (NavMeshAgent))]
public class Enemy : LivingEntity
{
    NavMeshAgent pathfinder;
    Transform target;
    public GameObject deathEffect; //15
    public static event System.Action OnDeathStatic; //25

    Color originalColor;
    Material skinEnemy;

    float attackDistHold = 1.5f; // inimigos atacando
    float timeBtwAttacks = 1; // inimigos atacando
    float nextAttackTime; // inimigos atacando
    float damage = 1;

    public enum State {Idle, Chasing, Attacking};  // inimigos atacando
    State currentState; // inimigos atacando

    float myCollisionRadius; // para os inimigos nao entrarem dentro do Player
    float targetCollisionRadius;  // para os inimigos nao entrarem dentro do Player

    LivingEntity targetEntity; // para detectar quando o player morrer
    bool hasTarget; // para detectar quando o player morrer

    void Awake () {
        pathfinder = GetComponent<NavMeshAgent>();    

            if (GameObject.FindGameObjectWithTag("Player") != null) { // verifica de o player existe, na parte de morte
            hasTarget = true; // para detectar quando o player morrer
        
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>(); // para detectar quando o player morrer

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;  // para os inimigos nao entrarem dentro do Player, pega o raio de colisao do player
            targetCollisionRadius = GetComponent<CapsuleCollider>().radius;  // para os inimigos nao entrarem dentro do Player, pega o raio de colisao do inimigo

        }   
    }

    protected override void Start()
    {
        base.Start();
        
        if (hasTarget) { // verifica de o player existe
            currentState = State.Chasing;  // inimigos atacando
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine (UpdatePath());
        }
    }

    public void SetCharacteristics (float moveSpeed, int hitsToKillPlayer, float enemyHealth, Color skinColour) { //18
        pathfinder.speed = moveSpeed;

        if (hasTarget) {
            damage = Mathf.Ceil (targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;
        skinEnemy = GetComponent<Renderer>().sharedMaterial;
        skinEnemy.color = skinColour;
        originalColor = skinEnemy.color;
    }

    public override void TakeHit (float damage, Vector3 hitPoint, Vector3 hitDirection) { //15
        AudioManager.instance.PlaySound("Impact", transform.position); //23
        if (damage >= health) {
            if (OnDeathStatic != null) { //25
                OnDeathStatic(); 
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, 3);

        }
        base.TakeHit (damage, hitPoint, hitDirection);
    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        if (hasTarget){ // so vai fazer esse codigo esse houver player
        if (Time.time > nextAttackTime) {  // inimigos atacando
            float squareDistToTarget = (target.position - transform.position).sqrMagnitude; 
                if (squareDistToTarget < Mathf.Pow (attackDistHold + myCollisionRadius + targetCollisionRadius, 2)) {
                    nextAttackTime = Time.time + timeBtwAttacks;
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack(){

        currentState = State.Attacking;
        pathfinder.enabled = false; 

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - dirToTarget * myCollisionRadius;

        float attackSpeed = 3;
        float percent = 0;
        
        skinEnemy.color = Color.red; //muda a cor para vermelho ao atacar
        bool hasAppliedDamage = false;


         while (percent <= 1) {
              
             if (percent >= .5f && !hasAppliedDamage) {
                 hasAppliedDamage = true;
                 targetEntity.TakeDamage(damage);
             }
            
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
                
            yield return null;
        }

        skinEnemy.color = originalColor; //volta para a cor original depois do ataque
        currentState = State.Chasing;
        pathfinder.enabled = true;
    }

    IEnumerator UpdatePath(){
        float refreshRate = .25f;

        while (hasTarget) { 
            if (currentState == State.Chasing){ // inimigos atacando
                Vector3 dirToTarget = (target.position - transform.position).normalized;  // para os inimigos nao entrarem dentro do Player
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistHold/2);  // os inimigos vao parar a uma certa distancia do player
                if (!dead) {
                    pathfinder.SetDestination(targetPosition);
                }
            }

            yield return new WaitForSeconds(refreshRate);
        }
    }
}
