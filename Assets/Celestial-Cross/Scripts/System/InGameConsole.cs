using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.System
{
    public class InGameConsole : MonoBehaviour
    {
        private string logText = "";
        private Queue<string> logQueue = new Queue<string>();
        private const int MAX_LINES = 15;
        private bool showConsole = true;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            // Oculta logs normais para exibir apenas Erros e Warnings (que quebram o jogo)
            if (type == LogType.Log) return; 

            string color = "white";
            if (type == LogType.Error || type == LogType.Exception) color = "red";
            else if (type == LogType.Warning) color = "yellow";

            string logLine = $"<color={color}>[{type}] {logString}</color>\n";
            logQueue.Enqueue(logLine);

            if (type == LogType.Exception || type == LogType.Error)
            {
                // Limita a stacktrace para não lotar a tela
                string shortStack = stackTrace.Length > 300 ? stackTrace.Substring(0, 300) + "..." : stackTrace;
                logQueue.Enqueue($"<color=red>{shortStack}</color>\n");
            }

            while (logQueue.Count > MAX_LINES)
            {
                logQueue.Dequeue();
            }

            logText = string.Join("", logQueue);
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 200, 80), showConsole ? "Ocultar Logs" : "Mostrar Logs (DEBUG)"))
            {
                showConsole = !showConsole;
            }

            if (!showConsole) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = Screen.width > 1000 ? 35 : 25;
            style.richText = true;
            style.wordWrap = true;

            GUI.Label(new Rect(10, 100, Screen.width - 20, Screen.height - 110), logText, style);
        }
    }
}
