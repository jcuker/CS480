using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Client
{
    public static Socket server;

    static void Main(string[] args)
    {
        SetUp(args);

        string strFileName = GetFileToSend();

        SendFileToServer(strFileName);
        
    }
    
    private static string GetFileToSend()
    {
        Console.WriteLine("Enter the full path of the file to send.");
        string strFileName = Console.ReadLine();
        return strFileName;
    }

    private static void SendFileToServer(string strFilePath)
    {
        byte[] arrDataBuffer = new byte[1024];
        int intNumberOfBytes = server.Receive(arrDataBuffer);
        String strRecievedString = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);
        if(strRecievedString != "ready")
        {
            Console.WriteLine("error!");
            Environment.Exit(0);
        }

        server.SendFile(strFilePath);

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
    }
}

// class Client
// {
//     public static Socket server;
    
//     public static void Main(string[] arrCommandLineParams)
//     {
//         SetUp(arrCommandLineParams);

//         while (true)
//         {
//             string strMessage = RecieveMessageFromServer();
//             Console.WriteLine(strMessage);
//             string strResponse = Console.ReadLine();
//             SendStringToServer(strResponse);
//             if (strResponse.ToLower() == "ls")
//             {
//                 String files = RecieveMessageFromServer();
//                 Console.WriteLine(files);
//             }
//             else
//             {
//                 strMessage = RecieveMessageFromServer();

//                 // if file is found
//                 if (strMessage.ToLower().Contains("send"))
//                 {
//                     RecieveFileFromServer();
//                 }
//                 else
//                 {
//                     strMessage = RecieveMessageFromServer();
//                     Console.WriteLine(strMessage);
//                 }
//             }
            
//         }
        
//     }

//     private static void RecieveFileFromServer()
//     {
//         Console.WriteLine("File path to save file to?");
//         string strFilePathToSaveTo = Console.ReadLine();
//         BinaryWriter binaryWriter = new BinaryWriter(File.Open(strFilePathToSaveTo, FileMode.OpenOrCreate));
//         int read;
//         byte[] buffer = new byte[4096];
//         while ((read = server.Receive(buffer)) > 0)
//         {
//             binaryWriter.Write(buffer, 0, read);
//         }
//         binaryWriter.Close();
//         Console.WriteLine("File recieved and available at: " + strFilePathToSaveTo + "\nPress any key to exit");
//         Console.ReadKey();
//         Environment.Exit(0);
//     }

//     public static string RecieveMessageFromServer()
//     {
//         byte[] arrDataBuffer = new byte[1024];
//         int intNumberOfBytes = server.Receive(arrDataBuffer);
//         String strRecievedString = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);
//         return strRecievedString;
//     }

//     private static void SendStringToServer(string strToSend)
//     {
//         byte[] arrDataBuffer = Encoding.ASCII.GetBytes(strToSend);
//         int intNumberOfBytes = arrDataBuffer.Length;

//         try
//         {
//             server.Send(arrDataBuffer, intNumberOfBytes, SocketFlags.None);
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e.Message);
//         }
//     }

//     private static void SetUp(string[] arrCommandLineParams)
//     {
//         if (arrCommandLineParams.Length != 2)
//         { // Test for correct # of args
//             Console.WriteLine("Parameters: <Server> <Port>\nPress any key to exit.");
//             Console.ReadKey();
//             Environment.Exit(0);
//         }

//         IPHostEntry serverInfo = Dns.GetHostEntry(arrCommandLineParams[0]);//using IPHostEntry support both host name and host IPAddress inputs
//         IPAddress[] serverIPaddr = serverInfo.AddressList; //addresslist may contain both IPv4 and IPv6 addresses
        
//         server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//         try
//         {
//             server.Connect(serverIPaddr, Int32.Parse(arrCommandLineParams[1]));
//         }
//         catch (SocketException e)
//         {
//             Console.WriteLine("Unable to connect to server.");
//             Console.WriteLine(e.ToString());
//             Environment.Exit(0);
//         }
//     }
    
// }