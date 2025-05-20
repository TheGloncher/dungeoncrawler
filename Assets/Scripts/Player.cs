using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Player : Character
{

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        

        return new List<CharacterAction>
        {
            new CharacterAction("Strike", () => Strike(manager)),
            new CharacterAction("Talk", () => Talk(manager))
        };
    }



    private IEnumerator Strike(BattleManager manager)
    {
        Character target;

        if (IsPlayerControlled)
            target = manager.GetEnemy();
        else
            target = manager.GetLowestHPPlayer();

        if (target == null)
        {
            Debug.LogWarning("No valid target found!");
            manager.OnActionComplete(true);
            yield break;
        }

        bool isDead = target.TakeDamage(3, this);
        manager.UpdateHUDForCharacter(target);
        manager.dialogue.text = $"{CharacterName} strikes!";

        yield return new WaitForSeconds(2f);

        manager.OnActionComplete(isDead);
    }

    private IEnumerator Talk(BattleManager manager)
    {
        Debug.Log("Talk action initiated.");
        Character target = IsPlayerControlled ? manager.GetEnemy() : manager.GetLowestHPPlayer();

        if (target == null)
        {
            Debug.LogWarning("No valid target found!");
            manager.OnActionComplete(true);
            yield break;
        }

        List<string> options = target.GetDialogueOptions();
        Debug.Log("Dialogue options count: " + options.Count);
        manager.ShowDialogueOptions(options, this);

        // Wait for the dialogue selection to complete via BattleManager
        while (manager.State == BattleState.PLAYERTURN && manager.IsAwaitingDialogue)
            yield return null;

        Debug.Log("Talk coroutine resumed after dialogue selection.");
    }

}