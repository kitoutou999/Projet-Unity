using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Three : MonoBehaviour
{
    public float heal;
    public GameObject three;
    public GameObject wood;
    public KeyCode cutwood = KeyCode.L;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(cutwood))
        {
            heal = heal-1f;
            
        }
        if(heal<4)
        {
            Instantiate(wood, new Vector3(transform.position.x,transform.position.y-1.8f,transform.position.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z-12f));
            Instantiate(wood, new Vector3(transform.position.x,transform.position.y,transform.position.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z-12f));
            Instantiate(wood, new Vector3(transform.position.x,transform.position.y+1.8f,transform.position.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z-12f));
            Destroy(three);
        }
    }
}
