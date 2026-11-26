using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UP_02._01_Bikes
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private List<ProductDisplay> _allProducts;
        private List<ProductDisplay> _displayedProducts;

        public ProductsPage()
        {
            InitializeComponent();

            string role = App.CurrentUser?.UserRole?.Role;
            if (role != "Менеджер" && role != "Администратор")
            {
                SpControls.Visibility = Visibility.Collapsed;
            }
            else
            {
                BtnAddProduct.Visibility = (role == "Администратор")
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            LoadProducts();
        }

        /// <summary>
        /// Открытие окна редактирования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (App.CurrentUser?.UserRole?.Role != "Администратор") return;

            if (sender is Border border && border.DataContext is ProductDisplay product)
            {
                var editWindow = new ProductEditWindow(product);
                editWindow.ShowDialog();

                if (editWindow.IsSaved)
                {
                    LoadProducts();
                }
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow(null);
            editWindow.ShowDialog();

            if (editWindow.IsSaved)
            {
                LoadProducts();
                TbSearch.Text = "";
                CbSort.SelectedIndex = 0;
                CbFilter.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Отображение товаров, согласно словарю стилей
        /// </summary>
        private void LoadProducts()
        {
            var products = Core.Context.Product.ToList();
            _allProducts = new List<ProductDisplay>();

            foreach (var p in products)
            {
                int qty = Core.Context.ProductStock
                    .Where(ps => ps.ProductID == p.ID)
                    .Sum(ps => (int?)ps.Quantity) ?? 0;

                var d = new ProductDisplay
                {
                    ID = p.ID,
                    Name = p.Name,
                    Category = p.Category,
                    Description = p.Description,
                    Manufacturer = p.Manufacturer,
                    Provider = p.Provider,
                    MeasureUnit = p.MeasureUnit,
                    Discount = p.Discount,
                    Price = p.Price,
                    Quantity = qty
                };

                if (d.Discount > 0)
                {
                    d.HasDiscount = true;
                    d.DiscountPrice = d.Price * (1 - d.Discount / 100m);
                }
                else
                {
                    d.HasDiscount = false;
                    d.DiscountPrice = d.Price;
                }

                d.PhotoPath = string.IsNullOrEmpty(p.Photo)
                    ? "pictures/picture.png"
                    : $"pictures/{p.Photo}";

                if (d.Discount > 15)
                    d.CardBackground = "#483D8B";
                else if (d.Quantity == 0)
                    d.CardBackground = "LightGray";
                else
                    d.CardBackground = "White";

                _allProducts.Add(d);
            }

            LvProducts.ItemsSource = _allProducts;
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void CbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void CbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            if (_allProducts == null)
                return;

            IEnumerable<ProductDisplay> query = _allProducts;

            string search = TbSearch?.Text?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.Name?.ToLower().Contains(search) ?? false) ||
                    (p.Category?.ToLower().Contains(search) ?? false) ||
                    (p.Description?.ToLower().Contains(search) ?? false) ||
                    (p.Manufacturer?.ToLower().Contains(search) ?? false) ||
                    (p.Provider?.ToLower().Contains(search) ?? false) ||
                    (p.MeasureUnit?.ToLower().Contains(search) ?? false));
            }

            if (CbFilter?.SelectedItem is ComboBoxItem filterItem)
            {
                string filterText = filterItem.Content.ToString();
                if (filterText == "0 - 11,99%")
                    query = query.Where(p => p.Discount >= 0 && p.Discount < 12);
                else if (filterText == "12 - 18,99%")
                    query = query.Where(p => p.Discount >= 12 && p.Discount < 19);
                else if (filterText == "19% и более")
                    query = query.Where(p => p.Discount >= 19);
            }

            if (CbSort?.SelectedItem is ComboBoxItem sortItem)
            {
                string sortText = sortItem.Content.ToString();
                switch (sortText)
                {
                    case "Цена ↑": query = query.OrderBy(p => p.Price); break;
                    case "Цена ↓": query = query.OrderByDescending(p => p.Price); break;
                    case "Скидка ↑": query = query.OrderBy(p => p.Discount); break;
                    case "Скидка ↓": query = query.OrderByDescending(p => p.Discount); break;
                    case "Остаток ↑": query = query.OrderBy(p => p.Quantity); break;
                    case "Остаток ↓": query = query.OrderByDescending(p => p.Quantity); break;
                }
            }

            _displayedProducts = query.ToList();
            LvProducts.ItemsSource = _displayedProducts;
        }
    }


    public class ProductDisplay
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Provider { get; set; }
        public decimal Price { get; set; }
        public string MeasureUnit { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public bool HasDiscount { get; set; }
        public decimal DiscountPrice { get; set; }
        public string PhotoPath { get; set; }
        public string CardBackground { get; set; }
    }
} 
