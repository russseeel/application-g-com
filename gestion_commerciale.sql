-- Création de la base de données
CREATE DATABASE IF NOT EXISTS gestion_commerciale
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE gestion_commerciale;

-- Table CLIENTS
CREATE TABLE IF NOT EXISTS clients (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nom VARCHAR(100) NOT NULL,
    prenom VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    telephone VARCHAR(20) UNIQUE,
    adresse TEXT,
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_email (email),
    INDEX idx_telephone (telephone),
    INDEX idx_nom (nom)
) ENGINE=InnoDB;

-- Table PRODUITS
CREATE TABLE IF NOT EXISTS produits (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nom VARCHAR(150) NOT NULL,
    description TEXT,
    prix_unitaire DECIMAL(10,2) NOT NULL CHECK (prix_unitaire >= 0),
    stock INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
    categorie VARCHAR(50),
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_nom_categorie (nom, categorie),
    INDEX idx_nom (nom),
    INDEX idx_categorie (categorie)
) ENGINE=InnoDB;

-- Table COMMANDES
CREATE TABLE IF NOT EXISTS commandes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    client_id INT NOT NULL,
    produit_id INT NOT NULL,
    quantite INT NOT NULL CHECK (quantite > 0),
    date_commande TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    montant_total DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_commandes_clients FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE CASCADE,
    CONSTRAINT fk_commandes_produits FOREIGN KEY (produit_id) REFERENCES produits(id) ON DELETE CASCADE,
    INDEX idx_client (client_id),
    INDEX idx_produit (produit_id),
    INDEX idx_date (date_commande)
) ENGINE=InnoDB;

-- Données de test
INSERT INTO clients (nom, prenom, email, telephone, adresse) VALUES
('Dupont', 'Jean', 'jean.dupont@email.com', '0601020304', '10 Rue de Paris, Lyon'),
('Martin', 'Sophie', 'sophie.martin@email.com', '0602030405', '25 Avenue Victor Hugo, Lyon'),
('Bernard', 'Luc', 'luc.bernard@email.com', '0603040506', '5 Place Bellecour, Lyon');

INSERT INTO produits (nom, description, prix_unitaire, stock, categorie) VALUES
('Ordinateur Portable HP', 'HP Pavilion 15, 8GB RAM, 256GB SSD', 599.99, 15, 'Informatique'),
('Souris Logitech', 'Souris sans fil ergonomique', 29.99, 50, 'Accessoires'),
('Clavier Mécanique', 'Clavier RGB rétroéclairé', 79.99, 30, 'Accessoires'),
('Écran 24 pouces', 'Full HD IPS 75Hz', 149.99, 20, 'Informatique'),
('Casque Audio', 'Bluetooth avec réduction de bruit', 89.99, 25, 'Audio');

INSERT INTO commandes (client_id, produit_id, quantite, montant_total) VALUES
(1, 1, 2, 1199.98),
(2, 2, 3, 89.97),
(3, 3, 1, 79.99),
(1, 4, 1, 149.99);

-- Vérification des données insérées
SELECT * FROM clients;

SELECT * FROM produits;

SELECT * FROM commandes;