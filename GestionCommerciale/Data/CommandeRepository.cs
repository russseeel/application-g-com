using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GestionCommerciale.Models;

namespace GestionCommerciale.Data
{
    public class CommandeRepository
    {
        private readonly string _connectionString;

        public CommandeRepository()
        {
            _connectionString = DatabaseConnection.GetConnectionString();
        }

        // Récupérer toutes les commandes avec JOIN
        public List<Commande> GetAll()
        {
            var commandes = new List<Commande>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        c.id,
                        c.client_id,
                        c.produit_id,
                        c.quantite,
                        c.montant_total,
                        c.date_commande,
                        cl.id AS cl_id,
                        cl.nom AS cl_nom,
                        cl.prenom AS cl_prenom,
                        cl.email AS cl_email,
                        cl.telephone AS cl_telephone,
                        cl.adresse AS cl_adresse,
                        p.id AS p_id,
                        p.nom AS p_nom,
                        p.description AS p_description,
                        p.prix_unitaire AS p_prix,
                        p.stock AS p_stock,
                        p.categorie AS p_categorie
                    FROM commandes c
                    JOIN clients cl ON c.client_id = cl.id
                    JOIN produits p ON c.produit_id = p.id
                    ORDER BY c.date_commande DESC";

                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var commande = new Commande
                        {
                            Id = reader.GetInt32("id"),
                            ClientId = reader.GetInt32("client_id"),
                            ProduitId = reader.GetInt32("produit_id"),
                            Quantite = reader.GetInt32("quantite"),
                            MontantTotal = reader.GetDecimal("montant_total"),
                            DateCommande = reader.GetDateTime("date_commande"),
                            
                            // Navigation properties
                            Client = new Client
                            {
                                Id = reader.GetInt32("cl_id"),
                                Nom = reader.GetString("cl_nom"),
                                Prenom = reader.GetString("cl_prenom"),
                                Email = reader.GetString("cl_email"),
                                Telephone = reader.IsDBNull(reader.GetOrdinal("cl_telephone")) ? string.Empty : reader.GetString("cl_telephone"),
                                Adresse = reader.IsDBNull(reader.GetOrdinal("cl_adresse")) ? string.Empty : reader.GetString("cl_adresse")
                                // SUPPRIMÉ: Ville n'existe pas dans Models.Client
                            },
                            
                            Produit = new Produit
                            {
                                Id = reader.GetInt32("p_id"),
                                Nom = reader.GetString("p_nom"),
                                Description = reader.IsDBNull(reader.GetOrdinal("p_description")) ? string.Empty : reader.GetString("p_description"),
                                PrixUnitaire = reader.GetDecimal("p_prix"),
                                Stock = reader.GetInt32("p_stock"),
                                Categorie = reader.IsDBNull(reader.GetOrdinal("p_categorie")) ? string.Empty : reader.GetString("p_categorie")
                            }
                        };
                        commandes.Add(commande);
                    }
                }
            }
            
            return commandes;
        }

        // Ajouter une commande avec transaction
        public void Add(Commande commande)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. Vérifier que le client existe
                    string checkClientQuery = "SELECT COUNT(*) FROM clients WHERE id = @clientId";
                    using (var cmdClient = new MySqlCommand(checkClientQuery, connection, transaction))
                    {
                        cmdClient.Parameters.AddWithValue("@clientId", commande.ClientId);
                        int clientExists = Convert.ToInt32(cmdClient.ExecuteScalar());
                        if (clientExists == 0)
                        {
                            throw new Exception($"Client avec ID {commande.ClientId} n'existe pas.");
                        }
                    }

                    // 2. Vérifier que le produit existe et récupérer ses informations
                    string checkProduitQuery = "SELECT prix_unitaire, stock FROM produits WHERE id = @produitId";
                    decimal prixUnitaire;
                    int stockDisponible;
                    
                    using (var cmdProduit = new MySqlCommand(checkProduitQuery, connection, transaction))
                    {
                        cmdProduit.Parameters.AddWithValue("@produitId", commande.ProduitId);
                        using (var reader = cmdProduit.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                throw new Exception($"Produit avec ID {commande.ProduitId} n'existe pas.");
                            }
                            prixUnitaire = reader.GetDecimal("prix_unitaire");
                            stockDisponible = reader.GetInt32("stock");
                        }
                    }

                    // 3. Vérifier le stock disponible
                    if (commande.Quantite > stockDisponible)
                    {
                        throw new Exception($"Stock insuffisant. Disponible: {stockDisponible}, Demandé: {commande.Quantite}");
                    }

                    // 4. Calculer le montant total
                    decimal montantTotal = commande.Quantite * prixUnitaire;

                    // 5. Insérer la commande
                    string insertQuery = @"
                        INSERT INTO commandes (client_id, produit_id, quantite, montant_total, date_commande)
                        VALUES (@clientId, @produitId, @quantite, @montantTotal, @dateCommande)";
                    
                    using (var cmdInsert = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmdInsert.Parameters.AddWithValue("@clientId", commande.ClientId);
                        cmdInsert.Parameters.AddWithValue("@produitId", commande.ProduitId);
                        cmdInsert.Parameters.AddWithValue("@quantite", commande.Quantite);
                        cmdInsert.Parameters.AddWithValue("@montantTotal", montantTotal);
                        cmdInsert.Parameters.AddWithValue("@dateCommande", commande.DateCommande);
                        
                        cmdInsert.ExecuteNonQuery();
                    }

                    // 6. Mettre à jour le stock
                    string updateStockQuery = "UPDATE produits SET stock = stock - @quantite WHERE id = @produitId";
                    using (var cmdUpdate = new MySqlCommand(updateStockQuery, connection, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@quantite", commande.Quantite);
                        cmdUpdate.Parameters.AddWithValue("@produitId", commande.ProduitId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // Commit de la transaction
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Rollback en cas d'erreur
                    transaction.Rollback();
                    throw;
                }
            }
        }

        // Récupérer toutes les commandes d'un client
        public List<Commande> GetByClient(int clientId)
        {
            var commandes = new List<Commande>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        c.id,
                        c.client_id,
                        c.produit_id,
                        c.quantite,
                        c.montant_total,
                        c.date_commande,
                        cl.id AS cl_id,
                        cl.nom AS cl_nom,
                        cl.prenom AS cl_prenom,
                        cl.email AS cl_email,
                        cl.telephone AS cl_telephone,
                        cl.adresse AS cl_adresse,
                        p.id AS p_id,
                        p.nom AS p_nom,
                        p.description AS p_description,
                        p.prix_unitaire AS p_prix,
                        p.stock AS p_stock,
                        p.categorie AS p_categorie
                    FROM commandes c
                    JOIN clients cl ON c.client_id = cl.id
                    JOIN produits p ON c.produit_id = p.id
                    WHERE c.client_id = @clientId
                    ORDER BY c.date_commande DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var commande = new Commande
                            {
                                Id = reader.GetInt32("id"),
                                ClientId = reader.GetInt32("client_id"),
                                ProduitId = reader.GetInt32("produit_id"),
                                Quantite = reader.GetInt32("quantite"),
                                MontantTotal = reader.GetDecimal("montant_total"),
                                DateCommande = reader.GetDateTime("date_commande"),
                                
                                Client = new Client
                                {
                                    Id = reader.GetInt32("cl_id"),
                                    Nom = reader.GetString("cl_nom"),
                                    Prenom = reader.GetString("cl_prenom"),
                                    Email = reader.GetString("cl_email"),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("cl_telephone")) ? string.Empty : reader.GetString("cl_telephone"),
                                    Adresse = reader.IsDBNull(reader.GetOrdinal("cl_adresse")) ? string.Empty : reader.GetString("cl_adresse")
                                },
                                
                                Produit = new Produit
                                {
                                    Id = reader.GetInt32("p_id"),
                                    Nom = reader.GetString("p_nom"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("p_description")) ? string.Empty : reader.GetString("p_description"),
                                    PrixUnitaire = reader.GetDecimal("p_prix"),
                                    Stock = reader.GetInt32("p_stock"),
                                    Categorie = reader.IsDBNull(reader.GetOrdinal("p_categorie")) ? string.Empty : reader.GetString("p_categorie")
                                }
                            };
                            commandes.Add(commande);
                        }
                    }
                }
            }
            
            return commandes;
        }

        // Récupérer toutes les commandes d'un produit
        public List<Commande> GetByProduit(int produitId)
        {
            var commandes = new List<Commande>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        c.id,
                        c.client_id,
                        c.produit_id,
                        c.quantite,
                        c.montant_total,
                        c.date_commande,
                        cl.id AS cl_id,
                        cl.nom AS cl_nom,
                        cl.prenom AS cl_prenom,
                        cl.email AS cl_email,
                        cl.telephone AS cl_telephone,
                        cl.adresse AS cl_adresse,
                        p.id AS p_id,
                        p.nom AS p_nom,
                        p.description AS p_description,
                        p.prix_unitaire AS p_prix,
                        p.stock AS p_stock,
                        p.categorie AS p_categorie
                    FROM commandes c
                    JOIN clients cl ON c.client_id = cl.id
                    JOIN produits p ON c.produit_id = p.id
                    WHERE c.produit_id = @produitId
                    ORDER BY c.date_commande DESC";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@produitId", produitId);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var commande = new Commande
                            {
                                Id = reader.GetInt32("id"),
                                ClientId = reader.GetInt32("client_id"),
                                ProduitId = reader.GetInt32("produit_id"),
                                Quantite = reader.GetInt32("quantite"),
                                MontantTotal = reader.GetDecimal("montant_total"),
                                DateCommande = reader.GetDateTime("date_commande"),
                                
                                Client = new Client
                                {
                                    Id = reader.GetInt32("cl_id"),
                                    Nom = reader.GetString("cl_nom"),
                                    Prenom = reader.GetString("cl_prenom"),
                                    Email = reader.GetString("cl_email"),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("cl_telephone")) ? string.Empty : reader.GetString("cl_telephone"),
                                    Adresse = reader.IsDBNull(reader.GetOrdinal("cl_adresse")) ? string.Empty : reader.GetString("cl_adresse")
                                },
                                
                                Produit = new Produit
                                {
                                    Id = reader.GetInt32("p_id"),
                                    Nom = reader.GetString("p_nom"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("p_description")) ? string.Empty : reader.GetString("p_description"),
                                    PrixUnitaire = reader.GetDecimal("p_prix"),
                                    Stock = reader.GetInt32("p_stock"),
                                    Categorie = reader.IsDBNull(reader.GetOrdinal("p_categorie")) ? string.Empty : reader.GetString("p_categorie")
                                }
                            };
                            commandes.Add(commande);
                        }
                    }
                }
            }
            
            return commandes;
        }
    }
}