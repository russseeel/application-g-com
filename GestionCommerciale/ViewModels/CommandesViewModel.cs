using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.ViewModels
{
    public partial class CommandesViewModel : ViewModelBase
    {
        private readonly CommandeRepository _commandeRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ProduitRepository _produitRepository;

        // Collections pour les données
        [ObservableProperty]
        private ObservableCollection<Commande> _commandes;

        [ObservableProperty]
        private ObservableCollection<Client> _clients;

        [ObservableProperty]
        private ObservableCollection<Produit> _produits;

        // Sélections
        [ObservableProperty]
        private Commande? _selectedCommande;

        [ObservableProperty]
        private Client? _selectedClient;

        [ObservableProperty]
        private Produit? _selectedProduit;

        [ObservableProperty]
        private int _quantite;

        // Propriétés calculées
        [ObservableProperty]
        private decimal _prixUnitaire;

        [ObservableProperty]
        private decimal _montantTotal;

        [ObservableProperty]
        private int _stockDisponible;

        [ObservableProperty]
        private bool _isStockInsuffisant;

        [ObservableProperty]
        private string _statusMessage;

        public CommandesViewModel()
        {
            // CORRECTION : Passer la chaîne de connexion aux repositories
            string connectionString = DatabaseConnection.GetConnectionString();
            
            _commandeRepository = new CommandeRepository();
            _clientRepository = new ClientRepository(connectionString);
            _produitRepository = new ProduitRepository(connectionString);

            _commandes = new ObservableCollection<Commande>();
            _clients = new ObservableCollection<Client>();
            _produits = new ObservableCollection<Produit>();
            _statusMessage = "Prêt à créer une nouvelle commande";

            // Charger les données initiales
            LoadClients();
            LoadProduits();
            LoadCommandes();
        }

        // Méthode appelée automatiquement quand SelectedProduit change
        partial void OnSelectedProduitChanged(Produit? value)
        {
            if (value != null)
            {
                PrixUnitaire = value.PrixUnitaire;
                StockDisponible = value.Stock;
                CalculerMontantTotal();
                VerifierStock();
            }
            else
            {
                PrixUnitaire = 0;
                StockDisponible = 0;
                MontantTotal = 0;
                IsStockInsuffisant = false;
            }
        }

        // Méthode appelée automatiquement quand Quantite change
        partial void OnQuantiteChanged(int value)
        {
            CalculerMontantTotal();
            VerifierStock();
        }

        private void LoadClients()
        {
            try
            {
                var clients = _clientRepository.GetAll();
                Clients.Clear();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des clients: {ex.Message}";
            }
        }

        private void LoadProduits()
        {
            try
            {
                var produits = _produitRepository.GetAll();
                Produits.Clear();
                foreach (var produit in produits)
                {
                    Produits.Add(produit);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des produits: {ex.Message}";
            }
        }

        [RelayCommand]
        private void LoadCommandes()
        {
            try
            {
                var commandes = _commandeRepository.GetAll();
                Commandes.Clear();
                foreach (var commande in commandes)
                {
                    Commandes.Add(commande);
                }
                
                if (string.IsNullOrEmpty(StatusMessage) || StatusMessage.Contains("Erreur"))
                {
                    StatusMessage = $"{commandes.Count} commande(s) chargée(s)";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des commandes: {ex.Message}";
            }
        }

        private void CalculerMontantTotal()
        {
            if (SelectedProduit != null && Quantite > 0)
            {
                MontantTotal = Quantite * PrixUnitaire;
            }
            else
            {
                MontantTotal = 0;
            }
        }

        private void VerifierStock()
        {
            if (SelectedProduit == null)
            {
                IsStockInsuffisant = false;
                return;
            }

            if (Quantite > StockDisponible)
            {
                IsStockInsuffisant = true;
            }
            else if (Quantite <= 0)
            {
                IsStockInsuffisant = true;
            }
            else
            {
                IsStockInsuffisant = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanCreateCommande))]
        private void CreateCommande()
        {
            try
            {
                // Validation finale
                if (SelectedClient == null)
                {
                    StatusMessage = "⚠ Veuillez sélectionner un client";
                    return;
                }

                if (SelectedProduit == null)
                {
                    StatusMessage = "⚠ Veuillez sélectionner un produit";
                    return;
                }

                if (Quantite <= 0)
                {
                    StatusMessage = "⚠ La quantité doit être supérieure à 0";
                    return;
                }

                if (Quantite > StockDisponible)
                {
                    StatusMessage = $"⚠ Stock insuffisant. Disponible: {StockDisponible}";
                    return;
                }

                // Créer la commande
                var commande = new Commande
                {
                    ClientId = SelectedClient.Id,
                    ProduitId = SelectedProduit.Id,
                    Quantite = Quantite,
                    DateCommande = DateTime.Now
                };

                _commandeRepository.Add(commande);

                // Message de succès
                StatusMessage = $"✓ Commande créée avec succès ! {SelectedClient.NomComplet} a commandé {Quantite} {SelectedProduit.Nom} pour {MontantTotal:F2} €";

                // Rafraîchir les données
                LoadCommandes();
                LoadProduits(); // Pour mettre à jour les stocks

                // Réinitialiser le formulaire
                ResetForm();
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Erreur lors de la création de la commande: {ex.Message}";
            }
        }

        private bool CanCreateCommande()
        {
            return SelectedClient != null 
                   && SelectedProduit != null 
                   && Quantite > 0 
                   && !IsStockInsuffisant;
        }

        private void ResetForm()
        {
            SelectedClient = null;
            SelectedProduit = null;
            Quantite = 0;
            PrixUnitaire = 0;
            MontantTotal = 0;
            StockDisponible = 0;
            IsStockInsuffisant = false;
            
            // Notifier que CanExecute a changé
            CreateCommandeCommand.NotifyCanExecuteChanged();
        }

        // Méthode appelée quand n'importe quelle propriété change
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            // Mettre à jour CanExecute quand les propriétés pertinentes changent
            if (e.PropertyName == nameof(SelectedClient) 
                || e.PropertyName == nameof(SelectedProduit) 
                || e.PropertyName == nameof(Quantite) 
                || e.PropertyName == nameof(IsStockInsuffisant))
            {
                CreateCommandeCommand.NotifyCanExecuteChanged();
            }
        }
    }
}