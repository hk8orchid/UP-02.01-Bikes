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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Распределение ролевого доступа
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (App.CurrentUser != null)
            {
                TbFio.Text = App.CurrentUser.FIO;
            }
            else
            {
                TbFio.Text = "Гость";
            }

            MainFrame.Navigate(new ProductsPage());
            MainFrame.Navigated += (s, ev) => UpdateBackButton();

            string role = App.CurrentUser?.UserRole?.Role;
            if (role == "Менеджер" || role == "Администратор")
            {
                BtnOrders.Visibility = Visibility.Visible;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();

            UpdateBackButton();
        }

        private void UpdateBackButton()
        {
            BtnBack.Visibility = MainFrame.CanGoBack
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrderPage());
        }
    }
}
