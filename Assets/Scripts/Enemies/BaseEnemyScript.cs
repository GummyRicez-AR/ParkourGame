using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemyScript : TargetScr
{
    public EnemyInfo enemyInfo;
    public float walkSpeed = 2;
    public Vector3 modelRotationEulerOffset;
    [SerializeField] private float attackCooldown = 2;
    [SerializeField] private float attackRange = 8;
    private float cooldown;
    private bool attacking;
    private Rigidbody rb;
        
    protected Animator animator;
    protected PlayerController targetPlayer;
    protected PlayerController[] players;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        cooldown = 0;
        attacking = false;
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        animator = GetComponent<Animator>();
        InvokeRepeating("PickTargetPlayer", 0, 1);
    }

    protected virtual void Update()
    {
        if (!attacking)
        {
            MoveTowardsTarget();
        }

        if (cooldown <= 0 && Vector3.Distance(transform.position, targetPlayer.transform.position) < attackRange)
        {
            StartCoroutine(Attack());
        } else if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
        }
    }
    
    protected override void TakeDamage(float damageDealt, PlayerController playerDealingDamage)
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
            playerDealingDamage.levelStats.EnemyKilled(enemyInfo);
            Destroy(gameObject);
        }
    }

    public void SlideKick(float damageDealt, PlayerController playerDealingDamage, float kickStrength)
    {
        TakeDamage(damageDealt, playerDealingDamage);
        if (!enemyInfo.flying)
        {
            rb.AddForce(Vector3.up * kickStrength, ForceMode.Impulse);
        }
    }

    protected virtual void PickTargetPlayer()
    {
        PlayerController newTarget = players[0];
        float nearestTargetDistance = Vector3.Distance(transform.position, players[0].transform.position);

        foreach (PlayerController plr in players)
        {
            if (Vector3.Distance(transform.position, plr.transform.position) < nearestTargetDistance)
            {
                nearestTargetDistance = Vector3.Distance(transform.position, plr.transform.position);
                newTarget = plr;
            }
        }

        targetPlayer = newTarget;
    }

    protected virtual void MoveTowardsTarget()
    {
        transform.LookAt(new Vector3(targetPlayer.transform.position.x, transform.position.y, targetPlayer.transform.position.z));
        transform.Translate(new Vector3(0, 0, walkSpeed) * Time.deltaTime);
        transform.rotation = transform.rotation * Quaternion.Euler(modelRotationEulerOffset);
        
    }

    private IEnumerator Attack()
    {
        animator.SetTrigger("Attack");
        cooldown = attackCooldown;
        attacking = true;
        yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
        {
            yield return null;
        }
        StartCoroutine(BurstForwardMovement(0.5f, 5));
        
        yield return new WaitForSeconds(1);
        attacking = false;
    }

    private IEnumerator BurstForwardMovement(float time, float distance)
    {
        Quaternion facingDirection = transform.rotation * Quaternion.Euler(-modelRotationEulerOffset);
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime / time;
            transform.position += (facingDirection) * ((-(2 * progress) + 2) * Time.deltaTime * new Vector3(0, 0, distance));
            yield return null;
        }
    }
}
