using Avalonia;
using System;
using GestionCommerciale.Data;
using DotNetEnv;

namespace GestionCommerciale;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Env.Load();

        // --- ZONE DE TEST MYSQL ---
        Console.WriteLine("------------------------------------------");
        DatabaseConnection.TestConnection();
        Console.WriteLine("------------------------------------------");
        // --------------------------

        // Lancement normal de l'application
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
