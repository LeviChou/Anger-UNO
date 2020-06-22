using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackSceneButton : MonoBehaviour
{
    private GameManager gameManager;
    private SoundEffectManager SoundEffect;

    public void BackScene()
    {
        SoundEffect = GameObject.Find("SoundEffectManager(Clone)").GetComponent<SoundEffectManager>();
        string sceneName = Scene_Name();
        if(sceneName == "StartScene")
        {
            SceneManager.LoadScene(1);
        }
        else if(sceneName == "RuleScene2")
        {
            SceneManager.LoadScene(2);
        }
        else if(sceneName == "RuleScene3")
        {
            SceneManager.LoadScene(3);
        }
        else if(sceneName == "RuleScene4")
        {
            SceneManager.LoadScene(4);
        }
        else if(sceneName == "RuleScene5")
        {
            SceneManager.LoadScene(5);
        }
        SoundEffect.TurnPageVoice();
    }

    private string Scene_Name()
    {
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        return sceneName;
    }

}
