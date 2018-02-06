/*
 Author: Jordan Cuker
 Date: 11/29/2017
 Purpose: CS480 Socket Lab3
 
 To run this, you can either click the start button in VS, or build and run the .exe created.
 If you opt for the start button, set the debug params in the project settings, otherwise provide it when stating the .exe as command line parameters.

 This will dump the result of an HttpGet request to stdout.

 Input should contain:
 <domain> -> "http(s)://{website_name}.{extension}
 <path> -> If wanting anything other than the index page. IE https://semo.edu/it where "it" is the path (input '/' for index)
 <port> {optional} -> the port to use. Normally 80.
  */
  
using System;
using System.IO;
using System.Net;

namespace CS480SocketLab3
{
    class Client
    {
        static void Main(string[] arrCommandLineParameters)
        {
            if(arrCommandLineParameters.Length < 2)
            {
                Console.WriteLine("Parameters: <domain> <path> <port>(optional)");
                Environment.Exit(0);
            }

            WebRequest request = CreateWebRequest(arrCommandLineParameters);

            WebResponse response = GetResponse(request);

            Stream dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);

            string strResponseAsString = reader.ReadToEnd();

            Console.WriteLine(strResponseAsString);

        }

        private static WebResponse GetResponse(WebRequest request)
        {
            try
            {
                WebResponse response = request.GetResponse();
                return response;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to connect to server. Ensure server is listening and available to accept new clients.");
                Environment.Exit(0);
            }
            // the above two scenarios will account for all possibilities.
            return null;
        }

        private static WebRequest CreateWebRequest(string[] arrCommandLineParameters)
        {
            try
            {
                string strRequerstString = arrCommandLineParameters[0];

                if (arrCommandLineParameters.Length > 2)
                {
                    strRequerstString += ":" + arrCommandLineParameters[2];
                }

                if(!arrCommandLineParameters[1].Contains("/"))
                {
                    strRequerstString += "/" + arrCommandLineParameters[1];
                }
                else
                {
                    strRequerstString += arrCommandLineParameters[1];
                }
                
                WebRequest request = WebRequest.Create(strRequerstString);
                return request;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to create WebRequest - check input parameters.");
                Console.WriteLine("Parameters: <domain> <path> <port>");
                Environment.Exit(0);
            }
            // the above two scenarios will account for all possibilities.
            return null;
        }
    }
}
