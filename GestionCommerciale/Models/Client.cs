using System;
using System.Collections.Generic;

namespace GestionCommerciale.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }

        // Navigation : liste des commandes d'un client
        public List<Commande> Commandes { get; set; } = new List<Commande>();

        public Client()
        {
            DateCreation = DateTime.Now;
        }

        // Propriété calculée pour affichage
        public string NomComplet => $"{Prenom} {Nom}";

        public override string ToString()
        {
            return $"{NomComplet} - {Email}";
        }
    }
}
