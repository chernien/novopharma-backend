using Microsoft.EntityFrameworkCore;
using WebApiTest.Models;

namespace WebApiTest.Data
{
    public partial class MEDSOURCEContext : DbContext
    {
        public MEDSOURCEContext()
        {
        }

        public MEDSOURCEContext(DbContextOptions<MEDSOURCEContext> options)
            : base(options)
        {
        }
        public virtual DbSet<F_Article> F_Article { get; set; }
        public virtual DbSet<Msaarticlemed> MsSArticle { get; set; }
        public virtual DbSet<MbArticle> MbArticle { get; set; }
        public virtual DbSet<MsAClients> MsAClients { get; set; }
        public virtual DbSet<MBAArticle> MBAArticle { get; set; }
        public virtual DbSet<GiftAssignment> GiftAssignments { get; set; }
        public virtual DbSet<GiftOrder> GiftOrders { get; set; }
        public virtual DbSet<LoginRequest2> Authentication { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginRequest2>(entity =>
            {
                entity.ToTable("Authentication");
            });
            modelBuilder.Entity<GiftAssignment>()
                .ToTable("GiftAssignment"); // Nom de la table en base de données
            modelBuilder.Entity<MBAArticle>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("MB_A_ARTICLE");

                entity.Property(e => e.ArDesign)
                    .HasMaxLength(69)
                    .IsUnicode(false)
                    .HasColumnName("AR_Design");

                entity.Property(e => e.ArPrixAch)
                    .HasColumnType("numeric(24, 6)")
                    .HasColumnName("AR_PrixAch");


                entity.Property(e => e.ArPrixVen)
                    .HasColumnType("numeric(24, 6)")
                    .HasColumnName("AR_PrixVen");

                entity.Property(e => e.Nouveaute)
                    .HasMaxLength(17)
                    .IsUnicode(false)
                    .HasColumnName("Nouveauté");

                entity.Property(e => e.Recommande)
                .HasColumnName("Recommandé");

                entity.Property(e => e.ArPunet)
                    .HasColumnType("numeric(24, 6)")
                    .HasColumnName("AR_PUNet");

                entity.Property(e => e.ArRef)
                    .IsRequired()
                    .HasMaxLength(19)
                    .IsUnicode(false)
                    .HasColumnName("AR_Ref");

                entity.Property(e => e.AR_CodeBarre)
                   .HasMaxLength(19)
                   .IsUnicode(false)
                   .HasColumnName("AR_Codebarre");

                entity.Property(e => e.FaCodeFamille)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsUnicode(false)
                    .HasColumnName("FA_CodeFamille");

                entity.Property(e => e.Marque)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });


            modelBuilder.Entity<MsAClients>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("MS_A_CLIENTS");

                entity.Property(e => e.CbMarq).HasColumnName("cbMarq");

                //entity.Property(e => e.Collaborateur)
                //    .HasMaxLength(150)
                //    .IsUnicode(false)
                //    .HasColumnName("COLLABORATEUR");

                entity.Property(e => e.CtAdresse)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("CT_Adresse");

                entity.Property(e => e.CtClassement)
                    .HasMaxLength(17)
                    .IsUnicode(false)
                    .HasColumnName("CT_Classement");

                entity.Property(e => e.CtCodePostal)
                    .HasMaxLength(9)
                    .IsUnicode(false)
                    .HasColumnName("CT_CodePostal");

                entity.Property(e => e.CtCodeRegion)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("CT_CodeRegion");

                entity.Property(e => e.CtComplement)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("CT_Complement");

                entity.Property(e => e.CtContact)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("CT_Contact");

                entity.Property(e => e.CtEmail)
                    .HasMaxLength(69)
                    .IsUnicode(false)
                    .HasColumnName("CT_EMail");

                entity.Property(e => e.CtIdentifiant)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("CT_Identifiant");

                entity.Property(e => e.CtIntitule)
                    .HasMaxLength(69)
                    .IsUnicode(false)
                    .HasColumnName("CT_Intitule");

                entity.Property(e => e.CtNum)
                    .IsRequired()
                    .HasMaxLength(17)
                    .IsUnicode(false)
                    .HasColumnName("CT_Num");

                entity.Property(e => e.CtPays)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("CT_Pays");

                entity.Property(e => e.CtQualite)
                    .HasMaxLength(17)
                    .IsUnicode(false)
                    .HasColumnName("CT_Qualite");

                entity.Property(e => e.CtSiret)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("CT_Siret");

                entity.Property(e => e.CtSite)
                    .HasMaxLength(69)
                    .IsUnicode(false)
                    .HasColumnName("CT_Site");

                entity.Property(e => e.CtSommeil).HasColumnName("CT_Sommeil");

                entity.Property(e => e.CtTelephone)
                    .HasMaxLength(21)
                    .IsUnicode(false)
                    .HasColumnName("CT_Telephone");

                entity.Property(e => e.CtType).HasColumnName("CT_Type");

                entity.Property(e => e.CtVille)
                    .HasMaxLength(35)
                    .IsUnicode(false)
                    .HasColumnName("CT_Ville");

                entity.Property(e => e.NCatTarif).HasColumnName("N_CatTarif");

                //entity.Property(e => e.Prenomcoll)
                //    .HasMaxLength(150)
                //    .IsUnicode(false)
                //    .HasColumnName("PRENOMCOLL");
            });

            modelBuilder.Entity<Msaarticlemed>(entity =>
            {
                // 🔹 Associer à la vue MS_A_ARTICLE
                entity.ToView("MS_A_ARTICLE");

                // 🔹 Définir la clé primaire
                entity.HasNoKey();

                // 🔹 Mapping explicite des noms des colonnes
                entity.Property(a => a.ArRef).HasColumnName("AR_Ref");
                entity.Property(a => a.ArDesign).HasColumnName("AR_Design");
                entity.Property(a => a.Marque).HasColumnName("Marque");
                entity.Property(a => a.ArPrixAch).HasColumnName("AR_PrixAch");
                entity.Property(a => a.ArPrixVen).HasColumnName("AR_PrixVen");
                entity.Property(a => a.ArPunet).HasColumnName("AR_PUNet");
                entity.Property(a => a.FaCodeFamille).HasColumnName("FA_CodeFamille");
                entity.Property(a => a.ArPublie).HasColumnName("AR_Publie");
                entity.Property(a => a.Ar_Photo).HasColumnName("AR_Photo");
                entity.Property(a => a.Gl_Text).HasColumnName("GL_Text");
                entity.Property(a => a.Gl_Intitule).HasColumnName("GL_Intitule");
                entity.Property(a => a.Famille).HasColumnName("Famille");
                entity.Property(a => a.Nouveaute).HasColumnName("Nouveauté");
                entity.Property(a => a.AS_QteSto).HasColumnName("AS_QteSto");
                entity.Property(a => a.AR_Sommeil).HasColumnName("AR_Sommeil");
                entity.Property(a => a.Recommande).HasColumnName("Recommandé");
                entity.Property(e => e.AR_CodeBarre).HasMaxLength(19).IsUnicode(false).HasColumnName("AR_Codebarre");
            });
            modelBuilder.Entity<F_Article>(entity =>
            {
                entity.HasKey(e => e.ArRef);
                entity.ToTable("F_ARTICLE");
                entity.Property(e => e.Recommande)
                      .HasColumnName("Recommandé");
                entity.Property(e => e.ArRef)
               .HasColumnName("AR_Ref");

            });
            modelBuilder.Entity<MbArticle>(entity =>
            {
                entity.ToTable("MB_ARTICLE"); // Nom de la table

                entity.HasNoKey();
                entity.Property(e => e.ArRef).HasColumnName("AR_Ref");
                entity.Property(e => e.ArDesign).HasColumnName("AR_Design");
                entity.Property(e => e.Marque).HasColumnName("Marque");
                entity.Property(e => e.CtNum).HasColumnName("CT_Num");
                entity.Property(e => e.CtIntitule).HasColumnName("CT_Intitule");
                entity.Property(e => e.AfRefFourniss).HasColumnName("AF_RefFourniss");
                entity.Property(e => e.DeNo).HasColumnName("DE_No");
                entity.Property(e => e.AsQteMini).HasColumnName("AS_QteMini");
                entity.Property(e => e.AsQteMaxi).HasColumnName("AS_QteMaxi");
                entity.Property(e => e.AsQteSto).HasColumnName("AS_QteSto");
                entity.Property(e => e.AsQteRes).HasColumnName("AS_QteRes");
                entity.Property(e => e.ArPrixAch).HasColumnName("AR_PrixAch");
                entity.Property(e => e.ArPrixVen).HasColumnName("AR_PrixVen");
                entity.Property(e => e.ArPunet).HasColumnName("AR_PUNet");
                entity.Property(e => e.FaCodeFamille).HasColumnName("FA_CodeFamille");
                entity.Property(e => e.ArSommeil).HasColumnName("AR_Sommeil");
                entity.Property(e => e.CgNumPrinc).HasColumnName("CG_NumPrinc"); 
                entity.Property(e => e.TypeStock).HasColumnName("Type Stock");
                entity.Property(e => e.AR_CodeBarre).HasMaxLength(19).IsUnicode(false).HasColumnName("AR_Codebarre");
            });
        }

    }

}