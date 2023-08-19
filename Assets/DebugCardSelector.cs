using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class DebugCardSelector : MonoBehaviour
{
    public GameObject basicButton;
    private CardRegistry cardRegistry;
    private PlayerHandScript playerHand;

    void Start()
    {
        cardRegistry = GameObject.FindGameObjectWithTag("CardRegistry").GetComponent<CardRegistry>();
        playerHand = GameObject.FindGameObjectWithTag("PlayerHand").GetComponent<PlayerHandScript>();

        populateCardSelector();
    }

    private void populateCardSelector() {
        foreach (GameObject card in cardRegistry.cardList) {
            setupCardButton(card);
        }
    }

    private void setupCardButton(GameObject card) {
        GameObject cardButton = Instantiate(basicButton);
        string cardName = card.name.Replace("Card","");

        cardButton.GetComponentInChildren<TMP_Text>().text = cardName;

        //Make this button a child of the grid within scrollable list
        cardButton.transform.parent = GameObject.Find("GridContent").transform;
        //Set scale to 1 because it gets bigger for some reason...
        cardButton.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

        cardButton.GetComponent<Button>().onClick.AddListener(delegate { playerHand.drawCard(cardName); });
    }
}
