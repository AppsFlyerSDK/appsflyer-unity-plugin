using System.IO;
using UnityEngine;

public static class AFQALogger
{
    private static string _logFilePath;

    static AFQALogger()
    {
#if UNITY_IOS && !UNITY_EDITOR
        _logFilePath = Path.Combine(Application.persistentDataPath, "af_qa_logs.txt");
        // Truncate on each fresh app launch so phase captures are self-contained.
        File.WriteAllText(_logFilePath, string.Empty);
#endif
    }

    public static void Log(string message)
    {
        Debug.Log(message);

#if UNITY_IOS && !UNITY_EDITOR
        if (!string.IsNullOrEmpty(_logFilePath))
        {
            File.AppendAllText(_logFilePath, message + "\n");
        }
#endif
    }
}
