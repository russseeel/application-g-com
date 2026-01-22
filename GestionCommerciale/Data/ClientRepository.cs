using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GestionCommerciale.Models; // AJOUT : import du namespace Models

namespace GestionCommerciale.Data
{
    // Repository pour gérer les opérations CRUD sur la table clients
    public class ClientRepository
    {
        private readonly string _connectionString;

        // Constructeur avec chaîne de connexion MySQL
        public ClientRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Récupère tous les clients triés par nom et prénom (gère les NULL pour téléphone et adresse)
        public List<Client> GetAll()
        {
            List<Client> clients = new List<Client>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, prenom, email, telephone, adresse, date_creation 
                                   FROM clients 
                                   ORDER BY nom ASC, prenom ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                Id = reader.GetInt32("id"),
                                Nom = reader.GetString("nom"),
                                Prenom = reader.GetString("prenom"),
                                Email = reader.GetString("email"),
                                Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? string.Empty : reader.GetString("telephone"),
                                Adresse = reader.IsDBNull(reader.GetOrdinal("adresse")) ? string.Empty : reader.GetString("adresse"),
                                DateCreation = reader.GetDateTime("date_creation")
                            });
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetAll: {ex.Message}");
                throw new Exception("Erreur lors de la récupération des clients", ex);
            }

            return clients;
        }

        // Récupère un client par son ID (retourne null si inexistant)
        public Client? GetById(int id)
        {
            Client? client = null;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, prenom, email, telephone, adresse, date_creation 
                                   FROM clients 
                                   WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                client = new Client
                                {
                                    Id = reader.GetInt32("id"),
                                    Nom = reader.GetString("nom"),
                                    Prenom = reader.GetString("prenom"),
                                    Email = reader.GetString("email"),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? string.Empty : reader.GetString("telephone"),
                                    Adresse = reader.IsDBNull(reader.GetOrdinal("adresse")) ? string.Empty : reader.GetString("adresse"),
                                    DateCreation = reader.GetDateTime("date_creation")
                                };
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans GetById: {ex.Message}");
                throw new Exception($"Erreur lors de la récupération du client ID {id}", ex);
            }

            return client;
        }

        // Ajoute un nouveau client (retourne true si succès, false sinon)
        public bool Add(Client client)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO clients (nom, prenom, email, telephone, adresse) 
                                   VALUES (@nom, @prenom, @email, @telephone, @adresse)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nom", client.Nom);
                        cmd.Parameters.AddWithValue("@prenom", client.Prenom);
                        cmd.Parameters.AddWithValue("@email", client.Email);
                        cmd.Parameters.AddWithValue("@telephone", string.IsNullOrEmpty(client.Telephone) ? (object)DBNull.Value : client.Telephone);
                        cmd.Parameters.AddWithValue("@adresse", string.IsNullOrEmpty(client.Adresse) ? (object)DBNull.Value : client.Adresse);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans Add: {ex.Message}");
                // Gestion des erreurs spécifiques (email ou téléphone en doublon)
                if (ex.Number == 1062)
                {
                    Console.WriteLine("Email ou téléphone déjà existant");
                }
                return false;
            }
        }

        // Modifie un client existant (vérifie que l'ID existe)
        public bool Update(Client client)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Vérification que le client existe
                    string checkQuery = "SELECT COUNT(*) FROM clients WHERE id = @id";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", client.Id);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        
                        if (count == 0)
                        {
                            Console.WriteLine($"Client avec ID {client.Id} n'existe pas");
                            return false;
                        }
                    }

                    // Mise à jour du client
                    string updateQuery = @"UPDATE clients 
                                         SET nom = @nom, 
                                             prenom = @prenom, 
                                             email = @email, 
                                             telephone = @telephone, 
                                             adresse = @adresse 
                                         WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", client.Id);
                        cmd.Parameters.AddWithValue("@nom", client.Nom);
                        cmd.Parameters.AddWithValue("@prenom", client.Prenom);
                        cmd.Parameters.AddWithValue("@email", client.Email);
                        cmd.Parameters.AddWithValue("@telephone", string.IsNullOrEmpty(client.Telephone) ? (object)DBNull.Value : client.Telephone);
                        cmd.Parameters.AddWithValue("@adresse", string.IsNullOrEmpty(client.Adresse) ? (object)DBNull.Value : client.Adresse);

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
                    Console.WriteLine("Email ou téléphone déjà existant");
                }
                return false;
            }
        }

        // Supprime un client par ID (la cascade supprime automatiquement les commandes associées)
        public bool Delete(int id)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // La suppression cascade est gérée par la contrainte ON DELETE CASCADE
                    // Les commandes associées seront automatiquement supprimées
                    string query = "DELETE FROM clients WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Client {id} supprimé avec succès (commandes associées également supprimées)");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"Aucun client trouvé avec l'ID {id}");
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

        // Recherche des clients par nom OU email avec LIKE '%keyword%'
        public List<Client> Search(string keyword)
        {
            List<Client> clients = new List<Client>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT id, nom, prenom, email, telephone, adresse, date_creation 
                                   FROM clients 
                                   WHERE nom LIKE @keyword OR email LIKE @keyword
                                   ORDER BY nom ASC, prenom ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clients.Add(new Client
                                {
                                    Id = reader.GetInt32("id"),
                                    Nom = reader.GetString("nom"),
                                    Prenom = reader.GetString("prenom"),
                                    Email = reader.GetString("email"),
                                    Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? string.Empty : reader.GetString("telephone"),
                                    Adresse = reader.IsDBNull(reader.GetOrdinal("adresse")) ? string.Empty : reader.GetString("adresse"),
                                    DateCreation = reader.GetDateTime("date_creation")
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur MySQL dans Search: {ex.Message}");
                throw new Exception($"Erreur lors de la recherche avec le mot-clé '{keyword}'", ex);
            }

            return clients;
        }
    }

    // SUPPRIMÉ : La classe Client est maintenant uniquement dans GestionCommerciale.Models
}