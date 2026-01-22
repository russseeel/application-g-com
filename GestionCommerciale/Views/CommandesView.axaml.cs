using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using GestionCommerciale.ViewModels;
using GestionCommerciale.Models;

namespace GestionCommerciale.Views
{
    public partial class CommandesView : UserControl
    {
        private CommandesViewModel? ViewModel => DataContext as CommandesViewModel;

        public CommandesView()
        {
            InitializeComponent();
        }

        private void OnRowClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Commande commande && ViewModel != null)
            {
                ViewModel.SelectedCommande = commande;
                ViewModel.StatusMessage = $"Commande #{commande.Id} sélectionnée - {commande.NomClient} - {commande.NomProduit}";
            }
        }

        private void OnCreerCommandeClick(object? sender, RoutedEventArgs e)
        {
            ViewModel?.CreateCommandeCommand.Execute(null);
        }

        private void OnVoirDetailsClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.SelectedCommande != null)
            {
                var commande = ViewModel.SelectedCommande;
                ViewModel.StatusMessage = $"Détails: Commande #{commande.Id} | Client: {commande.NomClient} | " +
                    $"Produit: {commande.NomProduit} | Qté: {commande.Quantite} | Total: {commande.MontantTotal:F2} € | " +
                    $"Date: {commande.DateCommande:dd/MM/yyyy HH:mm}";
            }
            else
            {
                if (ViewModel != null)
                {
                    ViewModel.StatusMessage = "Veuillez sélectionner une commande dans la liste";
                }
            }
        }

        private void OnRafraichirClick(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.LoadCommandesCommand.Execute(null);
                ViewModel.StatusMessage = "Données actualisées avec succès";
            }
        }
    }
}