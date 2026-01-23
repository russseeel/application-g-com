using Avalonia.Controls;
using GestionCommerciale.ViewModels;

namespace GestionCommerciale.Views;

public partial class AccueilView : UserControl
{
    public AccueilView()
    {
        InitializeComponent();
        DataContext = new AccueilViewModel();
    }
};