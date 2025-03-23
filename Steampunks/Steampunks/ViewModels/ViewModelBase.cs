using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Steampunks.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isBusy;
        private string _errorMessage = string.Empty;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected void SetError(string message)
        {
            ErrorMessage = message;
        }

        protected void ClearError()
        {
            ErrorMessage = string.Empty;
        }
    }
} 