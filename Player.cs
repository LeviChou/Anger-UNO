using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameManager gameManager;
    private TurnControl turnScript;
    private SoundEffectManager SoundEffect;
    [Header("Player's Cards")]
    public List<int> PlayerCards;
    public int order;
    private bool UNOIsReady;
    private bool ableAttack;
    private bool JustAttack;
    private bool HasBeenPunished = false;
    public int Energy = 0;

    // Start is called before the first frame update
    void Start()
    {
        SoundEffect = GameObject.Find("SoundEffectManager(Clone)").GetComponent<SoundEffectManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        turnScript = GameObject.Find("TurnControl").GetComponent<TurnControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if(turnScript.PlayerAbleToPlay[order-1] && turnScript.currentState == GameState.Game) //只有遊戲進行中，且輪到你的回合時才可動作
        {
            this.GetComponent<Light>().enabled = true; //亮起光圈表示自己的回合
            JustAttack = false;
            Debug.Log("It's "+name +" turn!");
            if(CheckBan()) //先確定上一家是否禁止自己出牌
            {
                Debug.Log(name+" was banned!");
                gameManager.BanAction();
                DrawCardAction();
                HasBeenPunished = true;
            }
            else
            {
                if(AbleForThisRound()) //確認這回合可以出牌
                {
                    if(order==1)
                    {
                        UNOBottomAppear();
                        AttackBottomAppear();
                        gameManager.YouPlayCard = true; //你可以出牌
                    }
                    else
                    {
                        if(Energy >= 5)
                        {
                            int dice = UnityEngine.Random.Range(0,20);
                            if(dice > 20 - Energy)
                            {
                                Attack();
                            }
                            else
                            {
                                ComputerPlayCard();
                            }
                        }
                        else
                        {
                            ComputerPlayCard();
                        }
                    }
                }
                else
                {
                    DrawCard(); //如果這一回合無法出牌，則抽一張牌
                    if(AbleForThisRound()) //如果抽到的牌可以出，就會自動幫忙出牌
                    {
                        if(order==1)
                        {
                            UNOBottomAppear();
                            gameManager.YouPlayCard = true; //你可以出牌
                        }
                        else
                        {
                            StartCoroutine(PlayTheOnlyCard());
                        }
                    }
                }

                
            }
            StartCoroutine(WaitForNext());

            turnScript.PlayerAbleToPlay[order-1] = false;
        }
    }

    void UNOBottomAppear()
    {
        UNOIsReady = false;
        if(PlayerCards.Count == 2)
        {
            GameObject button = Instantiate(Resources.Load<GameObject>("Prefabs/ButtonForUNO"));
            button.transform.localPosition = new Vector3(10f,-9f,0);
        }   
    }

    public bool AbleAttack()
    {
        return ableAttack;
    }

    void AttackBottomAppear()
    {
        if(Energy >= 5)
        {
            ableAttack = true;
            GameObject button = Instantiate(Resources.Load<GameObject>("Prefabs/Attack"));
            button.transform.localPosition = new Vector3(-10f,-4f,0);
        }
    }

    void CancelAttack()
    {
        if(Energy >= 5)
        {
            ableAttack = false;
            StartCoroutine(WaitForDestroy("Attack(Clone)"));
        }
    }

    public void PlayerPlayUNO()
    {
        SoundEffect.UNOVoice();
        UNOIsReady = true;
        float[] NameToPosition = gameManager.Name_To_Position(name);
        float Xpos = NameToPosition[0];
        float Ypos = NameToPosition[1];
        GameObject UNO = Instantiate(Resources.Load<GameObject>("Prefabs/UNO"));
        UNO.transform.localPosition = new Vector3(Xpos,Ypos,-2f);
        StartCoroutine(WaitForDestroy("UNO(Clone)"));
        StartCoroutine(WaitForDestroy("ButtonForUNO(Clone)"));
    }

    void PlayerCheckUNO()
    {
        if(PlayerCards.Count == 1)
        {
            Debug.Log(UNOIsReady);
            if(!UNOIsReady && !JustAttack)
            {
                float[] NameToPosition = gameManager.Name_To_Position(name);
                float Xpos = NameToPosition[0];
                float Ypos = NameToPosition[1];
                GameObject UNO = Instantiate(Resources.Load<GameObject>("Prefabs/NoUNO"));
                UNO.transform.localPosition = new Vector3(Xpos,Ypos,-2f);
                StartCoroutine(WaitForDestroy("NoUNO(Clone)"));
                StartCoroutine(WaitForDestroy("ButtonForUNO(Clone)"));
                SoundEffect.ForgetUNOVoice();

                for(int i=1;i<=2;i++)
                {
                    DrawCard();
                }
            }
        }
    }

    IEnumerator WaitForNext()
    {
        yield return new WaitWhile(()=>{return gameManager.YouPlayCard;}); //直到你出完牌前，底下的程式都不會進行

        gameManager.CardPositionManager(this.name); //幫忙整理手牌
        yield return new WaitForSeconds(0.75f);
        
        //確認是否打出黑色功能牌
        if(order == 1)
        {
            PlayerPlayWild();
        }
        else
        {
            ComputerPlayWild();
        }

        CheckReverse();
        CheckWin();
        
        yield return new WaitWhile(()=>{return gameManager.YouChooseColor;}); //如果打出功能牌，直到你選完顏色，底下的程式都不會進行
        CheckReshuffle();
        yield return new WaitForSeconds(0.375f);
        if(order == 1)PlayerCheckUNO();
        if(order!=1)ComputerCheckUNO();
        yield return new WaitForSeconds(0.375f);
        this.GetComponent<Light>().enabled = false; //輪到別人的回合前，把自己的光圈關掉
        
        //輪到下一家出牌
        if(order == 4)
        {
            turnScript.PlayerAbleToPlay[0] = true;
        }
        else
        {
            turnScript.PlayerAbleToPlay[order] = true;
        }
    }

    private bool AbleForThisRound() //確認玩家這回合能不能出牌
    {
        bool able;
        bool result = false;
        int CardNum = RealCardNumber();
        int CardID;

        for(int i=0;i<CardNum;i++)
        {
            CardID = PlayerCards[i];
            able = gameManager.CompareCard(CardID);
            if(able) //假設有一張牌能出
            {
                result = true;
                break;
            }
        }
        return result;
    }

    private bool CheckBan() //確認上一家是否禁止自己出牌
    {
        bool result = false;
        GameObject LastCard = gameManager.IDtoGameobeject(gameManager.LastCardID);
        CardState cardState = LastCard.GetComponent<Card>().cardState;
        CardFunction cardFunction = LastCard.GetComponent<Card>().CardFunction;

        if(cardState == CardState.Played) //先確認功能牌尚未發揮作用
        {
            if((cardFunction == CardFunction.DrawTwo ||cardFunction == CardFunction.Skip) || cardFunction == CardFunction.WildDrawFour)
            {
                gameManager.BanAction();

                float[] NameToPosition = gameManager.Name_To_Position(name);
                float Xpos = NameToPosition[0];
                float Ypos = NameToPosition[1];
                GameObject Stop = Instantiate(Resources.Load<GameObject>("Prefabs/Stop"));
                Stop.transform.localPosition = new Vector3(Xpos,Ypos,-2f);
                StartCoroutine(WaitForDestroy("Stop(Clone)"));

                switch(cardFunction)
                {
                    case CardFunction.DrawTwo:
                        SoundEffect.PlusTwoVoice();
                        EnergyManager(2);
                        break;
                    case CardFunction.WildDrawFour:
                        SoundEffect.PlusFourVoice();
                        EnergyManager(4);
                        break;
                    case CardFunction.Skip:
                        SoundEffect.StopVoice();
                        EnergyManager(1);
                        break;
                }

                result = true;
            }
        }
        return result;
    }

    private void DrawCardAction() //依照功能牌，進行抽卡
    {
        GameObject LastCard = gameManager.IDtoGameobeject(gameManager.LastCardID);
        CardFunction cardFunction = LastCard.GetComponent<Card>().CardFunction;

        if(cardFunction == CardFunction.DrawTwo)
        {
            for(int i=1;i<=2;i++)
            {
                DrawCard();
            }
        }
        else if(cardFunction == CardFunction.WildDrawFour)
        {
            for(int i=1;i<=4;i++)
            {
                DrawCard();
            }
        }
    }

    public void PlayerPlayCard(int ID) //人類玩家出牌
    {
        CancelAttack();
        bool able = gameManager.CompareCard(ID); //先確認這張牌能被打出去
        if(able)
        {
            PlayerCards.Remove(ID);
            gameManager.YouPlayCard = false;
            gameManager.PlayCard(ID);
            CheckAddEnergy(gameManager.LastCardID);
        }
    }

    public void ComputerPlayCard() //電腦玩家出牌
    {        
        int CardNum = RealCardNumber();
        int[] CardAbleToPlay = new int[CardNum]; //用Array紀錄能被打出的牌
        int j = 0;
        bool able;
        for(int i=0;i<CardNum;i++)
        {
            able = gameManager.CompareCard(PlayerCards[i]);
            if(able)
            {
                CardAbleToPlay[j] = PlayerCards[i];                
                j = j + 1;
            }
        }
        int SelectNum = UnityEngine.Random.Range(0,j);
        PlayerCards.Remove(CardAbleToPlay[SelectNum]);
        gameManager.PlayCard(CardAbleToPlay[SelectNum]);
        CheckAddEnergy(gameManager.LastCardID);
    }

    IEnumerator PlayTheOnlyCard() //在抽牌時抽到可出的牌，則會自動出牌
    {
        yield return new WaitForSeconds(0.5f);
        int CardNum = RealCardNumber();
        int CardID;
        bool able;
        for(int i=0;i<CardNum;i++)
        {
            CardID = PlayerCards[i];
            able = gameManager.CompareCard(CardID);
            if(able)
            {
                PlayerCards.Remove(CardID);
                gameManager.PlayCard(CardID);
                CheckAddEnergy(gameManager.LastCardID);
                break;
            }
        }
    }

    private void PlayerPlayWild()  //確認玩家是否出了黑色功能牌
    {
        GameObject LastCard = gameManager.IDtoGameobeject(gameManager.LastCardID);
        CardState cardState = LastCard.GetComponent<Card>().cardState;
        CardFunction cardFunction = LastCard.GetComponent<Card>().CardFunction;
        if(cardState == CardState.Played)
        {
            if(cardFunction == CardFunction.Wild || cardFunction == CardFunction.WildDrawFour)
            {
                gameManager.YouChooseColor = true;
                GameObject WildCard = Instantiate(Resources.Load<GameObject>("Prefabs/WildCard"));
                Destroy(GameObject.Find("CoverUp(Clone)"));
                WildCard.transform.localPosition = new Vector3(0,0,0);
                WildCard.GetComponent<SpriteRenderer>().sortingOrder = 1;

                for(int i=0;i<4;i++)
                {  
                    gameManager.AddColorSelector(i);
                }
            }
        }
    }

    public void PlayerSelectColor(int colorID) //玩家出了黑色功能牌後選顏色
    {
        gameManager.WildCardHasColor(colorID);
        var WildCardExist = GameObject.Find("WildCard(Clone)");
        if(WildCardExist)
        {
            GameObject CoverUp = Instantiate(Resources.Load<GameObject>("Prefabs/CoverUp"));
            CoverUp.transform.localPosition = new Vector3(3f,0,0);
            Destroy(GameObject.Find("WildCard(Clone)"));
            for(int i=0;i<4;i++)
            {
                gameManager.DestroyColorSelector(i);
            }
        }
        gameManager.YouChooseColor = false;
    }

    private void ComputerPlayWild() //幫助電腦玩家在出黑色功能牌後選取顏色
    {
        GameObject LastCard = gameManager.IDtoGameobeject(gameManager.LastCardID);
        CardState cardState = LastCard.GetComponent<Card>().cardState;
        CardFunction cardFunction = LastCard.GetComponent<Card>().CardFunction;
        int[] LastCardManager = gameManager.CardIDManager(gameManager.LastCardID);
        int LastCardColor = LastCardManager[0];
        List<int> AvaliableColor = new List<int>();

        if(cardState == CardState.Played)
        {
            if(cardFunction == CardFunction.Wild || cardFunction == CardFunction.WildDrawFour)
            {
                if(PlayerCards.Count!=0)
                {
                    
                    for(int i = 0;i<PlayerCards.Count;i++)
                    {
                        int[] CardCard = gameManager.CardIDManager(PlayerCards[i]);
                        int CardCardColor = CardCard[0];
                        if(CardCardColor != 4 && CardCardColor != LastCardColor)
                        {
                            AvaliableColor.Add(CardCardColor);
                        }
                    }
                    int colorID = UnityEngine.Random.Range(0,AvaliableColor.Count);
                    gameManager.WildCardHasColor(AvaliableColor[colorID]);
                    AvaliableColor.Clear();
                }
                else
                {
                    int colorID = UnityEngine.Random.Range(0,4);
                    gameManager.WildCardHasColor(colorID);
                }
                
            }
        }
    }

    private void CheckReverse() //確認是否打出迴轉
    {
        GameObject LastCard = gameManager.IDtoGameobeject(gameManager.LastCardID);
        CardState cardState = LastCard.GetComponent<Card>().cardState;
        CardFunction cardFunction = LastCard.GetComponent<Card>().CardFunction;

        if(cardFunction == CardFunction.Reverse) //先確認功能牌尚未發揮作用
        {
            if(cardState == CardState.Played)
            {
                Debug.Log("Reverse");
                turnScript.ChangeOrder(order);
                LastCard.GetComponent<Card>().cardState = CardState.Used;
                GameObject Rotation = Instantiate(Resources.Load<GameObject>("Prefabs/Rotation"));
                Rotation.transform.localPosition = new Vector3(0,0,-8f);
                StartCoroutine(WaitForDestroy("Rotation(Clone)"));
                SoundEffect.ReverseVoice();
            }
        }
    }

    private void ComputerCheckUNO() //確認是否喊UNO
    {
        if(PlayerCards.Count == 1 && (HasBeenPunished == false && !JustAttack))
        {
            float[] NameToPosition = gameManager.Name_To_Position(name);
            float Xpos = NameToPosition[0];
            float Ypos = NameToPosition[1];
            GameObject UNO = Instantiate(Resources.Load<GameObject>("Prefabs/UNO"));
            UNO.transform.localPosition = new Vector3(Xpos,Ypos,-2f);
            StartCoroutine(WaitForDestroy("UNO(Clone)"));
            SoundEffect.UNOVoice();
        }
        HasBeenPunished = false;
    }

    private void CheckWin() //確認玩家是否手中沒有手牌
    {
        if(RealCardNumber()==0)
        {
            
            float[] NameToPosition = gameManager.Name_To_Position(name);
            float Xpos = NameToPosition[0];
            float Ypos = NameToPosition[1];
            GameObject Winner = Instantiate(Resources.Load<GameObject>("Prefabs/Winner"));
            Winner.transform.localPosition = new Vector3(Xpos,Ypos,-2f);
            SoundEffect.WinVoice();

            turnScript.currentState = GameState.Over;
        }
    }

    void CheckAddEnergy(int cardID)
    {
        int[] CardFeature = gameManager.CardIDManager(cardID);
        int CardFunction = CardFeature[1];
        if(CardFunction == 10 || CardFunction == 12)
        {
            EnergyManager(1);
        }
        else if(CardFunction == 11)
        {
            EnergyManager(2);
        }
    }

    private void CheckReshuffle()
    {
        if(gameManager.DeckTopOrder >= gameManager.MaxDeckCard - 14)
        {
            gameManager.Reshuffle();
        }
    }

    public int RealCardNumber() //確認玩家手上的總牌數
    {
        return PlayerCards.Count;
    }

    private void DrawCard() //玩家抽牌
    {
        SoundEffect.PlayCardVoice();
        int NewCardID = gameManager.Order_To_ID(gameManager.DeckTopOrder);
        GameObject NewCard = gameManager.IDtoGameobeject(NewCardID);

        Vector3 NewPosition = gameManager.LastCardPosition(name,NewCard);
        StartCoroutine(DrawCardAnimation(NewCard,NewPosition));

        PlayerCards.Add(NewCardID);
        gameManager.DeckTopOrder++;

        if(order==1)
        {
            NewCard.GetComponent<Card>().cardState = CardState.YourCard;
        }
        else
        {
            NewCard.GetComponent<Card>().cardState = CardState.OthersCard;
        }
    }

    public void PlayerPlayAttack()
    {
        Attack();
        ableAttack = false;
        gameManager.YouPlayCard = false;
        StartCoroutine(WaitForDestroy("Attack(Clone)"));

        GameObject UNObuttom = GameObject.Find("ButtonForUNO(Clone)");
        if(UNObuttom!=null)
        {
            StartCoroutine(WaitForDestroy("ButtonForUNO(Clone)"));
        }
    }

    public void Attack()
    {
        if(Energy == 10)
        {
            ChidoriAttack();
            SoundEffect.ChidoriVoice();
            string[] OtherPlayer = gameManager.OtherPlayer(name);
            GameObject Opposite;
            for(int i = 0;i < 3;i++)
            {
                Opposite = GameObject.Find(OtherPlayer[i]);
                for(int j = 0; j < 2; j++)
                {
                    Opposite.GetComponent<Player>().DrawCard();
                }
            }
            Energy = 0;
            JustAttack = true;
        }
        else
        {
            GameObject Rasengan = Instantiate(Resources.Load<GameObject>("Prefabs/rasengan"));
            StartCoroutine(RasenganAttack());
            SoundEffect.RasenganVoice();
            string Opposite_Name = gameManager.Name_to_opposite(name);
            GameObject Opposite = GameObject.Find(Opposite_Name);
            Opposite.GetComponent<Player>().DrawCard();
            Energy = Energy - 5;
            JustAttack = true;
        }
        var children = this.GetComponentsInChildren<Transform>();
        string EnergyName = Energy.ToString();
        children[2].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/energy"+EnergyName);
        
    }

    void ChidoriAttack()
    {
        string[] OtherPlayer = gameManager.OtherPlayer(name);

        float[] PlayerOne_pos = gameManager.Name_To_Position(OtherPlayer[0]);
        GameObject chidoriOne = Instantiate(Resources.Load<GameObject>("Prefabs/chidori"));
        chidoriOne.transform.localPosition = new Vector3(PlayerOne_pos[0],PlayerOne_pos[1],0);

        float[] PlayerTwo_pos = gameManager.Name_To_Position(OtherPlayer[1]);
        GameObject chidoriTwo = Instantiate(Resources.Load<GameObject>("Prefabs/chidori"));
        chidoriTwo.transform.localPosition = new Vector3(PlayerTwo_pos[0],PlayerTwo_pos[1],0);

        float[] PlayerThree_pos = gameManager.Name_To_Position(OtherPlayer[2]);
        GameObject chidoriThree = Instantiate(Resources.Load<GameObject>("Prefabs/chidori"));
        chidoriThree.transform.localPosition = new Vector3(PlayerThree_pos[0],PlayerThree_pos[1],0);

        StartCoroutine(WaitForMultiDestroy("chidori"));
    }

    IEnumerator RasenganAttack()
    {
        GameObject Rasengan = GameObject.Find("rasengan(Clone)");
        float portion = 10f;
        float[] InitialPosition = gameManager.Name_To_Position(name);
        string Opposite = gameManager.Name_to_opposite(name);
        float[] FinalPosition = gameManager.Name_To_Position(Opposite);

        int PositionOrder = gameManager.Name_To_Position_Order(name);
        if(PositionOrder == 0)
        {
            Rasengan.transform.eulerAngles = new Vector3(0,0,-45);
        }
        else if(PositionOrder == 1)
        {
            Rasengan.transform.eulerAngles = new Vector3(0,0,-135);
        }
        else if(PositionOrder == 2)
        {
            Rasengan.transform.eulerAngles = new Vector3(0,0,-225);
        }
        else if(PositionOrder == 3)
        {
            Rasengan.transform.eulerAngles = new Vector3(0,0,-315);
        }

        float xDisplacement = -(InitialPosition[0] - FinalPosition[0])/portion;
        float yDisplacement = -(InitialPosition[1] - FinalPosition[1])/portion;

        for(int i=1;i<=portion;i++)
        {
            Rasengan.transform.localPosition = new Vector3(InitialPosition[0] + i*xDisplacement,InitialPosition[1] + i*yDisplacement,0);
            yield return new WaitForSeconds(0.03f);
            Debug.Log(Rasengan.transform.localPosition);
        }

        Rasengan.GetComponent<Rasengan>().Explode();
        StartCoroutine(WaitForQuickDestroy("rasengan(Clone)"));
    }

    IEnumerator DrawCardAnimation(GameObject Card,Vector3 FinalPosition)
    {
        float portion = 10f;
        Vector3 OriginPosition = Card.transform.localPosition;
        
        int Num = gameManager.NumOfPlayedCard();
        float xDisplacement = -(OriginPosition.x - FinalPosition.x)/portion;
        float yDisplacement = -(OriginPosition.y - FinalPosition.y)/portion;
        float zDisplacement = -(OriginPosition.z - FinalPosition.z)/portion;
        for(int i=1;i<=portion;i++)
        {
            Card.transform.localPosition = new Vector3(OriginPosition.x + i*xDisplacement,OriginPosition.y + i*yDisplacement,OriginPosition.z + i*zDisplacement);
            yield return new WaitForSeconds(0.01f);
        } 
    }

    void EnergyManager(int deltaEnergy)
    {
        if(Energy + deltaEnergy > 10)
        {
            Energy = 10;
        }
        else
        {
            Energy = Energy + deltaEnergy;
        }
        var children = this.GetComponentsInChildren<Transform>();
        string EnergyName = Energy.ToString();
        children[2].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("graphic/energy"+EnergyName);
    }

    IEnumerator WaitForDestroy(string Name) //能使物件過一段時間才被消滅
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(GameObject.Find(Name));
    }


    IEnumerator WaitForQuickDestroy(string Name) //能使物件過一段時間才被消滅
    {
        yield return new WaitForSeconds(0.4f);
        Destroy(GameObject.Find(Name));
    }

    IEnumerator WaitForMultiDestroy(string Tag)
    {
        yield return new WaitForSeconds(0.7f);
        GameObject[] chidoris;
        chidoris = GameObject.FindGameObjectsWithTag(Tag);
        foreach(GameObject chidori in chidoris)
        {
        Destroy(chidori);
        }
    }
}
