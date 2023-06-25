using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

using static PowerUtils.Modifier;

public class Card : MonoBehaviour
{
    protected string cardName;
    protected string cardDescription;

    //Original values
    protected int coreCost;
    protected int corePower;

    //Potentially modified values
    protected int cost;
    protected int power;

    //List of values that change the card's power
    protected List<PowerUtils.Modifier> mods = new List<PowerUtils.Modifier>();

    /*****************************************
    Variables for whether cards can be moved
    *****************************************/

    //Whether card is locked into place (ex: card was played on previous turn)
    protected bool locked = false;
    //Whether the card can currently move to another zone DESPITE being locked
    public bool movable = false;

    /*****************
    * Zone variables *
    *****************/
    protected ZoneField currentZoneField;
    protected ZoneEffectScript zoneEffect;

    /**************************
    * Text for power and cost *
    **************************/
    private TextMeshPro powerText;
    private TextMeshPro costText;

    /****************
    * Logic Manager *
    ****************/

    LogicManagerScript logicManager;

    public virtual void Start() {
        logicManager  = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<LogicManagerScript>();

        establishCoreValues();

        cost = coreCost;
        power = corePower;

        powerText = this.gameObject.transform.Find("PowerText").gameObject.GetComponent<TextMeshPro>();
        costText = this.gameObject.transform.Find("CostText").gameObject.GetComponent<TextMeshPro>();

        powerText.text = corePower.ToString();
        costText.text = coreCost.ToString();
    }

    private void establishCoreValues() {
        if (cardName != null) {
            Dictionary<string, string> cardStats = logicManager.getCardStats()[cardName];

            coreCost = Int32.Parse(cardStats["Cost"]);
            corePower = Int32.Parse(cardStats["Power"]);
            cardDescription = cardStats["Description"];
        } else {
            coreCost = 0;
            corePower = 0;
        }
    }

    /*****************
    * Main Functions *
    *****************/

    //Special, on-reveal/on-going functionality
    public virtual void playCard() {
        applyPowerMods();
    }

    public virtual void onReveal() {}

    public virtual void onGoing() {}

    public virtual void onDestroy() {
        destroyCard();
    }

    public virtual void onDiscard() {
        discardCard();
    }

    public virtual void updateCard() {
        applyPowerMods();
    }

    //Exclusively for updating power mods
    public void perCard() {
        applyPowerMods();
    }

    /****************
    Getter functions
    ****************/

    public int getCost() {
        return cost;
    }

    public int getPower() {
        return power;
    }

    public ZoneField getZone() {
        return currentZoneField;
    }

    public bool isLocked() {
        return locked;
    }

    public bool isMovable() {
        return movable;
    }

    public void lockCard() {
        locked = true;
    }

    /******************
    * Update functions*
    ******************/

    public void updateMovable() {
        movable = !movable;
    }

    public void updateZone(ZoneField newZone) {
        currentZoneField = newZone;
        zoneEffect = currentZoneField.transform.parent.Find("ZoneEffect").GetComponent<ZoneEffectScript>();
    }

    public void updatePowerText() {
        powerText.text = power.ToString();
        if (power > corePower) {
            powerText.color = new Color(0,255,0);
        } else if (power < corePower) {
            powerText.color = new Color(255,0,0);
        } else {
            powerText.color = new Color(0,0,0);
        }
    }

    /******************************
    * Destroy & Discard functions *
    ******************************/

    private void destroyCard() {
        Destroy(this.gameObject);
    }

    private void discardCard() {
        Destroy(this.gameObject);
    }

    /*********************
    * Modifier functions *
    *********************/

    private void applyPowerMods() {
        int newPower = corePower;

        foreach (PowerUtils.Modifier mod in mods) {
            if (mod.getType() == "Add") {
                newPower += mod.getValue();
            }
        }

        foreach (PowerUtils.Modifier mod in mods) {
            if (mod.getType() == "Subtract") {
                newPower -= mod.getValue();
            }
        }

        foreach (PowerUtils.Modifier mod in mods) {
            if (mod.getType() == "Multiply") {
                newPower = newPower * mod.getValue();
            }
        }

        foreach (PowerUtils.Modifier mod in mods) {
            if (mod.getType() == "Divide") {
                newPower = newPower * mod.getValue();
            }
            power = power / mod.getValue();
        }

        power = newPower;
        updatePowerText();
    }

    public bool hasPowerMod(PowerUtils.Modifier mod) {
        bool hasMod = false;
        if (mods.Contains(mod)) {
            hasMod = true;
        }

        return hasMod;
    }

    public void addPowerMod(PowerUtils.Modifier mod) {
        mods.Add(mod);
    }

    /*****************
    * Zone Utilities *
    *****************/

    // Returns all cards at this card's zone except itself
    protected List<Card> getOtherCards() {
        List<Card> otherCards;

        if (currentZoneField.GetComponent<ZoneField>().isPlayerZone) {
            otherCards = zoneEffect.getPlayerZone().getOtherCards(this);
        } else {
            otherCards = zoneEffect.getEnemyZone().getOtherCards(this);
        }

        return otherCards;
    }

    // Returns all cards at the zone opposite of this card's zone
    protected List<Card> getOpposingCards() {
        List<Card> otherCards;

        if (currentZoneField.GetComponent<ZoneField>().isPlayerZone) {
            otherCards = zoneEffect.getEnemyZone().getCards();
        } else {
            otherCards = zoneEffect.getPlayerZone().getCards();
        }

        return otherCards;
    }
}
