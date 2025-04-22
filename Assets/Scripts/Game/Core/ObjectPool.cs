using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


/// <summary>
/// �����
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
    /// �Ӷ�����з���һ���µĶ���
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        var obj = instances.Count > 0 ? instances.Pop() : CreateInstance();
        obj.SetActive(true);
        return obj;
    }


    /// <summary>
    /// ��ָ������Ϸ���󷵻ص�����Դ�Ķ������
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
    /// �����������Ϊ���ʼ״̬
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
    /// �����ض������͵���ʵ��
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
/// ���ڱ�ʶ�ض���ص�ʵ�ó�����
/// </summary>
public class PooledObject : MonoBehaviour
{
    public ObjectPool pool;
}
