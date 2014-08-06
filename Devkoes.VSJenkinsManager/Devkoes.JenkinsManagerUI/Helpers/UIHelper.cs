using System;
using System.Threading;
using System.Windows;

namespace Devkoes.JenkinsManagerUI.Helpers
{
    internal class UIHelper
    {
        internal static void InvokeUI(Action uiAction, bool asyncAllowed = false)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                if (object.ReferenceEquals(Application.Current.Dispatcher.Thread, Thread.CurrentThread))
                {
                    uiAction();
                }
                else
                {
                    if (asyncAllowed)
                    {
                        Application.Current.Dispatcher.BeginInvoke(uiAction);
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(uiAction);
                    }
                }
            }
            else
            {
                uiAction();
            }
        }
    }
}
