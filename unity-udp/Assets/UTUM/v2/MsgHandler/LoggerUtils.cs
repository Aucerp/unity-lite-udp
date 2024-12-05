using System;
namespace s2
{
    public static class LoggerUtils
    {
        public static string GetTimestamp()
        {
            return DateTime.Now.ToString("[yyyy.MM.dd,HH:mm:ss:fff]");
        }

        public static string FormatLog(string source, string endpoint, string action, string content, string senderRole, string senderEndpoint)
        {
            return string.Format(
                "{0} [{1}] [{2}] {3}: {4}, 來自: {5} {6}",
                GetTimestamp(),
                source,
                endpoint,
                action,
                content,
                senderRole,
                senderEndpoint
            );
        }
    }

}