using Devkoes.JenkinsManager.Model.Schema;
using System;
using System.Collections.Generic;

namespace Devkoes.JenkinsManager.UI.Comparers
{
    public class JobComparer : IEqualityComparer<JenkinsJob>
    {
        public bool Equals(JenkinsJob x, JenkinsJob y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x != null && y != null)
            {
                return string.Equals(x.Url, y.Url, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public int GetHashCode(JenkinsJob obj)
        {
            return obj.Url.ToLower().GetHashCode();
        }
    }
}
