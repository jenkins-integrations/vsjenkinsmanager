using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Devkoes.JenkinsClient.Model
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (PropertyChanged != null)
            {
                string propertyName = GetPropertyName(propertyExpression);
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            MemberExpression body = propertyExpression.Body as MemberExpression;

            if (body == null)
            {
                throw new ArgumentException("Invalid argument", "propertyExpression");
            }

            PropertyInfo prop = body.Member as PropertyInfo;

            if (prop == null)
            {
                throw new ArgumentException("Argument is not a property", "propertyExpression");
            }

            return prop.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
