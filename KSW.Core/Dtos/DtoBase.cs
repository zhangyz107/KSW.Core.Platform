using KSW.Data;
using KSW.Helpers;
using KSW.Language;
using KSW.Localization;
using Prism.Mvvm;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Xml.Serialization;

namespace KSW.Dtos
{
    /// <summary>
    /// Abstract base class for a DataModel implementation.
    /// </summary>
    [Serializable]
    public abstract class DtoBase : BindableBase, IDataKey, IDataErrorInfo
    {
        [NonSerialized]
        private readonly List<PropertyChangedEventListener> propertyChangedListeners = new List<PropertyChangedEventListener>();
        [NonSerialized]
        private readonly List<CollectionChangedEventListener> collectionChangedListeners = new List<CollectionChangedEventListener>();
        private readonly LanguageManager L;

        protected DtoBase()
        {
            L = LanguageManager.Instance;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Id { get; set; }

        public virtual string this[string columnName]
        {
            get
            {
                var pi = GetType().GetProperty(columnName);
                var value = pi.GetValue(this, null);
                if (pi.IsDefined(typeof(RequiredAttribute), true))
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                        return pi?.GetCustomAttribute<RequiredAttribute>().ErrorMessage ?? string.Format(L["CanNotBeEmpty"]);
                }
                else if (pi.IsDefined(typeof(MaxLengthAttribute), true))
                {
                    var maximun = pi.GetCustomAttribute<MaxLengthAttribute>().Length;
                    if (value is string stringValue && stringValue.Length > maximun)
                        return pi.GetCustomAttribute<MaxLengthAttribute>().ErrorMessage ?? string.Format(L["CannotExceedCharacters"],  maximun);
                }
                else if (pi.IsDefined(typeof(MinLengthAttribute), true))
                {
                    var minimun = pi.GetCustomAttribute<MinLengthAttribute>().Length;
                    if (value is string stringValue && stringValue.Length < minimun)
                        return pi.GetCustomAttribute<MinLengthAttribute>().ErrorMessage ?? string.Format(L["CannotBeLessThanCharacters"], minimun);
                }
                else if (pi.IsDefined(typeof(RangeAttribute), true))
                {
                    if (value is int intValue)
                    {
                        var minimun = pi.GetCustomAttribute<RangeAttribute>().Minimum as int?;
                        var maximun = pi.GetCustomAttribute<RangeAttribute>().Maximum as int?;
                        if (intValue < minimun || intValue > maximun)
                            return pi.GetCustomAttribute<RangeAttribute>().ErrorMessage ?? string.Format(L["MustBeBetween"], minimun, maximun);
                    }
                    else if (value is double doubleValue)
                    {
                        var minimun = pi.GetCustomAttribute<RangeAttribute>().Minimum as double?;
                        var maximun = pi.GetCustomAttribute<RangeAttribute>().Maximum as double?;
                        if (doubleValue < minimun || doubleValue > maximun)
                            return pi.GetCustomAttribute<RangeAttribute>().ErrorMessage ?? string.Format(L["MustBeBetween"], minimun, maximun);
                    }
                    else if (value is DateTime dateTimeValue)
                    {
                        var minimun = pi.GetCustomAttribute<RangeAttribute>().Minimum as DateTime?;
                        var maximun = pi.GetCustomAttribute<RangeAttribute>().Maximum as DateTime?;
                        if (dateTimeValue < minimun || dateTimeValue > maximun)
                            return pi.GetCustomAttribute<RangeAttribute>().ErrorMessage ?? string.Format(L["MustBeBetween"], minimun, maximun);
                    }
                }
                else if (pi.IsDefined(typeof(StringLengthAttribute), true))
                {
                    var minimun = pi.GetCustomAttribute<StringLengthAttribute>().MinimumLength;
                    var maximun = pi.GetCustomAttribute<StringLengthAttribute>().MaximumLength;
                    if (value is string stringValue && (stringValue.Length < minimun || stringValue.Length > maximun))
                        return pi.GetCustomAttribute<RangeAttribute>().ErrorMessage ?? string.Format(L["LengthMustBeBetween"], minimun, maximun);
                }
                else if (pi.IsDefined(typeof(EmailAddressAttribute), true))
                {
                    if (value is string stringValue && !stringValue.IsEmail())
                        return pi.GetCustomAttribute<EmailAddressAttribute>().ErrorMessage ?? L["InvalidEmailAddress"];
                }
                else if (pi.IsDefined(typeof(UrlAttribute), true))
                {
                    if (value is string stringValue && !stringValue.IsUrl())
                        return pi.GetCustomAttribute<UrlAttribute>().ErrorMessage ?? L["InvalidUrl"];
                }
                else if (pi.IsDefined(typeof(PhoneAttribute), true))
                {
                    if (value is string stringValue && !stringValue.IsPhone())
                        return pi.GetCustomAttribute<PhoneAttribute>().ErrorMessage ?? L["InvalidPhone"];
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Adds a weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }

            PropertyChangedEventListener listener = new PropertyChangedEventListener(source, handler);

            propertyChangedListeners.Add(listener);

            PropertyChangedEventManager.AddListener(source, listener, "");
        }

        /// <summary>
        /// Removes the weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }

            PropertyChangedEventListener listener = propertyChangedListeners.LastOrDefault(l =>
                l.Source == source && l.Handler == handler);

            if (listener != null)
            {
                propertyChangedListeners.Remove(listener);
                PropertyChangedEventManager.RemoveListener(source, listener, "");
            }
        }

        /// <summary>
        /// Adds a weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }

            CollectionChangedEventListener listener = new CollectionChangedEventListener(source, handler);

            collectionChangedListeners.Add(listener);

            CollectionChangedEventManager.AddListener(source, listener);
        }

        /// <summary>
        /// Removes the weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }

            CollectionChangedEventListener listener = collectionChangedListeners.LastOrDefault(l =>
                l.Source == source && l.Handler == handler);

            if (listener != null)
            {
                collectionChangedListeners.Remove(listener);
                CollectionChangedEventManager.RemoveListener(source, listener);
            }
        }

        public virtual string Error => string.Join("\n",
            from validationResult in Validate()
            select validationResult.ErrorMessage);

        public virtual IEnumerable<ValidationResult> Validate()
        {
            var validationContext = new ValidationContext(this);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(this, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
