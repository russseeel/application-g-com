using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GestionCommerciale.Models;

namespace GestionCommerciale.Data
{
    public class ProduitRepository
    {
        private readonly string _connectionString;

        public ProduitRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Récupère tous les produits triés par nom
        public List<Produit> GetAll()
        {
            List<Produit> produits = new List<Produit>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, description, prix_unitaire, stock, categorie, date_creation
                                   FROM produits
                                   ORDER BY nom ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            produits.Add(MapProduit(reader));
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetAll: {ex.Message}");
                throw new Exception("Erreur lors de la récupération des produits", ex);
            }

            return produits;
        }

        // Récupère un produit par son ID
        public Produit? GetById(int id)
        {
            Produit? produit = null;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, description, prix_unitaire, stock, categorie, date_creation
                                   FROM produits
                                   WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                produit = MapProduit(reader);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetById: {ex.Message}");
                throw new Exception($"Erreur lors de la récupération du produit ID {id}", ex);
            }

            return produit;
        }

        // Ajoute un nouveau produit avec validation des contraintes
        public bool Add(Produit produit)
        {
            // Validation des contraintes en C#
            if (produit.PrixUnitaire < 0)
            {
                Console.WriteLine("Erreur: Le prix unitaire doit être >= 0");
                return false;
            }

            if (produit.Stock < 0)
            {
                Console.WriteLine("Erreur: Le stock doit être >= 0");
                return false;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO produits (nom, description, prix_unitaire, stock, categorie)
                                   VALUES (@nom, @description, @prix_unitaire, @stock, @categorie)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", produit.Nom);
                        cmd.Parameters.AddWithValue("@description", produit.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@prix_unitaire", produit.PrixUnitaire);
                        cmd.Parameters.AddWithValue("@stock", produit.Stock);
                        cmd.Parameters.AddWithValue("@categorie", produit.Categorie ?? (object)DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans Add: {ex.Message}");
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Un produit avec ce nom et cette catégorie existe déjà");
                }
                return false;
            }
        }

        // Modifie un produit existant avec validation des contraintes
        public bool Update(Produit produit)
        {
            // Validation des contraintes en C#
            if (produit.PrixUnitaire < 0)
            {
                Console.WriteLine("Erreur: Le prix unitaire doit être >= 0");
                return false;
            }

            if (produit.Stock < 0)
            {
                Console.WriteLine("Erreur: Le stock doit être >= 0");
                return false;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Vérification que le produit existe
                    string checkQuery = "SELECT COUNT(*) FROM produits WHERE id = @id";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", produit.Id);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count == 0)
                        {
                            Console.WriteLine($"Produit avec ID {produit.Id} n'existe pas");
                            return false;
                        }
                    }

                    // Mise à jour du produit
                    string updateQuery = @"UPDATE produits
                                         SET nom = @nom,
                                             description = @description,
                                             prix_unitaire = @prix_unitaire,
                                             stock = @stock,
                                             categorie = @categorie
                                         WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", produit.Id);
                        cmd.Parameters.AddWithValue("@nom", produit.Nom);
                        cmd.Parameters.AddWithValue("@description", produit.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@prix_unitaire", produit.PrixUnitaire);
                        cmd.Parameters.AddWithValue("@stock", produit.Stock);
                        cmd.Parameters.AddWithValue("@categorie", produit.Categorie ?? (object)DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans Update: {ex.Message}");
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Un produit avec ce nom et cette catégorie existe déjà");
                }
                return false;
            }
        }

        // Supprime un produit par ID (la cascade supprime automatiquement les commandes associées)
        public bool Delete(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // La suppression cascade est gérée par la contrainte ON DELETE CASCADE
                    // Les commandes associées seront automatiquement supprimées
                    string query = "DELETE FROM produits WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Produit {id} supprimé avec succès (commandes associées également supprimées)");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Aucun produit trouvé avec l'ID {id}");
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans Delete: {ex.Message}");
                return false;
            }
        }

        // Récupère les produits par catégorie
        public List<Produit> GetByCategorie(string categorie)
        {
            List<Produit> produits = new List<Produit>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, description, prix_unitaire, stock, categorie, date_creation
                                   FROM produits
                                   WHERE categorie = @categorie
                                   ORDER BY nom ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@categorie", categorie);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                produits.Add(MapProduit(reader));
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetByCategorie: {ex.Message}");
                throw new Exception($"Erreur lors de la récupération des produits de la catégorie '{categorie}'", ex);
            }

            return produits;
        }

        // Récupère les produits avec un stock inférieur au seuil (pour alertes)
        public List<Produit> GetLowStock(int seuil)
        {
            List<Produit> produits = new List<Produit>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, description, prix_unitaire, stock, categorie, date_creation
                                   FROM produits
                                   WHERE stock < @seuil
                                   ORDER BY stock ASC, nom ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@seuil", seuil);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                produits.Add(MapProduit(reader));
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetLowStock: {ex.Message}");
                throw new Exception($"Erreur lors de la récupération des produits avec stock < {seuil}", ex);
            }

            return produits;
        }

        // Méthode utilitaire pour mapper un reader vers un objet Produit
        private Produit MapProduit(MySqlDataReader reader)
        {
            return new Produit
            {
                Id = reader.GetInt32("id"),
                Nom = reader.GetString("nom"),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString("description"),
                PrixUnitaire = reader.GetDecimal("prix_unitaire"),
                Stock = reader.GetInt32("stock"),
                Categorie = reader.IsDBNull(reader.GetOrdinal("categorie")) ? string.Empty : reader.GetString("categorie"),
                DateCreation = reader.GetDateTime("date_creation")
            };
        }
    }
}
