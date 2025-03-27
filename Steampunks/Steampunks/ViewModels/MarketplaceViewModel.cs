using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Steampunks.Domain.Entities;
using Steampunks.Services;
using System.Threading.Tasks;

namespace Steampunks.ViewModels
{
    public class MarketplaceViewModel : INotifyPropertyChanged
    {
        private readonly MarketplaceService _marketplaceService;
        private ObservableCollection<Item> _items;
        private string _searchText;
        private string _selectedGame;
        private string _selectedType;
        private string _selectedRarity;
        private List<Item> _allCurrentItems;
        private Item _selectedItem;
        private User _currentUser;
        private ObservableCollection<User> _availableUsers;

        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterItems();
                OnPropertyChanged();
            }
        }

        public string SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                FilterItems();
                OnPropertyChanged();
            }
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                FilterItems();
                OnPropertyChanged();
            }
        }

        public string SelectedRarity
        {
            get => _selectedRarity;
            set
            {
                _selectedRarity = value;
                FilterItems();
                OnPropertyChanged();
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CanBuyItem));
                }
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    _marketplaceService.SetCurrentUser(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanBuyItem));
                }
            }
        }

        public ObservableCollection<User> AvailableUsers
        {
            get => _availableUsers;
            set
            {
                _availableUsers = value;
                OnPropertyChanged();
            }
        }

        public bool CanBuyItem => SelectedItem != null && SelectedItem.IsListed && CurrentUser != null && CurrentUser.WalletBalance >= SelectedItem.Price;

        public async Task<bool> BuyItemAsync()
        {
            if (SelectedItem == null || !SelectedItem.IsListed)
                return false;

            try
            {
                bool success = _marketplaceService.BuyItem(SelectedItem);
                if (success)
                {
                    // Refresh the items list
                    LoadItems();
                    return true;
                }
                return false;
            }
            catch (InvalidOperationException ex)
            {
                // Handle specific error cases
                throw new Exception(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ObservableCollection<string> AvailableGames { get; set; }
        public ObservableCollection<string> AvailableTypes { get; set; }
        public ObservableCollection<string> AvailableRarities { get; set; }

        public MarketplaceViewModel(MarketplaceService marketplaceService)
        {
            _marketplaceService = marketplaceService;
            LoadUsers();
            LoadItems();
            InitializeCollections();
        }

        private void LoadUsers()
        {
            var users = _marketplaceService.GetAllUsers();
            AvailableUsers = new ObservableCollection<User>(users);
            CurrentUser = _marketplaceService.GetCurrentUser();
        }

        private void LoadItems()
        {
            var allItems = _marketplaceService.getAllListings();
            Items = new ObservableCollection<Item>(allItems);
        }

        private void InitializeCollections()
        {
            var allItems = Items.ToList();
            AvailableGames = new ObservableCollection<string>(allItems.Select(i => i.GetCorrespondingGame().GetTitle()).Distinct());
            AvailableTypes = new ObservableCollection<string>(allItems.Select(i => i.GetItemName().Split('|').First().Trim()).Distinct());
            AvailableRarities = new ObservableCollection<string>(new[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" });
        }

        private void FilterItems()
        {
            var filteredItems = _marketplaceService.getAllListings().AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchTextLower = SearchText.ToLower();
                filteredItems = filteredItems.Where(i => 
                    i.GetItemName().ToLower().Contains(searchTextLower) ||
                    i.Description.ToLower().Contains(searchTextLower));
            }

            if (!string.IsNullOrEmpty(SelectedGame))
            {
                filteredItems = filteredItems.Where(i => i.GetCorrespondingGame().GetTitle() == SelectedGame);
            }

            if (!string.IsNullOrEmpty(SelectedType))
            {
                filteredItems = filteredItems.Where(i => 
                    i.GetItemName().IndexOf('|') > 0 
                        ? i.GetItemName().Substring(0, i.GetItemName().IndexOf('|')).Trim() == SelectedType
                        : i.GetItemName().Trim() == SelectedType);
            }

            Items = new ObservableCollection<Item>(filteredItems);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
