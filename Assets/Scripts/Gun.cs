using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public enum FireMode {Auto, Burst, Single} //17
    public FireMode firemode; //17
    bool triggerReleaseSinceLastShot; //17
    public int burstCount; //17
    int shotsRemainingInBurst; //17

    public int projectilePerMag; //20
    int projectilesRemainingMag;//20
    bool isReloading; //20
    public float reloadTime = .3f; //20


    public Transform[] spawnShotPoint;
    public Projectile projectile;
    public float msBtwShots = 100;
    public float spawnSpeed = 35;
    float nextShotTime;

    [Header("Effects")]
    public Transform shell; //16
    public Transform shellEjection; //16
    MuzzleFlash muzzleflash; //16.2

    [Header("Recoil")]
    Vector3 recoilSmoothVelocity; //20
    float recoilAngle; //20
    float recoilRotSmoothVelocity; //20
    public Vector2 kickMinMax = new Vector2 (.05f, .2f); //20
    public Vector2 recoilAngleMinMax = new Vector2 (3,5);
    public float recoilMoveSettleTime = .1f;
    public float recoilRotationSettleTime = .1f;

    public AudioClip shootAudio;
    public AudioClip reloadAudio;


    void Start() { //16.2
        muzzleflash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount; // 17
        projectilesRemainingMag = projectilePerMag; //20
    }

    void LateUpdate () {
        // animacao recoil
        transform.localPosition = Vector3.SmoothDamp (transform.localPosition, Vector3.zero, ref recoilSmoothVelocity, recoilMoveSettleTime); //20
        recoilAngle = Mathf.SmoothDamp (recoilAngle,0, ref recoilRotSmoothVelocity, recoilRotationSettleTime); //20
        transform.localEulerAngles = transform.localEulerAngles +Vector3.left * recoilAngle;

        if (!isReloading && projectilesRemainingMag == 0 ) { //20
            Reload();
        }
    }

    void Shoot() {
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingMag > 0) { //20

            if (firemode == FireMode.Burst) { //17
                if (shotsRemainingInBurst == 0) {
                     return;
                }
                shotsRemainingInBurst --;
            }
            else if (firemode == FireMode.Single) { //17
                if (!triggerReleaseSinceLastShot) {
                    return;
                }
            }

            for (int i = 0; i < spawnShotPoint.Length; i++) { // 17
            if (projectilesRemainingMag == 0) { //20
                break;
            }
            projectilesRemainingMag --; //20
            nextShotTime = Time.time + msBtwShots / 1000;
            Projectile newProjectile = Instantiate (projectile, spawnShotPoint[i].position, spawnShotPoint[i].rotation) as Projectile;
            newProjectile.SetSpeed (spawnSpeed);
            }

            Instantiate(shell, shellEjection.position, shellEjection.rotation); //16
            muzzleflash.Activate(); //16.2
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y); //20
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y); //20
            recoilAngle = Mathf.Clamp(recoilAngle,0, 30);//20

            AudioManager.instance.PlaySound (shootAudio, transform.position);//22
        }
    }

    public void Reload () { //20
        if (!isReloading && projectilesRemainingMag != projectilePerMag) { // não recarrega se já estiver com a munição cheia
        StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound (reloadAudio, transform.position);//22
        }

    }

    IEnumerator AnimateReload () { //20
        isReloading = true;
        yield return new WaitForSeconds (.2f);

        float reloadSpeed = 1f / reloadTime;
        float percent = 0; 
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadSpeed = 30;

        while (percent < 1 ) {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadSpeed, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        projectilesRemainingMag = projectilePerMag;
    }

    public void Aim (Vector3 aimPoint) { //20
        if (!isReloading) { // não atira se estiver recarregando
        transform.LookAt (aimPoint);
        }
    }

    public void OnTriggerHold() { //17
        Shoot ();
        triggerReleaseSinceLastShot = false;
    }

    public void OnTriggerRelease() { //17
        triggerReleaseSinceLastShot = true;
        shotsRemainingInBurst = burstCount; // 17
    }
}
