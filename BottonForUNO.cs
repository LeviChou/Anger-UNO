using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottonForUNO : MonoBehaviour
{
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnMouseUp() //點擊出牌
    {
        if (gameManager.YouPlayCard == true) //只有在你能出牌時，才能按按鈕
        {
            GameObject You = GameObject.Find("You");
            You.GetComponent<Player>().PlayerPlayUNO();
        }
    }
}
