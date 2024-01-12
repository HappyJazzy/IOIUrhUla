using UnityEngine;
using TMPro;
using System.Collections;
using System.Net.Sockets; 
using System; 
using System.Text; 
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueCanvas;
    public TextMeshProUGUI questionText;
    public GameObject bunny;
    public GameObject flowerObject;

    private bool isDialogueActive = false;
    private bool isPlayerInTrigger = false;
    private string fullMessage;
    private int currentIndex;


    public static DialogueManager Instance { get; private set; }

    private ConcurrentQueue<string> commandQueue = new ConcurrentQueue<string>();

    void Start()
    {
        HideDialogue();
        StartReceivingData();
        Debug.Log("DialogueManager started.");
    }

    private NetworkStream stream;

    public void SetStream(NetworkStream networkStream)
    {
        Debug.Log("Setting stream.");
        stream = networkStream;
        StartReceivingData();
    }

    private void StartReceivingData()
    {
        Debug.Log("Starting receive thread.");
        Thread receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        Debug.Log("ReceiveData thread running.");
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                if (stream != null && stream.CanRead)
                {
                    Debug.Log("Receiveing data");
                    int length = stream.Read(bytes, 0, bytes.Length);
                    if (length > 0)
                    {
                        string command = Encoding.UTF8.GetString(bytes, 0, length).Trim();
                        commandQueue.Enqueue(command);
                    }
                }
            }
        }
        catch (ThreadAbortException)
        {
            Debug.Log("Receive thread aborted.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in ReceiveData: " + ex.Message);
        }
    }

    void Update()
    {
        while (commandQueue.TryDequeue(out string command))
        {
            ProcessCommand(command);
        }
    }



    private void ProcessCommand(string command)
    {
        Debug.Log("Starting dialogue " + command);
        if (command == "živijo")
        {
            StartDialogue("Hej, lahko najdeš gobo in jo prineseš na vrh hriba.");
        }
        else if (command == "adijo")
        {
            EndDialogue();
        }
        else if (isDialogueActive)
        {
            UpdateDialogue(command);
        }
    }

    public void StartDialogue(string message)
    {
        if (!isDialogueActive)
        {
            fullMessage = message;
            currentIndex = 0;
            ShowDialogue();
        }
    }


    public void EndDialogue()
    {
        if (isDialogueActive)
        {
            HideDialogue();
        }
    }

    public void UpdateDialogue(string message)
    {
        Debug.Log("Updating dialogue with message: " + message);
        if (isDialogueActive)
        {
            fullMessage = message;
            currentIndex = 0;
            StopAllCoroutines();
            StartCoroutine(TypeText());
        }
    }

    private void ShowDialogue()
    {
        isDialogueActive = true;
        dialogueCanvas.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueCanvas.GetComponent<RectTransform>());

        StartCoroutine(TypeText());
    }

    void HideDialogue()
    {
        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
    }

    IEnumerator TypeText()
    {
        questionText.text = "";
        while (currentIndex < fullMessage.Length)
        {
            questionText.text += fullMessage[currentIndex];
            currentIndex++;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }
}