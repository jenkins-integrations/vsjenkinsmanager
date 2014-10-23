using Devkoes.JenkinsManager.APIHandler.Managers;
using System;

namespace Devkoes.JenkinsManager.UI
{
    public static class Logger
    {
        public static void Log(string message)
        {
            if (ApiHandlerSettingsManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(message);
            }
        }

        public static void Log(string format, params object[] args)
        {
            if (ApiHandlerSettingsManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(format, args);
            }
        }

        public static void Log(Exception ex)
        {
            if (ApiHandlerSettingsManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(ex);
            }
        }
    }
}
