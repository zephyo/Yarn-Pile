using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity;
using Cinemachine;
using UnityEngine.SceneManagement;

/// <summary>
/// Requires components on this gameObject: SceneControl, AudioManager
/// Requires components to be in scene: PlayerInput
/// </summary>
public class MainSingleton : Singleton<MainSingleton>
{
    public CameraTarget cameraTarget;
    public DialogueRunner dialogueRunner;
    public Notification notification;
    public PlayerInput input;
    public InventoryManager inventoryManager;


    #region COMPONENTS THAT REGENERATE EVERY SCENE
    /* Keep reference to current CharacterManager 
     So that when a character says something,
     We know where to place the dialogue */
    CharacterManager characterManager;

    public CharacterManager CharacterManager
    {
        get
        {
            return characterManager;
        }
    }
    #endregion

    #region COMPONENTS ON THIS GAMEOBJECT
    CinemachineImpulseSource impulseSource;

    public CinemachineImpulseSource ImpulseSource
    {
        get
        {
            if (impulseSource == null)
            {
                impulseSource = gameObject.GetComponent<CinemachineImpulseSource>();
            }
            return impulseSource;
        }
    }

    AudioManager audioManager;

    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }
            return audioManager;
        }
    }
    #endregion

    private void OnValidate()
    {
        // get properties
        input = FindObjectOfType<PlayerInput>();
    }

    void Start()
    {
        // prepare settings
        FindObjectOfType<Settings>().PrepareAndLoadAll();
        SceneManager.sceneLoaded += CollectPerSceneComponents;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= CollectPerSceneComponents;
    }

    void CollectPerSceneComponents(Scene scene, LoadSceneMode mode)
    {
        // These two are not visual novel scenes, just menu screens, so don't fetch components
        if (scene.name != "Main" && scene.name != "Start Menu")
        {
            characterManager = FindObjectOfType<CharacterManager>();
        }
    }

    public void StartGame()
    {
        ((CustomStorage)dialogueRunner.variableStorage).UseSaveDataOnPlay();
        dialogueRunner.StartDialogue();
    }

    public void PlayButtonSoundEffect()
    {
        AudioManager.PlaySoundEffect("ButtonClick");
    }

    // Listener on Resume button
    public void ViewMain()
    {
        cameraTarget.SetBasePosition(Vector2.zero);
    }

    public void ViewPause()
    {
        Vector2 newPos = gameObject.GetComponentInChildren<Pause>().transform.position;
        cameraTarget.SetBasePosition(newPos);
    }

    public void ViewAbout()
    {
        Vector2 newPos = gameObject.GetComponentInChildren<About>().transform.position;
        cameraTarget.SetBasePosition(newPos);
    }

    public void ViewSettings()
    {
        Vector2 newPos = gameObject.GetComponentInChildren<Settings>().transform.position;
        cameraTarget.SetBasePosition(newPos);
    }

    public void ViewQuit()
    {
        ConfirmationModal modal = gameObject.GetComponentInChildren<ConfirmationModal>();
        modal.Init("Are you ready to quit?", () => Application.Quit(), ViewMain);
        Vector2 newPos = modal.transform.position;
        cameraTarget.SetBasePosition(newPos);
    }

    public void ViewConfirmDelete()
    {
        ConfirmationModal modal = gameObject.GetComponentInChildren<ConfirmationModal>();
        modal.Init("Delete all saved data?", () =>
        {
            ((CustomStorage)dialogueRunner.variableStorage).DeleteSaveData();
            // If not on start screen, reload
            if (SceneManager.GetSceneByName("Start Menu").IsValid())
            {
                return;
            }
            SceneManager.LoadSceneAsync(0);
        }, ViewSettings);
        Vector2 newPos = modal.transform.position;
        cameraTarget.SetBasePosition(newPos);
    }
}
