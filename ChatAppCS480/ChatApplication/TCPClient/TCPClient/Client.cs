using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Client
{
    // used to allow for seamless exit of application due to shutdown
    [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
    private static bool blnInShutdownState = false;

    public static String strMyAlias;
    public static String strAliasOfClient;
    public static List<KeyValuePair<String, String>> lstAllRecievedChats = new List<KeyValuePair<string, string>>();
    public static Socket server;
    
    public static void Main(string[] arrCommandLineParams)
    {
        SetUp(arrCommandLineParams);
        
        Thread thrRecieveMessages = new Thread(RecieveMessages);
        thrRecieveMessages.Start();

        while (thrRecieveMessages.IsAlive)
        {
            Console.Write(">");
            string strToSend = Console.ReadLine();

            if (strToSend.ToLower() == "exit app")
            {
                thrRecieveMessages.Abort();
                break;
            }

            SendStringToServer(strToSend);
        }

        server.Close();
        PromptForMessageLogAndExit();
    }

    private static void PromptForMessageLogAndExit()
    {
        Console.WriteLine("Would you like a message log? (Y/N)");
        Char choice = (Char)Console.Read();
        if (choice == 'Y' || choice == 'y')
        {
            PrintAllRecievedMessages(lstAllRecievedChats);
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void SendStringToServer(string strToSend)
    {
        byte[] arrDataBuffer = Encoding.ASCII.GetBytes(strToSend);
        int intNumberOfBytes = arrDataBuffer.Length;

        try
        {
            server.Send(arrDataBuffer, intNumberOfBytes, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void PrintAllRecievedMessages(List<KeyValuePair<string, string>> lstAllRecievedEchos)
    {
        foreach (KeyValuePair<String, String> kvpIpAndEcho in lstAllRecievedEchos)
        {
            Console.WriteLine("Message: " + kvpIpAndEcho.Value + " From: " + kvpIpAndEcho.Key + '\n');
        }
    }
    
    private static void RecieveMessages()
    {
        while (true)
        {
            byte[] arrDataBuffer = new byte[1024];

            try
            {
                int intNumberOfBytes = server.Receive(arrDataBuffer);

                if (intNumberOfBytes == 0)
                {
                    break;
                }

                Console.WriteLine();
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
                String strRecievedString = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);
                Console.WriteLine(strAliasOfClient + ": " + strRecievedString);
                lstAllRecievedChats.Add(new KeyValuePair<string, string>(strAliasOfClient, strRecievedString));
                Console.Write(">");
            }
            catch (SocketException)
            {
                blnInShutdownState = true;

                // needed to send a enter press to the console to interrupt the previous wait for enter key in main
                const int VK_RETURN = 0x0D;
                const int WM_KEYDOWN = 0x100;
                var hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
                Thread.CurrentThread.Abort();
            }
        }
    }


    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    private static void SetUp(string[] arrCommandLineParams)
    {
        if (arrCommandLineParams.Length != 2)
        { // Test for correct # of args
            Console.WriteLine("Parameters: <Server> <Port>\nPress any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        IPHostEntry serverInfo = Dns.GetHostEntry(arrCommandLineParams[0]);//using IPHostEntry support both host name and host IPAddress inputs
        IPAddress[] serverIPaddr = serverInfo.AddressList; //addresslist may contain both IPv4 and IPv6 addresses

        Console.WriteLine("Enter your alias.");
        strMyAlias = Console.ReadLine();
        
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Connect(serverIPaddr, Int32.Parse(arrCommandLineParams[1]));
        }
        catch (SocketException e)
        {
            Console.WriteLine("Unable to connect to server.");
            Console.WriteLine(e.ToString());
            Environment.Exit(0);
        }

        // first message will alwyas be a greeting with the server's name
        byte[] arrDataBuffer = new byte[1024];
        int intNumberOfBytes = server.Receive(arrDataBuffer);
        string strWelcomeMessage = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);
        Console.WriteLine(strWelcomeMessage);

        GetAliasOfServerFromWelcomeMessage(strWelcomeMessage);

        SendStringToServer(strMyAlias);
    }

    private static void GetAliasOfServerFromWelcomeMessage(string strWelcomeMessage)
    {
        int intIndexOfColon = strWelcomeMessage.IndexOf(":");
        strWelcomeMessage = strWelcomeMessage.Remove(0, intIndexOfColon + 2);
        strAliasOfClient = strWelcomeMessage.Remove(strWelcomeMessage.Length - 1);
    }
}