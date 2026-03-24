using Avalonia.Controls;
using Avalonia.Interactivity;
using GestionCommerciale.Views;

namespace GestionCommerciale
{
    public partial class MainWindow : Window
    {
        private Control? _mainMenu;

        public MainWindow()
        {
            InitializeComponent();
            _mainMenu = MainContentControl.Content as Control;
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new ClientsView();
            BtnBackToMenu.IsVisible = true;
        }

        private void BtnCommandes_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = new CommandesView();
            BtnBackToMenu.IsVisible = true;
        }

        private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
        {
            MainContentControl.Content = _mainMenu;
            BtnBackToMenu.IsVisible = false;
        }
    }
}