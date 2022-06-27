using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 5;
    PlayerController controller;
    GunController gunController;

    Camera viewCamera;

    public Crosshair crosshairs;

    protected override void Start()
    {
        base.Start();
    }

    void Awake () { //21
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();        
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave; //21
    }

    void OnNewWave(int waveNumber) { //21
        health = startingHealth;
        gunController.EquipGun (waveNumber -1);
    }
 

    void FixedUpdate()
    {
        // Comandos de movimento
        Vector3 moveInput = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw("Vertical")* Time.deltaTime);
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move (moveVelocity);

        // Comandos da camera
        Ray ray = viewCamera.ScreenPointToRay (Input.mousePosition); // para ajustar o campo de visao do player
        Plane groundPlane = new Plane (Vector3.up, Vector3.up * gunController.GunHeight); //19
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point; //19
            crosshairs.DetectTargets(ray);//19

            if ((new Vector2(point.x, point.z)- new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1) { //20
                gunController.Aim(point); //20
            }
        }

        // Comandos da arma
        if (Input.GetMouseButton(0)) {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0)) {
            gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R)) { //20
            gunController.Reload();
        }
    }

    public override void Die () {
        AudioManager.instance.PlaySound ("Player Death", transform.position); //23
        base.Die();
    }
}
