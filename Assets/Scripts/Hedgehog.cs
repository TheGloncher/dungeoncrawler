using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class HedgehogCharacter : Character
{
    private int approachCount = 0;
    private bool approached = false;
    private bool hasBeenAttacked = false;

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        return new List<CharacterAction>
    {
        new CharacterAction("Approach", () => Approach(manager))
    };
    }


    private IEnumerator Approach(BattleManager manager)
    {
        Character target = IsPlayerControlled ? manager.GetEnemy() : manager.GetLowestHPPlayer();

        if (target == null)
        {
            manager.OnActionComplete(true);
            yield break;
        }

        approachCount++;

        if (approachCount < 4)
        {
            switch (approachCount)
            {
                case 1:
                    manager.dialogue.text = "It's making its way to you from the distance.";
                    break;
                case 2:
                    manager.dialogue.text = "It's growing closer...";
                    break;
                case 3:
                    manager.dialogue.text = "It's right in front of " + target.CharacterName;
                    break;
            }

            yield return new WaitForSeconds(2f);
            manager.OnActionComplete(false);
        }
        else
        {
            manager.dialogue.text = target.CharacterName + " is impaled on the spikes.";
            yield return new WaitForSeconds(1f);

            bool isDead = target.TakeDamage(9, this);
            manager.UpdateHUDForCharacter(target);
            yield return new WaitForSeconds(1f);

            manager.OnActionComplete(isDead);
        }
    }

    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);
        hasBeenAttacked = true;

        if (attacker != null && attacker != this && attacker.IsAlive && !isDead)
        {
            
            
            attacker.TakeDamage(2, this);
            Manager.UpdateHUDForCharacter(attacker); // update the attacker's HUD!
        }

        // Retaliate only if attacker is valid and still alive


        return isDead;
    }

    public override List<string> GetDialogueOptions()
    {
        if(!approached)
        {
            return new List<string> { "Get away from me.", "Get closer." };
        }
        else if(!hasBeenAttacked)
        {
            return new List<string> { "Get closer again" , "I won't hurt you, so don't hurt me."};
        }
        else
        {
            return new List<string> { "Get closer again." };
        }
    }



    public override void OnTalk(Character speaker)
    {
        Manager.ShowDialogueChoices(
     GetDialogueOptions(),
     (selectedIndex) =>
     {
         if (approached && !hasBeenAttacked)
         {
             switch (selectedIndex)
             {
                 case 0:
                     Manager.dialogue.text = "You are impaled on the spikes.";
                     speaker.TakeDamage(9, this);
                     break;
                 case 1:
                     Manager.dialogue.text = "The spikes retract slowly...";
                     //recruit the hedgehog
                     break;
             }
         }
         else if (approached && hasBeenAttacked)
         {
             switch (selectedIndex)
             {
                 case 0:
                     Manager.dialogue.text = "You are impaled on the spikes.";
                     speaker.TakeDamage(9, this);
                     break;

             }
         }
         else
         {
             switch (selectedIndex)
             {

                 case 0:
                     Manager.dialogue.text = "The spines grow longer...";
                     approachCount++;
                     break;
                 case 1:
                     Manager.dialogue.text = "The spines quiver...";
                     approached = true;
                     break;
             }

             Manager.StartCoroutine(FinishDialogueAfterDelay());
         }
     }
 );

        IEnumerator FinishDialogueAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            Manager.OnActionComplete(false);
        }
    }
}