using UnityEngine;
using System.Collections.Generic;

public class TargetScr : MonoBehaviour
{
    public float health = 20;
    protected float maxHealth;
    protected readonly Dictionary<PlayerController, float> playerDamageCount = new();
    protected float chipReward;

    protected virtual void Start()
    {
        maxHealth = health;
        chipReward = health / 4f;
    }

    protected virtual void TakeDamage(float damageDealt, PlayerController playerDealingDamage)
    {
        health -= damageDealt;
        if (!playerDamageCount.TryAdd(playerDealingDamage, damageDealt))
        {
            playerDamageCount[playerDealingDamage] += damageDealt;
        }

        if (health <= 0)
        {
            foreach (KeyValuePair<PlayerController, float> pair in playerDamageCount)
            {
                pair.Key.inventory.chips += (int)(chipReward * (pair.Value / maxHealth));
            }
            playerDealingDamage.levelStats.TargetHit();
            Destroy(gameObject);
        }
    }

    public virtual void GunShot(float damageDealt, PlayerController playerDealingDamage)
    {
        TakeDamage(damageDealt, playerDealingDamage);
    }
}
