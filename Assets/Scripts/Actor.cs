using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : Character
{
    private List<CharacterAction> _copiedActions = new();
    private bool _hasFabricated = false;
    private static List<CharacterAction> _staticCopiedActions = new();
    private static bool _staticHasFabricated = false;


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
        if(_hasFabricated)
            return _copiedActions;

        var actions = new List<CharacterAction>
        {
            new CharacterAction("Fabricate", () => Fabricate(manager))
        };

        if (IsPlayerControlled)
            actions.Add(new CharacterAction("Pass", () => Pass(manager)));

        return actions;
    }

    private IEnumerator Fabricate(BattleManager manager)
    {
        Character target = IsPlayerControlled ? manager.GetEnemy(this) : manager.GetRandomAlivePlayer();

        if(target == null)
        {
            manager.dialogue.text = "The actor stares into space.";
            yield return new WaitForSeconds(1.5f);
            manager.OnActionComplete(false);
            yield break;
        }

        manager.dialogue.text = $"{target.CharacterName} is being looked at attentively...";
        yield return new WaitForSeconds(2f);

        
        _copiedActions = new();

        foreach (var action in target.GetActions(manager))
        {
            if (action.actionName != "Pass") 
                _copiedActions.Add(new CharacterAction(action.actionName, action.coroutineCallback));
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
        _staticHasFabricated = false;
    }

    public override List<string> GetDialogueOptions()
    {
        return new List<string> { "...yes?" };
    }

    public override void OnTalk(Character speaker, int optionIndex)
    {
        Manager.dialogue.text = "I... ah...";
    }
}
