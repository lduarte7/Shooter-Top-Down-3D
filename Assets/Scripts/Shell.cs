using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public Rigidbody myRb;
    public float forceMin;
    public float forceMax;

    float lifetime = 4;
    float fadetime = 2;

    // Start is called before the first frame update
    void Start()
    {
        float force = Random.Range (forceMin, forceMax);
        myRb.AddForce (transform.right * force);
        myRb.AddTorque (Random.insideUnitSphere * force);
    }

    IEnumerator Fade () { 
        yield return new WaitForSeconds (lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent <Renderer>().material;
        Color initialColor = mat.color;

        while (percent < 1) { 
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initialColor, Color.clear, percent);
            yield return null;
        }
        Destroy (gameObject);
    }
}
