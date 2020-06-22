using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnControl : MonoBehaviour
{
    public bool[] PlayerAbleToPlay = new bool[4];
    private string[] PlayerNameinPosition = new string[4];
    private GameManager gameManager;
    public GameState currentState = GameState.Wait;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void PlayerUpdate(string[] PlayerName) //記住玩家的資訊、初始化控制玩家能否出牌
    {

        PlayerNameinPosition = PlayerName;
        for(int i = 0;i<4;i++)
        {
            if(i==0)
            {
                PlayerAbleToPlay[i] = true;
            }
            else
            {
                PlayerAbleToPlay[i] = false;
            }
        }
        currentState = GameState.Game; //開始遊戲
    }

    public void ChangeOrder(int order) //當有人打出迴轉牌，必須改變玩家的出牌順序 (order)
    {
        Debug.Log("Change");
        GameObject Player;

        for(int i=0;i<4;i++)
        {
            PlayerAbleToPlay[i] = false;
        }
        
        for(int i=0;i<4;i++)
        {
            Player = GameObject.Find(PlayerNameinPosition[i]);
            int TheOriginOrder = Player.GetComponent<Player>().order;
            if(TheOriginOrder==2)Player.GetComponent<Player>().order = 4;
            else if(TheOriginOrder==4)Player.GetComponent<Player>().order = 2;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum GameState
{
    Game,Over,Wait
}