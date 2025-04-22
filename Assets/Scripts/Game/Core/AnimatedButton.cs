using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimatedButton : UIBehaviour,IPointerClickHandler
{
    [Serializable]
    public class ButtonClickedEvent : UnityEvent
    {
    }

    public bool interactable = true;

    [SerializeField]
    private ButtonClickedEvent m_onClick = new ButtonClickedEvent();

    private Animator animator;

    private bool blockInput;

    public ButtonClickedEvent onClick
    {
        get { return m_onClick; }
        set { m_onClick = value; }
    }


    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

 
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !interactable)
        {
            return;
        }

        if (!blockInput)
        {
            blockInput = true;
            Press();
            StartCoroutine(BlockInputTemporarily());
        }
    }


    private void Press()
    {
        if (!IsActive())
        {
            return;
        }

        animator.SetTrigger("Pressed");
        StartCoroutine(InvokeOnClickAction());
    }

    /// <summary>
    /// 调用与按钮按下关联的自定义用户定义事件
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator InvokeOnClickAction()
    {
        yield return new WaitForSeconds(0.1f);
        m_onClick.Invoke();
    }

    /// <summary>
    /// 暂时阻止输入以防止垃圾输入
    /// </summary>
    /// <returns>The coroutine.</returns>
    private IEnumerator BlockInputTemporarily()
    {
        yield return new WaitForSeconds(0.5f);
        blockInput = false;
    }
}
