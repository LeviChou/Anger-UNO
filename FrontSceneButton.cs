using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FrontSceneButton : MonoBehaviour
{
    private SoundEffectManager SoundEffect;
    public void FrontScene()
    {
        SoundEffect = GameObject.Find("SoundEffectManager(Clone)").GetComponent<SoundEffectManager>();
        string sceneName = Scene_Name();
        Debug.Log(sceneName);
        if(sceneName == "StartScene")
        {
            SceneManager.LoadScene(2);
        }
        else if(sceneName == "RuleScene1")
        {
            SceneManager.LoadScene(3);
        }
        else if(sceneName == "RuleScene2")
        {
            SceneManager.LoadScene(4);
        }
        else if(sceneName == "RuleScene3")
        {
            SceneManager.LoadScene(5);
        }
        else if(sceneName == "RuleScene4")
        {
            SceneManager.LoadScene(6);
        }
        else if(sceneName == "RuleScene5")
        {
            SceneManager.LoadScene(0);
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
