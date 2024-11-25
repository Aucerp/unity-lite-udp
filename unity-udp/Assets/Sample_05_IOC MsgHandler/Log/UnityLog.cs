using UnityEngine;
using System;

namespace Sample_05
{
    public interface ILog
    {
        void Log(string scriptName, string message);
        void LogError(string scriptName, string message, System.Exception ex = null);
    }

    public class UnityLog : ILog
    {
        private string GetTimestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff");
        }

        private string FormatMessage(string scriptName, string message)
        {
            return string.Format("{0} | [{1}] {2}", GetTimestamp(), scriptName, message);
        }

        public void Log(string scriptName, string message)
        {
            Debug.Log(FormatMessage(scriptName, message));
        }

        public void LogError(string scriptName, string message, Exception ex = null)
        {
            string errorMessage = FormatMessage(scriptName, message);
            if (ex != null)
            {
                errorMessage = string.Format("{0}\nStackTrace:\n{1}", errorMessage, ex.StackTrace);
            }
            Debug.LogError(errorMessage);
        }
    }
}