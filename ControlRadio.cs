using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlRadio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject Radio = GameObject.Find("Radio(Clone)");
        if(Radio == null)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/Radio"));
        }
        GameObject SoundEffectManager = GameObject.Find("SoundEffectManager(Clone)");
        if(Radio == null)
        {
            Instantiate(Resources.Load<GameObject>("Prefabs/SoundEffectManager"));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
