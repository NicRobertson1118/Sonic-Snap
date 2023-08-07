using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class ZoneField : MonoBehaviour
{
    //Main variables
    public bool isRevealed = false;
    private int zonePower = 0;
    List<Card> cards = new List<Card>();

    //Variables for outside scripts/objects
    private ZoneEffectScript zoneEffectScript;
    private LogicManagerScript logicScript;
    private PlayerHandScript handScript;
    private TextMeshPro powerText;

    //Collision detection variables
    BoxCollider2D zoneCollider;
    List<Collider2D> TriggerList = new List<Collider2D>();
    List<Vector3> zoneSpots = new List<Vector3>();

    public virtual void Start()
    {
        zoneCollider = this.gameObject.GetComponent<BoxCollider2D>();
        establishCardSpots();

        zoneEffectScript = this.transform.parent.Find("ZoneEffect").GetComponent<ZoneEffectScript>();

        logicScript = GameObject.FindGameObjectWithTag("LogicManager").GetComponent<LogicManagerScript>();

        handScript = GameObject.FindGameObjectWithTag("PlayerHand").GetComponent<PlayerHandScript>();

        powerText = this.gameObject.transform.Find("PowerText").gameObject.GetComponent<TextMeshPro>();
        powerText.text = zonePower.ToString();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)) {
            List<Card> newCards = gatherNewCards();

            foreach (Card card in newCards) {
                if (enoughEnergy(card)) {
                    addCard(card);
                }
            }
            checkAbsentCards();
        }

    }

    public virtual void onReveal() {}
    
    public virtual void onGoing() {}

    public List<Card> getCards() {
        return cards;
    }


    private bool enoughEnergy(Card card) {
        return logicScript.getPlayerEnergy() >= card.getCost();
    }

    /*******************
    * Getter Functions *
    *******************/

    public ZoneEffectScript getZoneEffect() {
        return zoneEffectScript;
    }

    /************************************
    Functions for initializing the zone
    *************************************/

    private void establishCardSpots() {
        Vector3 zoneSize = zoneCollider.bounds.size;
        float startPosX = transform.position.x - zoneSize.x/2;
        float startPosY = transform.position.y - zoneSize.y/2;

        zoneSpots.Add(new Vector3(
            startPosX + zoneSize.x/3,
            startPosY + zoneSize.y/3,
            0
        ));
        zoneSpots.Add(new Vector3(
            startPosX + (zoneSize.x/3)*2,
            startPosY + zoneSize.y/3,
            0
        ));
        zoneSpots.Add(new Vector3(
            startPosX + zoneSize.x/3,
            startPosY + (zoneSize.y/3)*2,
            0
        ));
        zoneSpots.Add(new Vector3(
            startPosX + (zoneSize.x/3)*2,
            startPosY + (zoneSize.y/3)*2,
            0
        ));
    }


    /**********************
    End of turn functions
    **********************/

    public void updateZone() {
        List<Card> latestCards = gatherLatestCards();
        lockZone();
        revealLatestCards(latestCards);
        updateOtherCards(latestCards);
        calculatePower();
    }

    private List<Card> gatherLatestCards() {
        List<Card> latest = new List<Card>();

        foreach (Card card in cards) {
            if (!card.isLocked()) {
                latest.Add(card);
            }
        }

        return latest;
    }

    //Lock all of the cards in the zone
    private void lockZone() {
        foreach(Card card in cards) {
            card.GetComponent<Card>().lockCard();
            card.GetComponent<Card>().updateZone(this);
        }
    }

    private void revealLatestCards(List<Card> latest) {
        foreach (Card card in latest) {
            card.playCard();
            card.onReveal();
            card.onGoing();

            zoneEffectScript.perCard(card);
            card.perCard();
        }
    }

    private void updateOtherCards(List<Card> latest) {
        foreach (Card card in cards) {
            if (!latest.Contains(card)) {
                card.onGoing();
                card.updateCard();
            }
        }
    }

    private void calculatePower() {
        int newPower = 0;
        foreach (Card card in cards) {
            newPower += card.getPower();
        }

        zonePower = newPower;
        powerText.text = zonePower.ToString();
    }

    /************************************************
    * Functions for getting information about cards *
    ************************************************/

    public List<Card> getOtherCards(Card card) {
        List<Card> otherCards = new List<Card>();

        foreach (Card other in cards) {
            if (other != card) {
                otherCards.Add(other);
            }
        }

        return otherCards;
    }

    /****************************************
    Functions for adding and removing cards
    ****************************************/

    private void addCard(Card newCard) {
        if (cards.Count < zoneSpots.Count) {
            int slotNum = cards.Count;

            cards.Add(newCard);
            newCard.gameObject.transform.position = zoneSpots[slotNum];

            logicScript.alterPlayerEnergy(newCard.getCost() * -1);

            handScript.removeCard(newCard);
        } else {
            Debug.Log("FULL");
        }
    }

    private void removeCard(Card card) {
        cards.Remove(card);
        logicScript.alterPlayerEnergy(card.getCost());
    }

    private void checkAbsentCards() {
        foreach (Card card in cards.ToList()) {
            if (!TriggerListContainsCard(card)) {
                removeCard(card);
            }
        }
    }

    /*********************************
    Functions for detecting collision
    *********************************/

    private List<Card> gatherNewCards() {
        List<Card> newCards = new List<Card>();

        foreach(Collider2D collider in TriggerList) {
            Card newCard = collider.gameObject.GetComponent<Card>();
            if (!cards.Contains(newCard)) {
                newCards.Add(newCard);
            }
        }

        return newCards;
    }

    private bool TriggerListContainsCard(Card card) {
        bool containsCard = false;
        foreach (Collider2D collider in TriggerList) {
            if (collider.gameObject.GetComponent<Card>() == card) {
                containsCard = true;
                break;
            }
        }

        return containsCard;
    }

     //Called when something enters the trigger
    private void OnTriggerEnter2D(Collider2D collider)
    {
        //if the object is not already in the list
        if(!TriggerList.Contains(collider))
        {
            //add the object to the list
            TriggerList.Add(collider);
        }
    }
    
    //Called when something exits the trigger
    private void OnTriggerExit2D(Collider2D collider)
    {
        //if the object is in the list
        if(TriggerList.Contains(collider))
        {
            //remove it from the list
            TriggerList.Remove(collider);
        }
    }
}
