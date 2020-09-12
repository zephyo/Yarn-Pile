using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CharTween;
using DG.Tweening;
using System;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TweenText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private CharTweener _tweener;

    // Animations applied to last dialogue. Clear this before showing a new line of dialogue
    List<Tweener> lastAnimations = new List<Tweener>();

    // Sequence that plays when showing dialogue for that 'fade-in typewriter' effect
    Sequence showSequence;

    const float MIN_PUNCTUATION_DELAY = 0.2f;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        _tweener = text.GetCharTweener();
    }

    public void Clear()
    {
        foreach (var tween in lastAnimations)
        {
            tween.fullPosition = 0;
            if (tween.IsActive())
                tween.Kill();
        }
        lastAnimations.Clear();
    }

    // Animation to typewrite text
    // Returns total time it'll take for this animation to complete
    public Sequence Show(string s, float duration, float delay = 0)
    {
        text.text = s;

        if (duration > 0.011f)
        {
            text.ForceMeshUpdate();
            int count = text.textInfo.characterCount;

            // Create new animation
            showSequence = DOTween.Sequence();
            float puncDelay = Mathf.Max(duration * 1.5f, MIN_PUNCTUATION_DELAY);
            float charDelays = 0;

            for (var i = 0; i < count; i++)
            {
                var timeOffset = Mathf.Lerp(0, 1, (i) / (float)(count + 1));
                var charSequence = DOTween.Sequence();
                charSequence.Append(_tweener.DOFade(i, 0, duration * 2f + 0.2f).From().SetEase(Ease.InOutCubic))
                    .Join(_tweener.DOScale(i, 0, duration).From().SetEase(Ease.OutBack, 5))
                    .SetDelay(charDelays);

                char c = text.textInfo.characterInfo[i].character;
                if (c == '.' || c == ',' || c == ':' || c == ';' || c == '!' || c == '?')
                {
                    charDelays += puncDelay;
                }

                showSequence.Insert(timeOffset, charSequence);
            }

            showSequence.SetDelay(delay).Play();
            return showSequence;
        }
        return null;
    }

    // Show all text, skip 'Show' animation
    public void KillShow()
    {
        if (showSequence != null)
        {
            if (showSequence.IsActive())
                showSequence.Kill(true);
            showSequence = null;
        }
    }

    public void Wave(int start, int end, float speed, float radius = 6)
    {
        for (var i = start; i <= end; ++i)
        {
            var timeOffset = Mathf.Lerp(0, 1, (i - start) / (float)(end - start + 1));
            var circleTween = _tweener.DOCircle(i, radius, speed)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
            circleTween.fullPosition = timeOffset;
            lastAnimations.Add(circleTween);
        }
    }

    public void Shake(int start, int end, float strength, int vibrato = 20, float randomness = 20)
    {
        for (var i = start; i <= end; ++i)
        {
            var posTween = _tweener.DOShakePosition(i, 1, strength, vibrato, randomness, false, false)
                .SetLoops(-1, LoopType.Restart);
            lastAnimations.Add(posTween);

        }
    }
}
