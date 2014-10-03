using Devkoes.JenkinsManager.APIHandler.Managers;
using System;

namespace Devkoes.JenkinsManager.UI
{
    public static class Logger
    {
        public static void Log(string message)
        {
            if (SettingManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(message);
            }
        }

        public static void Log(string format, params object[] args)
        {
            if (SettingManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(format, args);
            }
        }

        public static void Log(Exception ex)
        {
            if (SettingManager.DebugEnabled)
            {
                ServicesContainer.OutputWindowLogger.LogOutput(ex);
            }
        }
    }
}
