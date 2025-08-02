using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    [Header("Visual")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;

    [Header("Effects")]
    [SerializeField] private GameObject deathEffect;
    private NavMeshAgent agent;


    private void Start()
    {
        currentHP = maxHP;
        agent = GetComponent<NavMeshAgent>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
    }

    public void OnDamage(int amount, Vector3 hitDirection, Transform attacker, float knockbackForce = 5f)
    {
        currentHP -= amount;
        FlashHit();
        Debug.Log("Agent Damaged! Current HP = " + currentHP);

        GetComponent<AgentMoveScript>()?.ReactToHit(attacker);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode.Impulse);
            agent.enabled = false;
            Invoke(nameof(ReactivateAgent), 0.2f);
        }
        if (currentHP <= 0)
        {
            Die();
        }
    }
    private void ReactivateAgent()
    {
        agent.enabled = true;
    }

    private void FlashHit()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = hitColor;
            Invoke(nameof(ResetColor), flashDuration);
        }
    }

    private void ResetColor()
    {
       if(enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
