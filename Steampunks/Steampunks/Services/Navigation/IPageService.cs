using System;

namespace Steampunks.Services
{
    public interface IPageService
    {
        Type GetPageTypeForViewModel(string key);
    }
}