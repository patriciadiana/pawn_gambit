using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishManager : MonoBehaviour
{
    public static StockfishManager Instance;

    private Process process;
    private StreamWriter input;

    private bool engineReady = false;

    private string lastBestMove;
    private bool waitingForMove;

    private bool bannerReceived = false;
    private bool uciOkReceived = false;
    private bool readyOkReceived = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartEngine(string exePath)
    {
        if (!File.Exists(exePath))
        {
            Debug.LogError("Stockfish not found: " + exePath);
            return;
        }

        process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        process.OutputDataReceived += OnOutput;
        process.Start();

        input = process.StandardInput;
        process.BeginOutputReadLine();

        StartCoroutine(InitializeEngine());
    }

    private IEnumerator InitializeEngine()
    {
        Debug.Log("Waiting for Stockfish banner...");

        yield return new WaitUntil(() => bannerReceived);

        Debug.Log("Banner received, priming input stream");

        input.WriteLine();
        input.Flush();

        yield return null;

        Debug.Log("Sending UCI");
        Send("uci");

        yield return new WaitUntil(() => uciOkReceived);

        Debug.Log("uciok received, sending isready");
        Send("isready");

        yield return new WaitUntil(() => readyOkReceived);

        engineReady = true;
        Debug.Log("Stockfish is fully ready");
    }

    private void OnOutput(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;

        string line = e.Data.Trim();
        Debug.Log("[Stockfish] " + line);

        // Banner line (version info)
        if (!bannerReceived && line.StartsWith("Stockfish 17.1"))
        {
            bannerReceived = true;
        }

        if (line == "uciok")
        {
            uciOkReceived = true;
        }

        if (line == "readyok")
        {
            readyOkReceived = true;
        }

        if (line.StartsWith("bestmove"))
        {
            lastBestMove = line.Split(' ')[1];
            waitingForMove = false;
        }
    }

    public IEnumerator GetBestMove(string fen, int depth, Action<string> callback)
    {
        yield return new WaitUntil(() => engineReady);

        lastBestMove = null;
        waitingForMove = true;

        Send("ucinewgame");
        Send("isready");

        // Wait until Stockfish confirms new game state
        yield return new WaitUntil(() => readyOkReceived);

        Send($"position fen {fen}");
        Send($"go depth {depth}");

        yield return new WaitUntil(() => !waitingForMove);

        callback?.Invoke(lastBestMove);
    }

    private void Send(string command)
    {
        if (input == null)
            return;

        command = command
            .Replace("\uFEFF", "")
            .Replace("\0", "")
            .Trim();

        Debug.Log("[Sending] " + command);
        input.WriteLine(command);
        input.Flush();
    }

    void OnApplicationQuit()
    {
        if (process != null && !process.HasExited)
        {
            Send("quit");
            process.Kill();
        }
    }
}
