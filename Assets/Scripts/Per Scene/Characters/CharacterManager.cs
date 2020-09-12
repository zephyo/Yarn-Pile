using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using Yarn.Unity;

public class CharacterManager : SceneSetup
{
    [System.Serializable]
    class CharacterUiDictionary : SerializableDictionaryBase<CharacterName, CharacterUI> { }

    [SerializeField]
    private CharacterUiDictionary characterUiDictionary = new CharacterUiDictionary();

    CharacterName ParseChar(string name) => (CharacterName)Enum.Parse(typeof(CharacterName), name);

    Expression ParseExpress(string express) => (Expression)Enum.Parse(typeof(Expression), express);

    private void Start()
    {
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        runner.AddCommandHandler<string, string>("Show", ShowCharacter);
        runner.AddCommandHandler<string, string>("Change", ChangeCharacterExpression);
        runner.AddCommandHandler<string>("Hide", HideCharacter);
    }

    private void OnDestroy()
    {
        DialogueRunner runner = MainSingleton.Instance?.dialogueRunner;
        if (runner)
        {
            runner.RemoveCommandHandler("Show");
            runner.RemoveCommandHandler("Change");
            runner.RemoveCommandHandler("Hide");
        }
    }

    public void PositionDialogue(DialogueGroup group, string c)
    {
        CharacterName character = ParseChar(c);
        characterUiDictionary[character].PositionDialogue(group);
#if UNITY_EDITOR
        if (!characterUiDictionary.ContainsKey(character))
        {
            Debug.LogError($"Asking to position dialogue above {character.ToString()} but they're not being shown in scene {gameObject.scene.name}");
            return;
        }
#endif
    }

    public void ShowCharacter(string c, string e)
    {
        CharacterName character = ParseChar(c);
        Expression expression = ParseExpress(e);

        CharacterUI ui = characterUiDictionary[character];
        ui.gameObject.SetActive(true);
        ui.Init(expression);
#if UNITY_EDITOR
        if (!characterUiDictionary.ContainsKey(character))
        {
            Debug.LogError($"Asking to show {c} but {c} is null in scene {gameObject.scene.name}");
        }
#endif
    }

    public void ChangeCharacterExpression(string c, string e)
    {
        CharacterName character = ParseChar(c);
        Expression expression = ParseExpress(e);
        characterUiDictionary[character].ChangeExpression(expression);
#if UNITY_EDITOR
        if (!characterUiDictionary.ContainsKey(character))
        {
            Debug.LogError($"Cannot find {c} in nameToUiDict. Perhaps you forgot to Show {c}?");
        }
#endif
    }

    /* Disable character. If need to show character again, can activate later */
    public void HideCharacter(string characterName)
    {
        CharacterName character = ParseChar(characterName);
        CharacterUI ui = characterUiDictionary[character];
        ui.Exit(() =>
        {
            ui.gameObject.SetActive(false);
        });
    }
}
