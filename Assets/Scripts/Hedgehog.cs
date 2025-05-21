using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HedgehogCharacter : Character
{
    private int _approachCount = 0;
    private bool _approached = false;
    private bool _hasBeenAttacked = false;
    private bool _pacified = false;
    [SerializeField] private AudioClip _approachSfx;
    [SerializeField] private AudioClip _impaleSfx;

    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        var actions = new List<CharacterAction>();

        actions.Add(new CharacterAction("Approach", () => Approach(manager)));

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", () => Pass(manager)));

        return actions;
    }


    private IEnumerator Approach(BattleManager manager)
    {
        Character target = IsPlayerControlled ? manager.GetEnemy() : manager.GetRandomAlivePlayer();

        if (target == null)
        {
            manager.OnActionComplete(true);
            yield break;
        }

        _approachCount++;

        if (_approachCount < 4)
        {
            switch (_approachCount)
            {
                case 1:
                    manager.dialogue.text = "It's making its way from the distance...";
                    break;
                case 2:
                    manager.dialogue.text = "It's growing closer...";
                    break;
                case 3:
                    manager.dialogue.text = "It's right in front of " + target.CharacterName;
                    break;
            }
            Manager.AudioSource.volume = 0.2f * _approachCount;
            Manager.AudioSource.PlayOneShot(_approachSfx);
            yield return new WaitForSeconds(2.4f);
            //return volume to normal
            Manager.AudioSource.volume = 1f;




            manager.OnActionComplete(false);
        }
        else
        {
            manager.dialogue.text = target.CharacterName + " is impaled on the spikes.";
            yield return new WaitForSeconds(1f);

            bool isDead = target.TakeDamage(9, this);
            Manager.AudioSource.PlayOneShot(_impaleSfx);
            manager.UpdateHUDForCharacter(target);
            yield return new WaitForSeconds(1f);

            manager.OnActionComplete(isDead);
        }
    }

    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);
        _hasBeenAttacked = true;

        if (attacker != null && attacker != this && attacker.IsAlive && !isDead)
        {
            
            
            attacker.TakeDamage(2, this);
            Manager.UpdateHUDForCharacter(attacker); // update the attacker's HUD!
        }

        // Retaliate only if attacker is valid and still alive

        Manager.UpdateHUDForCharacter(this); // update the hedgehog's HUD
        return isDead;
    }

    public override List<string> GetDialogueOptions()
    {
        if(!_approached)
        {
            return new List<string> { "'Get away from me.'", "Get closer." };
        }
        else if(!_hasBeenAttacked)
        {
            return new List<string> { "Get closer again" , "'I won't hurt you, so don't hurt me.'"};
        }
        else 
        {
            return new List<string> { "Get closer again." };
        }
    }



    public override void OnTalk(Character speaker, int optionIndex)
    {
        if (!_approached)
        {
            if (optionIndex == 0)
            {
                Manager.dialogue.text = "It's still walking towards you...";
                _approachCount++;
            }
            else if (optionIndex == 1)
            {
                Manager.dialogue.text = "The spines quiver...";
                _approached = true;
            }
        }
        else if (_pacified && !_hasBeenAttacked)
        {
            if (optionIndex == 0)
            {
                Manager.dialogue.text = "It allows you to get closer to it.";
                Manager.StartCoroutine(DelayedRecruitment());
            }
        }
        else if (!_hasBeenAttacked)
        {
            if (optionIndex == 0)
            {
                Manager.dialogue.text = "You are impaled on the spikes.";
                speaker.TakeDamage(9, this);
                Manager.AudioSource.PlayOneShot(_impaleSfx);
            }
            else if (optionIndex == 1)
            {
                Manager.dialogue.text = "The spikes retract slowly...";
                _pacified = true;
            }
        }
        else if (!_pacified)
        {
            if (optionIndex == 0)
            {
                Manager.dialogue.text = "You are impaled on the spikes.";
                speaker.TakeDamage(9, this);
            }
        }
    }
    private IEnumerator DelayedRecruitment()
    {
        yield return new WaitForSeconds(2f);
        Manager.RecruitEnemy(this);
        
    }




}


    
