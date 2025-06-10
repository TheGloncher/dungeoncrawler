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
        actions.Add(new CharacterAction("Strike", (self) => Strike(manager, self)));
        actions.Add(new CharacterAction("Talk", (self) => Talk(manager, self)));

        // Add Pass only if player-controlled
        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", (self) => Pass(manager, self)));

        return actions;
    }



    private IEnumerator Strike(BattleManager manager, Character self)
    {
        Character target = self.IsPlayerControlled ? manager.GetEnemy(self) : manager.GetLowestHPPlayer();
        

        

        if (target == null)
        {
            Debug.LogWarning("No valid target found!");
            manager.OnActionComplete(true);
            yield break;
        }

        bool isDead = target.TakeDamage(3, this);
        Manager.AudioSource.PlayOneShot(_strikeSfx);
        manager.UpdateHUDForCharacter(target);
        manager.dialogue.text = $"{self.CharacterName} strikes!";

        yield return new WaitForSeconds(2f);

        manager.OnActionComplete(isDead);
    }

    private IEnumerator Talk(BattleManager manager, Character self)
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