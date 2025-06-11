using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Anger : Character
{
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _ultraHit;
    [SerializeField] private TextMeshProUGUI angerCounter;
    private int _angerCount = 0;



    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        var actions = new List<CharacterAction>();

        if (!IsPlayerControlled && _angerCount >= 10)
        {
            actions.Add(new CharacterAction("Unbound Wrath", (self) => UnboundWrath(manager, self)));
            return actions;
        }

        actions.Add(new CharacterAction("Rampage", (self) => Rampage(manager, self)));
        actions.Add(new CharacterAction("Madden", (self) => Madden(manager, self)));
        actions.Add(new CharacterAction("Unbound Wrath", (self) => UnboundWrath(manager, self)));

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", (self) => Pass(manager, self)));

        return actions;
    }

    IEnumerator Rampage(BattleManager manager, Character self)
    {
        Character target = self.IsPlayerControlled ? manager.GetEnemy(self) : manager.GetLowestHPPlayer();


        if (target == null)
        {
            Debug.LogWarning("No valid target found!");
            manager.OnActionComplete(true);
            yield break;
        }

        
        
        
        manager.dialogue.text = $"It recklessly throws itself at {self.CharacterName}!";

        yield return new WaitForSeconds(2f);
        bool isDead = target.TakeDamage(_angerCount, this);
        Manager.AudioSource.PlayOneShot(_hitSound);
        manager.UpdateHUDForCharacter(target);

        StartCoroutine(RageCheckDialogue());

        manager.OnActionComplete(isDead);
        
    }



    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);
        AddRage(1);
        return isDead;
    }
    private IEnumerator RageCheckDialogue()
    {
        if (_angerCount > 10)
        {
            Manager.dialogue.text = "The end draws near...";
            yield return new WaitForSeconds(1.5f);
        }

        if (CurrentHP < 10)
        {
            Manager.dialogue.text = "The creature is tired!";
            yield return new WaitForSeconds(1.5f);
        }
    }
    private void AddRage(int amount)
    {
        _angerCount += amount;
        angerCounter.text = _angerCount.ToString();
        StartCoroutine(RageCheckDialogue());
    }

    public override List<string> GetDialogueOptions()
    {
        {
            return new List<string>
            {
                "gronger",
                "grpeas",
                "Unbound Wrath"
            };
        }
    }

    IEnumerator Madden(BattleManager manager, Character self)
    {
        manager.dialogue.text = "The creature looks like it's about to lose its mind...";
        yield return new WaitForSeconds(2f);
        AddRage(2);
        manager.OnActionComplete(false);
    }

    IEnumerator UnboundWrath(BattleManager manager, Character self)
    {
        if (_angerCount < 10 && IsPlayerControlled)
        {
            manager.dialogue.text = "You're not angry enough to unleash this.";
            yield return new WaitForSeconds(1.5f);
            manager.OnActionComplete(false);
            yield break;
        }

        manager.dialogue.text = "The end draws near...";
        yield return new WaitForSeconds(2f);

        List<Character> targets = self.IsPlayerControlled
            ? manager.GetAllEnemies()
            : manager.GetAllPlayers();

        foreach (var target in targets)
        {
            bool isDead = target.TakeDamage(10, this);
            Manager.AudioSource.PlayOneShot(_ultraHit);
            manager.UpdateHUDForCharacter(target);
        }

        _angerCount = 0;
        angerCounter.text = _angerCount.ToString();
        StartCoroutine(RageCheckDialogue());
        manager.OnActionComplete(false);
    }

    public override void OnTalk(Character speaker, int optionIndex)
    {
        Manager.dialogue.text = "nuthin yet";
    }





}



