using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/// <summary>
/// 对象池
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize;

    private readonly Stack<GameObject> instances = new Stack<GameObject>();

    private void Awake()
    {
        Assert.IsNotNull(prefab);
    }

    private void Start()
    {
        for (var i = 0; i < initialSize; i++)
        {
            var obj = CreateInstance();
            obj.SetActive(false);
            instances.Push(obj);
        }
    }


    /// <summary>
    /// 从对象池中返回一个新的对象
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        var obj = instances.Count > 0 ? instances.Pop() : CreateInstance();
        obj.SetActive(true);
        return obj;
    }


    /// <summary>
    /// 将指定的游戏对象返回到其来源的对象池中
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnObject(GameObject obj)
    {
        var pooledObject = obj.GetComponent<PooledObject>();
        Assert.IsNotNull(pooledObject);
        Assert.IsTrue(pooledObject.pool == this);

        obj.SetActive(false);
        instances.Push(obj);
    }


    /// <summary>
    /// 将对象池重置为其初始状态
    /// </summary>
    public void Reset()
    {
        var objectsToReturn = new List<GameObject>();
        foreach (var instance in transform.GetComponentsInChildren<PooledObject>())
        {
            if (instance.gameObject.activeSelf)
            {
                objectsToReturn.Add(instance.gameObject);
            }
        }
        foreach (var instance in objectsToReturn)
        {
            ReturnObject(instance);
        }
    }

    /// <summary>
    /// 创建池对象类型的新实例
    /// </summary>
    /// <returns></returns>
    private GameObject CreateInstance()
    {
        var obj = Instantiate(prefab);
        var pooledObject = obj.AddComponent<PooledObject>();
        pooledObject.pool = this;
        obj.transform.SetParent(transform);
        return obj;
    }
}


/// <summary>
/// 用于标识池对象池的实用程序类
/// </summary>
public class PooledObject : MonoBehaviour
{
    public ObjectPool pool;
}
