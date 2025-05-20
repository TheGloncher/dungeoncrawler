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
        CurrentHP = CurrentHP - amount;

        _shake.TriggerShake(amount*0.15f); // shake the screen a little bit

        if (HUD != null)
            HUD.Shake(amount); //stronger the more damage

        SpawnFloatingText(amount);

        if (CurrentHP <= 0)
            return true;
        else
            return false;



    }

    public void SpawnFloatingText(int damage)
    {
        if (FloatingTextPrefab == null) return;

        Vector3 position = transform.position + Vector3.up; // above the head
        GameObject go = Instantiate(FloatingTextPrefab, position, Quaternion.identity);
        go.GetComponent<FloatingText>().Initialize("-" + damage.ToString(), Color.red);
    }

    public virtual void OnTalk()
    {
        Debug.Log($"{CharacterName} was talked to.");
    }

    public bool IsAlive => CurrentHP > 0;

    public void DisplayMessage(string message, float duration)
    {
        if (Manager != null)
            Manager.StartCoroutine(Manager.ShowMessage(message, duration));
        else
            Debug.LogWarning("No BattleManager assigned to Character when trying to display message.");
    }
}