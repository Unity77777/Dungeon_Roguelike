using UnityEngine;
using System.Collections.Generic;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance;
    public GameObject damageTextPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    public int initialPoolSize = 30;
    // 풀에서 가져옴
    // 월드 좌표배치
    // setup 으로 텍스트와 색상 설정
    // update에서 위로 떠오름
    // lifetime 후 풀로 반환

    void Awake()
    {
        // 싱글턴 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

            Instance = this;

        for(int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(damageTextPrefab, transform);
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
        
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
