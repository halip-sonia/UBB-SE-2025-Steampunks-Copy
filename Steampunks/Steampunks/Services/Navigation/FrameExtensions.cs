namespace Steampunks.Services
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public static class FrameExtensions
    {
        public static object? GetPageViewModel(this Frame frame)
            => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content);
    }
}