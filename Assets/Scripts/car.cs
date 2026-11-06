using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class car : MonoBehaviour
{
    public float speed;

    void Update()
    {
        if(Input.GetKey(KeyCode.LeftArrow)){
            GetComponent<Rigidbody>().linearVelocity = new Vector3(-speed, 0,0);
            transform.rotation = Quaternion.Euler(0, -10, 0);
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            GetComponent<Rigidbody>().linearVelocity = new Vector3(speed, 0,0);
            transform.rotation = Quaternion.Euler(0, 10, 0);

        }
        if(Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0,0);
            transform.rotation = Quaternion.Euler(0, 0, 0);

        }
    }
}
