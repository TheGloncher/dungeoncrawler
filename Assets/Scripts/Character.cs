using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string CharacterName = "Georgie"; // Default name
    public int MaxHP = 10; // Default HP
    public int CurrentHP = 10; // Default HP
    public int Speed = 5; // Default speed
    public bool IsPlayerControlled = false; // true for the main player or recruited allies
    public bool IsEnemy = false;             // false if recruited
    public BattleHUD HUD;

    public GameObject FloatingTextPrefab; // Drag your prefab here in the inspector
    private ScreenShake _shake; // Reference to the ScreenShake component

    public BattleManager Manager { get; set; }


    public virtual List<CharacterAction> GetActions(BattleManager manager)
    {
        return new List<CharacterAction>(); // Default: no actions
    }

    public virtual void Start()
    {
        _shake = this.GetComponent<ScreenShake>();
    }


    public virtual bool TakeDamage(int amount, Character attacker)
    {
        CurrentHP -= amount;

        _shake?.TriggerShake(amount * 0.15f);

        if (HUD != null)
            HUD.Shake(amount);

        SpawnFloatingText(amount);

        //update the HUD
        if (HUD != null)
            HUD.SetHUD(this);

        bool isDead = CurrentHP <= 0;



        if (Manager != null && isDead)
            Manager.HandleCharacterDeath(this); //  Handle death right here

        return isDead;
    }

    public void SpawnFloatingText(int damage)
    {
        if (FloatingTextPrefab == null) return;

        Vector3 position = transform.position + Vector3.up; // above the head
        GameObject go = Instantiate(FloatingTextPrefab, position, Quaternion.identity);
        go.GetComponent<FloatingText>().Initialize("-" + damage.ToString(), Color.red);
    }




    public virtual void OnTalk(Character speaker, int optionIndex)
    {
        Debug.Log($"{CharacterName} had no specific response to the dialogue.");
    }

    public virtual List<string> GetDialogueOptions()
    {
        return new List<string>();
    }

    protected IEnumerator Pass(BattleManager manager)
    {
        manager.dialogue.text = $"{CharacterName} waits.";
        yield return new WaitForSeconds(1.5f);
        manager.OnActionComplete(false);
    }


    public bool IsAlive => CurrentHP > 0;

}