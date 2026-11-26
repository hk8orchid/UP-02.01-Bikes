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
using System.Data.Entity;

namespace UP_02._01_Bikes
{
    /// <summary>
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        private Order _order;
        private Product _product;
        public bool IsSaved { get; private set; } = false;

        /// <summary>
        /// Отображение заказов с возможным редактированием или добавлением
        /// </summary>
        /// <param name="displayOrder">Отображение заказов (берется из OrderPage)</param>
        public OrderEditWindow(OrderDisplay displayOrder)
        {
            InitializeComponent();

            CbStatus.ItemsSource = Core.Context.Order.Select(o => o.Status).Distinct().ToList();

            var points = Core.Context.PickUpPoints.ToList();
            CbPickUpPoint.ItemsSource = points;
            CbPickUpPoint.DisplayMemberPath = "FullAddress";
            CbPickUpPoint.SelectedValuePath = "ID";

            if (displayOrder != null)
            {
                _order = Core.Context.Order.Find(displayOrder.ID);
                Title = "Редактирование заказа";

                TbArticul.Text = _order.Articul;
                CbStatus.Text = _order.Status;
                CbPickUpPoint.SelectedValue = _order.PickUpPointID;
                DpOrderDate.SelectedDate = _order.OrderDate;
                DpOrderDelivery.SelectedDate = _order.OrderDelivery;

                BtnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                Title = "Добавление заказа";
                TbArticul.Text = "ORD" + DateTime.Now.ToString("yyyyMMdd");
                CbStatus.SelectedIndex = 0;
                DpOrderDate.SelectedDate = DateTime.Now;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbArticul.Text))
            {
                MessageBox.Show("Артикул обязателен для заполнения.");
                return;
            }

            if (CbPickUpPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи.");
                return;
            }

            if (DpOrderDate.SelectedDate == null || DpOrderDelivery.SelectedDate == null)
            {
                MessageBox.Show("Укажите обе даты.");
                return;
            }

            if (DpOrderDelivery.SelectedDate < DpOrderDate.SelectedDate)
            {
                MessageBox.Show("Дата выдачи не может быть раньше даты заказа.");
                return;
            }

            try
            {
                int pointId = (int)CbPickUpPoint.SelectedValue;

                if (_order == null)
                {
                    _order = new Order
                    {
                        Articul = TbArticul.Text,
                        Status = CbStatus.Text,
                        PickUpPointID = pointId,
                        OrderDate = DpOrderDate.SelectedDate.Value,
                        OrderDelivery = DpOrderDelivery.SelectedDate.Value,
                        UserID = App.CurrentUser.ID,
                        Code = new Random().Next(1000, 9999)
                    };
                    Core.Context.Order.Add(_order);
                }
                else
                {
                    _order.Articul = TbArticul.Text;
                    _order.Status = CbStatus.Text;
                    _order.PickUpPointID = pointId;
                    _order.OrderDate = DpOrderDate.SelectedDate.Value;
                    _order.OrderDelivery = DpOrderDelivery.SelectedDate.Value;
                }

                Core.Context.SaveChanges();
                IsSaved = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            bool inOrderProduct = Core.Context.OrderProduct.Any(op => op.ProductID == _product.ID);

            bool inOrderArticul = !string.IsNullOrEmpty(_product.Articul) && Core.Context.Order.Any(o => o.Articul.Contains(_product.Articul));

            if (inOrderProduct || inOrderArticul)
            {
                MessageBox.Show("Нельзя удалить товар, который присутствует в заказах.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var orderProducts = Core.Context.OrderProduct.Where(op => op.OrderID == _order.ID).ToList();
                    foreach (var op in orderProducts)
                        Core.Context.OrderProduct.Remove(op);
                    Core.Context.Order.Remove(_order);
                    Core.Context.SaveChanges();

                    IsSaved = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}