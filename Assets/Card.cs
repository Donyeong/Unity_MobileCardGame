using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Vector2Int cardNumber;
    
    public Image image;
    public Image Icon;
    public Text cardText;

    public Image Icon2;
    public Text cardText2;
    public Text cardName;

    public Button button;
    public Animator ani;

    public CardInfo info;

    public GameObject interactPanel;
    public Text interText;
    public Image InterIconImage;
    public Text interIconText;

    public bool Updated = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (info == null) return;
        interactPanel.SetActive(false); 
        if (info.type == CardType.Player)
        {
            button.interactable = Game.instance.currentGameState == GameState.EquipSelect;
            if (button.interactable == true)
            {
                interactPanel.SetActive(true);

                InterIconImage.sprite = Game.instance.icons[1];
                int srcDam = info.GetPower();

                if (info.equipItem != null)
                {
                    srcDam = info.equipItem.GetPower();
                }

                int addDam = (Game.instance.equipSelectedCard.info.GetPower() - srcDam);

                if(addDam < 0)
                {
                    interIconText.text = addDam.ToString();
                } else
                {
                    interIconText.text = "+" + addDam.ToString();
                }
                

                if(Game.instance.gold < Game.instance.equipSelectedCard.info.price)
                {
                    interText.text = "돈 부족";
                    button.interactable = false;
                } else
                {
                    interText.text = "장착";
                }
                
            }
        }
        else if(info.type == CardType.EquipItem && Game.instance.currentGameState == GameState.EquipSelect)
        {
            button.interactable = this == Game.instance.equipSelectedCard;
            if (button.interactable == true)
            {
                interactPanel.SetActive(true);

                InterIconImage.sprite = Game.instance.icons[0];

                interIconText.text = "+" + Math.Max(info.price / 5, 1).ToString();

                interText.text = "파괴";
            }
        }
        else
        {
            button.interactable = Game.instance.currentGameState == GameState.Idle;
        }
       
    }

    public void CharUpdate()
    {
        if(info.currentAttackTurn > 0)
        {
            info.currentAttackTurn--;
        }
    }
    
    public void Make()
    {
        gameObject.SetActive(true);
        ani.Play("Make");
        //ani.play();

    }

    public void remove()
    {
        gameObject.SetActive(false);
    }

    public void PlayerCardClick()
    {
        if (Game.instance.currentGameState == GameState.EquipSelect)
        {
            if(Game.instance.gold < Game.instance.equipSelectedCard.info.price)
            {
                Game.instance.Cancel();
                return;
            }

            Game.instance.EquipSelectPlayer(cardNumber.x);
            UpdateUI();
            ani.Play("Normal");
        }
        
    }

    public void EquipCardClick()
    {
        if (Game.instance.currentGameState == GameState.Idle)
        {
            
            Game.instance.EquipCardClick(cardNumber);
            return;
        }

        if (Game.instance.currentGameState == GameState.EquipSelect)
        {
            if(Game.instance.equipSelectedCard == this)
            {
                Game.instance.gold += Mathf.Max(1,info.price/5);
                Damage(1);
                Game.instance.EquipDestroy(cardNumber);
            }
            return;
        }
    }

    public void Click()
    {
        if(info.type == CardType.Player)
        {
            PlayerCardClick();
            return;
        }

        if (info.type == CardType.EquipItem)
        {
            EquipCardClick();


            return;
        }

        if (info.type == CardType.Monster)
        {
            if(Game.instance.playerPower == 0)
            {
                return;
            }
            Game.instance.cardManager.AttackCharCard(this);
            //Damage(Game.instance.playerPower);
        } else
        {
            Damage(1);
        }


        

        

        if(info.type == CardType.Item)
        {
            Game.instance.hp += info.amount;
            Game.instance.hp = Math.Min(Game.instance.maxHp, Game.instance.hp);
        }

        if (info.type == CardType.Gold)
        {
            Game.instance.gold += info.price;
        }

        


        UpdateUI();

        Game.instance.CardClick(cardNumber);
    }

    public bool Damage(int num)
    {
        Debug.Log("Damage " + name + " dam " + num);

        info.hp -= num;
        if (info.hp <= 0)
        {
            Game.instance.cardManager.RemoveCard(cardNumber);
            return true;
        } else
        {
            if(num > 0)
            {
                ani.Play("Hit");
            }
        }
        UpdateUI();

        return false;
    }

    public bool Attack()
    {
        if (info.type != CardType.Monster) return false;
        Updated = true;
        Game.instance.hp -= info.GetPower();
        Game.instance.stateRenderer.UpdateUI();
        ani.Play("Attack");
        return true;
    }


    public void UpdateUI()
    {
        image.sprite = info.sprite;
        image.SetNativeSize();

        Icon2.enabled = false;
        cardText2.enabled = false;
        cardName.text = info.name;

        if (info.type == CardType.Monster)
        {
            Icon.sprite = Game.instance.icons[2];

            cardText.text = info.hp.ToString();

            Icon2.enabled = true;
            cardText2.enabled = true;
            Icon2.sprite = Game.instance.icons[1];
            cardText2.text = info.power.ToString();
        }
        
        if (info.type == CardType.Player)
        {
            if(info.currentAttackTurn == 0)
            {
                cardText.text = "공격 가능";
            }
            else
            {
                cardText.text = info.currentAttackTurn + "턴";
            }

            Icon.sprite = Game.instance.icons[1];

            Icon2.enabled = true;
            cardText2.enabled = true;
            Icon2.sprite = Game.instance.icons[1];

            string equipName = "맨손";
            if (info.equipItem != null)
            {
                equipName = info.equipItem.name;
            }
            cardText2.text = info.GetPower() + " ( " + equipName + " )";
        }
        if (info.type == CardType.Gold)
        {
            Icon.sprite = Game.instance.icons[0];
            cardText.text = info.price.ToString();
        }

        if(info.type == CardType.EquipItem)
        {

            cardText.text = info.price.ToString();
            Icon.sprite = Game.instance.icons[0];

            Icon2.enabled = true;
            cardText2.enabled = true;
            Icon2.sprite = Game.instance.icons[1];

            if (info.attackTurn == 0)
            {
                cardText2.text = info.power.ToString();
            }
            else {
                cardText2.text = info.power.ToString() + ", 공속 -" + info.attackTurn.ToString();
            }
        }


        if (info.type == CardType.Item)
        {
            cardText.text = info.amount.ToString();
            Icon.sprite = Game.instance.icons[2];
        }
    }

}
