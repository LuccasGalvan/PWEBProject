using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWEB.Entity;
using System.Collections.Generic;

namespace RESTfulAPIPWEB.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; } = default!;
        public DbSet<Produto> Produtos { get; set; } = default!;
        public DbSet<ModoEntrega> ModosEntrega { get; set; } = default!;
        public DbSet<Favoritos> Favoritos { get; set; } = default!;
        public DbSet<CarrinhoCompras> CarrinhoCompras { get; set; } = default!;
        public DbSet<Encomenda> Encomendas { get; set; } = default!;
        public DbSet<EncomendaItem> EncomendaItens { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Categoria>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nome = "Eletrónicos", Ordem = 1 },
                new Categoria { Id = 2, Nome = "Computadores", Ordem = 1, ParentId = 1 },
                new Categoria { Id = 3, Nome = "Portáteis", Ordem = 1, ParentId = 2 },
                new Categoria { Id = 4, Nome = "Telemóveis", Ordem = 2, ParentId = 1 },
                new Categoria { Id = 5, Nome = "Acessórios", Ordem = 3, ParentId = 1 }
            );

            // --- Produto ownership: don't allow deleting a supplier to cascade-delete products
            builder.Entity<Produto>()
                .HasOne(p => p.Fornecedor)
                .WithMany()
                .HasForeignKey(p => p.FornecedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- Cart: FK + avoid duplicate rows per (UserId, ProdutoId)
            builder.Entity<CarrinhoCompras>()
                .HasOne(c => c.Produto)
                .WithMany()
                .HasForeignKey(c => c.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CarrinhoCompras>()
                .HasIndex(c => new { c.UserId, c.ProdutoId })
                .IsUnique();

            // --- Favorites: FK + avoid duplicate rows per (UserId, ProdutoId)
            builder.Entity<Favoritos>()
                .HasOne(f => f.Produto)
                .WithMany()
                .HasForeignKey(f => f.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favoritos>()
                .HasIndex(f => new { f.UserId, f.ProdutoId })
                .IsUnique();

            // --- Orders: Encomenda -> Items (explicit relationship; convention would also work)
            builder.Entity<EncomendaItem>()
                .HasOne(i => i.Encomenda)
                .WithMany(e => e.Itens)
                .HasForeignKey(i => i.EncomendaId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Order item -> Produto (keep items even if product gets deleted later)
            builder.Entity<EncomendaItem>()
                .HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
