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
        private bool _isValidating;

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
            if (string.Equals(e.PropertyName, "IsValidating"))
            {
                return;
            }

            try
            {
                IsValidating = true;

                await RefreshValidationResults(e.PropertyName, new[] { e.PropertyName });
            }
            catch
            {
            }
            finally
            {
                IsValidating = false;
            }
        }

        public bool IsValidating
        {
            get { return _isValidating; }
            set
            {
                if (_isValidating != value)
                {
                    _isValidating = value;
                    RaisePropertyChanged(() => IsValidating);
                }
            }
        }


        private async Task RefreshValidationResults(string propertyName, IEnumerable<string> alreadyRefreshedPropertyNames)
        {
            var shouldRefreshPropertyNames = new List<string>();
            var validationResults = new List<ValidationResult>();

            ExecuteValidationRules(propertyName, shouldRefreshPropertyNames, validationResults);

            await ExecuteAsyncValidationRules(propertyName, shouldRefreshPropertyNames, validationResults);

            _propertyValidationResults[propertyName] = validationResults;

            RaiseErrorChanged(propertyName);

            await RefreshSideAffectedProperties(alreadyRefreshedPropertyNames, shouldRefreshPropertyNames);
        }

        private async Task ExecuteAsyncValidationRules(
            string propertyName,
            List<string> shouldRefreshPropertyNames,
            List<ValidationResult> validationResults)
        {
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
                                shouldRefreshPropertyNames.Add(resetValidationResult.PropertyName);
                            }
                            else
                            {
                                validationResults.Add(validationRuleResult);
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteValidationRules(
            string propertyName,
            List<string> shouldRefreshPropertyNames,
            List<ValidationResult> validationResults)
        {
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
                                shouldRefreshPropertyNames.Add(resetValidationResult.PropertyName);
                            }
                            else
                            {
                                validationResults.Add(validationRuleResult);
                            }
                        }
                    }
                }
            }
        }

        private async Task RefreshSideAffectedProperties(
            IEnumerable<string> alreadyRefreshedPropertyNames,
            IEnumerable<string> shouldRefreshPropertyNames)
        {
            // prevent recursive property updates
            // Eg: Username can trigger ApiToken to refresh, but in it's turn, ApiToken can't update Username again.
            //     So in one update call, each property can only be refreshed once.
            var allResettedPropertyNames = new List<string>(shouldRefreshPropertyNames);
            allResettedPropertyNames.AddRange(alreadyRefreshedPropertyNames);

            foreach (var resetProperty in shouldRefreshPropertyNames)
            {
                if (!alreadyRefreshedPropertyNames.Contains(resetProperty))
                {
                    await RefreshValidationResults(resetProperty, allResettedPropertyNames);
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

        public bool IsValid
        {
            get
            {
                return
                    !_propertyValidationResults.Any() ||
                    _propertyValidationResults.All((p) => !p.Value.Any()) ||
                    !_propertyValidationResults.Any((p) => p.Value.Any((q) => q.ValidationResultType == ValidationResultType.Error));
            }
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
