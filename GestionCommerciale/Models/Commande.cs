using System;

namespace GestionCommerciale.Models
{
    public class Commande
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }
        public DateTime DateCommande { get; set; }

        // Montant total peut être calculé dynamiquement si Produit est assigné
        public decimal MontantTotal
        {
            get
            {
                return Produit != null ? Produit.PrixUnitaire * Quantite : _montantTotal;
            }
            set
            {
                _montantTotal = value;
            }
        }
        private decimal _montantTotal;

        // Navigation : pour MVVM et affichage
        public Client? Client { get; set; }
        public Produit? Produit { get; set; }

        // Propriétés pratiques pour affichage
        public string NomClient => Client?.NomComplet ?? string.Empty;
        public string NomProduit => Produit?.Nom ?? string.Empty;

        public Commande()
        {
            DateCommande = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Commande #{Id} - {NomClient} - {NomProduit} x {Quantite} - {MontantTotal:C}";
        }
    }
}
