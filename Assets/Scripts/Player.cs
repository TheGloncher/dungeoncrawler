using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Player : Character
{
    [SerializeField] private AudioClip _strikeSfx;
    [SerializeField] private AudioClip _talkSfx;
    public override List<CharacterAction> GetActions(BattleManager manager)
    {
       

        var actions = new List<CharacterAction>();

        // Add custom character-specific actions
        actions.Add(new CharacterAction("Strike", () => Strike(manager)));
        actions.Add(new CharacterAction("Talk", () => Talk(manager)));

        // Add Pass only if player-controlled
        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", () => Pass(manager)));

        return actions;
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
        Manager.AudioSource.PlayOneShot(_strikeSfx);
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
        Manager.AudioSource.PlayOneShot(_talkSfx);
        manager.ShowDialogueOptions(options, this);

        // Wait for the dialogue selection to complete via BattleManager
        while (manager.State == BattleState.PLAYERTURN && manager.IsAwaitingDialogue)
            yield return null;

        Debug.Log("Talk coroutine resumed after dialogue selection.");
    }

}