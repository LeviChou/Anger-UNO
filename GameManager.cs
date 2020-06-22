using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    //玩家數量、卡牌數量、控制
    const int TotalPlayer = 4;
    const int TotalCard = 108;
    public int MaxDeckCard;
    public int DeckTopOrder;
    public int LastCardID;
    public bool YouPlayCard = false;
    public bool YouChooseColor = false;

    [Header("卡牌顏色清單")]
    public List<Color> cardColor;

    [Header("卡牌功能清單")]
    public List<CardFunction> cardFunctions;

    [Header("洗牌用號碼")]
    public List<int> ReshuffleNumber;
    [Header("把ID換成順序")]
    public List<int> IDToOrder;
    [Header("已經出的牌")]
    public List<int> cardHasPlayed;
    public string[] PlayerName = {"You","John","David","Andrew"};
    private string[] ScenesName = {"GameScene","RuleScene1","RuleScene2","RuleScene3","StartScene"};
    

    public float[] Xposition = {-10f,-18f,10f,18f};
    public float[] Yposition = {-10f,5f,10f,-5f};

    private TurnControl turnScript;
    private SoundEffectManager SoundEffect;

    // Start is called before the first frame update
    void Start()
    {
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {
        if(turnScript.currentState == GameState.Over)
        {
            StartCoroutine(GameOver());
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(0);
    }

    public void GameStart()
    {
        SoundEffect = GameObject.Find("SoundEffectManager(Clone)").GetComponent<SoundEffectManager>();
        turnScript = GameObject.Find("TurnControl").GetComponent<TurnControl>();
        CardInitialize();
        shuffle();
        DeckTopOrder = 1;
        PlayerInitialize();
        LastCardID = Order_To_ID(DeckTopOrder - 1);
    }

    void CardInitialize() //初始化牌組，新增一副牌
    {
        SetupCardsColorToBePutIn();
        SetupCardsFunctionToBePutIn();
        for(int i=1;i<=TotalCard;i++)
            {
                int[] cardIDmanager = CardIDManager(i);
                int colorID = cardIDmanager[0];
                int FunctionID = cardIDmanager[1];
                int repeatTimes = cardIDmanager[2];
                AddNewCard(cardColor[colorID],cardFunctions[FunctionID],i,repeatTimes);
            }
    }

    void shuffle() //遊戲剛開始洗牌
    {
        for(int i=0;i<TotalCard;i++)
        {
            ReshuffleNumber.Add(i);
        }
        float distance = -0.1f;
        for(int i=1;i<=TotalCard;i++)
        {
            GameObject TheCard = IDtoGameobeject(i);

            int SelectNum = UnityEngine.Random.Range(0,ReshuffleNumber.Count);
            int Order = ReshuffleNumber[SelectNum];

            TheCard.GetComponent<Card>().order = Order;
            TheCard.GetComponent<Card>().cardState = CardState.InDeck;
            TheCard.transform.localPosition = new Vector3(3f,0,distance * (float)(Order - 1));

            ReshuffleNumber.Remove(Order);
            IDToOrder.Add(Order);
        }
        GameObject cover = Instantiate(Resources.Load<GameObject>("Prefabs/CoverUp"));
        cover.transform.localPosition = new Vector3(3f,0,0);
        MaxDeckCard = TotalCard;
    }

    public void Reshuffle() //遊戲過程洗牌
    {
        cardHasPlayed.Remove(LastCardID);
        for(int i = DeckTopOrder;i <= MaxDeckCard;i++)
        {
            cardHasPlayed.Add(Order_To_ID(i));
        }
        
        MaxDeckCard = NumOfPlayedCard();
        DeckTopOrder = 1;
        for(int i=0;i < MaxDeckCard;i++)
        {
            ReshuffleNumber.Add(i);
        }

        for(int i=0;i < TotalCard;i++)
        {
            IDToOrder[i] = -1;
        }

        float distance = -0.1f;
        for(int i=0;i < MaxDeckCard;i++)
        {
            GameObject TheCard = IDtoGameobeject(cardHasPlayed[i]);
            int SelectNum = UnityEngine.Random.Range(0,ReshuffleNumber.Count);
            int Order = ReshuffleNumber[SelectNum];
            TheCard.GetComponent<Card>().order = Order;
            TheCard.GetComponent<Card>().cardState = CardState.InDeck;

            //復原功能牌的顏色
            var children = TheCard.GetComponentsInChildren<Transform>();
            int[] cardIDmanager = CardIDManager(cardHasPlayed[i]);
            int FunctionID = cardIDmanager[1];
            if(FunctionID==13)
            {
                children[3].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/AllWild");
            }
            else if(FunctionID==14)
            {
                children[3].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/AllWildDrawFour");
            }
            TheCard.transform.localPosition = new Vector3(3f,0,distance * (float)(Order - 1));
            ReshuffleNumber.Remove(Order);
            IDToOrder[cardHasPlayed[i] - 1] = Order;
        }
        
        cardHasPlayed.Clear();
        cardHasPlayed.Add(LastCardID);
        GameObject LastCard = IDtoGameobeject(LastCardID);
        LastCard.transform.localPosition = new Vector3(-3f,0,0);
    }

    public void PlayerInitialize() //玩家初始化
    {
        int ID;
        GameObject TheCard;
        GameObject player;        

        for(int i=1;i<=TotalPlayer;i++)
        {
            player = Instantiate(Resources.Load<GameObject>("Prefabs/Player"));
            var children = player.GetComponentsInChildren<Transform>();
            children[1].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/"+PlayerName[i-1]);
            
            player.transform.localPosition = new Vector3(Xposition[i-1],Yposition[i-1],0);
            Debug.Log(Xposition[i-1]);
            player.GetComponent<Player>().order = i;           
            player.name = PlayerName[i-1];

            for(int j=1;j<=7;j++)
            {
                ID  = Order_To_ID(DeckTopOrder); //先拿到位於牌堆頂端的卡牌
                player.GetComponent<Player>().PlayerCards.Add(ID);                
                TheCard = IDtoGameobeject(ID);
                if(i==1)
                {
                    TheCard.GetComponent<Card>().cardState = CardState.YourCard;
                }
                else
                {
                    TheCard.GetComponent<Card>().cardState = CardState.OthersCard;
                }
                DeckTopOrder++;
            }
            CardPositionManager(player.name);
        }
        //遊戲開始，發完牌後取牌堆最頂端的牌，作為棄牌堆第一張牌
        ID = Order_To_ID(DeckTopOrder);
        TheCard = IDtoGameobeject(ID);

        TheCard.transform.localPosition = new Vector3(-3f,0,0);
        TheCard.transform.eulerAngles = new Vector3(0,0,0); 
        DeckTopOrder++;
        PlayCard(ID);
        TheCard.GetComponent<Card>().cardState = CardState.Used; //為使第一個玩家不受棄牌堆第一張牌的功能影響

        //如果牌堆最頂端的牌為Wild或Wild Draw Four，則再取下一張，直到不是Wild或Wild Draw Four
        CardFunction cardFunction;
        cardFunction = TheCard.GetComponent<Card>().CardFunction;
        bool ReadyForGame = false;
        while(ReadyForGame==false)
        {
            if(cardFunction == CardFunction.WildDrawFour || cardFunction == CardFunction.Wild)
            {
                ID = Order_To_ID(DeckTopOrder);
                TheCard = IDtoGameobeject(ID);

                TheCard.transform.localPosition = new Vector3(-3f,0,0);
                TheCard.transform.eulerAngles = new Vector3(0,0,0);
                DeckTopOrder++;
                PlayCard(ID);
                TheCard.GetComponent<Card>().cardState = CardState.Used; 
                cardFunction = TheCard.GetComponent<Card>().CardFunction;
            }
            else
            {
                ReadyForGame = true;
            }
        }

        turnScript.PlayerUpdate(PlayerName);
    }

    void SetupCardsColorToBePutIn() //Color Enum轉List
    {
        Array array = Enum.GetValues(typeof(Color));
        foreach (var item in array)
        {
            cardColor.Add((Color)item);
        }
    }

    void SetupCardsFunctionToBePutIn() //CardFunction Enum轉List
    {
        Array array = Enum.GetValues(typeof(CardFunction));
        foreach (var item in array)
        {
            cardFunctions.Add((CardFunction)item);
        }
    }

    
    public GameObject IDtoGameobeject(int ID) //輸入ID就可以抓到卡牌
    {
        int[] cardIDmanager = CardIDManager(ID);
        Color color = cardColor[cardIDmanager[0]];
        CardFunction cardfunction = cardFunctions[cardIDmanager[1]];
        int repeatTimes = cardIDmanager[2];
        string CardName = "Card_" + color.ToString() + " " + cardfunction.ToString() + " " + repeatTimes.ToString();
        GameObject TheCard = GameObject.Find(CardName);
        return TheCard;
    }

    public int[] CardIDManager(int ID) //輸入ID即可知道是哪張卡
    {
        int FunctionID;
        int colorID;
        int repeatTimes;
        if(ID<=56)
        {
            FunctionID = ID % 14 - 1;
            colorID = (ID - ID % 14)/14;
            if(FunctionID == -1)
            {
                FunctionID = 13;
                colorID = 4;
                repeatTimes = (ID - ID % 14)/14;
            }
            else
            {
                repeatTimes = 1;
            }     
        }
        else
        {
            FunctionID = (ID-56) % 13;
            colorID = ((ID-56) - (ID-56) % 13)/13;
            if(FunctionID == 0)
            {
                FunctionID = 14;
                colorID = 4;
                repeatTimes = ((ID-56) - (ID-56) % 13)/13;
            }
            else
            {
                repeatTimes = 2;
            }
        }    
        int[] result = {colorID,FunctionID,repeatTimes};
        return result;
    }

    void AddNewCard(Color color,CardFunction cardfunction,int ID,int repeatTimes) //新增一張牌
    {
        GameObject card = Instantiate(Resources.Load<GameObject>("Prefabs/TheCard"));
        card.GetComponent<Card>().color = color;
        card.GetComponent<Card>().CardFunction = cardfunction;
        card.GetComponent<Card>().ID = ID;
        card.name = "Card_" + color.ToString() + " " + cardfunction.ToString() + " " + repeatTimes.ToString();
        card.GetComponent<Card>().cardState = CardState.InDeck;

        GameObject graphic = Instantiate(Resources.Load<GameObject>("Prefabs/Pattern"));

        graphic.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/" + color.ToString() + cardfunction.ToString());
        graphic.transform.SetParent(card.transform);
        graphic.transform.localPosition = new Vector3(0,0,-0.1f);
        graphic.transform.eulerAngles = new Vector3(0,180,0);
    }

    public void PlayCard(int PlayedCardID) //打出一張牌
    {
        GameObject Card = IDtoGameobeject(PlayedCardID);
        int Num = NumOfPlayedCard();
        Card.transform.eulerAngles = new Vector3(0,0,0);
        StartCoroutine(PlayCardAnimation(Card));
        Card.GetComponent<Card>().cardState = CardState.Played;
        cardHasPlayed.Add(PlayedCardID);
        LastCardID = PlayedCardID;
        SoundEffect.PlayCardVoice();
    }

    IEnumerator PlayCardAnimation(GameObject Card)
    {
        float portion = 10f;
        Vector3 OriginPosition = Card.transform.localPosition;
        int Num = NumOfPlayedCard();
        Vector3 FinalPosition = new Vector3(-3f,0,(float)-0.1*(Num-1));
        float xDisplacement = -(OriginPosition.x - FinalPosition.x)/portion;
        float yDisplacement = -(OriginPosition.y - FinalPosition.y)/portion;
        float zDisplacement = -(OriginPosition.z - FinalPosition.z)/portion;
        for(int i=1;i<=portion;i++)
        {
            Card.transform.localPosition = new Vector3(OriginPosition.x + xDisplacement,OriginPosition.y + yDisplacement,OriginPosition.z + zDisplacement);
            yield return new WaitForSeconds(0.015f);
            OriginPosition = Card.transform.localPosition;
        }
    }

    public int NumOfPlayedCard()
    {
        return cardHasPlayed.Count;
    }

    public int ID_To_Order(int ID) //輸入ID可知道該牌在洗牌後的順序
    {
        return IDToOrder[ID-1];
    }

    public int Order_To_ID(int Order) //輸入順序可以得知ID
    {
        return IDToOrder.IndexOf(Order - 1) + 1;
    }

    public int Name_To_Position_Order(string Name)
    {
        return Array.IndexOf(PlayerName,Name);
    }

    public float[] Name_To_Position(string Name) //輸入玩家名可以找到位置
    {
        float[] Pos = new float[2];
        int Index = Array.IndexOf(PlayerName,Name);
        Pos[0] = Xposition[Index];
        Pos[1] = Yposition[Index];
        return Pos;
    }

    public string Name_to_opposite(string Name)
    {
        int Opposite_index;
        int Index = Array.IndexOf(PlayerName,Name);
        if(Index == 0)
        {
            Opposite_index = 2;
        }
        else if(Index == 1)
        {
            Opposite_index = 3;
        }
        else if(Index == 2)
        {
            Opposite_index = 0;
        }
        else
        {
            Opposite_index = 1;
        }
        return PlayerName[Opposite_index];
    }

    public string[] OtherPlayer(string Name)
    {
        string[] OtherPlayerList = new string[3];
        int Index = Array.IndexOf(PlayerName,Name);
        for(int i = 0;i < 4;i ++)
        {
            if(i < Index)
            {
                OtherPlayerList[i] = PlayerName[i];
            }
            else if(i > Index)
            {
                OtherPlayerList[i - 1] = PlayerName[i];
            }
        }
        return OtherPlayerList;
    }

    public void CardPositionManager(string playerName) //整理玩家手中卡牌，純粹為了美觀
    {
        GameObject Player = GameObject.Find(playerName);
        int positionOrder = Array.IndexOf(PlayerName,playerName);
        float[] xcard = {0,-20f,2f,20f};
        float[] ycard = {-10f,-1f,10f,1f};
        float distance = -0.2f;
        List<int> PlayerCards = Player.GetComponent<Player>().PlayerCards;
        GameObject Card;
        int j = 0;
        Vector3 Position;
        Vector3 Euler;

        switch (positionOrder)
        {
          case 0:
          foreach(int CardID in PlayerCards)
          {
            Card = IDtoGameobeject(CardID);
            Position = Card.transform.localPosition;
            Euler = Card.transform.eulerAngles;
            if(Position!=new Vector3((float)(xcard[0]+1.6*j),ycard[0],distance * j))Card.transform.localPosition = new Vector3((float)(xcard[0]+1.6*j),ycard[0],distance * j);
            if(Euler!=new Vector3(0,0,0))Card.transform.eulerAngles = new Vector3(0,0,0);
            j++;
          }
          break;
          case 1:
          foreach(int CardID in PlayerCards)
          {
            Card = IDtoGameobeject(CardID);
            Position = Card.transform.localPosition;
            Euler = Card.transform.eulerAngles;
            if(Position!=new Vector3(xcard[1],(float)(ycard[1]-0.8*j),-distance * j))Card.transform.localPosition = new Vector3(xcard[1],(float)(ycard[1]-0.8*j),-distance * j);
            if(Euler!=new Vector3(0,180,360/4))Card.transform.eulerAngles = new Vector3(0,180,360/4);
            j++;
          }
          break;
          case 2:
          foreach(int CardID in PlayerCards)
          {
            Card = IDtoGameobeject(CardID);
            Position = Card.transform.localPosition;
            Euler = Card.transform.eulerAngles;
            if(Position!=new Vector3((float)(xcard[2]-0.8*j),ycard[2],-distance * j))Card.transform.localPosition = new Vector3((float)(xcard[2]-0.8*j),ycard[2],-distance * j);
            if(Euler!=new Vector3(0,180,360/2))Card.transform.eulerAngles = new Vector3(0,180,360/2);
            j++;
          }
          break;
          case 3:
          foreach(int CardID in PlayerCards)
          {
            Card = IDtoGameobeject(CardID);
            Position = Card.transform.localPosition;
            Euler = Card.transform.eulerAngles;
            if(Position!=new Vector3(xcard[3],(float)(ycard[3]+0.8*j),-distance * j))Card.transform.localPosition = new Vector3(xcard[3],(float)(ycard[3]+0.8*j),distance * -j);
            if(Euler!=new Vector3(0,180,360/4*3))Card.transform.eulerAngles = new Vector3(0,180,360/4*3);
            j++;
          }
          break;
      }
    }

    public Vector3 LastCardPosition(string playerName,GameObject Card) //在Player的add前面
    {
        GameObject Player = GameObject.Find(playerName);
        int positionOrder = Array.IndexOf(PlayerName,playerName);
        float[] xcard = {0,-20f,2f,20f};
        float[] ycard = {-10f,-1f,10f,1f};
        float distance = -0.2f;
        int Num = Player.GetComponent<Player>().RealCardNumber();
        Vector3 Position = Card.transform.localPosition;
        Vector3 Euler = Card.transform.eulerAngles;
        Vector3 NewPosition = new Vector3(0,0,0);

        switch (positionOrder)
        {
          case 0:
          NewPosition = new Vector3((float)(xcard[0]+1.6*Num),ycard[0],distance * Num);
          Card.transform.eulerAngles = new Vector3(0,0,0);
          break;
          case 1:
          NewPosition = new Vector3(xcard[1],(float)(ycard[1]-0.8*Num),-distance * Num);
          Card.transform.eulerAngles = new Vector3(0,180,360/4);
          break;
          case 2:
          NewPosition = new Vector3((float)(xcard[2]-0.8*Num),ycard[2],-distance * Num);
          Card.transform.eulerAngles = new Vector3(0,180,360/2);
          break;
          case 3:
          NewPosition = new Vector3(xcard[3],(float)(ycard[3]+0.8*Num),-distance * Num);
          Card.transform.eulerAngles = new Vector3(0,180,360/4*3);
          break;
        }

        return NewPosition;
    }

    public bool CompareCard(int GiveCardID) //比較兩張卡牌
    {
        GameObject lastcard = IDtoGameobeject(LastCardID);
        Color lastcardColor = lastcard.GetComponent<Card>().color;
        CardFunction lastcardFunction = lastcard.GetComponent<Card>().CardFunction;

        int[] cardIDmanager_GiveCard = CardIDManager(GiveCardID);
        Color givecardColor = cardColor[cardIDmanager_GiveCard[0]];
        CardFunction givecardFunction = cardFunctions[cardIDmanager_GiveCard[1]];

        if(lastcardColor == givecardColor || lastcardFunction == givecardFunction)
        {
            return true; //同色或同數字
        }
        else if(cardIDmanager_GiveCard[1] == 13 ||cardIDmanager_GiveCard[1] == 14)
        {
            return true; //黑色功能牌
        }
        else
        {
            return false;
        }
    }

    public void BanAction() //讓擁有禁止下一家動作的卡牌，下一回合不再發揮功能
    {
        GameObject card = IDtoGameobeject(LastCardID);
        card.GetComponent<Card>().cardState = CardState.Used;
    }

    public void WildCardHasColor(int colorID) //萬用卡在指定顏色後變色
    {
        GameObject card = IDtoGameobeject(LastCardID);
        int[] cardIDmanager = CardIDManager(LastCardID);
        int FunctionID = cardIDmanager[1];
        Color NewColor = cardColor[colorID];
        var children = card.GetComponentsInChildren<Transform>();
        if(FunctionID==13)
        {
            children[3].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/" + NewColor.ToString() + "Wild");
            card.GetComponent<Card>().cardState = CardState.Used;
        }
        else if(FunctionID==14)
        {
            children[3].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/" + NewColor.ToString() + "WildDrawFour");
        }
        card.GetComponent<Card>().color = NewColor;
    }

    public void AddColorSelector(int ColorID) //新增顏色選擇器
    {
        string ColorName = cardColor[ColorID].ToString();
        GameObject ColorSelector = Instantiate(Resources.Load<GameObject>("Prefabs/"+ColorName));
        ColorSelector.GetComponent<ColorSelector>().colorID = ColorID;
        ColorSelector.transform.localPosition = new Vector3(0,0,-0.1f);
        ColorSelector.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    public void DestroyColorSelector(int ColorID) //消滅顏色選擇器
    {
        string ColorName = cardColor[ColorID].ToString();
        GameObject ColorSelector = GameObject.Find(ColorName+"(Clone)");
        Destroy(ColorSelector);        
    }

}
