using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HedgehogCharacter : Character
{
    private int approachCount = 0;
    private bool pacified = false;

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        return new List<CharacterAction>
    {
        new CharacterAction("Approach", () => Approach(manager))
    };
    }

    private IEnumerator Approach(BattleManager manager)
    {
        Character target;
        if (!IsPlayerControlled)
        {
             target = manager.GetLowestHPPlayer();
        }
        else
        {
             target = manager.GetEnemy();
        }

        if (target == null)
        {
            manager.OnActionComplete(true);
            yield break;
        }

        if (approachCount < 4)
        {
            approachCount++;

            switch (approachCount)
            {
                case 1:
                    manager.dialogue.text = "It's making its way to you from the distance.";
                    break;
                case 2:
                    manager.dialogue.text = "It's growing closer...";
                    break;
                case 3:
                    manager.dialogue.text = "It's right in front of "+ target.CharacterName;
                    break;
            }
            yield return new WaitForSeconds(2f);
            manager.OnActionComplete(false);
        }
        else
        {
            manager.dialogue.text = target.name + " is impaled on the spikes.";
            yield return new WaitForSeconds(1f);
            bool isDead = target.TakeDamage(9, this);
            manager.UpdateHUDForCharacter(target);
            yield return new WaitForSeconds(1f);
            manager.dialogue.text = "You quickly retreat from it!";
            yield return new WaitForSeconds(1f);
            manager.OnActionComplete(isDead);
        }



    }

    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);

        if (attacker != null && attacker != this && attacker.IsAlive && !isDead)
        {
            

            attacker.TakeDamage(2, this);
            Manager.UpdateHUDForCharacter(attacker); // update the attacker's HUD!
        }

        // Retaliate only if attacker is valid and still alive


        return isDead;
    }


    public void CalmStatement()
    {
        pacified = true;
        Debug.Log("The spikes slowly retract...");
    }
}