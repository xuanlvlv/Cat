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
    /// 当此图块实体在某个关卡上生成时调用
    /// </summary>
    public virtual void Spawn()
    {
        onSpawn.Invoke();
    }


    /// <summary>
    /// 当此图块实体在某个关卡上爆炸时调用
    /// </summary>
    public virtual void Explode()
    {
        onExplode.Invoke();
    }
}
