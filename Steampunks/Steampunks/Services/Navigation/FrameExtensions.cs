// <copyright file="FrameExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

    public static class FrameExtensions
    {
        public static object? GetPageViewModel(this Frame frame)
            => frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content);
    }
}