using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip[] soundEffect;
    bool isFirstCalled = true;
    // Start is called before the first frame update
    void Start()
    {
        if(isFirstCalled)
        {
            isFirstCalled = false;
            DontDestroyOnLoad(transform.gameObject);
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayCardVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[0]);
    }

    public void StopVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[1]);
    }

    public void ReverseVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[2]);
    }

    public void PlusTwoVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[3]);
    }

    public void PlusFourVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[4]);
    }

    public void UNOVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[5]);
    }

    public void WinVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[6]);
    }

    public void TurnPageVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[7]);
    }

    public void ForgetUNOVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[8]);
    }

    public void RasenganVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[9]);
    }

    public void ChidoriVoice()
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundEffect[10]);
    }
}
