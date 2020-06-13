using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum CardType
{
    Monster,
    EquipItem,
    Item,
    Gold,
    Player,
    NONE
}

public class CardData
{
    public GameObject cardObject;
    public Card cardScript;
    public bool isOn = false;
}


public enum InputType
{
    Down,
    Hold,
    Up
}

public class CardManager : MonoBehaviour
{
    [SerializeField] GameObject card;

    [SerializeField] GameObject charHolder;
    [SerializeField] GameObject dunHolder;

    CardData[,] dunCards;

    CardData[] charCards;

    int charArraySize = 3;

    Vector2Int cardArraySize = new Vector2Int(3,3);

    Vector2 cardImageSize = new Vector2();
    Vector2 cardLength = new Vector2(10,10);

    public Card GetPlayerCard(int charNum)
    {
        return charCards[charNum].cardScript;
    }

    public void GenPlayerCard()
    {
        int hp = 0;
        for (int x = 0; x < charArraySize; x++)
        {
            CardData genCard = charCards[x];
            genCard.isOn = true;
            genCard.cardObject.SetActive(true);

            genCard.cardScript.info = CardDB.CreateCardInfo(CardDB.GetRandomCharCard(), Game.instance.turn);
            genCard.cardScript.cardNumber.x = x;
            genCard.cardScript.UpdateUI();
            hp += genCard.cardScript.info.hp;
        }
        Game.instance.maxHp = Game.instance.hp = hp;
    }

    bool AttackCardToCard(Card attacker, Card target)
    {
        Debug.Log("Attack Call" + attacker.name);
        if (target.info.hp <= 0)
        {
            return false;
        }
        attacker.info.currentAttackTurn = attacker.info.GetAttackTurn();
        target.Damage(attacker.info.GetPower());
        Debug.Log("Attack" + attacker.name + " -> " + target.name);

        attacker.UpdateUI();
        return true;
    }

    public void AttackCharCard(Card targetMonster)
    {
        for (int x = 0; x < charArraySize; x++)
        {
            CardData genCard = charCards[x];

            if (genCard.cardScript.info.currentAttackTurn == 0)
            {
                Game.instance.eventQueue.Enqueue(new EventData(genCard.cardScript, targetMonster, AttackCardToCard));
            }

   
        }
    }

    public void UpdateCard()
    {
        Game.instance.playerPower = 0;
        for (int x = 0; x < charArraySize; x++)
        {
            CardData genCard = charCards[x];
            genCard.cardScript.Updated = false;
            genCard.cardScript.CharUpdate();
            if (genCard.cardScript.info.currentAttackTurn == 0)
            {
                Game.instance.playerPower += genCard.cardScript.info.GetPower();
                
            }
            genCard.cardScript.UpdateUI();
        }

        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                CardData genCard = dunCards[y, x];
                genCard.cardScript.Updated = false;
            }
        }
    }

    int fieldMob = 0;
    int fieldEquip = 0;

    CardInfo GetRandomNewCard()
    {
        return CardDB.CreateCardInfo(CardDB.GetRandomDungeonCard(), Game.instance.turn);
    }

    public void RegenCard()
    {
        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                if(dunCards[y,x].isOn == false)
                {
                    CardData genCard = dunCards[y, x];
                    genCard.isOn = true;
                    genCard.cardScript.Make();

                    genCard.cardScript.info = GetRandomNewCard();

                    if(genCard.cardScript.info.type == CardType.Monster)
                    {
                        fieldMob += 1;
                    }
                    else if (genCard.cardScript.info.type == CardType.Gold)
                    {
                    } else if(genCard.cardScript.info.type == CardType.EquipItem)
                    {
                        fieldEquip += 1;
                    }
                    
                    genCard.cardScript.UpdateUI();
                }
            }
        }
    }

    public bool AttackPerformPlayer()
    {
        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                if (dunCards[y, x].isOn && dunCards[y, x].cardScript.Updated == false)
                {
                    CardData genCard = dunCards[y, x];
                    if (genCard.cardScript.Attack() == false) continue;
                    return true;
                }
            }
        }
        return false;
    }


    public bool AttackPerformMob()
    {
        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                if (dunCards[y, x].isOn && dunCards[y, x].cardScript.Updated == false)
                {
                    CardData genCard = dunCards[y, x];
                    if (genCard.cardScript.Attack() == false) continue;
                    return true;
                }
            }
        }
        return false;
    }


    public void RemoveCard(Vector2Int number)
    {
        if(dunCards[number.y, number.x].cardScript.info.type == CardType.Monster)
        {
            fieldMob -= 1;
        }
        if (dunCards[number.y, number.x].cardScript.info.type == CardType.EquipItem)
        {
            fieldEquip -= 1;
        }
        dunCards[number.y, number.x].isOn = false;
        dunCards[number.y, number.x].cardScript.remove();
    }

    public Card GetDungeonCard(Vector2Int number)
    {
        return dunCards[number.y, number.x].cardScript;
    }

    void Start()
    {
        RectTransform rt = card.GetComponent<RectTransform>();
        cardImageSize.x = rt.rect.width + cardLength.x;
        cardImageSize.y = rt.rect.height + cardLength.y;
        //char make

        charCards = new CardData[charArraySize];
        for (int i = 0; i < charArraySize; i++)
        {
            //create card
            charCards[i] = new CardData();

            charCards[i].cardObject = Instantiate(card, charHolder.transform);
            charCards[i].cardScript = charCards[i].cardObject.GetComponent<Card>();

            charCards[i].cardObject.name = "CharCard " + (i).ToString();

            //position set

            Vector3 pos = new Vector3(
                (-cardImageSize.x) + (i * cardImageSize.x)
                , 0
                , 0);

            charCards[i].cardObject.transform.localPosition = pos;
        }

        //dungeon make

        dunCards = new CardData[cardArraySize.y, cardArraySize.x];
        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                //create card
                dunCards[y,x] = new CardData();

                dunCards[y, x].cardObject = Instantiate(card, dunHolder.transform);
                dunCards[y, x].cardScript = dunCards[y, x].cardObject.GetComponent<Card>();
                dunCards[y, x].cardScript.cardNumber = new Vector2Int(x, y);

                dunCards[y, x].cardObject.name = "DunCard " + (y * cardArraySize.x + x).ToString();

                //position set

                Vector3 pos = new Vector3(
                    (-cardImageSize.x) + (x * cardImageSize.x)
                    , (-cardImageSize.y) + (y * cardImageSize.y)
                    , 0);

                dunCards[y, x].cardObject.transform.localPosition = pos;
            }
        }
    }

    private void Update()
    {
        //InputCheck();
    }

    public void InputCheck()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            PerformInput(InputType.Down, Input.mousePosition);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            PerformInput(InputType.Hold, Input.mousePosition);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            PerformInput(InputType.Up, Input.mousePosition);
        }
#endif
#if UNITY_ANDROID
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            PerformInput(InputType.Hold, touch.position);
        }
        
#endif
    }



    public void PerformInput(InputType type, Vector3 position)
    {
        for (int y = 0; y < cardArraySize.y; y++)
        {
            for (int x = 0; x < cardArraySize.x; x++)
            {
                Vector3 realPos = Camera.main.ScreenToWorldPoint(position);

                Vector3 cardPos = dunCards[y, x].cardObject.transform.position;

                Vector2 checkSize = new Vector2(cardImageSize.x * 0.5f, cardImageSize.y * 0.5f);

                if (realPos.x >= cardPos.x - checkSize.x
                && realPos.x <= cardPos.x + checkSize.x
                && realPos.y >= cardPos.y - checkSize.y
                && realPos.y <= cardPos.y + checkSize.y)
                {
                    Debug.Log("Click" + y + "," + x);
                }

            }
        }
       
        
        
    }

}
