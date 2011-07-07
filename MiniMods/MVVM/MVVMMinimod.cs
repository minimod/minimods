using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Minimod.RxMessageBus;

namespace Minimod.MVVM
{
    /// <summary>
    /// Minimod.MVVM, Version 0.0.1
    /// <para>A minimod for small message based MVVM.</para>
    /// </summary>
    /// <remarks>
    /// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.
    /// http://www.apache.org/licenses/LICENSE-2.0
    /// </remarks>
    public class ViewModelField<T> : INotifyPropertyChanged
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
                    RxMessageBusMinimod.Default.Send(new PropertyChangedMessage<T>(_value, oldValue));
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

        public override string ToString()
        {
            return Value == null ? String.Empty : Value.ToString();
        }
    }

    public class UiMessageCommand<T> : ICommand, INotifyPropertyChanged
    {
        public IMessage<T> Message { get; private set; }
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

        public UiMessageCommand(IMessage<T> message)
        {
            Message = message;
        }

        public void Execute(object parameter)
        {
            Message.Value = (T)parameter;
            RxMessageBusMinimod.Default.Send(Message);
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged = delegate { };
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    public interface IMessage<T>
    {
        T Value { get; set; }
    }

    public interface ICallbackMessage<T> : IMessage<T>
    {
        Action<T> Callback { get; set; }
    }

    public class PropertyChangedMessage<T> : IMessage<T>
    {
        public T Value { get; set; }
        public T OldValue { get; set; }

        public PropertyChangedMessage(T value, T oldValue)
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
        void Handle(T commandMessage);
    }
}