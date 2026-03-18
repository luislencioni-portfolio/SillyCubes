using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;

public class SerialController : MonoBehaviour
{
    #region Variables
    public string port = "COM3";
    public int baudRate = 9600;

    public static SerialPort sp;
    public Thread thread;

    public static ConcurrentQueue<string> TX = new ConcurrentQueue<string>();
    public static ConcurrentQueue<string> RX = new ConcurrentQueue<string>();

    private readonly object lockObject = new object();
    
    public static volatile bool isActive = true;

    public event Action<string> OnMessageReceived;

    #endregion

    #region Internal Functions

    void Start()
    {
        thread = new Thread (LoopingThread);
        thread.Start();
    }

    void Update()
    {
        lock (lockObject)
        {
            while (RX.Count > 0)
            {
                RX.TryDequeue(out string protocol);
                //string message = mainThreadMessages.Dequeue();
                OnMessageReceived?.Invoke(protocol);
            }
        }
    }

    public void LoopingThread ()
    {
        thread.IsBackground = true;

        try {
            sp = new SerialPort(port, baudRate);
            sp.Parity = Parity.None;
            sp.ReadTimeout = 50;
            sp.Open();

            while (isActive)
            {

                if (TX.Count > 0)
                {
                    TX.TryDequeue(out string msg);
                    Write(msg);
                }

                string readThis = Receive(); 

                if (readThis != null)
                {
                    RX.Enqueue(readThis);
                    // CheckProtocol();
                }

            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Write(string msg)
    {
        sp.WriteLine(msg);
    }

    public void Send(string msg)
    {
        TX.Enqueue(msg);
    }

    public string Receive() {

        try {
            return sp.ReadLine();
        }
        catch (TimeoutException e) {
            return null;
        }
    }

    public void CheckProtocol()
    {
        // RX.TryDequeue(out string protocol);

        // ReceivedSerialMessage(protocol);
    }

    public void OnApplicationQuit()
    {

        isActive = false;

        if(thread.IsAlive)
        {
            thread.Abort();
        }

        sp.Close();
    }

    #endregion

    public void ReceivedSerialMessage(string message)
    {
        Debug.Log("Received message: " + message);
        if (!string.IsNullOrEmpty(message))
        {
            // Trigger the event
            OnMessageReceived?.Invoke(message);
        }
    }

    public void SendSerialMessage(string message)
    {
        Debug.Log("Sending Message: " + message);
        this.Send(message);
    }

}
