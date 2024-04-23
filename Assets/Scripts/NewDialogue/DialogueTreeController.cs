using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueTreeController : MonoBehaviour
{
    public static DialogueTreeController instance;

    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] GameObject answerBox;
    [SerializeField] Button[] answerObjects;
    [SerializeField] GameObject continueText;
    [SerializeField] Image[] images;

    public static event Action OnDialogueStarted;
    public static event Action OnDialogueEnded;

    public bool isAcceptClicked = false;

    public float charactersPerSecond = 30;
    bool isPrinting = false;
    bool waitForAnswer = false;
    bool skipLineTriggered;
    bool answerTriggered;
    int answerIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        continueText.GetComponent<Button>().onClick.AddListener(SkipLine);
    }

    public void StartDialogue(DialogueTree dialogueTree, int startSection)
    {
        ResetBox();
        waitForAnswer = false;
        dialogueBox.SetActive(true);
        OnDialogueStarted?.Invoke();
        GameEventsManager.instance.playerEvents.DisablePlayerMovement();
        GameplayStateManager.Instance.isInDialogue = true;
        StartCoroutine(RunDialogue(dialogueTree, startSection));
    }

    IEnumerator RunDialogue(DialogueTree dialogueTree, int section)
    {
        continueText.SetActive(true);
        for (int i = 0; i < dialogueTree.sections[section].dialogue.Length; i++)
        {
            //dialogueText.text = dialogueTree.sections[section].dialogue[i];
            //print(i);
            nameText.text = dialogueTree.sections[section].dialogue[i].characterName;
            StartCoroutine(TypeTextUncapped(dialogueText.text = dialogueTree.sections[section].dialogue[i].text));
            ResetSprites();
            for (int j = 0; j < dialogueTree.sections[section].dialogue[i].sprites.Length; j++)
            {
                CharacterSprite cs = dialogueTree.sections[section].dialogue[i].sprites[j];
                images[j].gameObject.SetActive(true);
                images[j].sprite = cs.sprite;
                images[j].rectTransform.localPosition = cs.offset;
                images[j].color = cs.fadeColor;
            }
            ResolveAction(dialogueTree.sections[section].dialogue[i].action);
            while (skipLineTriggered == false)
            {
                yield return null;
            }

            //print(i+"end");
            skipLineTriggered = false;
        }
        continueText.SetActive(false);

        if (dialogueTree.sections[section].endAfterDialogue)
        {
            OnDialogueEnded?.Invoke();
            GameEventsManager.instance.playerEvents.EnablePlayerMovement();
            GameplayStateManager.Instance.isInDialogue = false;
            dialogueBox.SetActive(false);
            yield break;
        }

        if (dialogueTree.sections[section].goToNextDialogue)
        {
            StartCoroutine(RunDialogue(dialogueTree, dialogueTree.sections[section].nextDialogue));
            yield break;
        }

        StartCoroutine(TypeTextUncapped(dialogueText.text = dialogueTree.sections[section].branchPoint.question));
        ShowAnswers(dialogueTree.sections[section].branchPoint);

        waitForAnswer = true;

        while (answerTriggered == false)
        {
            yield return null;
        }
        answerBox.SetActive(false);
        answerTriggered = false;

        waitForAnswer = false;

        ResolveAction(dialogueTree.sections[section].branchPoint.answers[answerIndex].action);

        StartCoroutine(RunDialogue(dialogueTree, dialogueTree.sections[section].branchPoint.answers[answerIndex].nextElement));
    }

    IEnumerator TypeTextUncapped(string line)
    {
        isPrinting = true;
        float timer = 0;
        float interval = 1 / charactersPerSecond;
        string textBuffer = null;
        char[] chars = line.ToCharArray();
        int i = 0;

        while (i < chars.Length)
        {
            if (!isPrinting)
            {
                dialogueText.text = line;
                yield break;
            }
            if (timer < Time.deltaTime)
            {
                textBuffer += chars[i];
                dialogueText.text = textBuffer;
                timer += interval;
                i++;
            }
            else
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        isPrinting = false;
    }

    void ResetSprites()
    {
        foreach(Image image in images)
        {
            image.gameObject.SetActive(false);
        }
    }

    void ResetBox()
    {
        StopAllCoroutines();
        dialogueBox.SetActive(false);
        answerBox.SetActive(false);
        skipLineTriggered = false;
        answerTriggered = false;
    }

    void ShowAnswers(BranchPoint branchPoint)
    {
        // Reveals the aselectable answers and sets their text values
        answerBox.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            if (i < branchPoint.answers.Length)
            {
                answerObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = branchPoint.answers[i].answerLabel;
                answerObjects[i].gameObject.SetActive(true);
            }
            else
            {
                answerObjects[i].gameObject.SetActive(false);
            }
        }
    }

    public void SkipLine()
    {
        if (isPrinting)
        {
            isPrinting = false;
            return;
        }
        if (waitForAnswer) return;
        skipLineTriggered = true;
    }

    public void AnswerQuestion(int answer)
    {
        answerIndex = answer;
        answerTriggered = true;
    }

    public void ResolveAction(DialogueAction action)
    {
        DialogueActionType func = action.type;
        if (func == DialogueActionType.Nothing) return;

        switch (func)
        {
            case DialogueActionType.AcceptQuest: isAcceptClicked = true; break;
            case DialogueActionType.RejectQuest: isAcceptClicked = false; break;
            case DialogueActionType.StartQuest: GameEventsManager.instance.questEvents.StartQuest(action.parameter); break;
            case DialogueActionType.FinishQuest: GameEventsManager.instance.questEvents.FinishQuest(action.parameter); break;
            case DialogueActionType.AddItem: PlayerInventory.Instance.AddItem(action.parameter) ; break;
            case DialogueActionType.RemoveItem: PlayerInventory.Instance.RemoveItem(action.parameter); break;
            case DialogueActionType.SetMainObjective: GameplayStateManager.Instance.Player.SetMainObjective(action.parameter); break;
            case DialogueActionType.PlaySound: SoundManager.TryPlayNew(action.parameter); break;
            case DialogueActionType.PlayBGM: SoundManager.TryPlayMusic(action.parameter); break;
            case DialogueActionType.OpenBossZone3: Zone3Manager.Instance.RemoveDog(); Zone3Manager.Instance.ClearBossEntrance(); break;
        }
    }
}