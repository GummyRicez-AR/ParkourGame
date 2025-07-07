using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(SoundFX))]
// this script is meant to go on the dialogue canvas
public class DialogueScript : MonoBehaviour
{
    public DialogueTree dialogueTreeRef;
    public PlayerController playerInDialogue;
    public NPCInteractable interactableScr;
    public GameObject buttonChoicePrefab;

    [Header("UI References")]
    public Image dialogueBoxFill;
    public Image dialogueBoxBorder;
    public TMP_Text text;
    public TMP_Text speakerName;
    public TMP_Text textToProgress;
    public Image speakerIcon;
    [Space]

    public bool makingChoice;

    private AudioSource audioSrc;
    // tree progression variables
    private int currentIndex;
    private bool progressingThroughDialogue;
    private float rateOfCharacterPrint = 30;
    private List<GameObject> currChoices = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
        makingChoice = false;
        text.text = "";
        currentIndex = 0;
        StartCoroutine(ProgressTMPDialogue(dialogueTreeRef.dialogues[currentIndex]));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(playerInDialogue.interact) && !progressingThroughDialogue && !makingChoice)
        {
            currentIndex++;
            if (currentIndex >= dialogueTreeRef.dialogues.Count)
            {
                interactableScr.DialogueFinished(this);
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(ProgressTMPDialogue(dialogueTreeRef.dialogues[currentIndex]));
            }
        }
    }

    /*
    private IEnumerator ProgressDialogue(Dialogue d)
    {
        yield return null;
        
        dialogueBoxFill.color = d.speaker.boxFill;
        dialogueBoxBorder.color = d.speaker.boxBorder;
        speakerIcon.sprite = d.speaker.icon;
        speakerName.text = d.speaker.name;
        text.text = "";
        textToProgress.text = "press [" + playerInDialogue.interact + "] to continue";
        textToProgress.enabled = false;

        string dialogueStr = d.dialogue;
        dialogueStr = dialogueStr.Replace("<ITEM_SPECIAL>", "[" + playerInDialogue.itemSpecial + "]");

        progressingThroughDialogue = true;
        string currStr = "";
        float timeBetweenCharacterPrint = 1 / rateOfCharacterPrint;
        float currTimer = 0;
        while (currStr.Length < dialogueStr.Length)
        {
            if (Input.GetKeyDown(playerInDialogue.interact) && currStr.Length >= 1)
            {
                text.text = dialogueStr;
                break;
            }

            currTimer += Time.deltaTime;
            int charactersToProgress = 0;
            if (currTimer > timeBetweenCharacterPrint)
            {
                charactersToProgress = (int)(currTimer / timeBetweenCharacterPrint);
                currTimer %= timeBetweenCharacterPrint;
                audioSrc.PlayOneShot(d.speaker.talkingSFX);
            }

            currStr = dialogueStr.Substring(0, currStr.Length + charactersToProgress);
            text.text = currStr;

            if (currStr.Length > 0)
            {
                switch (currStr[^1])
                {
                    case '!':
                    case '?':
                    case '.':
                        yield return new WaitForSeconds(0.25f);
                        break;
                    case ',':
                        yield return new WaitForSeconds(0.15f);
                        break;
                    default:
                        yield return null;
                        break;
                }
            }
            else
            {
                yield return null;
            }
        }

        if (d.hasChoices)
        {
            makingChoice = true;
            Cursor.lockState = CursorLockMode.None;
            float currXPos = -300 * (d.dialogueChoices.Count - 1);
            foreach (DialogueChoice choice in d.dialogueChoices)
            {
                GameObject newChoice = Instantiate(buttonChoicePrefab, transform);
                newChoice.GetComponent<RectTransform>().localPosition = new Vector3(currXPos, 0, 10);
                newChoice.GetComponentInChildren<TMP_Text>().text = choice.playerChoice;
                newChoice.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => ChangeTreeByPlayerChoice(choice.resultTree));
                currChoices.Add(newChoice);
                currXPos += 600;
            }
        } else
        {
            textToProgress.enabled = true;
        }
        progressingThroughDialogue = false;
    }
    */

    private IEnumerator ProgressTMPDialogue(Dialogue d)
    {
        progressingThroughDialogue = true;
        dialogueBoxFill.color = d.speaker.boxFill;
        dialogueBoxBorder.color = d.speaker.boxBorder;
        speakerIcon.sprite = d.speaker.icon;
        speakerName.text = d.speaker.name;
        text.text = "";
        textToProgress.text = "press [" + playerInDialogue.interact + "] to continue";
        textToProgress.enabled = false;
        
        yield return null;
        
        string dialogueStr = d.dialogue;
        dialogueStr = dialogueStr.Replace("<ITEM_SPECIAL>", "[" + playerInDialogue.itemSpecial + "]");
        text.text = dialogueStr;
        
        int textMeshIndex = 1;
        int targetIndex = dialogueStr.Length;
        float timeBetweenCharacterPrint = 1 / rateOfCharacterPrint;
        float currTimer = 0;

        while (textMeshIndex < targetIndex)
        {
            if (Input.GetKeyDown(playerInDialogue.interact))
            {
                yield return null;
                textMeshIndex = targetIndex;
            }
            
            text.ForceMeshUpdate(ignoreActiveState: true);
            Mesh tMesh = text.mesh;
            Vector3[] vertices = tMesh.vertices;
            
            currTimer += Time.deltaTime;
            int charactersToProgress = 0;
            if (currTimer > timeBetweenCharacterPrint)
            {
                charactersToProgress = (int)(currTimer / timeBetweenCharacterPrint);
                currTimer %= timeBetweenCharacterPrint;
            }

            if (charactersToProgress > 0)
            {
                textMeshIndex += charactersToProgress;
                print(textMeshIndex);
                if (textMeshIndex >= targetIndex) textMeshIndex = targetIndex;
                audioSrc.PlayOneShot(d.speaker.talkingSFX);
            }

            for (int i = 1; i < targetIndex; i++)
            {
                TMP_CharacterInfo c = text.textInfo.characterInfo[i];
                Color32[] materialColors = text.textInfo.meshInfo[c.materialReferenceIndex].colors32;
                int vertexIndex = c.vertexIndex;

                if (i <= textMeshIndex)
                {
                    materialColors[vertexIndex] = Color.black;
                    materialColors[vertexIndex + 1] = Color.black;
                    materialColors[vertexIndex + 2] = Color.black;
                    materialColors[vertexIndex + 3] = Color.black;
                }
                else
                {
                    materialColors[vertexIndex] = Color.clear;
                    materialColors[vertexIndex + 1] = Color.clear;
                    materialColors[vertexIndex + 2] = Color.clear;
                    materialColors[vertexIndex + 3] = Color.clear;
                }
            }

            tMesh.vertices = vertices;
            text.canvasRenderer.SetMesh(tMesh);
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            
            if (textMeshIndex < targetIndex)
            {
                switch (text.text[textMeshIndex])
                {
                    case '!':
                    case '?':
                    case '.':
                        yield return new WaitForSeconds(0.15f);
                        break;
                    case ',':
                        yield return new WaitForSeconds(0.075f);
                        break;
                    default:
                        yield return null;
                        break;
                }
            }
            else
            {
                yield return null;
            }
        }
        
        if (d.hasChoices)
        {
            makingChoice = true;
            Cursor.lockState = CursorLockMode.None;
            float currXPos = -300 * (d.dialogueChoices.Count - 1);
            foreach (DialogueChoice choice in d.dialogueChoices)
            {
                GameObject newChoice = Instantiate(buttonChoicePrefab, transform);
                newChoice.GetComponent<RectTransform>().localPosition = new Vector3(currXPos, 0, 10);
                newChoice.GetComponentInChildren<TMP_Text>().text = choice.playerChoice;
                newChoice.GetComponent<Button>().onClick.AddListener(() => ChangeTreeByPlayerChoice(choice.resultTree));
                currChoices.Add(newChoice);
                currXPos += 600;
            }
        } else
        {
            textToProgress.enabled = true;
        }

        progressingThroughDialogue = false;
    }

    private void ChangeTreeByPlayerChoice(DialogueTree newTree)
    {
        dialogueTreeRef = newTree;
        currentIndex = 0;
        makingChoice = false;

        foreach (GameObject choice in currChoices)
        {
            Destroy(choice);
        }

        currChoices.Clear();
        Cursor.lockState = CursorLockMode.Locked;

        StartCoroutine(ProgressTMPDialogue(dialogueTreeRef.dialogues[currentIndex]));
    }
}
