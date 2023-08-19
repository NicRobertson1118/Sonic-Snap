using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HandUtilities.CardSlot;

public class PlayerHandScript : MonoBehaviour
{
    public GameObject deckObject;
    private PlayerDeckScript deck;

    private bool canDraw = true;

    //Variables for managing position of cards
    private Vector3 centerPos;
    private List<HandUtilities.CardSlot> slots = new List<HandUtilities.CardSlot>();

    //Registry for instantiating cards
    CardRegistry cardRegistry;
    
    void Start()
    {
        cardRegistry  = GameObject.FindGameObjectWithTag("CardRegistry").GetComponent<CardRegistry>();

        centerPos = this.gameObject.transform.localPosition;

        deck = deckObject.gameObject.GetComponent<PlayerDeckScript>();
    }

    public void drawCard(string name = "None") {
        if (canDraw && slots.Count <= 7) {
            string cardName = (name == "None") ? deck.drawCard() : name;

            GameObject card = Instantiate(cardRegistry.retreiveCard(cardName));
            card.GetComponent<Card>().updateArt("default");

            addToSlots(card.GetComponent<Card>());
        } else {
            Debug.Log("HAND FULL");
        }
    }

    public void drawOpeningHand() {
        drawCard();
        drawCard();
        drawCard();
    }

    private void addToSlots(Card card) {
        addToTop(card);
        shiftSlots();
    }

    private void addToTop(Card card) {
        slots.Add(new HandUtilities.CardSlot(card));
    }

    private void shiftSlots() {
        float spaceLength = 2.0f;
        float offsetLength;

        if (isEven()) {
            offsetLength = spaceLength/2;
        } else {
            offsetLength = 0;
        }

        float centerNum = (float)Math.Ceiling(slots.Count/2.0);

        for (int i=0; i<slots.Count; i++) {
            float xPos;
            float difference = (float)Math.Floor(Math.Abs(centerNum-i));
            float centerOffset = ((difference*spaceLength) + offsetLength);

            if (i < centerNum) {
                xPos = centerPos.x - centerOffset;
            } else if (i > centerNum) {
                xPos = centerPos.x + centerOffset;
            } else {
                xPos = centerPos.x;
            }

            slots[i].changePosition(new Vector3(xPos, centerPos.y, centerPos.z));
        }
    }

    public bool containsCard(Card card) {
        bool contains = false;
        
        for (int i=0; i< slots.Count; i++) {
            if (slots[i].compareCard(card)) {
                contains = true;
                break;
            }
        }

        return contains;
    }

    public void removeCard(Card card) {
        for (int i=0; i< slots.Count; i++) {
            if (slots[i].compareCard(card)) {
                slots.RemoveAt(i);
            }
        }

        shiftSlots();
    }

    private bool isEven() {
        bool even;

        if (slots.Count%2 == 0) {
            even = true;
        } else {
            even = false;
        }

        return even;
    }
}
