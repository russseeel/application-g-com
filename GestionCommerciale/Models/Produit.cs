using System;

namespace GestionCommerciale.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
        public int Stock { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public DateTime DateCreation { get; set; }

        public Produit()
        {
            DateCreation = DateTime.Now;
        }

        // Propriété calculée : disponibilité
        public bool EstDisponible => Stock > 0;

        // Propriété calculée : valeur totale en stock
        public decimal ValeurStock => PrixUnitaire * Stock;

        public override string ToString()
        {
            return $"{Nom} - {PrixUnitaire:C} (Stock: {Stock})";
        }
    }
}
