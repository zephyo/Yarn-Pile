using UnityEngine;
using System;
using DG.Tweening;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class Extensions
{
    const float menuAnimationDuration = 0.4f;
    public static void TurnOn(this CanvasGroup group)
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public static void TurnOff(this CanvasGroup group)
    {
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public static void FadeOut(this CanvasGroup group, Action onComplete = null)
    {
        DOTween.Kill(group.GetInstanceID());
        DOTween.Sequence().Append(group.DOFade(group.alpha, 0)).AppendCallback(() =>
        {
            group.TurnOff();
            if (onComplete != null)
                onComplete();
        }).SetId(group.GetInstanceID())
        .Play();
    }

    public static void FadeIn(this CanvasGroup group, Action onComplete = null)
    {
        DOTween.Kill(group.GetInstanceID());
        DOTween.Sequence().Append(group.DOFade(group.alpha, 1)).AppendCallback(() =>
       {
           group.TurnOn();
           if (onComplete != null)
               onComplete();
       }).SetId(group.GetInstanceID())
       .Play();
    }

    public static string YarnFormat(this string str)
    {
        if (str[0] != '$') return '$' + str;
        return str;
    }
}