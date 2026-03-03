using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PESYONG.ApplicationLogic.Services;
using PESYONG.Domain.Entities.Financial.AcknowledgementReceipts;
using PESYONG.Domain.Entities.Meals.MealItem;
using PESYONG.Domain.Entities.Meals.MealProduct;
using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Entities.Users.Identity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PESYONG.Presentation.Views.Customer
{
    /// <summary>
    /// Represents the catering packages page where customers can view and select meal packages.
    /// Supports both fixed packages and customizable packages where users can choose their viands.
    /// </summary>
    public sealed partial class CateringPackagesPage : Page
    {
        /// <summary>
        /// Collection of available catering packages displayed in the UI.
        /// </summary>
        private ObservableCollection<MealProduct> Packages { get; set; }

        /// <summary>
        /// Collection of available viands that can be selected for customizable packages.
        /// </summary>
        private ObservableCollection<Meal> AvailableViands { get; set; }

        /// <summary>
        /// List of viands currently selected by the user in the selection dialog.
        /// </summary>
        private List<Meal> SelectedViands { get; set; }

        /// <summary>
        /// The package currently being configured in the viand selection dialog.
        /// </summary>
        private MealProduct CurrentSelectedPackage { get; set; }

        /// <summary>
        /// Service for managing cart operations.
        /// </summary>
        private CartService _cartService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CateringPackagesPage"/> class.
        /// Sets up the page, loads packages and available viands, and initializes the cart service.
        /// </summary>
        public CateringPackagesPage()
        {
            try
            {
                this.InitializeComponent();
                SelectedViands = new List<Meal>();

                // For testing purposes, create a mock user if authentication not implemented yet
                var currentUser = GetCurrentUser() ?? new AppUser
                {
                    Id = 1,
                    UserName = "testuser",
                    UserOrders = new List<Order>() // Initialize collections
                };

                _cartService = new CartService(currentUser);

                LoadPackages();
                LoadAvailableViands();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in constructor: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the currently logged-in user.
        /// </summary>
        /// <returns>The current AppUser object, or a mock user if not implemented.</returns>
        /// <remarks>
        /// TODO: Implement this method based on your authentication system.
        /// This should return the actual logged-in user from your auth service.
        /// </remarks>
        private AppUser GetCurrentUser()
        {
            try
            {
                // Try to get from App instance
                var currentUser = (App.Current as App)?.CurrentUser;

                if (currentUser == null)
                {
                    // Return a default/test user for development
                    return new AppUser
                    {
                        Id = 1,
                        UserName = "test@email.com",
                        FirstName = "Test",
                        LastName = "User",
                        UserOrders = new List<Order>(), // Initialize collections
                        UserMealProducts = new List<MealProduct>(),
                        UserReceipts = new List<AcknowledgementReceipt>()
                    };
                }

                // Ensure collections are initialized
                currentUser.UserOrders ??= new List<Order>();
                currentUser.UserMealProducts ??= new List<MealProduct>();
                currentUser.UserReceipts ??= new List<AcknowledgementReceipt>();

                return currentUser;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCurrentUser: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Loads sample catering packages into the Packages collection.
        /// In a production environment, this would fetch data from a database or service.
        /// </summary>
        private void LoadPackages()
        {
            try
            {
                // edit later with database data, this is just for testing
                Packages = new ObservableCollection<MealProduct>
                {
                    new MealProduct
                    {
                        MealProductID = 1,
                        ProductName = "Package 1 - 3 Viands",
                        ProductDescription = "Perfect for small gatherings and family meals",
                        MealProductItems = new List<MealProductItem>
                        {
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 1,
                                    MealName = "Battered Chicken",
                                    MealPrice = 450,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 2,
                                    MealName = "Bihon Guisado",
                                    MealPrice = 350,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 3,
                                    MealName = "Fish Fillet",
                                    MealPrice = 400,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            }
                        }
                    },
                    new MealProduct
                    {
                        MealProductID = 2,
                        ProductName = "Package 2 - 5 Viands",
                        ProductDescription = "Great for medium-sized celebrations",
                        MealProductItems = new List<MealProductItem>
                        {
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 1,
                                    MealName = "Battered Chicken",
                                    MealPrice = 450,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 4,
                                    MealName = "Buttered Shrimp",
                                    MealPrice = 550,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 2,
                                    MealName = "Bihon Guisado",
                                    MealPrice = 350,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 5,
                                    MealName = "Tuna Kinilaw",
                                    MealPrice = 400,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            },
                            new MealProductItem {
                                Meal = new Meal {
                                    MealID = 3,
                                    MealName = "Fish Fillet",
                                    MealPrice = 400,
                                    ImageSourceString = "ms-appx:///Assets/SampleMeal.png"
                                },
                                Quantity = 1
                            }
                        }
                    },
                    new MealProduct
                    {
                        MealProductID = 3,
                        ProductName = "Package 3 - 8 Viands + Free Dessert",
                        ProductDescription = "Our most popular package! Choose your favorite viands",
                        MealProductItems = new List<MealProductItem>() // Empty for selectable
                    }
                };

                PackagesItemsControl.ItemsSource = Packages;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadPackages: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads sample available viands into the AvailableViands collection.
        /// In a production environment, this would fetch data from a database or service.
        /// </summary>
        private void LoadAvailableViands()
        {
            try
            {
                // This would normally come from your database
                AvailableViands = new ObservableCollection<Meal>
                {
                    new Meal { MealID = 1, MealName = "Battered Chicken", MealPrice = 450, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 4, MealName = "Buttered Shrimp", MealPrice = 550, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 2, MealName = "Bihon Guisado", MealPrice = 350, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 5, MealName = "Tuna Kinilaw", MealPrice = 400, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 3, MealName = "Fish Fillet", MealPrice = 400, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 6, MealName = "Pork Menudo", MealPrice = 450, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 7, MealName = "Chicken Adobo", MealPrice = 400, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" },
                    new Meal { MealID = 8, MealName = "Beef Caldereta", MealPrice = 500, ImageSourceString = "ms-appx:///Assets/SampleMeal.png" }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadAvailableViands: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Click event of the Add to Cart button.
        /// Determines whether the package is fixed or customizable and processes accordingly.
        /// </summary>
        /// <param name="sender">The button that was clicked.</param>
        /// <param name="e">Event arguments containing additional event data.</param>
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button?.Tag == null) return;

                // Safely parse the package ID
                int packageId;
                if (button.Tag is int intTag)
                {
                    packageId = intTag;
                }
                else
                {
                    int.TryParse(button.Tag.ToString(), out packageId);
                }

                var package = Packages?.FirstOrDefault(p => p.MealProductID == packageId);

                if (package != null)
                {
                    // Check if it's the selectable package (Package 3)
                    if (package.MealProductItems == null || !package.MealProductItems.Any())
                    {
                        // Show selection dialog for custom package
                        CurrentSelectedPackage = package;
                        SelectedViands?.Clear();
                        ShowViandSelectionDialog(package);
                    }
                    else
                    {
                        // Add fixed package to cart
                        AddToCart(package, null);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart_Click: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays the viand selection dialog for customizable packages.
        /// </summary>
        /// <param name="package">The package being configured.</param>
        private async void ShowViandSelectionDialog(MealProduct package)
        {
            try
            {
                ViandsGrid.ItemsSource = AvailableViands;
                DialogDescription.Text = $"Choose your favorite dishes for {package.ProductName}";
                SelectedCountText.Text = $"Selected: 0 / 8";

                // Reset selection states
                SelectedViands?.Clear();

                // Make sure XamlRoot is set
                if (ViandSelectionDialog.XamlRoot == null)
                {
                    ViandSelectionDialog.XamlRoot = this.XamlRoot;
                }

                await ViandSelectionDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowViandSelectionDialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Checked event of viand checkboxes in the selection dialog.
        /// Adds the selected viand to the SelectedViands list if under the 8-item limit.
        /// </summary>
        /// <param name="sender">The checkbox that was checked.</param>
        /// <param name="e">Event arguments containing additional event data.</param>
        private void ViandCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;

                // Safely get Tag value
                if (checkBox?.Tag == null) return;

                // Handle both int and string conversions
                int mealId;
                if (checkBox.Tag is int intTag)
                {
                    mealId = intTag;
                }
                else
                {
                    int.TryParse(checkBox.Tag.ToString(), out mealId);
                }

                var selectedMeal = AvailableViands?.FirstOrDefault(m => m.MealID == mealId);

                if (selectedMeal != null && SelectedViands.Count < 8)
                {
                    SelectedViands.Add(selectedMeal);
                    UpdateSelectedCount();
                }
                else if (SelectedViands.Count >= 8)
                {
                    // Uncheck if already 8 selected
                    checkBox.IsChecked = false;

                    // Show warning
                    var warningDialog = new ContentDialog
                    {
                        Title = "Maximum Selection",
                        Content = "You can only select up to 8 viands.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = warningDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandCheckBox_Checked: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Unchecked event of viand checkboxes in the selection dialog.
        /// Removes the unselected viand from the SelectedViands list.
        /// </summary>
        /// <param name="sender">The checkbox that was unchecked.</param>
        /// <param name="e">Event arguments containing additional event data.</param>
        private void ViandCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var checkBox = sender as CheckBox;

                // Safely get Tag value
                if (checkBox?.Tag == null) return;

                // Handle both int and string conversions
                int mealId;
                if (checkBox.Tag is int intTag)
                {
                    mealId = intTag;
                }
                else
                {
                    int.TryParse(checkBox.Tag.ToString(), out mealId);
                }

                var selectedMeal = AvailableViands?.FirstOrDefault(m => m.MealID == mealId);

                if (selectedMeal != null)
                {
                    SelectedViands.Remove(selectedMeal);
                    UpdateSelectedCount();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandCheckBox_Unchecked: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the selected count display text in the viand selection dialog.
        /// </summary>
        private void UpdateSelectedCount()
        {
            try
            {
                SelectedCountText.Text = $"Selected: {SelectedViands?.Count ?? 0} / 8";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateSelectedCount: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the PrimaryButtonClick event of the viand selection dialog.
        /// Validates that exactly 8 viands are selected before adding to cart.
        /// </summary>
        /// <param name="sender">The content dialog that was clicked.</param>
        /// <param name="args">Event arguments that allow canceling the dialog close.</param>
        private void ViandSelectionDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                if (SelectedViands?.Count == 8 && CurrentSelectedPackage != null)
                {
                    AddToCart(CurrentSelectedPackage, SelectedViands);
                }
                else
                {
                    args.Cancel = true;
                    // Show error message
                    var errorDialog = new ContentDialog
                    {
                        Title = "Invalid Selection",
                        Content = "Please select exactly 8 viands.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    _ = errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViandSelectionDialog_PrimaryButtonClick: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a package to the shopping cart.
        /// </summary>
        /// <param name="package">The package to add to cart.</param>
        /// <param name="selectedViands">
        /// The list of selected viands for customizable packages.
        /// Null for fixed packages.
        /// </param>
        private void AddToCart(MealProduct package, List<Meal> selectedViands)
        {
            try
            {
                // Calculate total price
                decimal totalPrice;
                string itemName;

                if (selectedViands != null && selectedViands.Any())
                {
                    // Custom package with selected viands
                    totalPrice = selectedViands.Sum(v => v.MealPrice);
                    itemName = $"{package.ProductName} (Custom)";
                }
                else
                {
                    // Fixed package
                    totalPrice = package.ProductBasePrice;
                    itemName = package.ProductName;
                }

                // FIXED: Convert decimal to double properly
                double priceAsDouble = (double)totalPrice;

                // Use CartService to add to cart
                _cartService.AddToCart(
                    productId: package.MealProductID,
                    price: (decimal)priceAsDouble,  // Now passing double, not decimal
                    quantity: 1
                );

                // Show success message
                var successDialog = new ContentDialog
                {
                    Title = "Added to Cart!",
                    Content = $"{itemName} has been added to your cart.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = successDialog.ShowAsync();

                // Update cart badge
                UpdateCartBadge();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to add item to cart. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = errorDialog.ShowAsync();
            }
        }

        /// <summary>
        /// Updates the cart badge count in the UI.
        /// </summary>
        private void UpdateCartBadge()
        {
            try
            {
                // You can raise an event or use a messenger to update the cart badge
                // in your main layout or navigation view
                var cartCount = _cartService.Cart?.Count ?? 0;

                // For example, if you have a static event:
                CartUpdated?.Invoke(this, cartCount);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateCartBadge: {ex.Message}");
            }
        }

        /// <summary>
        /// Event that fires when the cart is updated, allowing parent components to update cart badges.
        /// </summary>
        public event EventHandler<int> CartUpdated;

        /// <summary>
        /// Helper method to get formatted package price for XAML binding.
        /// </summary>
        /// <param name="package">The package to get the price for.</param>
        /// <returns>Formatted price string with thousand separators.</returns>
        private string GetPackagePrice(MealProduct package)
        {
            return package?.ProductBasePrice.ToString("N0") ?? "0";
        }

        /// <summary>
        /// Helper method to get comma-separated viand names for a package.
        /// </summary>
        /// <param name="package">The package to get viand names for.</param>
        /// <returns>A string of viand names separated by commas, or a default message for customizable packages.</returns>
        private string GetViandNames(MealProduct package)
        {
            if (package?.MealProductItems == null || !package.MealProductItems.Any())
                return "Choose any 8 viands";

            return string.Join(", ", package.MealProductItems.Select(i => i.Meal?.MealName));
        }

        /// <summary>
        /// Determines whether a package is selectable (customizable) or fixed.
        /// </summary>
        /// <param name="package">The package to check.</param>
        /// <returns>True if the package is customizable (no predefined viands), false otherwise.</returns>
        private bool IsSelectablePackage(MealProduct package)
        {
            return package?.MealProductItems == null || !package.MealProductItems.Any();
        }
    }
}