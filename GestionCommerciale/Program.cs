using Avalonia;
using System;
using GestionCommerciale.Data; // <--- INDISPENSABLE : Pour trouver ta classe DatabaseConnection

namespace GestionCommerciale;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // --- ZONE DE TEST MYSQL ---
        Console.WriteLine("------------------------------------------");
        DatabaseConnection.TestConnection(); // Lancement du test
        Console.WriteLine("------------------------------------------");
        // --------------------------

        // Lancement normal de l'application
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}