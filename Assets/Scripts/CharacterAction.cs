// CharacterAction.cs
using System;
using System.Collections;
using UnityEngine;

public class CharacterAction
{
    public string actionName;
    public Func<Character, IEnumerator> coroutineWithUser;

    public CharacterAction(string name, Func<Character, IEnumerator> callback)
    {
        actionName = name;
        coroutineWithUser = callback;
    }

    public IEnumerator Invoke(Character user)
    {
        return coroutineWithUser(user);
    }
}