using UnityEngine;
using System.Collections.Generic;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance;

    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private int initialPoolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Transform worldCanvas;

    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (isInitialized)
            return;

        GameObject canvasObj = GameObject.Find("WorldCanvas");
        if (canvasObj == null)
        {
            Debug.LogError("WorldCanvas를 찾을 수 없습니다.");
            return;
        }

        Initialize(canvasObj.transform);
    }

    private void Initialize(Transform canvas)
    {
        if (isInitialized)
            return;

        worldCanvas = canvas;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }

        isInitialized = true;
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
        if (!isInitialized)
            return null;

        if (pool.Count == 0)
            CreateNewObject();

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null || worldCanvas == null)
            return;

        MonsterDamageText dt = obj.GetComponent<MonsterDamageText>();
        if (dt != null)
            dt.ResetState();

        obj.transform.SetParent(worldCanvas);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}