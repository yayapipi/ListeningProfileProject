using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SelectCharacterView : MonoBehaviour
{
    public GameObject characterSelectionPanel;
    public GameObject[] characters;
    public GameObject[] charactersDialogPoint;
    public Button[] characterButtons;
        
    public CinemachineCamera cinemachineCamera;
    public DialogueBubbleUI dialogueBubbleUI;

    private void Start()
    {
        characterSelectionPanel.SetActive(true);
        for (var index = 0; index < characterButtons.Length; index++)
        {
            var btn = characterButtons[index];
            var index1 = index;
            btn.onClick.AddListener(()=>SetCharacter(index1));
        }
    }

    private void SetCharacter(int index)
    {
        foreach (var character in characters)
        {
            character.SetActive(false);
        }
        characters[index].SetActive(true);
        cinemachineCamera.Target.TrackingTarget = characters[index].transform;
        dialogueBubbleUI.playerObject = charactersDialogPoint[index];
        characterSelectionPanel.SetActive(false);
    }
}