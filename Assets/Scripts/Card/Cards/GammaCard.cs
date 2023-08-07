using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GammaCard: Card {
    
    public override void Awake() {
        cardName = "Gamma";
        base.Awake();
    }
    
     public override void onGoing() {
        base.onGoing();

        int oppositeCardsCount = getOpposingCards().Count;

        PowerUtils.Modifier addXPower = new PowerUtils.Modifier("Gamma", "Add", oppositeCardsCount);

        addPowerMod(addXPower);
    }

}