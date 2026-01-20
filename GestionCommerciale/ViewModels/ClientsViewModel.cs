using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GestionCommerciale.Data;

namespace GestionCommerciale.ViewModels
{
    public class ClientsViewModel : INotifyPropertyChanged
    {
        private readonly ClientRepository _repository = null!;
        
        // Collection pour le DataGrid
        private ObservableCollection<ClientDisplayModel> _clients = new();
        public ObservableCollection<ClientDisplayModel> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged();
            }
        }

        // Client sélectionné dans le DataGrid
        private ClientDisplayModel? _selectedClient;
        public ClientDisplayModel? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                // Remplir les champs de saisie quand on sélectionne un client
                if (value != null)
                {
                    Nom = value.Nom;
                    Prenom = value.Prenom;
                    Email = value.Email;
                    Telephone = value.Telephone;
                    Adresse = value.Adresse;
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

        private string _prenom = string.Empty;
        public string Prenom
        {
            get => _prenom;
            set
            {
                _prenom = value;
                OnPropertyChanged();
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _telephone = string.Empty;
        public string Telephone
        {
            get => _telephone;
            set
            {
                _telephone = value;
                OnPropertyChanged();
            }
        }

        private string _adresse = string.Empty;
        public string Adresse
        {
            get => _adresse;
            set
            {
                _adresse = value;
                OnPropertyChanged();
            }
        }

        // Champ de recherche
        private string _searchKeyword = string.Empty;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
            }
        }

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

        public ClientsViewModel()
        {
            try
            {
                // Initialisation du repository avec la connexion
                string connectionString = DatabaseConnection.GetConnection().ConnectionString;
                _repository = new ClientRepository(connectionString);
                
                Console.WriteLine("ClientsViewModel initialisé");
                
                // Charger les données au démarrage
                LoadClients();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR dans constructeur ClientsViewModel: {ex.Message}");
                StatusMessage = $"Erreur d'initialisation: {ex.Message}";
            }
        }

        // Charger tous les clients
        public void LoadClients()
        {
            try
            {
                Console.WriteLine("Tentative de chargement des clients...");
                var clientsList = _repository.GetAll();
                Console.WriteLine($"Nombre de clients récupérés: {clientsList.Count}");
                
                // Créer une nouvelle collection au lieu de modifier l'existante
                var newClients = new ObservableCollection<ClientDisplayModel>();
                
                foreach (var client in clientsList)
                {
                    newClients.Add(new ClientDisplayModel
                    {
                        Id = client.Id,
                        Nom = client.Nom,
                        Prenom = client.Prenom,
                        Email = client.Email,
                        Telephone = client.Telephone ?? "N/A",
                        Adresse = client.Adresse ?? "N/A",
                        NomComplet = $"{client.Prenom} {client.Nom}",
                        Ville = ExtraireVille(client.Adresse ?? "")
                    });
                }
                
                // Remplacer la collection entière pour forcer le rafraîchissement
                Clients = newClients;
                
                Console.WriteLine($"Clients ajoutés à la collection: {Clients.Count}");
                StatusMessage = $"{Clients.Count} client(s) chargé(s)";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR dans LoadClients: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                StatusMessage = $"Erreur de chargement: {ex.Message}";
            }
        }

        // Ajouter un nouveau client
        public void AjouterClient()
        {
            if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(Email))
            {
                StatusMessage = "Veuillez remplir au minimum Nom, Prénom et Email";
                return;
            }

            try
            {
                var newClient = new Client
                {
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = string.IsNullOrWhiteSpace(Telephone) ? null : Telephone,
                    Adresse = string.IsNullOrWhiteSpace(Adresse) ? null : Adresse
                };

                bool success = _repository.Add(newClient);
                
                if (success)
                {
                    StatusMessage = "Client ajouté avec succès";
                    ClearFields();
                    LoadClients();
                }
                else
                {
                    StatusMessage = "Échec de l'ajout du client";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur: {ex.Message}";
            }
        }

        // Modifier le client sélectionné
        public void ModifierClient()
        {
            if (SelectedClient == null)
            {
                StatusMessage = "Veuillez sélectionner un client à modifier";
                return;
            }

            if (string.IsNullOrWhiteSpace(Nom) || string.IsNullOrWhiteSpace(Prenom) || string.IsNullOrWhiteSpace(Email))
            {
                StatusMessage = "Veuillez remplir au minimum Nom, Prénom et Email";
                return;
            }

            try
            {
                var updatedClient = new Client
                {
                    Id = SelectedClient.Id,
                    Nom = Nom,
                    Prenom = Prenom,
                    Email = Email,
                    Telephone = string.IsNullOrWhiteSpace(Telephone) ? null : Telephone,
                    Adresse = string.IsNullOrWhiteSpace(Adresse) ? null : Adresse
                };

                bool success = _repository.Update(updatedClient);
                
                if (success)
                {
                    StatusMessage = "Client modifié avec succès";
                    ClearFields();
                    LoadClients();
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

        // Supprimer le client sélectionné
        public void SupprimerClient()
        {
            if (SelectedClient == null)
            {
                StatusMessage = "Veuillez sélectionner un client à supprimer";
                return;
            }

            try
            {
                bool success = _repository.Delete(SelectedClient.Id);
                
                if (success)
                {
                    StatusMessage = "Client supprimé avec succès";
                    ClearFields();
                    LoadClients();
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

        // Rechercher des clients
        public void RechercherClients()
        {
            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                LoadClients();
                return;
            }

            try
            {
                var results = _repository.Search(SearchKeyword);
                Clients.Clear();
                
                foreach (var client in results)
                {
                    Clients.Add(new ClientDisplayModel
                    {
                        Id = client.Id,
                        Nom = client.Nom,
                        Prenom = client.Prenom,
                        Email = client.Email,
                        Telephone = client.Telephone ?? "N/A",
                        Adresse = client.Adresse ?? "N/A",
                        NomComplet = $"{client.Prenom} {client.Nom}",
                        Ville = ExtraireVille(client.Adresse ?? "")
                    });
                }
                
                StatusMessage = $"{Clients.Count} résultat(s) trouvé(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur de recherche: {ex.Message}";
            }
        }

        // Vider les champs de saisie
        private void ClearFields()
        {
            Nom = string.Empty;
            Prenom = string.Empty;
            Email = string.Empty;
            Telephone = string.Empty;
            Adresse = string.Empty;
            SelectedClient = null;
        }

        // Extraire la ville depuis l'adresse (dernière partie séparée par virgule)
        private string ExtraireVille(string adresse)
        {
            if (string.IsNullOrWhiteSpace(adresse))
                return "N/A";

            var parts = adresse.Split(',');
            return parts.Length > 0 ? parts[parts.Length - 1].Trim() : "N/A";
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Modèle pour l'affichage dans le DataGrid
    public class ClientDisplayModel
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Ville { get; set; } = string.Empty;
    }
}