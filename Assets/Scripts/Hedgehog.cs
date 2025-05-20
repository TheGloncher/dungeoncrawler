using UnityEngine;

public class HedgehogCharacter : Character
{
    private int approachCount = 0;
    private bool pacified = false;

    public override void OnTalk()
    {

        if (pacified)
        {
            Debug.Log("It allows you to get closer to it... (fight ends, recruited)");
            // Add to party logic
        }
        else if (approachCount == 0)
        {
            Debug.Log("The spines grow longer...");
            approachCount++;
        }
        else if (approachCount == 1)
        {
            Debug.Log("The spines quiver...");
            approachCount++;
        }
        else
        {
            Debug.Log("You are impaled on the spikes. (Take 9 damage)");
            // Call BattleManager to apply 9 damage to the player
        }
    }

    public override bool TakeDamage(int amount, Character attacker)
    {
        bool isDead = base.TakeDamage(amount, attacker);

        // Retaliate only if attacker is valid and still alive
        if (attacker != null && attacker != this && attacker.IsAlive && !isDead)
        {
            
            attacker.TakeDamage(2, this);
            Manager.UpdateHUDForCharacter(attacker); // update the attacker's HUD!
        }

        return isDead;
    }

    public void CalmStatement()
    {
        pacified = true;
        Debug.Log("The spikes slowly retract...");
    }
}