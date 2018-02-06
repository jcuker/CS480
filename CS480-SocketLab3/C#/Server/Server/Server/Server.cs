/*
 Author: Jordan Cuker
 Date: 11/29/2017
 Purpose: CS480 Socket Lab3
 
 To run this, you can either click the start button in VS, or build and run the .exe created.
 If you opt for the start button, set the debug params in the project settings, otherwise provide it when stating the .exe as command line parameters.

 From a web-browser (or the client project) visit localhost:{portnum} where portnum is the number you specified when starting the exe. 
  */

using System;
using System.IO;
using System.Net;
using System.Threading;

namespace CS480SocketLab3
{
    public class Server
    {        
        private static HttpListener httpListener;

        static void Main(string[] arrCommandLineParameters)
        {
            // Check for corrent input params - we want exactly one
            if(arrCommandLineParameters.Length != 1)
            {
                Console.WriteLine("Parameters: <port>");
                Environment.Exit(0);
            }

            string portnum = arrCommandLineParameters[0];

            // HttpListener is only supported on Windoiws XP SP2 or above - not really a problem now-a-days as XP is not supported by MS anymore
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                Environment.Exit(0);
            }

            // Create a listener
            httpListener = new HttpListener();

            // Add the supported prefixes
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/");
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/index/");
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/anotherpage/");
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/gif/");
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/gif.html/");
            httpListener.Prefixes.Add("http://localhost:" + portnum + "/image.gif/");

            // Start the HttpListener
            httpListener.Start();

            Console.WriteLine("Listening... (ctrl + c to abort)");

            while (true)
            {
                // Enable concurrency to handle as many clients as possible
                ThreadPool.QueueUserWorkItem(ReceiveAndHandleRequest, httpListener.GetContext());
            }
            
        }

        static void ReceiveAndHandleRequest(object caller)
        {
            HttpListenerContext context = caller as HttpListenerContext;

            // process request and make response
            HttpListenerRequest request = context.Request;

            // Obtain a response object.
            HttpListenerResponse response = context.Response;

            // Construct a response.
            byte[] arrBytesToSend;
            
            // look to see what was requested and deliver the appropriate response
            // Note, all web pages are loaded from a file!
            if (request.Url.LocalPath.ToLower() == "/" || request.Url.LocalPath.ToLower().Contains("index"))
            {
                arrBytesToSend = File.ReadAllBytes("index.html");
                
                // Add the content type header to display html instead of plaintext
                response.AddHeader("Content-Type", "text/html");
            }
            else if (request.Url.LocalPath.ToLower().Contains("/anotherpage"))
            {
                arrBytesToSend = File.ReadAllBytes("AnotherPage.html");

                // Add the content type header to display html instead of plaintext
                response.AddHeader("Content-Type", "text/html");
            }
            else if(request.Url.LocalPath.ToLower().Contains("gif"))
            {
                arrBytesToSend = File.ReadAllBytes("image.gif");
                
                // add the content type header to the response to signify that its a gif image
                response.AddHeader("Content-Type", "image/gif");
            }
            else
            {
                arrBytesToSend = File.ReadAllBytes("404.html");

                // Add the content type header to display html instead of plaintext
                response.AddHeader("Content-Type", "text/html");
            }

            // Get a response stream and write the response to it.
            response.ContentLength64 = arrBytesToSend.Length;

            Stream output = response.OutputStream;

            output.Write(arrBytesToSend, 0, arrBytesToSend.Length);

            // Close stream (important)
            output.Close();
        }

    }
}
