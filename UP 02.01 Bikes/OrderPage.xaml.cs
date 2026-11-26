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
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private List<OrderDisplay> _allOrders;

        public OrderPage()
        {
            InitializeComponent();

            if (App.CurrentUser?.UserRole?.Role == "Администратор")
            {
                BtnAddOrder.Visibility = Visibility.Visible;
            }

            LoadOrders();
        }

        private void LoadOrders()
        {
            var orders = Core.Context.Order.ToList();
            _allOrders = new List<OrderDisplay>();

            foreach (var o in orders)
            {
                var point = Core.Context.PickUpPoints.FirstOrDefault(p => p.ID == o.PickUpPointID);
                string address = point != null
                    ? $"{point.City}, {point.Street}, {point.House}"
                    : "Не указан";

                _allOrders.Add(new OrderDisplay
                {
                    ID = o.ID,
                    Articul = o.Articul,
                    Status = o.Status,
                    Address = address,
                    OrderDate = o.OrderDate.ToString("dd.MM.yyyy"),
                    OrderDelivery = o.OrderDelivery,
                });
            }

            LvOrders.ItemsSource = _allOrders;
        }

       /// <summary>
       /// Открытие окна с редактированием через нажатие на заказ
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (App.CurrentUser?.UserRole?.Role != "Администратор") return;

            if (sender is Border border && border.DataContext is OrderDisplay order)
            {
                var editWindow = new OrderEditWindow(order);
                editWindow.ShowDialog();

                if (editWindow.IsSaved)
                {
                    LoadOrders();
                }
            }
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new OrderEditWindow(null);
            editWindow.ShowDialog();

            if (editWindow.IsSaved)
            {
                LoadOrders();
            }
        }
    }

    public class OrderDisplay
    {
        public int ID { get; set; }
        public string Articul { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string OrderDate { get; set; }
        public DateTime OrderDelivery { get; set; }
    }
}

