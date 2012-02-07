using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Minimod.RxMessageBroker;

namespace Minimod.MVVM
{
    /// <summary>
    /// Minimod.MVVM, Version 0.0.1
    /// <para>A small reactive message based MVVM.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class ViewModelField<T> : INotifyPropertyChanged, IEventHandler<IUiMessage<T>>
    {
        public ViewModelField()
        {
            _isVisible = true;
            _isEnabled = true;
            _validationMessage = String.Empty;
        }

        public ViewModelField(T defaultValue)
        {
            _value = defaultValue;
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value) == false)
                {
                    var oldValue = _value;
                    _value = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    RxMessageBrokerMinimod.Default.Send(new PropertyChangedUiMessage<T>(_value, oldValue));
                }
            }
        }

        private bool _isValid;
        public bool IsValid { get { return _isValid; } set { _isValid = value; PropertyChanged(this, new PropertyChangedEventArgs("IsValid")); } }
        private string _validationMessage;
        public string ValidationMessage { get { return _validationMessage; } set { _validationMessage = value; PropertyChanged(this, new PropertyChangedEventArgs("ValidationMessage")); } }
        private bool _isVisible;
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; PropertyChanged(this, new PropertyChangedEventArgs("IsVisible")); } }
        private bool _isEnabled;
        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled")); } }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void Handle(IUiMessage<T> commandUiMessage)
        {
            Value = commandUiMessage.Value;
        }

        public override string ToString()
        {
            return Value == null ? String.Empty : Value.ToString();
        }
    }

    public class UiMessageCommand<T> : ICommand, INotifyPropertyChanged
    {
        public IUiMessage<T> UiMessage { get; private set; }
        private bool _isVisible;
        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; PropertyChanged(this, new PropertyChangedEventArgs("IsVisible")); } }
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public UiMessageCommand(IUiMessage<T> uiMessage)
        {
            UiMessage = uiMessage;
        }

        public void Execute(object parameter)
        {
            if (parameter != null)
                UiMessage.Value = (T)parameter;
            RxMessageBrokerMinimod.Default.Send(UiMessage);
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public interface IUiMessage<T>
    {
        T Value { get; set; }
    }

    public interface IUiCallbackMessage<T> : IUiMessage<T>
    {
        Action<T> Callback { get; set; }
        Func<T, bool> ConfirmationCallback { get; set; }
    }

    public class PropertyChangedUiMessage<T> : IUiMessage<T>
    {
        public T Value { get; set; }
        public T OldValue { get; set; }

        public PropertyChangedUiMessage(T value, T oldValue)
        {
            Value = value;
            OldValue = oldValue;
        }
    }

    public interface ICommandHandler<in T>
    {
        void Execute(T commandMessage);
    }

    public interface IEventHandler<in T>
    {
        void Handle(T eventMessage);
    }
}