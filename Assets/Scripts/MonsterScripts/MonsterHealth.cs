using UnityEngine;
using System;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int maxHP;
    public int MaxHP => maxHP;

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int, bool> OnDamaged;
    public event Action OnDead;

    private void Awake()
    {
        CurrentHP = maxHP;
        IsDead = false;
    }

    public void TakeDamage(int damage, bool isCritical = false)
    {
        if (IsDead)
            return;

        CurrentHP -= damage;
        CurrentHP = Mathf.Max(CurrentHP, 0);

        OnDamaged?.Invoke(damage, isCritical);

        if(CurrentHP <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        OnDead?.Invoke();
    }
}
