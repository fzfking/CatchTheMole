using System;
using System.IO;
using UnityEngine;
using Object = System.Object;

namespace Source.Codebase.Architecture.Logger
{
    public static class Logger
    {
#if !UNITY_EDITOR
        private static readonly string LogFilePath =
            $"{Application.persistentDataPath}/LogFile_{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}.log";
#else
        private static readonly string LogFilePath =
            $"{Application.streamingAssetsPath}/LogFile_{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}.log";
#endif

        public static void Clear()
        {
            if (File.Exists(LogFilePath))
            {
                File.WriteAllText(LogFilePath, $"");
            }
        }

        public static void Log(this Object context, string value)
        {
            Debug.Log(value);
            if (File.Exists(LogFilePath))
            {
                using (var writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{context}: {value}");
                }
            }
            else
            {
                File.Create(LogFilePath).Close();
                Log(context, value);
            }
        }
    }
}