using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : Character
{
    private bool _hasCondemned = false;
    [SerializeField] private AudioClip _condemnSound;
    [SerializeField] private AudioClip _purgeSound;
    public override void Start()
    {
        base.Start();
        _hasCondemned = false;
    }
    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        var actions = new List<CharacterAction>();

        if (!_hasCondemned && !IsPlayerControlled)
        {
            // Enemy-controlled: Condemn must be first and only option
            return new List<CharacterAction> { new CharacterAction("Condemn", (self) => CondemnAction(manager, self)) };
        }
        else if (!_hasCondemned)
        {
            // Player-controlled: let them choose
            actions.Add(new CharacterAction("Condemn", (self) => CondemnAction(manager, self)));
        }

        actions.Add(new CharacterAction("Purge", (self) => Purge(manager, self)));

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", (self) => Pass(manager, self)));

        return actions;
    }



    private IEnumerator CondemnAction(BattleManager manager, Character self)
    {
        Character target;

        if (self.IsPlayerControlled)
        {
            target = manager.GetEnemy(self);
        }
        else
        {
            // Get all alive players except the main one
            List<Character> candidates = new List<Character>(manager.GetPlayerParty());
            Character mainPlayer = manager.GetMainPlayer(); // ← we’ll add this helper below

            candidates.RemoveAll(c => c == null || !c.IsAlive || c == mainPlayer);

            if (candidates.Count > 0)
            {
                target = candidates[Random.Range(0, candidates.Count)];
            }
            else
            {
                target = mainPlayer; // fallback
            }
        }
        if (target == null)
        {
            manager.dialogue.text = "There's no one to judge.";
            yield return new WaitForSeconds(1f);
            manager.OnActionComplete(false);
            yield break;
        }

        target.Condemn();
        Manager.AudioSource.PlayOneShot(_condemnSound);
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
        Manager.AudioSource.PlayOneShot(_purgeSound);
        target.TakeDamage(4, self);
        yield return new WaitForSeconds(0.5f);

        manager.OnActionComplete(false);
    }

    public override List<string> GetDialogueOptions()
    {
        bool anyCondemnedAlive = false;

        foreach (var member in Manager.GetPlayerParty())
        {
            if (member != null && member.IsAlive && member.IsCondemned)
            {
                anyCondemnedAlive = true;
                break;
            }
        }
        if (!anyCondemnedAlive)
        {
            return new List<string> { "'Your actions are not righteous.'", "Im on the same side as you.", "'They were deserving of judgement.'" };
        }
        else
        {
            return new List<string> { "'Your actions are not righteous.'", "Im on the same side as you."};
        }
    }

    public override void OnTalk(Character speaker, int optionIndex)
    {
        bool anyCondemnedAlive = false;

        foreach (var member in Manager.GetPlayerParty())
        {
            if (member != null && member.IsAlive && member.IsCondemned)
            {
                anyCondemnedAlive = true;
                break;
            }
        }
        if (!anyCondemnedAlive)
        {
            switch (optionIndex)
            {
                case 0:
                    speaker.Condemn();
                    Manager.AudioSource.PlayOneShot(_condemnSound);
                    Manager.dialogue.text = "You have been deemed evil.";
                    break;
                case 1:
                    speaker.Condemn();
                    Manager.AudioSource.PlayOneShot(_condemnSound);
                    break;
                case 2:
                    Manager.dialogue.text = "The entity kneels in respect.";
                    //delayed recruitment
                    Manager.StartCoroutine(DelayedRecruitment());
                    break;
            }
        }
        else
        {
            switch (optionIndex)
            {
                case 0:
                    Manager.dialogue.text = "You have been deemed evil.";
                    speaker.Condemn();
                    Manager.AudioSource.PlayOneShot(_condemnSound);
                    break;
                case 1:
                    Manager.dialogue.text = "You have been deemed evil.";
                    speaker.Condemn();
                    Manager.AudioSource.PlayOneShot(_condemnSound);
                    break;
            }
        }

    }
}
