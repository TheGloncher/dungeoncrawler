using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        

        return new List<CharacterAction>
        {
            new CharacterAction("Strike", () => manager.StartCoroutine(Strike(manager))),
            new CharacterAction("Talk", () => manager.StartCoroutine(Talk(manager)))
        };
    }

    

    private IEnumerator Strike(BattleManager manager)
    {
        Character target = manager.GetEnemy();
        
        bool isDead = target.TakeDamage(3, this);
        manager.UpdateHUDForCharacter(target);
        manager.dialogue.text = $"{CharacterName} strikes!";

        yield return new WaitForSeconds(1f);

        manager.OnActionComplete(isDead);
    }
    private IEnumerator Talk(BattleManager manager)
    {
        manager.dialogue.text = $"{CharacterName} is talking.";
        yield return new WaitForSeconds(1f);
        manager.OnActionComplete(false);
    }
}