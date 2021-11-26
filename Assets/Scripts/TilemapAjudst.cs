using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapAjudst : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Floor(2*Camera.main.transform.position.x)/2;
        pos.y = Mathf.Floor(4*Camera.main.transform.position.y) / 16;
        transform.position = pos;
    }
}
