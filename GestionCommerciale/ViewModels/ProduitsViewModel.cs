using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using GestionCommerciale.Data;
using GestionCommerciale.Models;

namespace GestionCommerciale.ViewModels
{
    public class ProduitsViewModel : INotifyPropertyChanged
    {
        private readonly ProduitRepository _repository = null!;

        // Collection pour le DataGrid
        private ObservableCollection<ProduitDisplayModel> _produits = new();
        public ObservableCollection<ProduitDisplayModel> Produits
        {
            get => _produits;
            set
            {
                _produits = value;
                OnPropertyChanged();
            }
        }

        // Produit sélectionné
        private ProduitDisplayModel? _selectedProduit;
        public ProduitDisplayModel? SelectedProduit
        {
            get => _selectedProduit;
            set
            {
                _selectedProduit = value;
                OnPropertyChanged();
                if (value != null)
                {
                    Nom = value.Nom;
                    Description = value.Description;
                    PrixUnitaire = value.PrixUnitaireValue;
                    Stock = value.StockValue;
                    SelectedCategorie = value.Categorie;
                }
            }
        }

        // Champs de saisie
        private string _nom = string.Empty;
        public string Nom
        {
            get => _nom;
            set
            {
                _nom = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private decimal _prixUnitaire;
        public decimal PrixUnitaire
        {
            get => _prixUnitaire;
            set
            {
                _prixUnitaire = value;
                OnPropertyChanged();
            }
        }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set
            {
                _stock = value;
                OnPropertyChanged();
            }
        }

        private string _selectedCategorie = "Autre";
        public string SelectedCategorie
        {
            get => _selectedCategorie;
            set
            {
                _selectedCategorie = value;
                OnPropertyChanged();
            }
        }

        // Liste des catégories pour le ComboBox
        public ObservableCollection<string> Categories { get; } = new()
        {
            "Informatique",
            "Accessoires",
            "Audio",
            "Autre"
        };

        // Filtre par catégorie
        private string _filtreCategorie = "Toutes";
        public string FiltreCategorie
        {
            get => _filtreCategorie;
            set
            {
                _filtreCategorie = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> CategoriesFiltre { get; } = new()
        {
            "Toutes",
            "Informatique",
            "Accessoires",
            "Audio",
            "Autre"
        };

        // Message de statut
        private string _statusMessage = "Prêt";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ProduitsViewModel()
        {
            try
            {
                string connectionString = DatabaseConnection.GetConnection().ConnectionString;
                _repository = new ProduitRepository(connectionString);
                Console.WriteLine("ProduitsViewModel initialisé");
                LoadProduits();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR dans constructeur ProduitsViewModel: {ex.Message}");
                StatusMessage = $"Erreur d'initialisation: {ex.Message}";
            }
        }

        // Charger tous les produits
        public void LoadProduits()
        {
            try
            {
                Console.WriteLine("Chargement des produits...");
                var produitsList = _repository.GetAll();
                Console.WriteLine($"Nombre de produits récupérés: {produitsList.Count}");

                var newProduits = new ObservableCollection<ProduitDisplayModel>();

                foreach (var produit in produitsList)
                {
                    newProduits.Add(CreateDisplayModel(produit));
                }

                Produits = newProduits;
                StatusMessage = $"{Produits.Count} produit(s) chargé(s)";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR dans LoadProduits: {ex.Message}");
                StatusMessage = $"Erreur de chargement: {ex.Message}";
            }
        }

        // Ajouter un produit
        public void AjouterProduit()
        {
            if (string.IsNullOrWhiteSpace(Nom))
            {
                StatusMessage = "Veuillez remplir le nom du produit";
                return;
            }

            if (PrixUnitaire < 0)
            {
                StatusMessage = "Le prix unitaire doit être >= 0";
                return;
            }

            if (Stock < 0)
            {
                StatusMessage = "Le stock doit être >= 0";
                return;
            }

            try
            {
                var newProduit = new Produit
                {
                    Nom = Nom,
                    Description = Description,
                    PrixUnitaire = PrixUnitaire,
                    Stock = Stock,
                    Categorie = SelectedCategorie
                };

                bool success = _repository.Add(newProduit);

                if (success)
                {
                    StatusMessage = "Produit ajouté avec succès";
                    ClearFields();
                    LoadProduits();
                }
                else
                {
                    StatusMessage = "Échec de l'ajout du produit";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
            }
        }

        // Modifier un produit
        public void ModifierProduit()
        {
            if (SelectedProduit == null)
            {
                StatusMessage = "Veuillez sélectionner un produit à modifier";
                return;
            }

            if (string.IsNullOrWhiteSpace(Nom))
            {
                StatusMessage = "Veuillez remplir le nom du produit";
                return;
            }

            if (PrixUnitaire < 0)
            {
                StatusMessage = "Le prix unitaire doit être >= 0";
                return;
            }

            if (Stock < 0)
            {
                StatusMessage = "Le stock doit être >= 0";
                return;
            }

            try
            {
                var updatedProduit = new Produit
                {
                    Id = SelectedProduit.Id,
                    Nom = Nom,
                    Description = Description,
                    PrixUnitaire = PrixUnitaire,
                    Stock = Stock,
                    Categorie = SelectedCategorie
                };

                bool success = _repository.Update(updatedProduit);

                if (success)
                {
                    StatusMessage = "Produit modifié avec succès";
                    ClearFields();
                    LoadProduits();
                }
                else
                {
                    StatusMessage = "Échec de la modification";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
            }
        }

        // Supprimer un produit
        public void SupprimerProduit()
        {
            if (SelectedProduit == null)
            {
                StatusMessage = "Veuillez sélectionner un produit à supprimer";
                return;
            }

            try
            {
                bool success = _repository.Delete(SelectedProduit.Id);

                if (success)
                {
                    StatusMessage = "Produit supprimé avec succès";
                    ClearFields();
                    LoadProduits();
                }
                else
                {
                    StatusMessage = "Échec de la suppression";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
            }
        }

        // Filtrer par catégorie
        public void FiltrerParCategorie()
        {
            try
            {
                if (FiltreCategorie == "Toutes")
                {
                    LoadProduits();
                    return;
                }

                var produitsList = _repository.GetByCategorie(FiltreCategorie);
                var newProduits = new ObservableCollection<ProduitDisplayModel>();

                foreach (var produit in produitsList)
                {
                    newProduits.Add(CreateDisplayModel(produit));
                }

                Produits = newProduits;
                StatusMessage = $"{Produits.Count} produit(s) dans la catégorie '{FiltreCategorie}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur de filtrage: {ex.Message}";
            }
        }

        // Créer le modèle d'affichage
        private ProduitDisplayModel CreateDisplayModel(Produit produit)
        {
            return new ProduitDisplayModel
            {
                Id = produit.Id,
                Nom = produit.Nom,
                Description = produit.Description,
                DescriptionTronquee = TronquerDescription(produit.Description),
                PrixUnitaireValue = produit.PrixUnitaire,
                PrixUnitaire = produit.PrixUnitaire.ToString("C", new CultureInfo("fr-FR")),
                StockValue = produit.Stock,
                Stock = produit.Stock.ToString(),
                Categorie = produit.Categorie,
                EstEnStock = produit.Stock > 0,
                Statut = produit.Stock > 0 ? "✓ En stock" : "✗ Rupture",
                StockFaible = produit.Stock > 0 && produit.Stock < 10
            };
        }

        // Tronquer la description
        private string TronquerDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "N/A";

            return description.Length > 30 ? description.Substring(0, 30) + "..." : description;
        }

        // Vider les champs
        private void ClearFields()
        {
            Nom = string.Empty;
            Description = string.Empty;
            PrixUnitaire = 0;
            Stock = 0;
            SelectedCategorie = "Autre";
            SelectedProduit = null;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Modèle pour l'affichage
    public class ProduitDisplayModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionTronquee { get; set; } = string.Empty;
        public decimal PrixUnitaireValue { get; set; }
        public string PrixUnitaire { get; set; } = string.Empty;
        public int StockValue { get; set; }
        public string Stock { get; set; } = string.Empty;
        public string Categorie { get; set; } = string.Empty;
        public bool EstEnStock { get; set; }
        public string Statut { get; set; } = string.Empty;
        public bool StockFaible { get; set; }
    }
}
