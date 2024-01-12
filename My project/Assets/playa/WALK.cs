using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class WALK : MonoBehaviour 
{
    private Rigidbody2D rb;
    public Animator anim;
    public float moveSpeed;
    private bool IsMoving;
    private Vector3 moveDir;
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        try
        {
            Debug.Log("Attempting to connect to server...");
            client = new TcpClient("localhost", 8080);
            Debug.Log("Connected to server.");
            stream = client.GetStream();
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.SetStream(stream);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Connection error: " + ex.Message);
        }
    }

    private void ReceiveData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                if (stream.CanRead)
                {
                    int length = stream.Read(bytes, 0, bytes.Length);
                    if (length > 0)
                    {
                        string command = Encoding.UTF8.GetString(bytes, 0, length);
                        ProcessCommand(command);
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


  
    private bool inDialogue = false;
    private void ProcessCommand(string command)
    {
        command = command.Trim();

        if (command == "živijo")
        {
            inDialogue = true;
            StopMoving();
            return;
        }

        if (command == "adijo")
        {
            inDialogue = false;
            return;
        }
        if (!inDialogue)
        {
            switch (command)
            {
                case "gor":
                    moveDir = Vector3.up;
                    break;
                case "dol":
                    moveDir = Vector3.down;
                    break;
                case "levo":
                    moveDir = Vector3.left;
                    break;
                case "desno":
                    moveDir = Vector3.right;
                    break;
                case "stop":
                    StopMoving();
                    break;
            }
        }
    }

    private void Update()
    {
        if (moveDir != Vector3.zero)
        {
            anim.SetFloat("X", moveDir.x);
            anim.SetFloat("Y", moveDir.y);
            if (!IsMoving)
            {
                IsMoving = true;
                anim.SetBool("IsMoving", IsMoving);
            }
        }
        else
        {
            if (IsMoving)
            {
                IsMoving = false;
                anim.SetBool("IsMoving", IsMoving);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = moveDir * moveSpeed * Time.fixedDeltaTime;
    }

    private void StopMoving()
    {
        moveDir = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
    }
}