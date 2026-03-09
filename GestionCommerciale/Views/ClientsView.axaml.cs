using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GestionCommerciale.ViewModels;

namespace GestionCommerciale.Views
{
    public partial class ClientsView : UserControl
    {
        private ClientsViewModel? ViewModel => DataContext as ClientsViewModel;
        private Grid? _lastSelectedRow;

        public ClientsView()
        {
            InitializeComponent();
            Console.WriteLine("ClientsView constructeur appelé");
        }

        private void OnRowClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is ClientDisplayModel client)
            {
                // Réinitialiser la couleur de la ligne précédente
                if (_lastSelectedRow != null)
                {
                    _lastSelectedRow.Background = Brushes.Transparent;
                }

                // Mettre en surbrillance la nouvelle ligne (Gris moyen)
                grid.Background = new SolidColorBrush(Color.Parse("#333333"));
                _lastSelectedRow = grid;

                // Mettre à jour les TextBlocks de la ligne sélectionnée en blanc/jaune
                foreach (var child in grid.Children)
                {
                    if (child is TextBlock tb)
                    {
                        if (tb.Text == client.NomComplet)
                        {
                            tb.Foreground = new SolidColorBrush(Color.Parse("#F1C40F")); // AccentYellow
                        }
                        else
                        {
                            tb.Foreground = Brushes.White;
                        }
                    }
                }

                // Mettre à jour le ViewModel
                if (ViewModel != null)
                {
                    ViewModel.SelectedClient = client;
                    Console.WriteLine($"Client sélectionné: {client.NomComplet}");
                }
            }
        }

        private void OnAjouterClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Ajouter cliqué");
            if (ViewModel != null)
            {
                ViewModel.AjouterClient();
            }
            else
            {
                Console.WriteLine("ERREUR: ViewModel est null dans OnAjouterClick");
            }
        }

        private void OnModifierClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Modifier cliqué");
            if (ViewModel != null)
            {
                ViewModel.ModifierClient();
            }
            else
            {
                Console.WriteLine("ERREUR: ViewModel est null dans OnModifierClick");
            }
        }

        private void OnSupprimerClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Supprimer cliqué");
            if (ViewModel != null)
            {
                ViewModel.SupprimerClient();
            }
            else
            {
                Console.WriteLine("ERREUR: ViewModel est null dans OnSupprimerClick");
            }
        }

        private void OnRechercherClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Rechercher cliqué");
            if (ViewModel != null)
            {
                ViewModel.RechercherClients();
            }
            else
            {
                Console.WriteLine("ERREUR: ViewModel est null dans OnRechercherClick");
            }
        }

        private void OnRafraichirClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Rafraîchir cliqué");
            if (ViewModel != null)
            {
                ViewModel.LoadClients();
            }
            else
            {
                Console.WriteLine("ERREUR: ViewModel est null dans OnRafraichirClick");
            }
        }
    }
}