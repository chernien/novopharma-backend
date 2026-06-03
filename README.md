# Novopharma Backend — API REST (.NET 6 / EF Core)

**API REST** qui alimente l'application mobile de force de vente **Novopharma MSL** (délégués commerciaux) ainsi que le dashboard d'administration web. Construite avec **ASP.NET Core 6**, **Entity Framework Core** et **SQL Server**, suivant un **pattern Repository**.

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-7-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)

---

## 📑 Sommaire

- [Présentation](#-présentation)
- [Technologies](#-technologies)
- [Architecture](#-architecture)
- [Prérequis](#-prérequis)
- [Configuration (appsettings.json)](#-configuration-appsettingsjson)
- [Installation & lancement](#-installation--lancement)
- [Documentation API (Swagger)](#-documentation-api-swagger)
- [Principaux endpoints](#-principaux-endpoints)
- [Déploiement](#-déploiement)
- [Sécurité](#-sécurité)

---

## 🎯 Présentation

Ce service expose les données métier de Novopharma à travers une API REST :

- **Catalogue produits** (articles, marques, familles)
- **Pharmacies / clients** et authentification des commerciaux
- **Prise de commande** (mono et multi-pharmacies), **factures** et historiques
- **Cadeaux / promotions** (gifts)
- **Réclamations** et **contacts**

Il sert également un **dashboard d'administration** statique (SPA) hébergé dans `wwwroot/`.

> **Consommateurs** : l'app mobile Ionic *Novopharma MSL* et le dashboard web d'administration.

---

## 🛠 Technologies

| Technologie | Usage |
|-------------|-------|
| **ASP.NET Core 6** | Framework Web API |
| **Entity Framework Core 7** | ORM, accès aux données |
| **SQL Server** | Bases de données (NOVOPHARMA, MEDSOURCE, POWERAPPS) |
| **Swashbuckle / Swagger** | Documentation OpenAPI interactive |
| **Repository Pattern** | Séparation logique d'accès aux données |
| **CORS** | Autorisation des origines mobile & web |
| **IMemoryCache** | Mise en cache en mémoire |

---

## 🏗 Architecture

```
WebApiTest/
├── Controllers/          # Points d'entrée HTTP (REST)
│   ├── MbArticleController.cs       # Articles
│   ├── MBAArticleController.cs      # Marques
│   ├── MsAClientController.cs       # Clients / pharmacies / auth / commandes
│   ├── MsAEnttController.cs         # En-têtes commandes & factures
│   ├── FactureController.cs         # Factures
│   ├── FDocLigneController.cs       # Lignes de documents
│   ├── GiftController.cs            # Cadeaux
│   ├── ReclamationController.cs     # Réclamations
│   ├── MsContactController.cs       # Contacts
│   └── FallbackController.cs        # Fallback SPA (dashboard)
├── Data/                 # DbContext SQL Server
│   ├── NOVOPHARMAContext.cs
│   └── MEDSOURCEContext.cs
├── Data2/
│   └── PowerAppContext.cs           # Base réclamations (POWERAPPS)
├── Interface/            # Contrats des repositories
├── Repository/           # Implémentations (accès données)
├── Models/               # Entités EF (scaffold base de données)
├── wwwroot/              # Dashboard admin statique + assets
├── Program.cs            # Configuration & démarrage
└── appsettings.example.json   # Modèle de configuration (À COPIER)
```

**Trois bases de données** sont connectées via trois `DbContext` distincts :

| DbContext | Base | Connection string |
|-----------|------|-------------------|
| `NOVOPHARMAContext` | NOVOPHARMA | `DefaultConnection` |
| `MEDSOURCEContext` | MEDSOURCE | `MedsourceConnection` |
| `PowerAppContext` | POWERAPPS | `ReclamationConnection` |

---

## ✅ Prérequis

| Outil | Version |
|-------|---------|
| **.NET SDK** | 6.0 |
| **SQL Server** | Accès aux bases NOVOPHARMA / MEDSOURCE / POWERAPPS |
| **EF Core Tools** (optionnel) | `dotnet tool install --global dotnet-ef` |

---

## ⚙️ Configuration (appsettings.json)

> ⚠️ **Le fichier `appsettings.json` n'est PAS versionné** (il contient les chaînes de connexion SQL et est exclu par `.gitignore`). Vous devez le créer localement.

1. Copiez le modèle :

   ```bash
   # Windows (PowerShell)
   Copy-Item appsettings.example.json appsettings.json

   # Linux / macOS
   cp appsettings.example.json appsettings.json
   ```

2. Renseignez vos chaînes de connexion réelles dans `appsettings.json` :

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=SERVEUR\\INSTANCE;Initial Catalog=NOVOPHARMA;User ID=...;Password=...;TrustServerCertificate=True",
       "ReclamationConnection": "...POWERAPPS...",
       "MedsourceConnection": "...MEDSOURCE..."
     }
   }
   ```

> 💡 En développement, préférez **.NET User Secrets** pour ne pas stocker de mot de passe sur disque :
> ```bash
> dotnet user-secrets init
> dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
> ```
> En production, injectez les chaînes via des **variables d'environnement** (`ConnectionStrings__DefaultConnection`).

---

## 🚀 Installation & lancement

```bash
# Cloner le dépôt
git clone https://github.com/chernien/novopharma-backend.git
cd novopharma-backend

# Restaurer les dépendances
dotnet restore

# Créer et configurer appsettings.json (voir section Configuration)

# Lancer en développement
dotnet run
```

L'API démarre par défaut sur `https://localhost:7156` (voir `Properties/launchSettings.json`).

---

## 📖 Documentation API (Swagger)

En mode **Development**, l'interface Swagger est disponible sur :

```
https://localhost:7156/swagger
```

Elle liste et permet de tester l'ensemble des endpoints.

---

## 🔌 Principaux endpoints

| Domaine | Exemples |
|---------|----------|
| **Authentification** | `POST /api/MsAClient/loginComm` |
| **Visites terrain** | `POST /api/MsAClient/check-in` · `POST /api/MsAClient/check-out` |
| **Pharmacies / clients** | `GET /api/MsAClient` · `GET /api/MsAClient/auth` |
| **Articles** | `GET /api/MbArticle` · `GET /api/MbArticle/all-articles` · `GET /api/MbArticle/article-by-barcode/{barcode}` |
| **Marques** | `GET /api/MBAArticle/all-marques` |
| **Commandes** | `POST /api/MsAClient/commandeComm` · `POST /api/MsAClient/commandeMultiple` |
| **En-têtes & factures** | `GET /api/MsAEntt/facture` · `GET /api/MsAEntt/commande` (mois / trimestre) |
| **Cadeaux** | `GET /api/gift/gifts-by-dermo/{id}` · `POST /api/gift/order-gift` |
| **Réclamations** | `…/api/Reclamation` |

> La liste complète et à jour est disponible via **Swagger**.

---

## 📦 Déploiement

Le projet est configuré pour un hébergement **IIS** ou **Kestrel** (limite de taille de requête levée pour les envois volumineux de commandes).

```bash
# Publier
dotnet publish -c Release -o ./publish
```

Sur le serveur cible, fournissez le fichier `appsettings.json` (ou les variables d'environnement) **hors du dépôt git**.

---

