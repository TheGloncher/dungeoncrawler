// CharacterAction.cs
using System;
using System.Collections;
using UnityEngine;

public class CharacterAction
{
    public string actionName;
    public Func<IEnumerator> coroutineCallback;  // Coroutine-compatible

    public CharacterAction(string name, Func<IEnumerator> coroutineCallback)
    {
        this.actionName = name;
        this.coroutineCallback = coroutineCallback;
    }
}