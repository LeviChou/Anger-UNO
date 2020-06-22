using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rasengan : MonoBehaviour
{
    public Animator rasengaAni;
    // Start is called before the first frame update
    void Start()
    {
        rasengaAni.SetInteger("Status",0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Explode()
    {
        rasengaAni.SetInteger("Status",1);
    }
}
