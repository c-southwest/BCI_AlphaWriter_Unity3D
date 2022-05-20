using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.IO;
//using System.Diagnostics;

public class mind_script : MonoBehaviour
{
    public Color[] colors = new Color[] { Color.white, Color.yellow };
    public bool confirmed = false;
    public int duration = 3;
    public int index;
    public int port = 8080;
    IPAddress serverAddress = IPAddress.Parse("127.0.0.1");
    Text msg;
    Text text_linux;
    Coroutine main;
    BinaryTree head;
    BinaryTree current;
    Text back;
    bool received = false;
    byte[] buffer = new byte[2048];
    // Discord bot server
    public string bot_post_url;//= "https://43.135.6.253/send";
    // Linux 
    public string linux_host;// = Environment.GetEnvironmentVariable("LINUX_HOST"); //"root@fedora";
    void Start()
    {
        index = 0;
        msg = GameObject.Find("Text").GetComponent<Text>();
        text_linux = GameObject.Find("Text_Linux").GetComponent<Text>();
        back = GameObject.Find("Text_back").GetComponent<Text>();
        main = StartCoroutine(Loop());
        linux_host = Environment.GetEnvironmentVariable("LINUX_HOST");
        bot_post_url = Environment.GetEnvironmentVariable("BCI_BOT_URL");
        Debug.Log(bot_post_url); 
        // Build binary tree
        var queue = new Queue<string>();
        for (int i = 'a'; i <= 'z'; i++)
        {
            queue.Enqueue(((char)i).ToString());
        }
        for (int i = '0'; i <= '9'; i++)
        {
            queue.Enqueue(((char)i).ToString());
        }
        queue.Enqueue("Space");
        queue.Enqueue(','.ToString());
        queue.Enqueue('.'.ToString());
        queue.Enqueue('!'.ToString());
        queue.Enqueue('@'.ToString());
        queue.Enqueue('?'.ToString());
        queue.Enqueue('<'.ToString());
        queue.Enqueue('>'.ToString());
        queue.Enqueue('('.ToString());
        queue.Enqueue(')'.ToString());
        queue.Enqueue('|'.ToString());
        queue.Enqueue("Linux");
        //queue.Enqueue("Discord");

        //queue.Enqueue("Backspace");
        head = new BinaryTree();
        int depth = 1;
        BinaryTree.Build(head, queue, depth, 6);
        head.right.right.left = new BinaryTree() { left = new BinaryTree() { data = "Discord" },
                                                    right=new BinaryTree() { data = "Clear"}
        };
        head.right.right.right = new BinaryTree() { left = new BinaryTree() { data = "\\" }, 
                                                    right = new BinaryTree() { data="Speek"} 
        };

        current = head;
        show(current);

        // Instantiate socket
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(new IPEndPoint(serverAddress, port));

        // Start socket connection
        Thread thread;
        thread = new Thread(Connection);
        thread.Start(socket);
    }

    /// <summary>
    /// Display options based on current node
    /// </summary>
    /// <param name="node">tree node</param>
    void show(BinaryTree node)
    {
        var left = new List<string>();
        var right = new List<string>();
        BinaryTree.GetTree(node.left, left);
        BinaryTree.GetTree(node.right, right);

        GameObject.Find("Text_left").GetComponent<Text>().text = string.Join(" ", left);
        GameObject.Find("Text_right").GetComponent<Text>().text = string.Join(" ", right);
        if (node.parent is null)
        {
            back.text = "Delete";
        }
        else
        {
            back.text = "Go back";
        }

    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            if (confirmed == false)
            {
                Debug.Log("Confirm");
                StartCoroutine(Confirmed());
            }
        }

        // Test
        if (Input.GetKeyUp(KeyCode.T))
        {
            Linux(msg.text);
        }

        // Test
        if (Input.GetKeyUp(KeyCode.V))
        {
            Speeker.Speek(msg.text);
        }

        if (received)
        {
            if (confirmed == false)
            {
                Debug.Log("Confirm");
                StartCoroutine(Confirmed());
            }
            received = false;
        }
    }

    /// <summary>
    /// Selection Polling
    /// </summary>
    /// <returns></returns>
    IEnumerator Loop()
    {
        while (true)
        {

            index += 1;
            if (index > 3)
            {
                index = 1;
            }
            yield return new WaitForSecondsRealtime(duration);
        }
    }

    /// <summary>
    /// Stop Loop() and enter confirm state, restart Loop() after duration time.
    /// </summary>
    /// <returns></returns>
    IEnumerator Confirmed()
    {

        StopCoroutine(main);
        confirmed = true;




        if (index == 1)
        {
            if (current.left.left is null)
            {
                if (!isSpecialFunction(current.left.data)) msg.text += current.left.data;
                current = head;
            }
            else
            {
                current = current.left;
            }
            show(current);
        }
        else if (index == 2)
        {
            if (current.right is null)
            {
                current = head;
            }
            else
            {
                if (current.right.left is null)
                {
                    if (!isSpecialFunction(current.right.data)) msg.text += current.right.data;
                    current = head;
                }
                else
                {
                    current = current.right;
                }
            }
            show(current);
        }
        else if (index == 3)
        {
            if (current.parent is null)
            {
                if (msg.text.Length > 0)
                {
                    msg.text = msg.text.Substring(0, msg.text.Length - 1);
                }
            }
            else
            {
                current = current.parent;
            }

            show(current);
        }

        yield return new WaitForSecondsRealtime(duration);

        bool isSpecialFunction(string data)
        {
            switch (data)
            {
                //case "Backspace":
                //    backspace();    // Delete
                //    return true;
                case "Linux":
                    Linux(msg.text);    // Execute Linux command
                    msg.text = "";
                    return true;
                case "Discord":
                    Discord(msg.text);  // Send message to Discord channel
                    msg.text = "";
                    return true;
                case "Space":
                    msg.text += " ";    // Whitespace
                    return true;
                case "Speek":           // Text-To-Speech
                    Speeker.Speek(msg.text);
                    //msg.text = "";
                    return true;
                case "Clear":
                    msg.text = "";
                    return true;
                default:
                    return false;
            }
        }


        confirmed = false;
        index = 0;
        main = StartCoroutine(Loop());
    }


    void Connection(object obj)
    {
        Debug.Log("Connection thread started successfully!");
        while (true)
        {
            Socket socket = obj as Socket;
            try
            {
                int result = socket.Receive(buffer);
                if (result == 0)
                {
                    break;
                }
                else
                {
                    string str = Encoding.Default.GetString(buffer, 0, result);
                    if (str == "1")
                    {
                        Debug.Log("Confirm");
                        received = true;

                    }
                    Debug.Log($"Receive data:{str}");

                }
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }
        }

    }
    void backspace()
    {
        if (msg.text.Length > 0) msg.text = msg.text.Substring(0, msg.text.Length - 1);
    }
    void Linux(string command)
    {
        Debug.Log(linux_host); 
        var p = new System.Diagnostics.Process();
        p.StartInfo.FileName = "ssh";
        p.StartInfo.Arguments = $"{linux_host} {command}";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        
        if(p.ExitCode == 0)
        {
            text_linux.text = output;
        }
        else
        {
            text_linux.text = "Failed to execute, check please.";
        }
    }
    async void Discord(string msg)
    {
        var key = Environment.GetEnvironmentVariable("BCI_KEY");

        var message = new Message(msg, key);

        var json = JsonConvert.SerializeObject(message);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(bot_post_url, data);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //this.msg.text = await response.Content.ReadAsStringAsync();
            }
            else
            {
                this.msg.text = "?";
            }
        }
    }
    class Message
    {
        public string text;
        public string key;
        public Message(string text, string key)
        {
            this.text = text;
            this.key = key;
        }
    }

    static class Speeker
    {
        static public void Speek(string message)
        {

            string err = "Do not include special character";
            var p = new System.Diagnostics.Process();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // tts.ps1 
                string content = @"param(
    # Parameter help description
    [Parameter()]
    [string]
    $message
)
Add-Type -AssemblyName System.Speech
$Speech = New-Object System.Speech.Synthesis.SpeechSynthesizer
$Speech.Speak($message)";
                Console.WriteLine(content);

                if (File.Exists("tts.ps1"))
                {
                    Console.WriteLine(Directory.GetCurrentDirectory());
                }
                else
                {
                    // Write file
                    using (StreamWriter sw = new StreamWriter("tts.ps1"))
                    {
                        sw.Write(content);
                    }
                }
                p.StartInfo.FileName = "powershell";
                p.StartInfo.Arguments = $@"powershell -ExecutionPolicy Bypass -File .\tts.ps1 '{message}'";
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
            }
            else
            {
                p.StartInfo.FileName = "espeak-ng";
                p.StartInfo.Arguments = $@" -vzh '{message}'";
            }

            p.Start();
            p.WaitForExit();
            if (p.ExitCode == 1)
            {
                p.StartInfo.Arguments = $@"powershell -ExecutionPolicy Bypass -File .\tts.ps1 '{err}'";
                p.Start();
            }
        }
    }
}

