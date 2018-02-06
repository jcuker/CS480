/******************************************************************************
webserver.c - Simple web server using Winsock

This program demonstrates the use of the Windows Sockets API for a simple web
server.  The user interface is via a MS Dos window.

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
* Program: webserver
* Purpose: serve hard-coded webpages to web clients
* Usage:   webserver <portnum>
*
*-----------------------------------------------------------------------
*/

// Necessary for deprecation suppression.
#define _WINSOCK_DEPRECATED_NO_WARNINGS


#include <Winsock2.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <time.h>
#include <sys/stat.h>
#include <fcntl.h>

#define BUFFSIZE	256
#define MAX_PATH	100
#define SERVER_NAME	"CS480 Demo Web Server"

#define ERROR_400	"<head></head><body><html><h1>Error 400</h1><p>The server couldn't understand your request.</html></body>\n"

#define ERROR_404	"<head></head><body><html><h1>Error 404</h1><p>Document not found.</html></body>\n"

#define HOME_PAGE	"<head></head><body><html><h1>Welcome to the CS480 Demo Server</h1><p>Why not visit: <ul><li><a href=\"http://www2.semo.edu/csdept/\"> Computer Science Home Page</a><li><a href=\"http://cstl-csm.semo.edu/liu/cs480_fall2012/index.htm\">CS480 Home Page<a></ul></html></body>\n"

#define DEFAULT_PROTO SOCK_STREAM





void send_head(SOCKET conn, int stat, int len);
int recvln(SOCKET conn, char *buff, int buffsz);

int main(int argc, char *argv[])
{
	int	 n;
	char cmd[16], path[MAX_PATH], vers[16], dirSpec[MAX_PATH];
	char Buffer[256];					/* hold data received from client */
	int bufcnt;							/* count the number of characters received from client */
	int retval, fromlen;				/* status return value */
	int socket_type = DEFAULT_PROTO;    /* use TCP protocol */
	struct sockaddr_in local, from;		/* point to host info struct */
	WSADATA wsaData;					/* Windows Sockets API data */
	SOCKET listen_socket, msgsock;		/* socket descriptor */
										/* ============================================================*/
										/* For lab1, declare a file descriptor below, using FILE       */
										/* ============================================================*/

	if (argc != 2) {
		(void)fprintf(stderr, "usage: %s <appnum>\n", argv[0]);
		exit(1);
	}

	/*------------------------------------------------------------------------*/
	/*                   Start up the Windows Sockets API.                    */
	/*------------------------------------------------------------------------*/
	if (WSAStartup(0x202, &wsaData) == SOCKET_ERROR) {
		fprintf(stderr, "WSAStartup failed with error %d\n", WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/*------------------------------------------------------------------------*/
	/*     assign IP address and Port number to server "local"                */
	/*------------------------------------------------------------------------*/
	local.sin_family = AF_INET;
	local.sin_addr.s_addr = htonl(INADDR_ANY);
	/* call atoi() function to convert CHAR number to INT number */
	/* call htons() function to convert Port number in Network Byte Order */
	local.sin_port = htons(atoi(argv[1]));

	/*-----------------------------------------------------------------------*/
	/*  Create the socket for the server to listen for connection request    */
	/*-----------------------------------------------------------------------*/
	listen_socket = socket(AF_INET, socket_type, 0);

	if (listen_socket == INVALID_SOCKET) {
		fprintf(stderr, "socket() failed with error %d\n", WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/*----------------------------------------------------------------------*/
	/* bind() associates a local address and port combination with the socket created */
	/*----------------------------------------------------------------------*/

	if (bind(listen_socket, (struct sockaddr*)&local, sizeof(local))
		== SOCKET_ERROR) {
		fprintf(stderr, "bind() failed with error %d\n", WSAGetLastError());
		WSACleanup();
		return -1;
	}

	/*----------------------------------------------------------------------*/
	/*       listen on the created socket, not use UDP					    */
	/*----------------------------------------------------------------------*/
	if (socket_type != SOCK_DGRAM) {
		if (listen(listen_socket, 5) == SOCKET_ERROR) {
			fprintf(stderr, "listen() failed with error %d\n", WSAGetLastError());
			WSACleanup();
			return -1;
		}
	}

	fromlen = sizeof(from);

	/* server uses inifinite loop to keep itself always on  */
	/* and allow multiple clients to contact it             */
	while (1) {
		printf("'Listening' on port %d, protocol %s\n", atoi(argv[1]),
			(socket_type == SOCK_STREAM) ? "TCP" : "UDP");

		/*----------------------------------------------------------------------*/
		/*  accept the connection request from a client and allocate a socket   */
		/*----------------------------------------------------------------------*/
		msgsock = accept(listen_socket, (struct sockaddr*)&from, &fromlen);
		if (msgsock == INVALID_SOCKET) {
			fprintf(stderr, "accept() error %d\n", WSAGetLastError());
			WSACleanup();
			return -1;
		}
		printf("accepted connection from %s, port %d\n",
			inet_ntoa(from.sin_addr),
			htons(from.sin_port));

		/*---------------------------------------------------------------------*/
		/* Read from socket and write to stdout, echo received data back to socket */
		/* Server use infinite loop to wait for client's request to terminate connetion */
		/*---------------------------------------------------------------------*/

		/* receive message from the client and store it in Buffer */
		retval = recvln(msgsock, Buffer, BUFFSIZE);

		if (retval == SOCKET_ERROR) {
			fprintf(stderr, "recv() failed: error %d\n", WSAGetLastError());
			closesocket(msgsock);
			break;
		}
		if (retval == 0) {
			printf("Client closed connection\n");
			closesocket(msgsock);
			break;
		}

		printf("RECEIVE FROM CLIENT: ");
		fwrite(Buffer, sizeof(char), retval, stdout);
		/* read and parse the request line */
		sscanf(Buffer, "%s %s %s", cmd, path, vers);

		/* skip all headers - read until we get \r\n alone */
		while ((n = recvln(msgsock, Buffer, BUFFSIZE)) > 0) {
			if (n == 2 && Buffer[0] == '\r' && Buffer[1] == '\n')
				break;
		}

		/* check for unexpected end of file */
		if (n < 1) {
			shutdown(msgsock, 1);
			continue;
		}

		/* check for a request that we cannot understand */
		if (strcmp(cmd, "GET") || (strcmp(vers, "HTTP/1.0") &&
			strcmp(vers, "HTTP/1.1"))) {
			send_head(msgsock, 400, strlen(ERROR_400));
			send(msgsock, ERROR_400, strlen(ERROR_400), 0);
			shutdown(msgsock, 1);
			continue;
		}

		/* send the requested web page or a "not found" error */
		if (strcmp(path, "/") == 0) {
			send_head(msgsock, 200, strlen(HOME_PAGE));
			send(msgsock, HOME_PAGE, strlen(HOME_PAGE), 0);
		}
		else { /* not found */
			send_head(msgsock, 404, strlen(ERROR_404));
			send(msgsock, ERROR_404, strlen(ERROR_404), 0);
		}
		shutdown(msgsock, 1);
		/*---------------------------------------------------------------------*/
		/* Terminate connection with the requested client                      */
		/*---------------------------------------------------------------------*/
		printf("Terminating connection with %s, port %d\n",
			inet_ntoa(from.sin_addr),
			htons(from.sin_port));
		closesocket(msgsock);

		/* server continue listening on the listen socket */
		printf("server continue listening on the listen socket\n");
	}

	return 0;
}

/*-----------------------------------------------------------------------
* send_head - send an HTTP 1.0 header with given status and content-len
*-----------------------------------------------------------------------
*/
void send_head(SOCKET conn, int stat, int len)
{
	char	*statstr, buff[BUFFSIZE];

	/* convert the status code to a string */

	switch (stat) {
	case 200:
		statstr = "OK";
		break;
	case 400:
		statstr = "Bad Request";
		break;
	case 404:
		statstr = "Not Found";
		break;
	default:
		statstr = "Unknown";
		break;
	}

	/*
	* send an HTTP/1.0 response with Server, Content-Length,
	* and Content-Type headers.
	*/

	sprintf(buff, "HTTP/1.0 %d %s\r\n", stat, statstr);
	send(conn, buff, strlen(buff), 0);

	sprintf(buff, "Server: %s\r\n", SERVER_NAME);
	send(conn, buff, strlen(buff), 0);

	sprintf(buff, "Content-Length: %d\r\n", len);
	send(conn, buff, strlen(buff), 0);

	sprintf(buff, "Content-Type: text/html\r\n");
	send(conn, buff, strlen(buff), 0);

	sprintf(buff, "\r\n");
	send(conn, buff, strlen(buff), 0);
}

/*------------------------------------------------------------------------
* recvln - recv from socket until newline or EOF is encountered
* Flush to newline or EOF and return on full buffer. Returns data length.
*------------------------------------------------------------------------
*/
int recvln(SOCKET conn, char *buff, int buffsz)
{
	char	*bp = buff, c;
	int	n;

	while (bp - buff < buffsz &&
		(n = recv(conn, bp, 1, 0)) > 0) {
		if (*bp++ == '\n')
			return (bp - buff);
	}

	if (n < 0)
		return -1;

	if (bp - buff == buffsz)
		while (recv(conn, &c, 1, 0) > 0 && c != '\n');

	return (bp - buff);
}
