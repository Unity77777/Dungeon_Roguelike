using UnityEngine;
using System.Collections.Generic;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance;
    public GameObject damageTextPrefab;
    public int initialPoolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform worldCanvas;

    void Awake()
    {
        // 싱글턴 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

            Instance = this;
    }

    public void Initialize(Transform canvas)
    {
        worldCanvas = canvas;
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(damageTextPrefab, worldCanvas);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }
    
    public GameObject Get()
    {
        if (pool.Count == 0)
            CreateNewObject();

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        MonsterDamageText dt = obj.GetComponent<MonsterDamageText>();
        if(dt != null)
            dt.ResetState();
        
        obj.transform.SetParent(worldCanvas);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
