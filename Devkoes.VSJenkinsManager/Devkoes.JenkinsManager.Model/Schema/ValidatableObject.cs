using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Devkoes.JenkinsManager.Model.Schema
{
    public class ValidatableObject<T> : ObservableObject, INotifyDataErrorInfo where T : ValidatableObject<T>
    {
        private Dictionary<string, IEnumerable<ValidationResult>> _propertyValidationResults;
        private Dictionary<string, List<Func<T, IEnumerable<ValidationResult>>>> _propertyValidationRules;
        private Dictionary<string, List<Func<T, Task<IEnumerable<ValidationResult>>>>> _asyncPropertyValidationRules;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ValidatableObject()
        {
            _propertyValidationResults = new Dictionary<string, IEnumerable<ValidationResult>>();
            _propertyValidationRules = new Dictionary<string, List<Func<T, IEnumerable<ValidationResult>>>>();
            _asyncPropertyValidationRules = new Dictionary<string, List<Func<T, Task<IEnumerable<ValidationResult>>>>>();

            PropertyChanged += ValidatableObject_PropertyChanged;
        }

        private async void ValidatableObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vResults = new List<ValidationResult>();
            if (_propertyValidationRules.ContainsKey(e.PropertyName))
            {
                foreach (var vr in _propertyValidationRules[e.PropertyName])
                {
                    var r = vr((T)this);
                    if (r != null)
                    {
                        vResults.AddRange(r);

                    }
                }
            }

            if (_asyncPropertyValidationRules.ContainsKey(e.PropertyName))
            {
                foreach (var vr in _asyncPropertyValidationRules[e.PropertyName])
                {
                    var r = await vr((T)this);
                    if (r != null)
                    {
                        vResults.AddRange(r);

                    }
                }
            }

            _propertyValidationResults[e.PropertyName] = vResults;
            RaiseErrorChanged(e.PropertyName);
        }

        public void RegisterAsyncValidationRule<R>(
            Expression<Func<T, R>> propertyExpression,
            Func<T, Task<IEnumerable<ValidationResult>>> validationRule)
        {
            string propertyName = GetPropertyName(propertyExpression);
            if (!_asyncPropertyValidationRules.ContainsKey(propertyName))
            {
                _asyncPropertyValidationRules.Add(propertyName, new List<Func<T, Task<IEnumerable<ValidationResult>>>>());
            }

            _asyncPropertyValidationRules[propertyName].Add(validationRule);
        }

        public void RegisterValidationRule<R>(Expression<Func<T, R>> propertyExpression, Func<T, IEnumerable<ValidationResult>> validationRule)
        {
            string propertyName = GetPropertyName(propertyExpression);
            if (!_propertyValidationRules.ContainsKey(propertyName))
            {
                _propertyValidationRules.Add(propertyName, new List<Func<T, IEnumerable<ValidationResult>>>());
            }

            _propertyValidationRules[propertyName].Add(validationRule);
        }

        protected void RaiseErrorChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (_propertyValidationResults.ContainsKey(propertyName))
            {
                return _propertyValidationResults[propertyName];
            }

            return null;
        }

        public bool HasErrors
        {
            get { return _propertyValidationResults.Any((p) => p.Value.Any()); }
        }

        protected string GetPropertyName<R>(Expression<Func<T, R>> propertyExpression)
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
    }
}
