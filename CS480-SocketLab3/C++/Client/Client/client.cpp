/******************************************************************************
webclient.cpp - Simple web client using Winsock

This program demonstrates the use of the Windows Sockets API for a simple
web client application.  The user interface is via a MS Dos window.

This program has been compiled and tested under Microsoft Visual Studio 2010.

Project building notes:
In solution explorer, select your project->project configuration properties->
linker->additional include library: Ws2_32.lib
Also add _CRT_SECURE_NO_WARNINGS as a preprocessor directive

Copyright 2008 by Ziping Liu
Updated on 2011 by Ziping Liu
Prepared for CS480, Southeast Missouri State University

******************************************************************************/
/*-----------------------------------------------------------------------
*
* Program: webclient
* Purpose: fetch page from webserver and dump to stdout with headers
* Usage:   webclient <compname> <path> [portnum]
* Note:    <compname> can be either a computer name, like localhost, xx.cs.semo.edu
*          or an IP address, like 150.168.0.1
*-----------------------------------------------------------------------
*/


// Necessary for deprecation suppression.
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <fcntl.h>

#define DEFAULT_PROTO SOCK_STREAM 
#define BUFFSIZE	256
#define STDIN_FILENO  0
#define STDOUT_FILENO 1

int main(int argc, char *argv[])
{
	int retval;                         /* status return value */
	unsigned int addr;
	int socket_type = DEFAULT_PROTO;    /* use TCP protocol */
	struct sockaddr_in server;          /* socket address and port */
	struct hostent *hp;                 /* ptr to host info struct */
	WSADATA wsaData;                    /* Windows Sockets API data */
	SOCKET  conn_socket;                /* socket descriptor */
	char BufferOut[BUFFSIZE];            /* buffer to hold out sentence */
	char BufferIn[BUFFSIZE];             /* buffer to hold in sentence */
	int bufcnt;                         /* no. of chars in buffer */

	if (argc < 3 || argc > 4) {
		fprintf(stderr, "%s%s%s", "usage: ", argv[0],
			" <compname> <path> [appnum]\n");
		exit(1);
	}

	/*------------------------------------------------------------------------*/
	/*                   Start up the Windows Sockets API.                    */
	/*------------------------------------------------------------------------*/
	printf("Client: Starting WinSock API...\n");
	if (WSAStartup(0x202, &wsaData) == SOCKET_ERROR) {
		fprintf(stderr, "WSAStartup failed with error %d\n", WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/*------------------------------------------------------------------------*/
	/* Attempt to detect if we should call gethostbyname() or gethostbyaddr() */
	/*------------------------------------------------------------------------*/
	/* call isalpha() function to determine the first character of the server name
	is a character or a numeric number */
	if (isalpha(*argv[1])) {   /* server address is a name */
		hp = gethostbyname(argv[1]);
	}
	else { /* Convert nnn.nnn address to a usable one */
		addr = inet_addr(argv[1]);
		hp = gethostbyaddr((char *)&addr, 4, AF_INET);
	}
	if (hp == NULL) {
		fprintf(stderr, "Client: Cannot resolve address [%s]: Error %d\n",
			argv[1], WSAGetLastError());
		WSACleanup();
		exit(1);
	}

	/*------------------------------------------------------------------------*/
	/*      Copy the resolved information into the sockaddr_in structure      */
	/*------------------------------------------------------------------------*/
	memset(&server, 0, sizeof(server));
	memcpy(&(server.sin_addr), hp->h_addr, hp->h_length);
	server.sin_family = hp->h_addrtype;
	server.sin_port = htons(atoi(argv[3]));


	/*-----------------------------------------------------------------------*/
	/*                  Create the socket                                    */
	/*-----------------------------------------------------------------------*/
	conn_socket = socket(AF_INET, socket_type, 0);
	if (conn_socket <0) {
		fprintf(stderr, "Client: Error Opening socket: Error %d\n",
			WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/*----------------------------------------------------------------------*/
	/*          Connect and bind the socket                                 */
	/*----------------------------------------------------------------------*/
	printf("Client connecting to: %s\n", hp->h_name);
	if (connect(conn_socket, (struct sockaddr*)&server, sizeof(server))
		== SOCKET_ERROR) {
		fprintf(stderr, "connect() failed: %d\n", WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/* send an HTTP/1.0 request to the webserver */
	bufcnt = sprintf(BufferOut, "GET %s HTTP/1.0\r\n\r\n", argv[2]);
	send(conn_socket, BufferOut, bufcnt, 0);
	printf("passed connection\n");

	/* dump all data received from the server to stdout */
	while ((retval = recv(conn_socket, BufferOut, BUFFSIZE, 0)) > 0)
		fwrite(BufferOut, sizeof(char), retval, stdout);

	/*---------------------------------------------------------------------*/
	/*					Close connection and Clean up                      */
	/*---------------------------------------------------------------------*/
	closesocket(conn_socket);
	WSACleanup();

	return 0;
}