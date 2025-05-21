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
    public GameObject dialogueBox; // Drag your dialogue box here in the inspector

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

    public List<Button> DialogueButtons;
    public List<TextMeshProUGUI> DialogueTexts;

    private Character _dialogueSpeaker;

    private bool dialogueResolved = false;
    public bool IsAwaitingDialogue => DialogueButtons.Exists(b => b.gameObject.activeSelf) && !dialogueResolved;

    private bool isRecruiting = false;

    public AudioSource AudioSource;
    public AudioClip DeathSfx;





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State = BattleState.START;
        StartCoroutine(SetupBattle());
        
    }

    //method that locks the cursor and hides the cursor
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    public Character GetRandomAlivePlayer()
    {
        List<Character> alivePlayers = new List<Character>();

        foreach (var player in playerParty)
        {
            if (player.IsAlive)
                alivePlayers.Add(player);
        }

        if (alivePlayers.Count == 0)
            return null;

        return alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
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
            ResetHighlight();
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
        //clear the dialogue text
        dialogue.text = "";

        moveSign.SetActive(true);
        moveText.text = selectedAction.actionName;
        yield return new WaitForSeconds(1.5f);
        moveSign.SetActive(false);

        //  Directly yield the coroutine
        yield return StartCoroutine(selectedAction.coroutineCallback());
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

                // Start coroutine on button click
                ActionButtons[i].onClick.AddListener(() =>
                {
                    StartCoroutine(ExecutePlayerAction(actions[index]));
                });
            }
            else
            {
                ActionButtons[i].gameObject.SetActive(false);
            }
        }

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
            if (_enemyEntity != null)
            {
                Destroy(_enemyEntity.gameObject); // or use SetActive(false) if you prefer
                _enemyEntity = null;
            }

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

    private IEnumerator ExecutePlayerAction(CharacterAction action)
    {
        HideSubmenu();
        actButton.gameObject.SetActive(false);

        moveSign.SetActive(true);
        
        moveText.text = action.actionName;
        yield return new WaitForSeconds(1.5f);
        moveSign.SetActive(false);

        yield return StartCoroutine(action.coroutineCallback());
    }

    public void ShowDialogueOptions(List<string> options, Character speaker)
    {
        HideHUD();
        Debug.Log("ShowDialogueOptions called for " + speaker.CharacterName);
        _dialogueSpeaker = speaker; // The one who chose to talk
        dialogueResolved = false; // <- reset the flag
        actButton.gameObject.SetActive(false);

        for (int i = 0; i < DialogueButtons.Count; i++)
        {
            if (i < options.Count)
            {
                DialogueButtons[i].gameObject.SetActive(true);
                DialogueTexts[i].text = options[i];

                int index = i;
                DialogueButtons[i].onClick.RemoveAllListeners();
                DialogueButtons[i].onClick.AddListener(() =>
                {
                    OnDialogueOptionSelected(index);
                });
            }
            else
            {
                DialogueButtons[i].gameObject.SetActive(false);
            }
        }

        StartCoroutine(SelectFirstDialogueButton());

    }
    private IEnumerator SelectFirstDialogueButton()
    {
        yield return null; // wait one frame to let Unity process the new active buttons

        if (DialogueButtons.Count > 0 && DialogueButtons[0].gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(DialogueButtons[0].gameObject);
        }
    }

    public void HideHUD()
    {
        player1HUD.HUD.SetActive(false);
        if (_player2Entity != null) player2HUD.HUD.SetActive(false);
        if (_player3Entity != null) player3HUD.HUD.SetActive(false);
        if (_player4Entity != null) player4HUD.HUD.SetActive(false);
        //and the dialogue box should be hidden too
        dialogueBox.SetActive(false);
        dialogue.text = "";

    }
    public void UnhideHUD()
    {
        player1HUD.HUD.SetActive(true);
        if (_player2Entity != null) player2HUD.HUD.SetActive(true);
        if (_player3Entity != null) player3HUD.HUD.SetActive(true);
        if (_player4Entity != null) player4HUD.HUD.SetActive(true);
        //and the dialogue box should be hidden too
        dialogueBox.SetActive(true);
    }
    public void HideDialogueOptions()
    {
        foreach (var button in DialogueButtons)
            button.gameObject.SetActive(false);
    }

    public void OnDialogueOptionSelected(int index)
    {
        HideDialogueOptions();
        UnhideHUD();

        Character enemy = GetEnemy();
        if (_dialogueSpeaker == null || enemy == null)
        {
            Debug.LogWarning("Missing speaker or target for dialogue!");
            dialogueResolved = true; // <- even on fallback, resolve dialogue
            OnActionComplete(false);
            return;
        }

        enemy.OnTalk(_dialogueSpeaker, index);

        dialogueResolved = true; // <- flag the dialogue as finished
        StartCoroutine(EndTalkTurn());
    }
    private IEnumerator EndTalkTurn()
    {
        yield return new WaitForSeconds(2f);
       
        // Don't end the turn if a recruitment is still in progress
        if (!isRecruiting)
            OnActionComplete(false);
    }

    public void RecruitEnemy(Character enemy)
    {
        StartCoroutine(HandleRecruitment(enemy));
    }

    private IEnumerator HandleRecruitment(Character enemy)
    {
        isRecruiting = true;
        dialogue.text = $"You have recruited {enemy.CharacterName}!";

        if (_player2Entity == null)
        {
            _player2Entity = enemy;
            playerParty.Add(enemy);
            player2HUD.HUD.SetActive(true);
            enemy.HUD = player2HUD;
            enemy.transform.position = Player2Battlestation.position;
        }
        else if (_player3Entity == null)
        {
            _player3Entity = enemy;
            playerParty.Add(enemy);
            player3HUD.HUD.SetActive(true);
            enemy.HUD = player3HUD;
            enemy.transform.position = Player3Battlestation.position;
        }
        else if (_player4Entity == null)
        {
            _player4Entity = enemy;
            playerParty.Add(enemy);
            player4HUD.HUD.SetActive(true);
            enemy.HUD = player4HUD;
            enemy.transform.position = Player4Battlestation.position;
        }
        else
        {
            dialogue.text = "You don't have room for more allies.";
            yield return new WaitForSeconds(2f);
            isRecruiting = false;
            OnActionComplete(false);
            yield break;
        }

        enemy.HUD.SetHUD(enemy);
        enemy.IsPlayerControlled = true;
        enemy.IsEnemy = false;
        _enemyEntity = null;

        yield return new WaitForSeconds(2f);

        if (_enemyEntity == null)
        {
            State = BattleState.WON;
            EndBattle();
        }
        else
        {
            OnActionComplete(false);
        }

        isRecruiting = false;
    }

    public void HandleCharacterDeath(Character character)
    {
        
        AudioSource.PlayOneShot(DeathSfx);
        if (character == _player1Entity)
        {
            State = BattleState.LOST;
            dialogue.text = "You have fallen...";
            EndBattle();
            return;
        }

        // Remove from party and HUD
        if (character == _player2Entity)
        {
            player2HUD.HUD.SetActive(false);
            playerParty.Remove(_player2Entity);
            Destroy(_player2Entity.gameObject); // destroy or deactivate
            _player2Entity = null;
        }
        else if (character == _player3Entity)
        {
            player3HUD.HUD.SetActive(false);
            playerParty.Remove(_player3Entity);
            Destroy(_player3Entity.gameObject);
            _player3Entity = null;
        }
        else if (character == _player4Entity)
        {
            player4HUD.HUD.SetActive(false);
            playerParty.Remove(_player4Entity);
            Destroy(_player4Entity.gameObject);
            _player4Entity = null;
        }

        dialogue.text = $"{character.CharacterName} has fallen...";
    }





}
