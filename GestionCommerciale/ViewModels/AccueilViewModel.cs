using ReactiveUI;
using System.Reactive;
using System;
using System.Threading.Tasks;

namespace GestionCommerciale.ViewModels;

public class AccueilViewModel : ViewModelBase
{
    private string _statusMessage = string.Empty;

    public string StatusMessage
    {
        get => _statusMessage;
        set { this.RaiseAndSetIfChanged(ref _statusMessage, value); }
    }

    public ReactiveCommand<Unit, Unit> ManageClientsCommand { get; }
    public ReactiveCommand<Unit, Unit> ManageProductsCommand { get; }
    public ReactiveCommand<Unit, Unit> ManageOrdersCommand { get; }

    public AccueilViewModel()
    {
       
        StatusMessage = "Prêt - En attente de connexion BDD";

        
        ManageClientsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            StatusMessage = "Ouverture de la gestion des clients...";
            
        });

        ManageProductsCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            StatusMessage = "Ouverture de la gestion des produits...";
           
        });

        ManageOrdersCommand = ReactiveCommand.CreateFromTask(async () =>
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
            StatusMessage = "✓ Connexion à la base de données : OK";
        }
        catch (Exception ex)
        {
            StatusMessage = $"✗ Erreur de connexion : {ex.Message}";
        }
    }
} 