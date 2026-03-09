using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GestionCommerciale.ViewModels;

namespace GestionCommerciale.Views
{
    public partial class ProduitsView : UserControl
    {
        private ProduitsViewModel? ViewModel => DataContext as ProduitsViewModel;
        private Grid? _lastSelectedRow;

        public ProduitsView()
        {
            InitializeComponent();
            Console.WriteLine("ProduitsView constructeur appelé");
        }

        private void OnRowClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is ProduitDisplayModel produit)
            {
                // Réinitialiser la couleur de la ligne précédente
                if (_lastSelectedRow != null)
                {
                    var previousProduit = _lastSelectedRow.DataContext as ProduitDisplayModel;
                    SetRowColor(_lastSelectedRow, previousProduit);
                }

                // Mettre en surbrillance la nouvelle ligne (bleu sélection)
                grid.Background = new SolidColorBrush(Color.Parse("#3498DB"));
                _lastSelectedRow = grid;

                // Mettre à jour les TextBlocks en blanc
                foreach (var child in grid.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = Brushes.White;
                    }
                }

                // Mettre à jour le ViewModel
                if (ViewModel != null)
                {
                    ViewModel.SelectedProduit = produit;
                    Console.WriteLine($"Produit sélectionné: {produit.Nom}");
                }
            }
        }

        private void SetRowColor(Grid grid, ProduitDisplayModel? produit)
        {
            if (produit == null)
            {
                grid.Background = Brushes.White;
                return;
            }

            // Colorisation selon le stock
            if (!produit.EstEnStock)
            {
                // Rupture de stock - rouge clair
                grid.Background = new SolidColorBrush(Color.Parse("#FADBD8"));
            }
            else if (produit.StockFaible)
            {
                // Stock faible (< 10) - orange clair
                grid.Background = new SolidColorBrush(Color.Parse("#FCE4D6"));
            }
            else
            {
                grid.Background = Brushes.White;
            }

            // Remettre le texte en noir/couleur appropriée
            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb)
                {
                    if (!produit.EstEnStock)
                    {
                        tb.Foreground = new SolidColorBrush(Color.Parse("#C0392B"));
                    }
                    else if (produit.StockFaible)
                    {
                        tb.Foreground = new SolidColorBrush(Color.Parse("#D35400"));
                    }
                    else
                    {
                        tb.Foreground = Brushes.Black;
                    }

                    // Colorer le statut spécifiquement
                    if (tb.Text == produit.Statut)
                    {
                        if (produit.EstEnStock)
                        {
                            tb.Foreground = new SolidColorBrush(Color.Parse("#27AE60"));
                        }
                        else
                        {
                            tb.Foreground = new SolidColorBrush(Color.Parse("#E74C3C"));
                        }
                    }
                }
            }
        }

        private void OnAjouterClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Ajouter cliqué");
            ViewModel?.AjouterProduit();
        }

        private void OnModifierClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Modifier cliqué");
            ViewModel?.ModifierProduit();
        }

        private void OnSupprimerClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Supprimer cliqué");
            ViewModel?.SupprimerProduit();
        }

        private void OnFiltrerClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Filtrer cliqué");
            ViewModel?.FiltrerParCategorie();
        }

        private void OnRafraichirClick(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("Bouton Rafraîchir cliqué");
            ViewModel?.LoadProduits();
        }
    }
}
