using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateRenderer : MonoBehaviour
{
    [SerializeField] Text hpText;
    [SerializeField] Image hpBar;
    [SerializeField] Image hpBarReduce;

    [SerializeField] Text TurnText;

    [SerializeField] Text GoldText;
    [SerializeField] Text powerText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hpBarReduce.fillAmount = Mathf.Lerp(hpBarReduce.fillAmount, hpBar.fillAmount, 0.2f);
    }

    public void UpdateUI()
    {
        hpBar.fillAmount = (float)Game.instance.hp / Game.instance.maxHp;

        hpText.text = Game.instance.hp + " / " + Game.instance.maxHp;

        TurnText.text = Game.instance.turn + " 턴";

        GoldText.text = Game.instance.gold + " G";

        powerText.text = "총 공격력 : " +Game.instance.playerPower;
}
}
