using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Notification : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    [SerializeField]
    Animator animator;

    void OnValidate()
    {
        text = GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void Notify(string notify)
    {
        text.text = notify;
        gameObject.SetActive(true);
        StartCoroutine(WaitThenInactive());
    }

    IEnumerator WaitThenInactive()
    {
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);
        gameObject.SetActive(false);
    }
}
