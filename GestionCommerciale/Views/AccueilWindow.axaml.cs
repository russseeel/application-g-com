using Avalonia.Controls;
using GestionCommerciale.ViewModels;

namespace GestionCommerciale.Views;

public partial class AccueilWindow : Window
{
    public AccueilWindow()
    {
        InitializeComponent();
        DataContext = new AccueilViewModel();
    }
}