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
            await RefreshValidationResults(e.PropertyName);
        }

        private async Task RefreshValidationResults(string propertyName)
        {
            var resetPropertyNames = new List<string>();
            var validationResults = new List<ValidationResult>();

            if (_propertyValidationRules.ContainsKey(propertyName))
            {
                foreach (var validationRule in _propertyValidationRules[propertyName])
                {
                    var validationRuleResults = validationRule((T)this);
                    if (validationRuleResults != null && validationRuleResults.Any())
                    {
                        foreach (var validationRuleResult in validationRuleResults.Distinct())
                        {
                            var resetValidationResult = validationRuleResult as ResetValidationResult;
                            if (resetValidationResult != null)
                            {
                                resetPropertyNames.Add(resetValidationResult.PropertyName);
                            }
                            else
                            {
                                validationResults.Add(validationRuleResult);
                            }
                        }
                    }
                }
            }

            if (_asyncPropertyValidationRules.ContainsKey(propertyName))
            {
                foreach (var validationRule in _asyncPropertyValidationRules[propertyName])
                {
                    var validationRuleResults = await validationRule((T)this);
                    if (validationRuleResults != null && validationRuleResults.Any())
                    {
                        foreach (var validationRuleResult in validationRuleResults.Distinct())
                        {
                            var resetValidationResult = validationRuleResult as ResetValidationResult;
                            if (resetValidationResult != null)
                            {
                                resetPropertyNames.Add(resetValidationResult.PropertyName);
                            }
                            else
                            {
                                validationResults.Add(validationRuleResult);
                            }
                        }
                    }
                }
            }

            _propertyValidationResults[propertyName] = validationResults;

            RaiseErrorChanged(propertyName);

            foreach (var resetProperty in resetPropertyNames)
            {
                if (!string.Equals(resetProperty, propertyName))
                {
                    await RefreshValidationResults(resetProperty);
                }
            }
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
