using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class BumpWebhookServer : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private int port = 56789;

    [Header("References")]
    [SerializeField] private ClimbingPlayer climbingPlayer;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning;

    private readonly ConcurrentQueue<Action> mainThreadActions =
        new ConcurrentQueue<Action>();

    private void Start()
    {
        StartServer();
    }

    private void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    private void OnDisable()
    {
        StopServer();
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    private void StartServer()
    {
        if (isRunning)
            return;

        try
        {
            listener =
                new TcpListener(
                    IPAddress.Any,
                    port
                );

            listener.Start();

            isRunning = true;

            listenerThread =
                new Thread(ListenLoop);

            listenerThread.IsBackground = true;
            listenerThread.Start();

            Debug.Log(
                $"Bump webhook listening on port {port}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Failed to start bump webhook: " +
                $"{exception.Message}"
            );
        }
    }

    private void ListenLoop()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client =
                    listener.AcceptTcpClient();

                HandleClient(client);
            }
            catch (SocketException)
            {
                if (isRunning)
                    Debug.LogError(
                        "Bump webhook socket error."
                    );
            }
            catch (Exception exception)
            {
                if (isRunning)
                    Debug.LogError(
                        $"Webhook error: {exception.Message}"
                    );
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[4096];

            int bytesRead =
                stream.Read(
                    buffer,
                    0,
                    buffer.Length
                );

            if (bytesRead <= 0)
                return;

            string request =
                Encoding.UTF8.GetString(
                    buffer,
                    0,
                    bytesRead
                );

            ProcessRequest(request, stream);
        }
    }

    private void ProcessRequest(
        string request,
        NetworkStream stream)
    {
        string[] lines =
            request.Split(
                new[] { "\r\n" },
                StringSplitOptions.None
            );

        if (lines.Length == 0)
        {
            SendResponse(
                stream,
                400,
                "Bad Request"
            );

            return;
        }

        string[] requestLine =
            lines[0].Split(' ');

        if (requestLine.Length < 2)
        {
            SendResponse(
                stream,
                400,
                "Bad Request"
            );

            return;
        }

        string method =
            requestLine[0].ToUpperInvariant();

        string path =
            requestLine[1];

        bool validMethod =
            method == "GET" ||
            method == "POST";

        bool validPath =
            path == "/bump";

        if (!validMethod || !validPath)
        {
            SendResponse(
                stream,
                404,
                "Not Found"
            );

            return;
        }

        Debug.Log(
            $"Webhook received: {method} {path}"
        );

        mainThreadActions.Enqueue(
            TriggerBump
        );

        SendResponse(
            stream,
            200,
            "Bump received"
        );
    }

    private void TriggerBump()
    {
        if (climbingPlayer == null)
        {
            Debug.LogWarning(
                "Webhook received, but no ClimbingPlayer is assigned."
            );

            return;
        }

        Debug.Log(
            "Webhook triggering player bump!"
        );

        climbingPlayer.ReceiveBump();
    }

    private void SendResponse(
        NetworkStream stream,
        int statusCode,
        string message)
    {
        string statusText =
            statusCode == 200
                ? "OK"
                : statusCode == 404
                    ? "Not Found"
                    : "Bad Request";

        string body = message;

        string response =
            $"HTTP/1.1 {statusCode} {statusText}\r\n" +
            "Content-Type: text/plain\r\n" +
            $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
            "Connection: close\r\n" +
            "\r\n" +
            body;

        byte[] responseBytes =
            Encoding.UTF8.GetBytes(response);

        stream.Write(
            responseBytes,
            0,
            responseBytes.Length
        );
    }

    private void StopServer()
    {
        if (!isRunning && listener == null)
            return;

        isRunning = false;

        try
        {
            listener?.Stop();
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Error stopping webhook listener: " +
                $"{exception.Message}"
            );
        }

        if (listenerThread != null &&
            listenerThread.IsAlive)
        {
            listenerThread.Join(500);
        }

        listener = null;
        listenerThread = null;

        Debug.Log("Bump webhook stopped.");
    }
}