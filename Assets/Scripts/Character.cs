using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Character : MonoBehaviour
{
    public string CharacterName = "Georgie"; // Default name
    public int MaxHP = 100; // Default HP
    public int CurrentHP = 10; // Default HP
    public int Speed = 5; // Default speed
    [SerializeField] private bool _isPlayerControlled = false;
    [SerializeField] private bool _isEnemy = true;
    [SerializeField] private AudioClip _recruited;
    public bool IsCondemned { get; set; } = false; // Used to track if the character is condemned


    public bool IsPlayerControlled
    {
        get => _isPlayerControlled;
        set => _isPlayerControlled = value;
    }

    public bool IsEnemy
    {
        get => _isEnemy;
        set => _isEnemy = value;
    }
    public BattleHUD HUD;
    public string PrefabName; // set by spawner when instantiated

    public GameObject FloatingTextPrefab; // Drag your prefab here in the inspector
    protected ScreenShake _shake; // Reference to the ScreenShake component

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
        if (IsCondemned)
            amount += 2;

        CurrentHP -= amount;

        _shake?.TriggerShake(amount * 0.15f);

        if (_shake == null)
            Debug.LogWarning($"{CharacterName} has no ScreenShake assigned!");
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

    protected IEnumerator Pass(BattleManager manager, Character self)
    {

        manager.dialogue.text = $"{CharacterName} waits.";
        yield return new WaitForSeconds(1.5f);
        manager.OnActionComplete(false);
    }

    protected IEnumerator DelayedRecruitment()
    {
        yield return new WaitForSeconds(2f);
        Manager.AudioSource.PlayOneShot(_recruited);
        Manager.RecruitEnemy(this);

    }

    public void Condemn()
    {
        IsCondemned = true;
        //change the color of the character to indicate condemnation
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.blue; // Change to red to indicate condemnation
        }
        else
        {
            Debug.LogWarning($"{CharacterName} has no Renderer component to change color.");
        }
    }

    public void ClearCondemn()
    {
        IsCondemned = false;
        // Reset the color of the character to normal
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white; // Reset to white or original color
        }
        else
        {
            Debug.LogWarning($"{CharacterName} has no Renderer component to reset color.");
        }
    }

    public bool IsAlive => CurrentHP > 0;

}