using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Steampunks.Services
{
    /// <summary>
    /// Manages the mapping of ViewModels to their corresponding Page types.
    /// This service is used to resolve the correct Page type for a given ViewModel.
    /// </summary>
    public class PageService : IPageService
    {
        private readonly Dictionary<string, Type> _viewModelToPagesMap = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes a new instance of the PageService class with the given configuration options.
        /// </summary>
        /// <param name="options">Options containing ViewModel to Page mappings.</param>
        public PageService(IOptions<PageServiceOptions> options)
        {
            LoadPageMappingsFromOptions(options.Value);
        }

        /// <summary>
        /// Retrieves the Page type associated with a given ViewModel key.
        /// </summary>
        /// <param name="key">The unique key representing a ViewModel.</param>
        /// <returns>The Type of the associated Page.</returns>
        /// <exception cref="ArgumentException">Thrown if the key is not found in the mapping.</exception>
        public Type GetPageTypeForViewModel(string key)
        {
            Type pageType;
            lock (_viewModelToPagesMap)
            {
                if (!_viewModelToPagesMap.TryGetValue(key, out pageType))
                {
                    throw new ArgumentException($"Page not found: {key}. Did you forget to call LoadPageMappingsFromOptions?");
                }
            }
            return pageType;
        }

        /// <summary>
        /// Loads ViewModel-to-Page mappings from the provided options.
        /// </summary>
        /// <param name="options">The options containing the mappings to be loaded.</param>
        private void LoadPageMappingsFromOptions(PageServiceOptions options)
        {
            lock (_viewModelToPagesMap)
            {
                foreach (var mapping in options.ViewModelToPagesMappings)
                {
                    _viewModelToPagesMap.Add(mapping.Key, mapping.Value);
                }
            }
        }
    }

    /// <summary>
    /// Stores and manages ViewModel-to-Page mappings.
    /// Used to configure which ViewModel corresponds to which Page type.
    /// </summary>
    public class PageServiceOptions
    {
        private readonly Dictionary<string, Type> _viewModelToPagesMap = new Dictionary<string, Type>();

        /// <summary>
        /// Gets the read-only dictionary of ViewModel-to-Page mappings.
        /// </summary>
        public IReadOnlyDictionary<string, Type> ViewModelToPagesMappings => _viewModelToPagesMap;

        /// <summary>
        /// Configures a mapping between a ViewModel and a Page type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TView">The corresponding Page type.</typeparam>
        /// <returns>The current instance of PageServiceOptions for method chaining.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the ViewModel is already mapped or if the Page type is already associated with another ViewModel.
        /// </exception>
        public PageServiceOptions Configure<TViewModel, TView>()
            where TViewModel : class
            where TView : Microsoft.UI.Xaml.Controls.Page
        {
            var viewModelKey = typeof(TViewModel).FullName;
            var pageType = typeof(TView);

            if (_viewModelToPagesMap.ContainsKey(viewModelKey))
            {
                throw new ArgumentException($"The viewModelKey {viewModelKey} is already configured in {nameof(PageServiceOptions)}");
            }

            if (_viewModelToPagesMap.Any(mapping => mapping.Value == pageType))
            {
                throw new ArgumentException($"This pageType is already configured with viewModelKey {_viewModelToPagesMap.First(mapping => mapping.Value == pageType).Key}");
            }

            _viewModelToPagesMap.Add(viewModelKey, pageType);
            return this;
        }
    }
}