using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Steampunks.Services
{
    public class PageService : IPageService
    {
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();

        public PageService(IOptions<PageServiceOptions> options)
        {
            Configure(options.Value);
        }

        public Type GetPageType(string key)
        {
            Type pageType;
            lock (_pages)
            {
                if (!_pages.TryGetValue(key, out pageType))
                {
                    throw new ArgumentException($"Page not found: {key}. Did you forget to call Configure?");
                }
            }
            return pageType;
        }

        private void Configure(PageServiceOptions options)
        {
            lock (_pages)
            {
                foreach (var kvp in options.Pages)
                {
                    _pages.Add(kvp.Key, kvp.Value);
                }
            }
        }
    }

    public class PageServiceOptions
    {
        private readonly Dictionary<string, Type> _pages = new Dictionary<string, Type>();

        public IReadOnlyDictionary<string, Type> Pages => _pages;

        public PageServiceOptions Configure<TViewModel, TView>()
            where TViewModel : class
            where TView : Microsoft.UI.Xaml.Controls.Page
        {
            var key = typeof(TViewModel).FullName;
            var type = typeof(TView);

            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in {nameof(PageServiceOptions)}");
            }

            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
            return this;
        }
    }
} 