using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class TriggerYarn : MonoBehaviour
{
    public void onTrigger(string node)
    {
        DialogueRunner d = MainSingleton.Instance.dialogueRunner;
        // Abort when we're in dialogue
        if (d.IsDialogueRunning == true)
        {
            return;
        }

        d.StartDialogue(node);
    }
}
