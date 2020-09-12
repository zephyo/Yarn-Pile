using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Keeps track of 'visited' nodes 
/// </summary>
[RequireComponent(typeof(DialogueRunner))]
public class NodeVisitedTracker : MonoBehaviour
{
    // reference to CustomStorage's currentData
    private CustomStorage storage;
    void Start()
    {
        storage = (CustomStorage)MainSingleton.Instance.dialogueRunner.variableStorage;
        DialogueRunner runner = MainSingleton.Instance.dialogueRunner;
        // Register a function on startup called "visited" that lets
        // Yarn scripts query to see if a node has been run before.
        runner.AddFunction("visited", delegate (string nodeName)
        {
            return storage.visitedNodes.Contains(nodeName);
        });
        runner.onNodeComplete.AddListener(NodeComplete);
    }

    // Called by the Dialogue Runner to notify us that a node finished
    // running. 
    public void NodeComplete(string nodeName)
    {
        // Log that the node has been run.
        storage.visitedNodes.Add(nodeName);
    }

}
