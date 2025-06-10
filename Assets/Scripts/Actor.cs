using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : Character
{
    private List<CharacterAction> _copiedActions = new();
    private bool _hasFabricated = false;
    private static List<CharacterAction> _staticCopiedActions = new();
    private static bool _staticHasFabricated = false;
    [SerializeField] private AudioClip _fabricateSfx;
    [SerializeField] private AudioClip _impaleSfx;
    private bool _isWobbly = false;
    private bool _inflatedEgo = false;


    private void Awake()
    {
        if (_staticHasFabricated)
        {
            _copiedActions = new List<CharacterAction>(_staticCopiedActions);
            _hasFabricated = true;
        }
    }
    public override void Start()
    {
        base.Start();
    }
    public override List<CharacterAction> GetActions(BattleManager manager)
    {
        if (_hasFabricated)
            return _copiedActions;

        var actions = new List<CharacterAction>
        {
            new CharacterAction("Fabricate", (self) => Fabricate(manager, self))
        };

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", (self) => Pass(manager, self)));

        return actions;
    }

    private IEnumerator Fabricate(BattleManager manager, Character self)
    {
        Character target = self.IsPlayerControlled ? manager.GetEnemy(self) : manager.GetLowestHPPlayer();

        if (target == null)
        {
            manager.dialogue.text = "The actor stares into space.";
            yield return new WaitForSeconds(1.5f);
            manager.OnActionComplete(false);
            yield break;
        }

        manager.dialogue.text = $"{target.CharacterName} is being looked at attentively...";
        Manager.AudioSource.PlayOneShot(_fabricateSfx);
        yield return new WaitForSeconds(2f);


        _copiedActions = new();

        foreach (var action in target.GetActions(manager))
        {
            if (action.actionName != "Pass")
                _copiedActions.Add(new CharacterAction(
                action.actionName,
                (self) => action.coroutineWithUser(self)
                ));
        }

        _hasFabricated = true;
        _staticCopiedActions = new List<CharacterAction>(_copiedActions);
        _staticHasFabricated = true;

        manager.OnActionComplete(false);

    }

    public void ResetToNormal()
    {
        _copiedActions.Clear();
        _hasFabricated = false;
        _staticCopiedActions.Clear();
        _staticCopiedActions = null;
        _staticHasFabricated = false;

    }

    public override List<string> GetDialogueOptions()
    {
        if (_inflatedEgo && _hasFabricated)
        {
            return new List<string> { "Alright then. You're better than me." };
        }
        else if (_hasFabricated)
        {
            return new List<string> { "'You're worthless.'", "Try and pry out the mask.", "You should not forget what you are.", "Look at us now. We're better." };
        }

        else
        {
            return new List<string> { "'You're worthless.'", "Try and pry out the mask." };
        }

    }

    private IEnumerator MaskRip()
    {
        Manager.dialogue.text = "The mask tears off. The structure falls apart.";
        yield return new WaitForSeconds(1.5f);
        this.TakeDamage(50, this); // The Actor is defeated
        Manager.OnActionComplete(true); // The Actor is defeated
    }



    public override void OnTalk(Character speaker, int optionIndex)
    {
        if (_inflatedEgo && _hasFabricated)
        {
            switch (optionIndex)
            {
                case 0:
                    Manager.dialogue.text = "The structure grows in size...";
                    Manager.StartCoroutine(DelayedRecruitment());
                    break;
            }
        }
        else if (_hasFabricated)
        {
            switch (optionIndex)
            {
                case 0:
                    Manager.dialogue.text = "The Actor retaliates!";
                    speaker.TakeDamage(5, this);
                    Manager.AudioSource.PlayOneShot(_impaleSfx);
                    break;
                case 1:
                    if (_isWobbly)
                    {
                        StartCoroutine(MaskRip());
                    }
                    else
                    {
                        Manager.dialogue.text = "The mask is stuck tight.";
                    }
                    break;
                case 2:
                    Manager.dialogue.text = "The structure moves from side to side.";
                    _isWobbly = true;
                    break;
                case 3:
                    Manager.dialogue.text = "The structure solidifies...";
                    _inflatedEgo = true;
                    break;
            }
        }
        else
        {
            switch (optionIndex)
            {
                case 0:
                    Manager.dialogue.text = "The Actor retaliates!";
                    speaker.TakeDamage(5, this);
                    Manager.AudioSource.PlayOneShot(_impaleSfx);
                    break;
                case 1:
                    Manager.dialogue.text = "The mask is stuck tight.";
                    break;
            }
        }
    }
}
