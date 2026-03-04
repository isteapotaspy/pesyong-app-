
# PESYONG (Catering & Kakanin Ordering System)

PESYONG is a modern desktop application built with **WinUI 3** and **.NET 8** designed to streamline catering services and native Filipino delicacy (Kakanin) orders. It features a reactive, multi-step checkout process and a robust product management system.

## 🚀 Features

* **Catering Package Builder**: Choose from pre-defined fiesta/deluxe packages or build a custom package with up to 8 different viands.
* **Kakanin Storefront**: Interactive product catalog for native delicacies like Puto, Kutsinta, Bibingka, and more.
* **Smart Quantity Management**: Automatically handles minimum order requirements (e.g., items sold by the dozen vs. per piece) and stock validation.
* **Multi-Step Checkout**: A "wizard-style" experience managing:
* **Cart Review**: Real-time price updates and item management.
* **Delivery**: Distance-based fee calculation (Poblacion vs. Outside areas).
* **Payment**: Support for COD, GCash, and Reservation payments.


* **Order History & Reviews**: Track order status (Pending, Out for Delivery, Delivered) and leave star-rated feedback for fulfilled orders.

## 🛠️ Tech Stack

* **Framework**: [WinUI 3 (Windows App SDK)](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
* **Language**: C# 12 / .NET 8
* **Pattern**: MVVM (Model-View-ViewModel)
* **Toolkit**: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) (for Observable properties and RelayCommands)

## 📂 Project Structure

* `PESYONG.Domain`: Core entities (Orders, Meals, Enums) and business logic.
* `PESYONG.ApplicationLogic`: Services like `CartService` and `CateringService` for data management.
* `PESYONG.Presentation`:
* **ViewModels**: Reactive logic for UI binding.
* **Views**: XAML-based pages for the customer interface (Cart, Kakanin Shop, Order History).


## 🚦 Getting Started

### Prerequisites

* Visual Studio 2022
* Windows App SDK workload installed
* .NET 8.0 SDK

### Installation

1. Clone the repository:
```bash
git clone https://github.com/isteapotaspy/pesyong-app-.git

```

2. Open `PESYONG.sln` in Visual Studio.
3. Restore NuGet packages.
4. Set the `PESYONG.Presentation` project (or the Package project) as the Startup Project.
5. Press `F5` to build and run.

## 📸 Core Modules

### 1. Catering & Packages

The app calculates base prices and discounts dynamically. It uses a `PackageDisplayDto` to present options like the "Fiesta Package" while allowing for custom selections.

### 2. Reactive Shopping Cart

Managed by a Singleton `CartService`, the cart ensures that changes made in the **Kakanin Page** are instantly reflected in the **Cart Page** through `ObservableCollection` and `INotifyPropertyChanged`.

### 3. Delivery Fee Logic

Fees are calculated based on location:

* **Poblacion**: Flat rate (₱15.00).
* **Outside Area**: Distance-based calculation (₱25.00 minimum + ₱5.00 per km).

## 📄 License

This project is licensed under the [MIT License](https://www.google.com/search?q=LICENSE).

---

*Developed by [isteapotaspy*](https://www.google.com/search?q=https://github.com/isteapotaspy)
