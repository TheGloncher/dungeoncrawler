using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Character
{
    private bool _hasCondemned = false;
    public override void Start()
    {
        base.Start();
        _hasCondemned = false;
    }
    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        var actions = new List<CharacterAction>();

        if (!_hasCondemned)
            actions.Add(new CharacterAction("Condemn", (self) => CondemnAction(manager, self)));

        actions.Add(new CharacterAction("Purge", (self) => Purge(manager, self)));

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", (self) => Pass(manager, self)));

        return actions;
    }

    private IEnumerator CondemnAction(BattleManager manager, Character self)
    {
        Character target = self.IsPlayerControlled ? manager.GetEnemy(self) : manager.GetLowestHPPlayer();
        if (target == null)
        {
            manager.dialogue.text = "There's no one to judge.";
            yield return new WaitForSeconds(1f);
            manager.OnActionComplete(false);
            yield break;
        }

        target.Condemn();
        manager.dialogue.text = $"{target.CharacterName} has been deemed evil.";
        _hasCondemned = true;

        yield return new WaitForSeconds(1.5f);
        manager.OnActionComplete(false);
    }

    private IEnumerator Purge(BattleManager manager, Character self)
    {
        Character target = self.IsPlayerControlled ? manager.GetCondemnedEnemy() : manager.GetLowestHPCondemnedPlayer();
        if (target == null)
        {
            manager.dialogue.text = "There is no evil to cleanse.";
            yield return new WaitForSeconds(1f);
            manager.OnActionComplete(false);
            yield break;
        }

        manager.dialogue.text = "It swings its blade!";
        yield return new WaitForSeconds(1f);

        target.TakeDamage(4, self);
        yield return new WaitForSeconds(0.5f);

        manager.OnActionComplete(false);
    }

    public override List<string> GetDialogueOptions()
    {
        return new List<string>
        {
            "\"Your actions are not righteous.\"",
            "\"I'm on the same side as you.\""
        };
    }

    public override void OnTalk(Character speaker, int optionIndex)
    {
        switch (optionIndex)
        {
            case 0:
                speaker.Condemn();
                Manager.dialogue.text = "You have been deemed evil.";
                break;
            case 1:
                Manager.dialogue.text = "The entity kneels in respect.";
                Manager.StartCoroutine(DelayedRecruitment());
                break;
        }
    }


}
