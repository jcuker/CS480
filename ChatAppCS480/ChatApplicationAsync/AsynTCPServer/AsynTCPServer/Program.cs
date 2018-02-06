/******************************************************************************
             AsynTCPServer.cs - Simple Asynchronous TCP echo server using sockets

  This program demonstrates the use of asynchronous socket APIs to echo back the
  client sentence.  The user interface is via a GUI window.

  This program has been compiled and tested under Microsoft Visual Studio 2010.

  Copyright 2012 by Ziping Liu for VS2010
  Prepared for CS480, Southeast Missouri State University

******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AsynTCPServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
