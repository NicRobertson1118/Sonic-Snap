using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneEffectScript : MonoBehaviour
{
    protected bool isRevealed = false;

    protected string zoneName = "";
    protected string effectText = "";
    protected PowerUtils.Modifier effect;

    protected ZoneField whiteZone;
    protected ZoneField blackZone;

    public virtual void Start() {
        whiteZone = this.transform.parent.Find("PlayerZone").GetComponent<ZoneField>();
        blackZone = this.transform.parent.Find("EnemyZone").GetComponent<ZoneField>();
    }

    public void updateZone() {
        whiteZone.updateZone();
        blackZone.updateZone();

        if (isRevealed) {
            onGoing();
        }
    }

    public void revealZoneEffect() {
        isRevealed = true;
        onReveal();
    }

    //Runs once when zone is revealed.
    public virtual void onReveal() {}

    //Runs after all cards are played on both sides
    public virtual void onGoing() {}

    //Runs after each card is played
    public virtual void perCard(Card cardScript) {}

    /*******************
    * Getter Functions *
    *******************/

    public ZoneField getOtherZone(ZoneField zone) {
        ZoneField otherZone;

        if (zone == whiteZone) {
            otherZone = blackZone;
        } else {
            otherZone = whiteZone;
        }

        return otherZone;
    }
}
