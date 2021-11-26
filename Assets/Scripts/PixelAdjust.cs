using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelAdjust : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        //pos.x = Adjust(pos.x);
        //pos.y = Adjust(pos.y);
        //pos.z = Adjust(pos.z);
        //transform.position = pos;
    }

    void Update()
    {
        //transform.localPosition = Vector3.zero;
    }

    float Adjust(float x)
    {
        return Mathf.Round(x*16)/16;
    }
}
