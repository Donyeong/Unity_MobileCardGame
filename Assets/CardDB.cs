using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityGoogleDrive;

public struct CardDbData
{
    public int type;
    public int hp;
    public int price;
    public int power;
    public int attack_turn;
    public int default_equip;
    public int remove_card_id;
    public int regen_chance;
    public string name;
    public string sprite_name;

    public float turn_hp;
    public float turn_price;
    public float turn_power;

    public int start_turn;
    public int end_turn;
}

public class CardInfo
{
    public string name;
    public Sprite sprite;
    public CardType type;
    public int hp;
    public int amount;
    public int power;
    public int price;
    public int attackTurn;
    public int removeCardID;
    public int chance;

    public CardInfo equipItem;

    public int currentAttackTurn;

    public void Init()
    {
        currentAttackTurn = 0;
    }

    public CardInfo()
    {
        equipItem = null;

        name = null;
        sprite = null;

        type = CardType.NONE;

        hp = 1;
        amount = 0;
        power = 0;

        power = 1;
        price = 0;
        attackTurn = 0;

        currentAttackTurn = 0;

        removeCardID = -1;

        chance = 1;
    }

    public CardInfo(CardDbData src, Sprite srcSprite)
    {
        name = src.name;
        sprite = srcSprite;
        type = (CardType)src.type;
        hp = src.hp;
        amount = src.power;
        power = src.power;

        price = src.price;
        attackTurn = src.attack_turn;
    }

    public int GetAttackTurn()
    {
        int calcTurn = attackTurn;

        if (equipItem != null)
        {
            calcTurn += equipItem.GetAttackTurn();
        }

        return calcTurn;
    }

    public int GetPower()
    {
        int calcPower = power;

        if(equipItem != null)
        {
            calcPower = equipItem.GetPower();
        }

        return calcPower;
    }
}

struct CardChanceData {
    public int id;
    public int startChanceNumber;
}

struct CardStartGenData
{
    public int id;
    public int startTurn;
}

public class CardDB
{



    static CardDB instance = new CardDB();

    CardDbData[] cardTableDatas;
    Sprite[] cardSprite;


    public static void UpdateTurn(int turn)
    {
        while (instance.cardStartGenQueue.Count > 0 && instance.cardStartGenQueue.Peek().startTurn <= turn)
        {
            CardStartGenData genCard = instance.cardStartGenQueue.Dequeue();
            CardChanceData ccd;
            ccd.id = genCard.id;
            ccd.startChanceNumber = instance.dungeonChanceMax;
            instance.DungeonCardPool.Add(ccd);
            instance.dungeonChanceMax += instance.cardTableDatas[genCard.id].regen_chance;
            Debug.Log(instance.cardTableDatas[genCard.id].name + "리젠 시작" + ccd.startChanceNumber);
        }
    }

    public static CardInfo CreateCardInfo(int id, int turn)
    {
        CardInfo card = new CardInfo(instance.cardTableDatas[id], instance.cardSprite[id]);
        card.Init();
        card.power += Mathf.RoundToInt( turn * instance.cardTableDatas[id].turn_power);
        card.hp += Mathf.RoundToInt(turn * instance.cardTableDatas[id].turn_hp);
        card.price += Mathf.RoundToInt(turn * instance.cardTableDatas[id].turn_price);
        return card;
    }

    public static CardDbData GetInfoDB(int id)
    {
        return instance.cardTableDatas[id];
    }

    List<CardChanceData> DungeonCardPool = new List<CardChanceData>(64);

    int dungeonChanceMax = 0;

    List<CardChanceData> CharCardPool = new List<CardChanceData>(8);
    int charChanceMax = 0;

    Queue<CardStartGenData> cardStartGenQueue = new Queue<CardStartGenData>();

    public static int GetRandomCharCard()
    {
        int ret = -1;

        int randomNumber = UnityEngine.Random.Range(0, instance.charChanceMax);

        int min = 0;
        int max = instance.CharCardPool.Count;

        int i = (min + max) / 2;

        int co = 00;
        while (co < 100)
        {
            if (i == instance.CharCardPool.Count - 1)
            {
                ret = i;
                break;
            }
            if (instance.CharCardPool[i].startChanceNumber <= randomNumber && randomNumber < instance.CharCardPool[i + 1].startChanceNumber)
            {
                ret = i;
                break;
            }

            if (randomNumber < instance.CharCardPool[i].startChanceNumber)
            {
                max = i;
            }
            else
            {
                min = i;
            }
            i = (min + max) / 2;
            co++;
        }
        if (co >= 100)
        {
            Debug.LogError("카드 난수생성 무한루프");
        }

        return instance.CharCardPool[ret].id;
    }

    public static int GetRandomDungeonCard()
    {
        int ret = -1;

        int randomNumber = UnityEngine.Random.Range(0, instance.dungeonChanceMax);

        int min = 0;
        int max = instance.DungeonCardPool.Count;

        int i = (min + max)/2;

        int co = 00;
        while (co < 100)
        {
            if (i == instance.DungeonCardPool.Count - 1)
            {
                ret = i;
                break;
            }
            if (instance.DungeonCardPool[i].startChanceNumber <= randomNumber && randomNumber < instance.DungeonCardPool[i + 1].startChanceNumber)
            {
                ret = i;
                break;
            }

            if (randomNumber < instance.DungeonCardPool[i].startChanceNumber)
            {
                max = i;
            }
            else
            {
                min = i;
            }
            i = (min + max) / 2;
            co++;
        }
        if (co >= 100)
        {
            Debug.LogError("카드 난수생성 무한루프");
        }
        return instance.DungeonCardPool[ret].id;
    }

    private CardDB()
    {
        //TextAsset data = Resources.Load("DataSheet") as TextAsset;


       // string path = Application.dataPath + "\\StreamingAssets" + "\\" + "GameData.carddata";

        string path = string.Empty;

#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)

        path = Application.dataPath + "/StreamingAssets";

#elif UNITY_ANDROID

            path = "jar:file://" + Application.dataPath + "!/assets";

#elif UNITY_IOS

           path = Application.dataPath + "/Raw";

#endif
        path += "/GameData.carddata";

        WWW wwwfile = new WWW(path);
        while (!wwwfile.isDone) { }

        MemoryStream memoryStream = new MemoryStream(wwwfile.bytes);
        //FileStream readStream = new FileStream(path, FileMode.Open, FileAccess.Read);

        GZipStream gZipReader = new GZipStream(memoryStream, CompressionMode.Decompress);

        ByteBuffer readBuffer = new ByteBuffer();
        byte[] arr = new byte[4];

        gZipReader.Read(arr, 0, 4);

        int bufSize = BitConverter.ToInt32(arr, 0);
        arr = new byte[bufSize];

        gZipReader.Read(arr, 0, bufSize);

        readBuffer.WriteBytes(arr);


        int len = readBuffer.ReadInteger();
        cardSprite = new Sprite[len];
        cardTableDatas = new CardDbData[len];
        for (int i = 0; i < len; i++)
        {
            cardTableDatas[i].name = readBuffer.ReadString();
            cardTableDatas[i].sprite_name = readBuffer.ReadString();
            cardTableDatas[i].type = readBuffer.ReadInteger();
            cardTableDatas[i].hp = readBuffer.ReadInteger();
            cardTableDatas[i].price = readBuffer.ReadInteger();
            cardTableDatas[i].power = readBuffer.ReadInteger();
            cardTableDatas[i].attack_turn = readBuffer.ReadInteger();
            cardTableDatas[i].default_equip = readBuffer.ReadInteger();
            cardTableDatas[i].remove_card_id = readBuffer.ReadInteger();
            cardTableDatas[i].regen_chance = readBuffer.ReadInteger();

            cardTableDatas[i].turn_hp = readBuffer.ReadFloat();
            cardTableDatas[i].turn_power = readBuffer.ReadFloat();
            cardTableDatas[i].turn_price = readBuffer.ReadFloat();

            cardTableDatas[i].start_turn = readBuffer.ReadInteger();
            cardTableDatas[i].end_turn = readBuffer.ReadInteger();
        }
        gZipReader.Close();
        memoryStream.Close();

        int maxCard = len;
        /*
        cardDatas = new CardInfo[maxCard];
        for (int i = 0; i < maxCard; i++)
        {
            cardDatas[i] = new CardInfo();
        }
        */


        /*
        int id;

        id = 0;
        cardDatas[id].name = "잡몹임";
        cardDatas[id].sprite = Resources.Load<Sprite>("dot2");
        cardDatas[id].type = CardType.Monster;
        cardDatas[id].hp = 2;
        cardDatas[id].power = 1;

        id = 1;
        cardDatas[id].name = "약한검";
        cardDatas[id].sprite = Resources.Load<Sprite>("Sword1");
        cardDatas[id].type = CardType.EquipItem;
        cardDatas[id].price = 1;
        cardDatas[id].power = 5;
        cardDatas[id].chance = 100;

        id = 2;
        cardDatas[id].name = "돈";
        cardDatas[id].sprite = Resources.Load<Sprite>("Coin1");
        cardDatas[id].type = CardType.Gold;
        cardDatas[id].price = 2;

        id = 3;
        cardDatas[id].name = "물약임";
        cardDatas[id].sprite = Resources.Load<Sprite>("mob1");
        cardDatas[id].type = CardType.Item;
        cardDatas[id].amount = 8;

        id = 4;
        cardDatas[id].name = "쎈몹임";
        cardDatas[id].sprite = Resources.Load<Sprite>("mouse");
        cardDatas[id].type = CardType.Monster;
        cardDatas[id].hp = 5;
        cardDatas[id].power = 2;

        id = 5;
        cardDatas[id].name = "강한검";
        cardDatas[id].sprite = Resources.Load<Sprite>("Sword1");
        cardDatas[id].type = CardType.EquipItem;
        cardDatas[id].price = 10;
        cardDatas[id].power = 8;

        id = 6;
        cardDatas[id].name = "캐릭임";
        cardDatas[id].sprite = Resources.Load<Sprite>("mob2");
        cardDatas[id].type = CardType.Player;
        cardDatas[id].hp = 1;
        cardDatas[id].attackTurn = 3;
        cardDatas[id].power = 2;

    */
        List<CardStartGenData> genDataList = new List<CardStartGenData>(maxCard);

        

        for (int i = 0; i < maxCard; i++)
        {
            CardChanceData ccd;
            ccd.id = i;

            if ((CardType)cardTableDatas[i].type == CardType.Player)
            {
                ccd.startChanceNumber = charChanceMax;
                CharCardPool.Add(ccd);
                charChanceMax += cardTableDatas[i].regen_chance;

            }
            else
            {
                if (cardTableDatas[i].start_turn == 0)
                {
                    ccd.startChanceNumber = dungeonChanceMax;
                    DungeonCardPool.Add(ccd);
                    dungeonChanceMax += cardTableDatas[i].regen_chance;
                }
                else
                {
                    CardStartGenData gd;
                    gd.id = i;
                    gd.startTurn = cardTableDatas[i].start_turn;
                    genDataList.Add(gd);
                }
            }
            cardSprite[i] = Resources.Load<Sprite>(cardTableDatas[i].sprite_name);
            if(cardSprite[i] == null)
            {
                Debug.LogError("스프라이트 리소스 로드 오류 : " + cardTableDatas[i].sprite_name);
            }
        }

        genDataList.Sort(delegate (CardStartGenData A, CardStartGenData B)
        {
            if (A.startTurn > B.startTurn) return 1;
            else return -1;
        });

        for (int i = 0; i < genDataList.Count; i++)
        {
            cardStartGenQueue.Enqueue(genDataList[i]);
        }

        genDataList.Clear();
    }


}
