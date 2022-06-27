using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold; // objeto que o player vai segurar
    public Gun[] allGuns; // arma que vai comecar
    Gun equippedGun; // arma equipada

    void Start() {

    }

    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null) {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate (gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.transform.parent = weaponHold;
    }

    public void EquipGun (int weaponIndex) { //21
        EquipGun (allGuns [weaponIndex]);
    }

    public void OnTriggerHold() { // 17
        if (equippedGun != null) {
            equippedGun.OnTriggerHold();
            }
        }

    public void OnTriggerRelease() { //17
        if (equippedGun != null) {
            equippedGun.OnTriggerRelease();       
        }
    }

    public float GunHeight {
		get {
			return weaponHold.position.y;
		}
	}

    public void Aim (Vector3 aimPoint) { //20
        if (equippedGun != null) {
            equippedGun.Aim(aimPoint);       
        }        
    }

    public void Reload () { //20
        if (equippedGun != null) {
            equippedGun.Reload();       
        }        
    }
}