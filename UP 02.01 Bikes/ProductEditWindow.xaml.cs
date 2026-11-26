using Microsoft.Win32;
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
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.Data.Entity;

namespace UP_02._01_Bikes
{
    /// <summary>
    /// Логика взаимодействия для ProductEditWindow.xaml
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        private Product _product;
        private string _newPhotoPath = null;
        private int _newProductId = 0;

        public bool IsSaved { get; private set; } = false;

        public ProductEditWindow(ProductDisplay displayProduct)
        {
            InitializeComponent();

            CbCategory.ItemsSource = Core.Context.Product.Select(p => p.Category).Distinct().ToList();
            CbProvider.ItemsSource = Core.Context.Product.Select(p => p.Provider).Distinct().ToList();
            CbManufacturer.ItemsSource = Core.Context.Product.Select(p => p.Manufacturer).Distinct().ToList();

            if (displayProduct != null)
            {
                _product = Core.Context.Product.Find(displayProduct.ID);
                Title = "Редактирование товара";
                TbId.Text = _product.ID.ToString();
                TbId.Visibility = Visibility.Visible;
                TbName.Text = _product.Name;
                CbCategory.Text = _product.Category;
                TbDescription.Text = _product.Description;
                CbManufacturer.Text = _product.Manufacturer;
                CbProvider.Text = _product.Provider;
                TbPrice.Text = _product.Price.ToString();
                TbMeasureUnit.Text = _product.MeasureUnit;
                TbQuantity.Text = displayProduct.Quantity.ToString();
                TbDiscount.Text = _product.Discount.ToString();
                ImgPhoto.Source = new BitmapImage(new Uri(displayProduct.PhotoPath, UriKind.Relative));

                BtnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Добавление товара";

                int maxId = Core.Context.Product.Max(p => (int?)p.ID) ?? 0;
                _newProductId = maxId + 1;

                TbId.Text = "Авто";
                TbPrice.Text = "0";
                TbQuantity.Text = "0";
                TbDiscount.Text = "0";
                TbMeasureUnit.Text = "шт.";
            }
        }

        /// <summary>
        /// Метод, запрещающий вводить все, кроме цифр, точки и запятой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".";
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newPhotoPath = openFileDialog.FileName;
                ImgPhoto.Source = new BitmapImage(new Uri(_newPhotoPath));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TbPrice.Text.Replace(".", ","), out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть числом и не может быть отрицательной.");
                return;
            }

            if (!int.TryParse(TbQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Количество должно быть целым числом и не может быть отрицательным.");
                return;
            }

            if (!decimal.TryParse(TbDiscount.Text, out decimal discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TbName.Text) ||
                string.IsNullOrWhiteSpace(TbDescription.Text) ||
                string.IsNullOrWhiteSpace(CbCategory.Text) ||
                string.IsNullOrWhiteSpace(CbManufacturer.Text) ||
                string.IsNullOrWhiteSpace(CbProvider.Text) ||
                string.IsNullOrWhiteSpace(TbMeasureUnit.Text))
            {
                MessageBox.Show("Заполните все обязательные поля: наименование, категория, описание, " +
                    "производитель, поставщик, единица измерения.");
                return;
            }

            try
            {
                string finalPhotoName = null;
                if (_newPhotoPath != null)
                {
                    if (_product != null && !string.IsNullOrEmpty(_product.Photo) && _product.Photo != "picture.png")
                    {
                        string oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pictures", _product.Photo);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                    }

                    finalPhotoName = Path.GetFileName(_newPhotoPath);
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pictures", finalPhotoName);

                    string destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pictures");
                    if (!Directory.Exists(destFolder))
                        Directory.CreateDirectory(destFolder);

                    File.Copy(_newPhotoPath, destPath, true);
                }
                else if (_product != null)
                {
                    finalPhotoName = _product.Photo;
                }

                if (_product == null)
                {
                    _product = new Product
                    {
                        ID = _newProductId,
                        Articul = "ART" + _newProductId,
                        Name = TbName.Text,
                        Category = CbCategory.Text,
                        Description = TbDescription.Text,
                        Manufacturer = CbManufacturer.Text,
                        Provider = CbProvider.Text,
                        Price = price,
                        MeasureUnit = TbMeasureUnit.Text,
                        Discount = discount,
                        Photo = finalPhotoName ?? "picture.png"
                    };
                    Core.Context.Product.Add(_product);
                }
                else
                {
                    _product.Name = TbName.Text;
                    _product.Category = CbCategory.Text;
                    _product.Description = TbDescription.Text;
                    _product.Manufacturer = CbManufacturer.Text;
                    _product.Provider = CbProvider.Text;
                    _product.Price = price;
                    _product.MeasureUnit = TbMeasureUnit.Text;
                    _product.Discount = discount;
                    _product.Photo = finalPhotoName;
                }

                UpdateProductStock(_product.ID, quantity);

                Core.Context.SaveChanges();
                IsSaved = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
            
        }

        private void UpdateProductStock(int productId, int newQuantity)
        {
            var stock = Core.Context.ProductStock.FirstOrDefault(ps => ps.ProductID == productId);
            if (stock != null)
            {
                stock.Quantity = newQuantity;
            }
            else
            {
                Core.Context.ProductStock.Add(new ProductStock { ProductID = productId, PickUpPointID = 1, Quantity = newQuantity });
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool inOrder = Core.Context.OrderProduct.Any(op => op.ProductID == _product.ID);
            if (inOrder)
            {
                MessageBox.Show("Нельзя удалить товар, который есть в заказах.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?", 
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                if (!string.IsNullOrEmpty(_product.Photo) && _product.Photo != "picture.png")
                {
                    string oldPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pictures", _product.Photo);
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }

                var stocks = Core.Context.ProductStock.Where(ps => ps.ProductID == _product.ID).ToList();
                foreach (var s in stocks)
                    Core.Context.ProductStock.Remove(s);

                Core.Context.Product.Remove(_product);
                Core.Context.SaveChanges();

                IsSaved = true;
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}

