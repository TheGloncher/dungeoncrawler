using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Anger : Character
{
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _ultraHit;
    [SerializeField] private AudioClip _maddenSound;
    [SerializeField] private AudioClip _scream;
    [SerializeField] private AudioClip _repelSound;
    [SerializeField] private GameObject _angerCounterGameObject;
     private TextMeshProUGUI angerCounter;
    private int _angerCount = 0;
    private bool _shouldUseUnboundWrath = false;
    private bool _hasBeenAttacked = false;



    public override void Start()
    {
        base.Start();
        angerCounter = _angerCounterGameObject.GetComponent<TextMeshProUGUI>();

    }

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        var actions = new List<CharacterAction>();

        // If AI and ready to explode → force Unbound Wrath
        if (!IsPlayerControlled && _shouldUseUnboundWrath)
        {
            actions.Add(new CharacterAction("Unbound Wrath", (self) => UnboundWrath(manager, self)));
            return actions;
        }

        actions.Add(new CharacterAction("Rampage", (self) => Rampage(manager, self)));
        actions.Add(new CharacterAction("Madden", (self) => Madden(manager, self)));

        // Only include Unbound Wrath in action list if:
        // 1. Player is controlling it
        // 2. Or it is allowed to be used
        if (IsPlayerControlled || _shouldUseUnboundWrath)
        {
            actions.Add(new CharacterAction("Unbound Wrath", (self) => UnboundWrath(manager, self)));
        }

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

        yield return RageCheckDialogue();

        manager.OnActionComplete(isDead);
        
    }



    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);
        if (attacker.name == "Player")
        {
            _hasBeenAttacked = true;
        }
        AddRage(2);
        return isDead;
    }
    private IEnumerator RageCheckDialogue()
    {
        bool showedSomething = false;

        if (_angerCount > 10)
        {
            Manager.dialogue.text = "The end draws near...";
            yield return new WaitForSeconds(1.5f);
            showedSomething = true;
        }

        if (CurrentHP < 10)
        {
            Manager.dialogue.text = "The creature is tired!";
            yield return new WaitForSeconds(1.5f);
            showedSomething = true;
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.gray; // Change color to red
            }
        }

        if (!showedSomething)
            yield return null;
    }
    private void AddRage(int amount)
    {
        _angerCount += amount;
        
        if (_angerCounterGameObject.activeSelf == false)
        {
            _angerCounterGameObject.SetActive(true);
        }

        angerCounter.text = _angerCount.ToString();

        // If rage passed threshold, prepare to use Unbound Wrath
        if (_angerCount >= 10)
            _shouldUseUnboundWrath = true;

        StartCoroutine(RageCheckDialogue());
    }



    IEnumerator Madden(BattleManager manager, Character self)
    {
        manager.dialogue.text = "The creature looks like it's about to lose its mind...";
        //shake the character
        _shake?.TriggerShake(0.5f);

        yield return new WaitForSeconds(2f);
        AddRage(2);
        yield return RageCheckDialogue();
        manager.OnActionComplete(false);
    }

    IEnumerator UnboundWrath(BattleManager manager, Character self)
    {
        if (_angerCount < 10 && IsPlayerControlled)
        {
            manager.dialogue.text = "Your anger is not enough to be unleashed.";
            yield return new WaitForSeconds(1.5f);
            manager.OnActionComplete(false);
            yield break;
        }

        manager.dialogue.text = "It lets out an earth shattering scream!";
        Manager.AudioSource.PlayOneShot(_scream);
        _shake?.TriggerShake(1f);

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
        _shouldUseUnboundWrath = false; // ← reset the flag

        yield return RageCheckDialogue();
        manager.OnActionComplete(false);
    }

    public override List<string> GetDialogueOptions()
    {
        if(_hasBeenAttacked)
        {
            return new List<string>
            {
                "Approach it.",
                "'Calm down.'"
            };
        }
        else
        {
            return new List<string>
            {
                "Approach it.",
                "'Calm down.'",
                "I'm not going to hurt you."
            };
        }
    }
    public override void OnTalk(Character speaker, int optionIndex)
    {
        if (_hasBeenAttacked)
        {
            switch(optionIndex)
            { 
                case 0: // Approach it
                    Manager.dialogue.text = "You are pushed with great force.";
                    Manager.AudioSource.PlayOneShot(_repelSound);
                    speaker.TakeDamage(9, this);
                    break;

                case 1: // Calm down
                    Manager.dialogue.text = "It grows even more enraged!";
                    //trigger the shake
                    _shake?.TriggerShake(0.3f);
                    AddRage(1);
                    break;


            }

        }
        else
        {
            switch (optionIndex)
            {
                case 0: // Approach it
                    if(CurrentHP < 10)
                    {
                        Manager.dialogue.text = "It's calming down...";
                        Manager.StartCoroutine(DelayedRecruitment());
                        break;
                    }
                    Manager.dialogue.text = "You are pushed with great force.";
                    Manager.AudioSource.PlayOneShot(_repelSound);
                    speaker.TakeDamage(9, this);
                    break;
                case 1: // Calm down
                    Manager.dialogue.text = "It grows even more enraged!";
                    _shake?.TriggerShake(0.3f);
                    AddRage(1);
                    break;
                case 2: // I'm not going to hurt you
                    Manager.dialogue.text = "It's breathing slows down for a moment.";
                    AddRage(-1);
                    break;
            }
        }
    }





}



