using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public enum GameState
{
    Lobby,
    Init,
    StartTurn,
    RegenWait,
    Idle,
    EquipSelect,
    EventProc,
    TurnEndProc,
    MonsterProc,
}

public delegate bool CardEvent(Card srv, Card target);

public class EventData
{
    Card mSrcCard;
    Card mTargetCard;
    CardEvent mCardEvent;

    public EventData(Card src, Card target, CardEvent cardEvent)
    {
        mSrcCard = src;
        mTargetCard = target;
        mCardEvent = cardEvent;
    }

    public bool PerformEvent()
    {
        return mCardEvent(mSrcCard , mTargetCard);
    }

}

public class Game : MonoBehaviour
{
    public ScreenEffect screenEffect;
    public Sprite[] icons;

    [SerializeField] GameObject blackPanel;

    static Game mInstance = null;
    public static Game instance
    {   
        get
        {
            return mInstance;
        }
    }

    public GameState currentGameState = GameState.Init;

    public Queue<EventData> eventQueue = new Queue<EventData>();

    public CardManager cardManager;
    public StateRenderer stateRenderer;


    public int maxHp = 50;
    public int hp = 50;

    public int gold = 10;

    public int turn = 1;

    public float turnTimer = 0;

    public int playerPower = 0;

    private void Awake()
    {
        Debug.Assert(mInstance == null);

        mInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        turnTimer -= Time.deltaTime;

        if (turnTimer > 0) return;


        switch (currentGameState)
        {
            case GameState.Init:
                Init();
                break;
            case GameState.StartTurn:
                StartTurn();
                break;
            case GameState.RegenWait:
                RegenWait();
                break;
            case GameState.Idle:
                Idle();
                break;
            case GameState.EquipSelect:
                EquipSelect();
                break;
            case GameState.EventProc:
                EventProc();
                break;
            case GameState.TurnEndProc:
                PlayerTurnEndProc();
                break;
            case GameState.MonsterProc:
                MonsterProc();
                break;
        }

        blackPanel.SetActive(currentGameState == GameState.EquipSelect);
    }
    #region gameStateRun
    void StartTurn()
    {
        turnTimer = 0.1f;
        CardDB.UpdateTurn(turn);

        cardManager.RegenCard();
        cardManager.UpdateCard();
        stateRenderer.UpdateUI();
        currentGameState = GameState.RegenWait;
    }

    void Init()
    {
        cardManager.GenPlayerCard();
        currentGameState = GameState.StartTurn;
        
    }

    void RegenWait()
    {
        currentGameState = GameState.Idle;
    }

    void Idle()
    {
        
    }

    
    void EquipSelect()
    {
        
    }

    void EventProc()
    {
        if(eventQueue.Count > 0)
        {
            EventData data= eventQueue.Dequeue();

            Debug.Log("EventProc");

            if(data.PerformEvent())
            {
                turnTimer = 0.1f;
            }
        } else
        {
            currentGameState = GameState.TurnEndProc;
        }
        
        stateRenderer.UpdateUI();
    }

    void PlayerTurnEndProc()    
    {
        currentGameState = GameState.MonsterProc;

        

        turnTimer = 0.1f;
        stateRenderer.UpdateUI();
    }

    void MonsterProc()
    {
        if (cardManager.AttackPerformPlayer())
        {
            screenEffect.Shake(20);
            turnTimer = 0.25f;
        } else
        {
            EndTurn();
            turnTimer = 0.1f;
        }
        
    }
    #endregion
    public Card equipSelectedCard = null;


    public void EquipCardClick(Vector2Int cardNumber)
    {
        if (currentGameState != GameState.Idle) return;

        
        equipSelectedCard = cardManager.GetDungeonCard(cardNumber);
        currentGameState = GameState.EquipSelect;

        blackPanel.transform.SetAsLastSibling();
        equipSelectedCard.transform.SetAsLastSibling();

        stateRenderer.UpdateUI();

    }

    public void EquipSelectPlayer(int charNum)
    {
        if (currentGameState != GameState.EquipSelect) return;

        CardInfo info = cardManager.GetPlayerCard(charNum).info;
        info.equipItem = equipSelectedCard.info;
        gold -= equipSelectedCard.info.price;
        equipSelectedCard.Damage(1);
        currentGameState = GameState.TurnEndProc;
        
        stateRenderer.UpdateUI();

    }

    public void Cancel()
    {
        if (currentGameState != GameState.EquipSelect) return;
        equipSelectedCard = null;

        currentGameState = GameState.Idle;
        stateRenderer.UpdateUI();

    }

    public void EquipDestroy(Vector2Int cardNumber)
    {
        if (currentGameState != GameState.EquipSelect) return;


        currentGameState = GameState.EventProc;
        stateRenderer.UpdateUI();

    }

    public void CardClick(Vector2Int cardNumber)
    {
        if (currentGameState != GameState.Idle) return;


        currentGameState = GameState.EventProc;
        stateRenderer.UpdateUI();

    }

    void EndTurn()
    {
        turn++;
        currentGameState = GameState.StartTurn;
        
    }
}
