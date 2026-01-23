using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace GestionCommerciale.ViewModels;

public class AccueilViewModel : ViewModelBase
{
    private string _statusMessage = string.Empty;

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public IRelayCommand ManageClientsCommand { get; }
    public IRelayCommand ManageProductsCommand { get; }
    public IRelayCommand ManageOrdersCommand { get; }

    public AccueilViewModel()
    {
        StatusMessage = "Prêt - En attente de connexion BDD";

        ManageClientsCommand = new RelayCommand(() =>
        {
            StatusMessage = "Ouverture de la gestion des clients...";
        });

        ManageProductsCommand = new RelayCommand(() =>
        {
            StatusMessage = "Ouverture de la gestion des produits...";
        });

        ManageOrdersCommand = new RelayCommand(() =>
        {
            StatusMessage = "Ouverture de la gestion des commandes...";
        });

        _ = CheckDatabaseConnectionAsync();
    }

    private async Task CheckDatabaseConnectionAsync()
    {
        try
        {
            await Task.Delay(500);
            StatusMessage = "Connexion à la base de données : OK";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur de connexion : {ex.Message}";
        }
    }
}
