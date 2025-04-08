// <copyright file="INavigationViewService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Steampunks.Services
{
    using Microsoft.UI.Xaml.Controls;

    public interface INavigationViewService
    {
        INavigationService NavigationService { get; }

        NavigationView? NavigationView { get; set; }

        void Initialize(NavigationView navigationView);

        void UnregisterEvents();
    }
}