using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

[System.Serializable]
public class YarnValuesDictionary : SerializableDictionaryBase<string, int> { }

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public ItemName item;
    public Sprite icon;
    public string itemDescription;
    public string effectsDescription;

    // Owning this item will increment Yarn variables of the name [string] by [int]
    public YarnValuesDictionary yarnValues = new YarnValuesDictionary();

#if UNITY_EDITOR
    private void Awake()
    {
        foreach (KeyValuePair<string, int> pair in yarnValues)
        {
            var storage = ((CustomStorage)MainSingleton.Instance?.dialogueRunner?.variableStorage);
            if (storage != null && storage.IsValidVariable(pair.Key) != true)
            {
                Debug.LogError($"ERROR! {pair.Key} is not a valid Yarn variable field.");
            }
        }
    }
#endif
}