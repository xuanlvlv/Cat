using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileEntity : MonoBehaviour
{
    public UnityEvent onSpawn;
    public UnityEvent onExplode;


    public virtual void OnEnable()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var newColor = spriteRenderer.color;
            newColor.a = 1.0f;
            spriteRenderer.color = newColor;
        }
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        transform.localRotation = Quaternion.identity;
    }


    public virtual void OnDisable()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var newColor = spriteRenderer.color;
            newColor.a = 1.0f;
            spriteRenderer.color = newColor;
        }
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// ����ͼ��ʵ����ĳ���ؿ�������ʱ����
    /// </summary>
    public virtual void Spawn()
    {
        onSpawn.Invoke();
    }


    /// <summary>
    /// ����ͼ��ʵ����ĳ���ؿ��ϱ�ըʱ����
    /// </summary>
    public virtual void Explode()
    {
        onExplode.Invoke();
    }
}
