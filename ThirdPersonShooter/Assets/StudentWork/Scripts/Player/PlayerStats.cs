using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Shown Stats")]
    [Tooltip("Set player information")]
    [SerializeField] private int MaxHP = 100;
    [SerializeField] private float BaseDamage = 5f;
    [SerializeField] private float BonusDamage = 0f;
    private int currentHP;
    private int recoveredHP;

    [Header("PlayerUI")]
    [Tooltip("Set Player UI")]
    [SerializeField] private Image HP;
    [SerializeField] private Image DelayedHP;

    [Header("Delayed Effect Settings")]
    [SerializeField] private float delaySpeed = 0.5f;

    private float targetFillAmount;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        currentHP = MaxHP;
        targetFillAmount = 1f;
        UpdateUI(true);
    }

    public void InitializeStats()
    {
        currentHP = MaxHP;
        targetFillAmount = 1f;
        UpdateUI(true);
    }

    private void Update()
    {
        if (DelayedHP != null)
        {
            float delta = delaySpeed * Time.unscaledDeltaTime;

            if (DelayedHP.fillAmount > targetFillAmount)
            {
                DelayedHP.fillAmount -= delaySpeed * delta;
                DelayedHP.fillAmount = Mathf.Max(DelayedHP.fillAmount, targetFillAmount);
            }
            else if (DelayedHP.fillAmount < targetFillAmount)
            {
                DelayedHP.fillAmount += delaySpeed * delta;
                DelayedHP.fillAmount = Mathf.Min(DelayedHP.fillAmount, targetFillAmount);
            }
        }
    }
    public void TakeDamage(int amount, Vector3 hitDirection, float knockbackForce = 10f)
    {
        currentHP = Mathf.Max(currentHP - amount, 0);
        targetFillAmount = (float)currentHP / MaxHP;
        UpdateUI(false);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(hitDirection.normalized * knockbackForce, ForceMode.Impulse);
        }
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public float GetTotalDamage()
    {
        return BaseDamage + BonusDamage;
    }

    public void AddBonusDamage(float amount)
    {
        BonusDamage += amount;
        Debug.Log("Bonus Damage Added! New Total: " + GetTotalDamage());
    }

    private void Die()
    {
        Debug.Log("Player Died");
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            LevelManager.Instance.HandlePlayerDeath();
        }
        else
        {
            Debug.LogWarning("LevelManager not found.");
        }
    }

    public void RecoverHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, MaxHP);
        targetFillAmount = (float)currentHP  / MaxHP;
        UpdateUI(false);
        Debug.Log("Potion Used!" + amount + "HP recovered! Current HP = " + currentHP);
    }

    private void UpdateUI(bool instantUpdate)
    {
        
        if (HP != null)
        {
            float fill = (float)currentHP / MaxHP;
            HP.fillAmount = fill;

            if (instantUpdate && DelayedHP != null)
            {
                DelayedHP.fillAmount = fill;
            }
        }
    }
}
