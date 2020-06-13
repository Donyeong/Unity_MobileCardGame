using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffect : MonoBehaviour
{
    Vector3 sv = Vector3.zero;
    float power = 0;

    public void Shake(float power)
    {
        sv = new Vector3(Random.Range(-0.3f,0.3f), Random.Range(-1.5f, 1.5f), 0);
        sv.Normalize();

        sv = sv * power;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = sv;

        sv = -sv * 0.5f;
    }
}
