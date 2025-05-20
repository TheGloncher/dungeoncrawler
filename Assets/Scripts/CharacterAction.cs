using System;
using UnityEngine;

public class CharacterAction
{

    public string actionName;
    public Action actionCallback;

    public CharacterAction(string name, Action callback)
    {
        actionName = name;
        actionCallback = callback;
    }
}
