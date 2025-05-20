using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }
public class BattleManager : MonoBehaviour
{
    public BattleState State;
    public GameObject EnemyPrefab;
    public GameObject Player1Prefab;
    private Character _player1Entity;
    public GameObject Player2Prefab;
    private Character _player2Entity;
    public GameObject Player3Prefab;
    private Character _player3Entity;
    public GameObject Player4Prefab;
    private Character _player4Entity;

    public Transform Player1Battlestation;
    public Transform Player2Battlestation;
    public Transform Player3Battlestation;
    public Transform Player4Battlestation;
    public Transform EnemyBattlestation;
    private Character _enemyEntity;

    public TextMeshProUGUI dialogue;

    public BattleHUD player1HUD;
    public BattleHUD player2HUD;
    public BattleHUD player3HUD;
    public BattleHUD player4HUD;

    public Button actButton; // Drag in the Inspector

    public List<Button> ActionButtons;
    public List<TextMeshProUGUI> ActionNames;


    private List<Character> turnOrder = new List<Character>();
    private int currentTurnIndex = 0;
    private Character currentCharacter; // who's taking their turn right now

    private List<Character> playerParty = new List<Character>();

    [SerializeField] private GameObject moveSign;
    [SerializeField] private TextMeshProUGUI moveText;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State = BattleState.START;
        StartCoroutine(SetupBattle());
        
    }

    IEnumerator SetupBattle()
    {
        GameObject player1 = Instantiate(Player1Prefab, Player1Battlestation);
        _player1Entity = player1.GetComponent<Character>();
        //check if player2Prefab is not null before instantiating
        if (Player2Prefab != null)
        {
            GameObject player2 = Instantiate(Player2Prefab, Player2Battlestation);
            _player2Entity = player2.GetComponent<Character>();

        }
        if (Player3Prefab != null)
        {
            GameObject player3 = Instantiate(Player3Prefab, Player3Battlestation);
            _player3Entity = player3.GetComponent<Character>();
        }

        if (Player4Prefab != null)
        {
            GameObject player4 = Instantiate(Player4Prefab, Player4Battlestation);
            _player4Entity = player4.GetComponent<Character>();
        }
        playerParty.Clear();
        playerParty.Add(_player1Entity);
        if (_player2Entity != null) playerParty.Add(_player2Entity);
        if (_player3Entity != null) playerParty.Add(_player3Entity);
        if (_player4Entity != null) playerParty.Add(_player4Entity);

        GameObject enemy = Instantiate(EnemyPrefab, EnemyBattlestation);

        _enemyEntity = enemy.GetComponent<Character>();

        _player1Entity.HUD = player1HUD;
        if (_player2Entity != null) _player2Entity.HUD = player2HUD;
        if (_player3Entity != null) _player3Entity.HUD = player3HUD;
        if (_player4Entity != null) _player4Entity.HUD = player4HUD;
        _enemyEntity.HUD = null; // or assign if enemies will have HUDs too

        _player1Entity.Manager = this;
        if (_player2Entity != null) _player2Entity.Manager = this;
        if (_player3Entity != null) _player3Entity.Manager = this;
        if (_player4Entity != null) _player4Entity.Manager = this;
        _enemyEntity.Manager = this;


        dialogue.text = "The " + _enemyEntity.CharacterName + " approaches.";

        player1HUD.SetHUD(_player1Entity);
        if (_player2Entity != null)
        {
            player2HUD.SetHUD(_player2Entity);
        }
        if (_player3Entity != null)
        {
            player3HUD.SetHUD(_player3Entity);
        }
        if (_player4Entity != null)
        {
            player4HUD.SetHUD(_player4Entity);
        }
        yield return new WaitForSeconds(2f);

        turnOrder.Clear();
        turnOrder.Add(_player1Entity);
        if (_player2Entity != null) turnOrder.Add(_player2Entity);
        if (_player3Entity != null) turnOrder.Add(_player3Entity);
        if (_player4Entity != null) turnOrder.Add(_player4Entity);
        turnOrder.Add(_enemyEntity);


        // Sort by speed, with player winning ties
        turnOrder.Sort((a, b) =>
        {
            if (a.Speed == b.Speed)
                return a.IsPlayerControlled ? -1 : 1; // player goes first on tie
            return b.Speed.CompareTo(a.Speed); // highest speed first
        });


        StartNextTurn();


    }
    public Character GetLowestHPPlayer()
    {
        List<Character> lowestHPPlayers = new List<Character>();
        int lowestHP = int.MaxValue;

        foreach (var player in playerParty)
        {
            if (!player.IsAlive)
                continue;

            if (player.CurrentHP < lowestHP)
            {
                lowestHP = player.CurrentHP;
                lowestHPPlayers.Clear();
                lowestHPPlayers.Add(player);
            }
            else if (player.CurrentHP == lowestHP)
            {
                lowestHPPlayers.Add(player);
            }
        }

        if (lowestHPPlayers.Count == 0)
            return null;

        // Choose randomly among those with the lowest HP
        return lowestHPPlayers[UnityEngine.Random.Range(0, lowestHPPlayers.Count)];
    }

    void StartNextTurn()
    {
        if (State == BattleState.WON || State == BattleState.LOST)
            return;

        currentCharacter = turnOrder[currentTurnIndex];
        currentTurnIndex = (currentTurnIndex + 1) % turnOrder.Count;

        if (!currentCharacter.IsAlive)
        {
            StartNextTurn(); // skip dead units
            return;
        }

        if (currentCharacter.IsPlayerControlled)
        {
            State = BattleState.PLAYERTURN;
            actButton.gameObject.SetActive(true);
            PlayerTurn();
        }
        else
        {
            State = BattleState.ENEMYTURN;
            actButton.gameObject.SetActive(false);
            StartCoroutine(EnemyTurn()); //currentCharacter
        }
        Debug.Log("TURN ORDER:");
        foreach (var unit in turnOrder)
        {
            if (unit == null)
            {
                Debug.LogWarning("NULL character in turnOrder!");
            }
            else
            {
                Debug.Log(unit.CharacterName + " | isEnemy: " + unit.IsEnemy + " | isPlayerControlled: " + unit.IsPlayerControlled);
            }
        }
    }

    //IEnumerator PlayerAttack(Character attacker, Character target)
    //{
    //    bool isDead = target.TakeDamage(3); // or attacker.attackPower if you want to expand later
    //    dialogue.text = attacker.characterName + " attacks!";
    //    yield return new WaitForSeconds(1f);

    //    if (isDead)
    //    {
    //        State = BattleState.WON;
    //        EndBattle();
    //    }
    //    else
    //    {
    //        StartNextTurn();
    //    }
    //}

    IEnumerator EnemyTurn()
    {
        List<CharacterAction> actions = currentCharacter.GetActions(this);

        if (actions == null || actions.Count == 0)
        {
            Debug.LogWarning(currentCharacter.CharacterName + " does not know what to do.");
            OnActionComplete(false);
            yield break;
        }

        CharacterAction selectedAction = actions[UnityEngine.Random.Range(0, actions.Count)];

        moveSign.SetActive(true);
        moveText.text = selectedAction.actionName;
        yield return new WaitForSeconds(1f);
        moveSign.SetActive(false);

        // Instead of .Invoke(), cast and run coroutine manually
        IEnumerator moveCoroutine = selectedAction.actionCallback.Method.Invoke(selectedAction.actionCallback.Target, new object[] { }) as IEnumerator;

        if (moveCoroutine != null)
        {
            yield return StartCoroutine(moveCoroutine);
        }
        else
        {
            Debug.LogError("Enemy move is not a coroutine.");
            OnActionComplete(false); // fail safe
        }
    }

    public void UpdateHUDForCharacter(Character character)
    {
        if (character.HUD != null)
            character.HUD.SetHUD(character);
    }

    private void EndBattle()
    {
        if (State == BattleState.WON)
        {
            dialogue.text = "You won the battle!";
        }
        else if (State == BattleState.LOST)
        {
            dialogue.text = "You lost the battle!";
        }
    }

    void PlayerTurn()
    {

        dialogue.text = "Choose an action.";
        ResetHighlight();
        //highlight the hud of the current character
        if (currentCharacter == _player1Entity)
        {
            player1HUD.StartHighlight();
        }
        else if (currentCharacter == _player2Entity)
        {
            player2HUD.StartHighlight();
        }
        else if (currentCharacter == _player3Entity)
        {
            player3HUD.StartHighlight();

        }
        else if (currentCharacter == _player4Entity)
        {

            player4HUD.StartHighlight();
        }
        // Select the attack button as the starting focus
        EventSystem.current.SetSelectedGameObject(actButton.gameObject);

    }

    void ResetHighlight()
    {
        player1HUD.StopHighlight();
        player2HUD.StopHighlight();
        player3HUD.StopHighlight();
        player4HUD.StopHighlight();
    }

    public void OnAttackButton()
    {
        if (State != BattleState.PLAYERTURN)
            return;

        ShowCharacterActions(currentCharacter);
    }

    private void ShowCharacterActions(Character currentCharacter)
    {
        List<CharacterAction> actions = currentCharacter.GetActions(this);

        for (int i = 0; i < ActionButtons.Count; i++)
        {
            if (i < actions.Count)
            {
                int index = i;
                ActionButtons[i].gameObject.SetActive(true);
                ActionNames[i].text = actions[i].actionName;
                ActionButtons[i].onClick.RemoveAllListeners();
                ActionButtons[i].onClick.AddListener(() => actions[index].actionCallback());
            }
            else
            {
                ActionButtons[i].gameObject.SetActive(false);
            }
        }

        // Optionally select the first submenu button
        if (actions.Count > 0)
            EventSystem.current.SetSelectedGameObject(ActionButtons[0].gameObject);
    }

    public Character GetEnemy()
    {
        return _enemyEntity;
    }

    public Character GetAttacker()
    {
        return currentCharacter;
    }

    public void OnActionComplete(bool enemyDied)
    {
        Debug.Log("OnActionComplete was called. Enemy died? " + enemyDied);
        HideSubmenu();
        actButton.gameObject.SetActive(false);

        if (enemyDied)
        {
            State = BattleState.WON;
            EndBattle();
        }
        else
        {
            StartNextTurn();
        }
    }

    public void HideSubmenu()
    {
        foreach (var btn in ActionButtons)
            btn.gameObject.SetActive(false);

    }
}
