using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a character's animation
/// Managed by CharacterManager
/// 
/// The animator should have one layer setting visibility,
/// and one layer settings sprites/expressions
/// 
/// The animator controller for the animator should
/// be an override animator controller, referencing Character.controller
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterUI : MonoBehaviour
{
    public Animator animator;

    readonly int VisibleHash = Animator.StringToHash("Visible");
    void OnValidate()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(Expression e)
    {
        animator.SetBool(VisibleHash, true);
        ChangeExpression(e);
    }

    public void PositionDialogue(DialogueGroup group)
    {
        RectTransform rect = (RectTransform)group.transform;
        rect.SetParent(transform);
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition3D = Vector3.zero;
        rect.localScale = Vector3.one;
    }

    public void ChangeExpression(Expression e)
    {
        animator.SetTrigger(e.ToString());
    }

    public void Exit(System.Action onComplete)
    {
        // Check if we're a parent of a dialogue group - if so, unattach
        DialogueGroup group = GetComponentInChildren<DialogueGroup>();
        if (group != null)
        {
            group.transform.SetParent(MainSingleton.Instance.dialogueRunner.transform.root);
        }
        StartCoroutine(RunExit(onComplete));
    }

    private IEnumerator RunExit(System.Action onComplete)
    {
        animator.SetBool(VisibleHash, false);
        yield return null;

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        onComplete();
    }
}
