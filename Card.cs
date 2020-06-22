using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private GameManager gameManager;
    private Player You;
    public Color color;
    public CardFunction CardFunction;
    public string Name;
    public CardState cardState;
    public int ID;
    public int order = 0;

    private void OnMouseUp() //點擊出牌
    {
        if (cardState == CardState.YourCard && gameManager.YouPlayCard == true) //只有在你能出牌且這張牌屬於你時，你才能試圖出這張牌
        {
            GameObject You = GameObject.Find("You");
            You.GetComponent<Player>().PlayerPlayCard(this.ID);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.YouChooseColor == true)
        {
            StartCoroutine(WaitForChooseColor());
        }
    }

    IEnumerator WaitForChooseColor()
    {
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitWhile(()=>{return gameManager.YouChooseColor;}); //直到你選顏色前，底下的程式都不會進行
        GetComponent<Collider2D>().enabled = true;
    }
}

public enum Color
{
    Red,Blue,Green,Yellow,All
}

public enum CardFunction
{
    Zero,One,Two,Three,Four,Five,Six,Seven,Eight,Nine,Skip,DrawTwo,Reverse,Wild,WildDrawFour
}

public enum CardState
{
    InDeck,Used,YourCard,OthersCard,Played
}