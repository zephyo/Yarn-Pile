using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// In each new scene, attach a VisualsManager to the root canvas
/// </summary>
public class VisualsManager : SceneSetup
{
    public Sprite[] sprites; // big list of all instantianted sprites
    public Image bgImage;

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string>("View", DoViewChange);
    }

    private void OnDestroy()
    {
        DialogueRunner runner = MainSingleton.Instance?.dialogueRunner;
        if (runner)
        { runner.RemoveCommandHandler("View"); }
    }

    public void DoViewChange(string spriteName)
    {
        bgImage.sprite = FetchSprite(spriteName);
    }

    private Sprite FetchSprite(string assetName)
    {
        foreach (var spr in sprites)
        {
            if (spr.name == assetName)
            {
                return spr;
            }
        }
        return null;
    }
}
