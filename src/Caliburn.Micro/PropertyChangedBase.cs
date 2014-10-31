using System.Threading.Tasks;
using Caliburn.Micro.JetBrains.Annotations;

namespace Caliburn.Micro {
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Serialization;

    /// <summary>
    /// A base class that implements the infrastructure for property change notification and automatically performs UI thread marshalling.
    /// </summary>
    [DataContract]
    public class PropertyChangedBase : INotifyPropertyChangedEx {
        /// <summary>
        /// Creates an instance of <see cref = "PropertyChangedBase" />.
        /// </summary>
        public PropertyChangedBase() {
            IsNotifying = true;
            NotifyOnUiThread = false;
            AsyncNotifications = false;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Enables/Disables property change notification.
        /// </summary>
        public bool IsNotifying { get; set; }

        /// <summary>
        /// Enables/Disables marshaling property change notification on Ui Thread. Enabled by default.
        /// </summary>
        public bool NotifyOnUiThread { get; set; }        
        
        /// <summary>
        /// Enables/Disables non-synchronous property change notifications (only queue notification and return). Enabled by default.
        /// </summary>
        public bool AsyncNotifications { get; set; }

        /// <summary>
        /// Raises a change notification indicating that all bindings should be refreshed.
        /// </summary>
        public virtual void Refresh() {
            NotifyOfPropertyChange(string.Empty);
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <param name = "propertyName">Name of the property.</param>
#if NET || SILVERLIGHT
        public virtual void NotifyOfPropertyChange(string propertyName) {
#else
        public virtual void NotifyOfPropertyChange([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) {
#endif
            if (!IsNotifying) { return; }

            if (NotifyOnUiThread) {
                if (AsyncNotifications)
                {
                    Execute.BeginOnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
                }
                else
                {
                    Execute.OnUIThread(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
                }
            } else {
                if (AsyncNotifications) {
                    Task.Factory.StartNew(() => OnPropertyChanged(new PropertyChangedEventArgs(propertyName)));
                }
                else
                {
                    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        /// <summary>
        /// Notifies subscribers of the property change.
        /// </summary>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <param name = "property">The property expression.</param>
        [NotifyPropertyChangedInvocator]
        public void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property) {
            NotifyOfPropertyChange(property.GetMemberInfo().Name);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged" /> event directly.
        /// </summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected void OnPropertyChanged(PropertyChangedEventArgs e) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, e);
            }
        }
    }

    namespace JetBrains.Annotations
    {
        /// <summary>
        /// Indicates that the method is contained in a type that implements
        /// <see cref="System.ComponentModel.INotifyPropertyChanged"/> interface
        /// and this method is used to notify that some property value changed
        /// </summary>
        /// <remarks>
        /// The method should be non-static and conform to one of the supported signatures:
        /// <list>
        /// <item><c>NotifyChanged(string)</c></item>
        /// <item><c>NotifyChanged(params string[])</c></item>
        /// <item><c>NotifyChanged{T}(Expression{Func{T}})</c></item>
        /// <item><c>NotifyChanged{T,U}(Expression{Func{T,U}})</c></item>
        /// <item><c>SetProperty{T}(ref T, T, string)</c></item>
        /// </list>
        /// </remarks>
        /// <example><code>
        /// public class Foo : INotifyPropertyChanged {
        ///   public event PropertyChangedEventHandler PropertyChanged;
        ///   [NotifyPropertyChangedInvocator]
        ///   protected virtual void NotifyChanged(string propertyName) { ... }
        ///
        ///   private string _name;
        ///   public string Name {
        ///     get { return _name; }
        ///     set { _name = value; NotifyChanged("LastName"); /* Warning */ }
        ///   }
        /// }
        /// </code>
        /// Examples of generated notifications:
        /// <list>
        /// <item><c>NotifyChanged("Property")</c></item>
        /// <item><c>NotifyChanged(() =&gt; Property)</c></item>
        /// <item><c>NotifyChanged((VM x) =&gt; x.Property)</c></item>
        /// <item><c>SetProperty(ref myField, value, "Property")</c></item>
        /// </list>
        /// </example>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
        {
#pragma warning disable 1591
            public NotifyPropertyChangedInvocatorAttribute() { }
            public NotifyPropertyChangedInvocatorAttribute(string parameterName)
            {
                ParameterName = parameterName;
            }
#pragma warning restore 1591

            public string ParameterName { get; private set; }
        }

    }
}
