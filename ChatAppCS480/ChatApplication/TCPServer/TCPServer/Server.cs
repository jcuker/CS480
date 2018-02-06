using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Server
{
    // used to allow for seamless exit of application due to shutdown
    [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
    private static bool blnInShutdownState = false;
    
    public static String strMyAlias;
    public static String strAliasOfClient;
    public static List<KeyValuePair<String, String>> lstAllRecievedChats = new List<KeyValuePair<string, string>>();
    public static Socket client;
    public static Socket server;


    public static void Main(string[] arrCommandLineParams)
    {
        SetUp(arrCommandLineParams);
        
        WaitForAClient();

        ExchangeAliases();

        // start receieve thread
        Thread thrRecieveMessages = new Thread(RecieveMessages);
        thrRecieveMessages.Start();

        while (thrRecieveMessages.IsAlive && ! blnInShutdownState)
        {
            Console.Write(">");
            string strToSend = Console.ReadLine();

            if(strToSend.ToLower() == "exit app")
            {
                thrRecieveMessages.Abort();
                break;
            }

            if ( ! blnInShutdownState)
            {
                SendStringToClient(strToSend);
            }
            
            
        }
        
        client.Close();
        server.Close();
        PromptForMessageLogAndExit();
    }

    private static void PromptForMessageLogAndExit()
    {
        Console.WriteLine("Would you like a message log? (Y/N)");
        Char choice = (Char) Console.Read();
        if(choice == 'Y' || choice == 'y')
        {
            PrintAllRecievedMessages(lstAllRecievedChats);
        }

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void ExchangeAliases()
    {
        string strGreetingMessage = "Welcome! My name is: " + strMyAlias + ".";
        SendStringToClient(strGreetingMessage);

        byte[] arrDataBuffer = new byte[1024];

        int intNumberOfBytes = client.Receive(arrDataBuffer);

        strAliasOfClient = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);

        SendStringToClient("Welcome, " + strAliasOfClient + ". Chat started at " + DateTime.Now.ToString());
    }

    private static void SendStringToClient(string strToSend)
    {
        byte[] arrDataBuffer = Encoding.ASCII.GetBytes(strToSend);
        int intNumberOfBytes = arrDataBuffer.Length;

        try
        {
            client.Send(arrDataBuffer, intNumberOfBytes, SocketFlags.None);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void WaitForAClient()
    {
        Console.WriteLine("Waiting for a client...");

        client = server.Accept();

        IPEndPoint objClientIpEndpoint = (IPEndPoint)client.RemoteEndPoint;

        Console.WriteLine("Connected with {0} at port {1}", objClientIpEndpoint.Address, objClientIpEndpoint.Port);
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
                int intNumberOfBytes = client.Receive(arrDataBuffer);

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
        // Test for correct # of args
        if (arrCommandLineParams.Length != 1)
        {
            Console.WriteLine("Parameters: <Port>\nPress any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        
        Console.WriteLine("Enter your alias.");
        strMyAlias = Console.ReadLine();


        IPEndPoint objIpEndpoint = new IPEndPoint(IPAddress.Any, Int32.Parse(arrCommandLineParams[0]));

        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(objIpEndpoint);
        server.Listen(10);

        Console.WriteLine("Server listening on port {0}", objIpEndpoint.Port);
    }
}