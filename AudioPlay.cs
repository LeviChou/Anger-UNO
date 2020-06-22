using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    public AudioClip[] audios;
    int AudioOrder;
    const int AudiosSizes = 4;
    [Header("isFirstCalled")]
    bool isFirstCalled = true;
    // Start is called before the first frame update
    void Start()
    {
        if(isFirstCalled)
        {
            isFirstCalled = false;
            this.GetComponent<AudioSource>().clip = audios[0];
            this.GetComponent<AudioSource>().Play();
            AudioOrder = 0;

            DontDestroyOnLoad(transform.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!this.GetComponent<AudioSource>().isPlaying)
        {
            this.GetComponent<AudioSource>().Stop();
            if(AudioOrder == AudiosSizes)
            {
                AudioOrder = 0;
            }
            else
            {
                AudioOrder++;
            }

            this.GetComponent<AudioSource>().clip = audios[AudioOrder];
            this.GetComponent<AudioSource>().Play();
        }
    }
}
