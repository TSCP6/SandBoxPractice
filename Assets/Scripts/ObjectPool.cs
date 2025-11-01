using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    public int initialSize = 20;
    public GameObject[] prefabs;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializePool();
    }

    void InitializePool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(GameObject prefab in prefabs)
        {
            if (prefab == null) continue;

            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for(int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.name = prefab.name;
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            poolDictionary.Add(prefab.name, objectQueue);
        }
    }

    //从对象池中获取一个对象
    public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefabName))
        {
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[prefabName];
        GameObject objectToSpawn;
        if(objectQueue.Count > 0)
        {
            //从池中国获取对象
            objectToSpawn = objectQueue.Dequeue();
        }
        else //空池
        {
            GameObject prefab = GetPrefabByName(prefabName);
            if(prefab == null) return null;

            objectToSpawn = Instantiate(prefab);
            objectToSpawn.name = prefab.name;
        }
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void Return(GameObject obj)
    {
        string prefabName = obj.name;

        if(!poolDictionary.ContainsKey(prefabName))
        {
            Destroy(obj);
            return;
        }
        obj.SetActive(false);
        poolDictionary[prefabName].Enqueue(obj);
    }

    private GameObject GetPrefabByName(string prefabName)
    {
        foreach(var prefab in prefabs)
        {
            if(prefab.name == prefabName) return prefab;
        }
        return null;
    }
}
