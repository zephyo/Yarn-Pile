using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Assumes all yarn variable names in 
/// yarnRelationshipNames are numbers.
/// 
/// Assuming hierachy is like this:
/// RelationshipUI
///     RelationshipPanel
///     RelationshipButton
/// </summary>
public class RelationshipUI : ClosableUI
{

    [Header("Items")]
    public VarUI VarUiPrefab;
    public Transform varsParent;


    protected override void Awake()
    {
        base.Awake();
        InstantiateVarUIs();
    }

    private void InstantiateVarUIs()
    {
        foreach (CharacterName n in Enum.GetValues(typeof(CharacterName)))
        {
            string name = n.ToString();
            VarUI ui = GameObject.Instantiate(VarUiPrefab.gameObject, varsParent).GetComponent<VarUI>();
            ui.varName = name;
        }
    }
}
