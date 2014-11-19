using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Devkoes.Core.MVVM
{
    /// <summary>
    /// Implementation is like the version in Microsoft PRISM
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler handler2;
                PropertyChangedEventHandler propertyChanged = this.propertyChanged;
                do
                {
                    handler2 = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Combine(handler2, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this.propertyChanged, handler3, handler2);
                }
                while (propertyChanged != handler2);
            }
            remove
            {
                PropertyChangedEventHandler handler2;
                PropertyChangedEventHandler propertyChanged = this.propertyChanged;
                do
                {
                    handler2 = propertyChanged;
                    PropertyChangedEventHandler handler3 = (PropertyChangedEventHandler)Delegate.Remove(handler2, value);
                    propertyChanged = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this.propertyChanged, handler3, handler2);
                }
                while (propertyChanged != handler2);
            }
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null)
            {
                throw new ArgumentNullException("propertyNames");
            }
            foreach (string str in propertyNames)
            {
                this.RaisePropertyChanged(str);
            }
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            string propertyName = PropertySupport.ExtractPropertyName<T>(propertyExpression);
            this.RaisePropertyChanged(propertyName);
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = this.propertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public static class PropertySupport
    {
        // Methods
        public static string ExtractPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            MemberExpression body = propertyExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("propertyExpression");
            }
            PropertyInfo member = body.Member as PropertyInfo;
            if (member == null)
            {
                throw new ArgumentException("propertyExpression");
            }
            if (member.GetGetMethod(true).IsStatic)
            {
                throw new ArgumentException("propertyExpression");
            }
            return body.Member.Name;
        }
    }
}
