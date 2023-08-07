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
    protected bool movable = false;

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

    /*************************************
    * Variables for setting card artwork *
    *************************************/
    private Material cardMaterial;

    public Texture[] artList;
    private Dictionary<string, Texture> artRegistry = new Dictionary<string, Texture>();

    /****************
    * Logic Manager *
    ****************/

    LogicManagerScript logicManager;

    public virtual void Awake() {
        cardName = this.name.Replace("Card","").Replace("(Clone)","");

        logicManager  = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<LogicManagerScript>();

        establishCoreValues();

        cost = coreCost;
        power = corePower;

        powerText = this.gameObject.transform.Find("PowerText").gameObject.GetComponent<TextMeshPro>();
        costText = this.gameObject.transform.Find("CostText").gameObject.GetComponent<TextMeshPro>();

        cardMaterial = this.GetComponent<MeshRenderer>().material;
        initializeArtRegistry();

        powerText.text = corePower.ToString();
        costText.text = coreCost.ToString();
    }

    private void establishCoreValues() {
        if (cardName != null) {
            Dictionary<string, string> cardStats = logicManager.getCardStats()[cardName];

            coreCost = Int32.Parse(cardStats["Cost"]);
            corePower = Int32.Parse(cardStats["Power"]);
            cardDescription = cardStats["Description"];
            if (cardStats["Movable"] == "true") {
                movable = true;
            } else {
                movable = false;
            }
        } else {
            coreCost = 0;
            corePower = 0;
        }
    }

    private void initializeArtRegistry() {
        foreach (Texture art in artList) {
            string artName = art.name;

            artRegistry.Add(artName, art);
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

    public virtual void onGoing() {
        removePowerMods(cardName);
    }

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

    public void updateArt(string artName) {

        if (artRegistry.ContainsKey(artName)) {
            Texture cardArt = artRegistry[artName];

            cardMaterial.SetTexture("_MainTex", cardArt);
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

    private void removePowerMods(string nameToRemove) {
        for (int i=0; i<mods.Count; i++) {
            PowerUtils.Modifier mod = mods[i];

            if (mod.getSource() == nameToRemove) {
                mods.Remove(mod);
                i--;
            }
        }
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

        otherCards = currentZoneField.getOtherCards(this);

        return otherCards;
    }

    // Returns all cards at the zone opposite of this card's zone
    protected List<Card> getOpposingCards() {
        List<Card> opposingCards;

        opposingCards = zoneEffect.getOtherZone(currentZoneField).getCards();

        return opposingCards;
    }
}
