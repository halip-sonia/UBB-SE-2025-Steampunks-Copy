namespace Steampunks.Services
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public class NavigationService : INavigationService
    {
        private readonly IPageService? pageService;
        private object? lastParameterUsed;
        private Frame? frame;

        public NavigationService(IPageService pageService)
        {
            this.pageService = pageService;
        }

        public event NavigatedEventHandler? Navigated;

        public Frame Frame
        {
            get
            {
                if (this.frame == null)
                {
                    this.frame = new Frame();
                    this.RegisterFrameEvents();
                }

                return this.frame;
            }

            set
            {
                this.UnregisterFrameEvents();
                this.frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public bool GoBack()
        {
            if (this.CanGoBack)
            {
                var vmBeforeNavigation = this.frame.GetPageViewModel();
                this.frame.GoBack();
                if (vmBeforeNavigation is INavigationAware navigationAware)
                {
                    navigationAware.OnNavigatedFrom();
                }

                return true;
            }

            return false;
        }

        public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
        {
            var pageType = this.pageService.GetPageTypeForViewModel(pageKey);
            if (pageType == null || !this.IsPageTypeValid(pageType))
            {
                return false;
            }

            if (this.Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(this.lastParameterUsed)))
            {
                this.frame.Tag = clearNavigation;
                var vmBeforeNavigation = this.frame.GetPageViewModel();
                var navigated = this.frame.Navigate(pageType, parameter);
                if (navigated)
                {
                    this.lastParameterUsed = parameter;
                    if (vmBeforeNavigation is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedFrom();
                    }
                }

                return navigated;
            }

            return false;
        }

        private void RegisterFrameEvents()
        {
            if (this.frame != null)
            {
                this.frame.Navigated += this.OnNavigated;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (this.frame != null)
            {
                this.frame.Navigated -= this.OnNavigated;
            }
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
                        SourcePageType = e.SourcePageType,
                    };
                    this.Navigated?.Invoke(sender, navigatedEventArgs);
                }
            }
        }

        private bool IsPageTypeValid(Type pageType)
        {
            return pageType.IsSubclassOf(typeof(Page));
        }
    }
}