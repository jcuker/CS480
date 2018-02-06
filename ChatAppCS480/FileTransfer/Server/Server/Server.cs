using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;


class Server
{
    private static Socket client, server;

    static void Main(string[] arrCommandLineParams)
    {
        SetUp(arrCommandLineParams);

        WaitForAClient();

        GetAndSaveFile();

    }
    
    private static void GetAndSaveFile()
    {
        string strDirectoryToSaveTo = Directory.GetCurrentDirectory() + "\\ReceivedFiles";

        if (!Directory.Exists(strDirectoryToSaveTo))
        {
            Directory.CreateDirectory(strDirectoryToSaveTo);
        }

        Console.WriteLine("What would you like to name the file? (include the correct file extension)");
        string strFileName = Console.ReadLine();
        string strFilePathToSaveTo = strDirectoryToSaveTo + "\\" + strFileName;

        byte[] arrDataBuffer = Encoding.ASCII.GetBytes("ready");
        int intNumberOfBytes = arrDataBuffer.Length;

        try
        {
            client.Send(arrDataBuffer, intNumberOfBytes, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
        BinaryWriter binaryWriter = new BinaryWriter(File.Open(strFilePathToSaveTo, FileMode.OpenOrCreate));
        int read;
        byte[] buffer = new byte[4096];
        read = client.Receive(buffer);
        
        binaryWriter.Write(buffer, 0, read);
        
        
        binaryWriter.Close();

        Console.WriteLine("File recieved and available at: " + strFilePathToSaveTo);
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

        IPEndPoint objIpEndpoint = new IPEndPoint(IPAddress.Any, Int32.Parse(arrCommandLineParams[0]));

        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(objIpEndpoint);
        server.Listen(10);

        Console.WriteLine("Server listening on port {0}", objIpEndpoint.Port);
    }

    private static void WaitForAClient()
    {
        Console.WriteLine("Waiting for a client...");

        client = server.Accept();

        IPEndPoint objClientIpEndpoint = (IPEndPoint)client.RemoteEndPoint;

        Console.WriteLine("Connected with {0} at port {1}", objClientIpEndpoint.Address, objClientIpEndpoint.Port);
    }


}

// class Server
// {
//     public static Socket client;
//     public static Socket server;

//     private static List<String> lstFilesInDirectory = new List<string>();

//     public static void Main(string[] arrCommandLineParams)
//     {
//         SetUp(arrCommandLineParams);

//         WaitForAClient();
        
//         while (true)
//         {
//             SendStringToClient("Enter the name of the file you would like to download. (type ls for all files available)");
//             string strRecievedMessage = RecieveMessageFromCilent();

//             if(strRecievedMessage.ToLower() == "ls")
//             {
//                 ListAllAvailableFiles();
//             }
//             else
//             {
//                 foreach (string file in lstFilesInDirectory)
//                 {
//                     if (file.ToLower() == strRecievedMessage.ToLower())
//                     {
//                         SendStringToClient("File found. Starting to send ...");
//                         SendFileToClient(file);

//                         client.Close();
//                         server.Close();

//                         Console.WriteLine("File: " + file + " sent.\nPress any key to exit...");
//                         Console.ReadKey();
//                         Environment.Exit(0);
//                     }
//                 }

//                 SendStringToClient("File not found. Try again.");
//             }
            
//         }

//     }

//     private static void SendFileToClient(string strRequestedFileName)
//     {
//         //int bytesToBeSent = arr.Length;
//         //int bytesActuallySent = 0;
//         //while (bytesActuallySent < bytesToBeSent)
//         //    bytesActuallySent += socket.Send(arr, bytesActuallySent, bytesToSend - bytesActuallySent, ....);
//         client.SendFile(strRequestedFileName);
//     }

//     private static void ListAllAvailableFiles()
//     {
//         string strToSend = "";
//         foreach(string file in lstFilesInDirectory)
//         {
//             strToSend += file;
//             strToSend += "\n";
//         }
//         SendStringToClient(strToSend);
//     }

//     public static string RecieveMessageFromCilent()
//     {
//         byte[] arrDataBuffer = new byte[1024];
//         int intNumberOfBytes = client.Receive(arrDataBuffer);
//         String strRecievedString = Encoding.ASCII.GetString(arrDataBuffer, 0, intNumberOfBytes);
//         return strRecievedString;
//     }

//     private static void SendStringToClient(string strToSend)
//     {
//         byte[] arrDataBuffer = Encoding.ASCII.GetBytes(strToSend);
//         int intNumberOfBytes = arrDataBuffer.Length;

//         try
//         {
//             client.Send(arrDataBuffer, intNumberOfBytes, SocketFlags.None);
//         }
//         catch (Exception e)
//         {
//             Console.WriteLine(e.Message);
//         }
//     }

//     private static void WaitForAClient()
//     {
//         Console.WriteLine("Waiting for a client...");

//         client = server.Accept();

//         IPEndPoint objClientIpEndpoint = (IPEndPoint)client.RemoteEndPoint;

//         Console.WriteLine("Connected with {0} at port {1}", objClientIpEndpoint.Address, objClientIpEndpoint.Port);
//     }
    

//     private static void SetUp(string[] arrCommandLineParams)
//     {
//         // Test for correct # of args
//         if (arrCommandLineParams.Length != 1)
//         {
//             Console.WriteLine("Parameters: <Port>\nPress any key to exit.");
//             Console.ReadKey();
//             Environment.Exit(0);
//         }

//         IPEndPoint objIpEndpoint = new IPEndPoint(IPAddress.Any, Int32.Parse(arrCommandLineParams[0]));

//         server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//         server.Bind(objIpEndpoint);
//         server.Listen(10);

//         Console.WriteLine("Server listening on port {0}", objIpEndpoint.Port);

//         ProcessFileDirectory(Directory.GetCurrentDirectory() + @"\Files");
//     }

//     private static void ProcessFileDirectory(string targetDirectory)
//     {
//         string[] fileEntries = Directory.GetFiles(targetDirectory);
//         foreach (string fileName in fileEntries)
//         {
//             lstFilesInDirectory.Add(fileName);
//         }
//     }
// }