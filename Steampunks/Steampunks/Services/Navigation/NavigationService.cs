using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Steampunks.Services
{
    public class NavigatedEventArgs : EventArgs
    {
        public object Parameter { get; set; }
        public NavigationMode NavigationMode { get; set; }
        public Type SourcePageType { get; set; }
    }

    public class NavigationService : INavigationService
    {
        private readonly IPageService _pageService;
        private object _lastParameterUsed;
        private Frame _frame;

        public event NavigatedEventHandler Navigated;

        public Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = new Frame();
                    RegisterFrameEvents();
                }
                return _frame;
            }
            set
            {
                UnregisterFrameEvents();
                _frame = value;
                RegisterFrameEvents();
            }
        }

        public bool CanGoBack => Frame.CanGoBack;

        public NavigationService(IPageService pageService)
        {
            _pageService = pageService;
        }

        private void RegisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated += OnNavigated;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated -= OnNavigated;
            }
        }

        public bool GoBack()
        {
            if (CanGoBack)
            {
                var vmBeforeNavigation = _frame.GetPageViewModel();
                _frame.GoBack();
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }
                return true;
            }
            return false;
        }

        public bool NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false)
        {
            var pageType = _pageService.GetPageTypeForViewModel(pageKey);
            if (pageType == null || !IsPageTypeValid(pageType))
            {
                return false;
            }

            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed)))
            {
                _frame.Tag = clearNavigation;
                var vmBeforeNavigation = _frame.GetPageViewModel();
                var navigated = _frame.Navigate(pageType, parameter);
                if (navigated)
                {
                    _lastParameterUsed = parameter;
                    if (vmBeforeNavigation is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedFrom();
                    }
                }
                return navigated;
            }
            return false;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            if (sender is Frame frame)
            {
                bool clearNavigation = (bool)frame.Tag;
                if (clearNavigation)
                {
                    frame.BackStack.Clear();
                }
                if (frame.Content is Page page)
                {
                    var navigatedEventArgs = new NavigatedEventArgs
                    {
                        Parameter = e.Parameter,
                        NavigationMode = e.NavigationMode,
                        SourcePageType = e.SourcePageType
                    };
                    Navigated?.Invoke(sender, navigatedEventArgs);
                }
            }
        }

        private bool IsPageTypeValid(Type pageType)
        {
            return pageType.IsSubclassOf(typeof(Page));
        }
    }

    public static class FrameExtensions
    {
        public static object GetPageViewModel(this Frame frame)
            => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content);
    }

    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom();
    }
}