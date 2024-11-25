using UnityEngine;
public interface ILog
{
    void Log(string message);
    void LogWarning(string message);
    void LogError(string message);
}

public class UnityLog : ILog
{
    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void LogError(string message)
    {
        // 使用 System.Diagnostics.StackTrace 获取更详细的堆栈信息
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true); // 参数 true 表示包含文件名和行号
        string formattedStackTrace = stackTrace.ToString();

        // 拼接字符串输出错误信息及堆栈
        string logMessage = message + "\nStackTrace:\n" + formattedStackTrace;
        Debug.LogError(logMessage);
    }
}

public static class LogSystem
{
    private static ILog _logger = new UnityLog(); // 預設使用 Unity 的日誌系統

    public static void InjectLogger(ILog logger)
    {
        _logger = logger;
    }

    public static void Log(string message)
    {
        _logger.Log(message);
    }

    public static void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    public static void LogError(string message)
    {
        _logger.LogError(message);
    }
}
