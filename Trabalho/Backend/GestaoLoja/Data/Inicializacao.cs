using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GestaoLoja.Entity;
using GestaoLoja.Entity.Enums;

namespace GestaoLoja.Data
{
    public class Inicializacao
    {
        public static async Task CriaDadosIniciais(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await context.Database.MigrateAsync();

            // Roles
            string[] roles = { "Admin", "Gestor", "Cliente", "Fornecedor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Default Admin
            var adminEmail = "admin@localhost.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador",
                    Apelido = "Local",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Estado = UserEstado.Activo
                };

                var create = await userManager.CreateAsync(adminUser, "Is3C..0");
                if (create.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Default Gestor
            var gestorEmail = "gestor@localhost.com";
            var gestorUser = await userManager.FindByEmailAsync(gestorEmail);
            if (gestorUser == null)
            {
                gestorUser = new ApplicationUser
                {
                    UserName = gestorEmail,
                    Email = gestorEmail,
                    Nome = "Gestor",
                    Apelido = "Funcionario",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Estado = UserEstado.Activo
                };

                var create = await userManager.CreateAsync(gestorUser, "Aa.123456");
                if (create.Succeeded)
                    await userManager.AddToRoleAsync(gestorUser, "Gestor");
            }

            // Default Fornecedor
            var fornecedorEmail = "fornecedor@localhost.com";
            var fornecedorUser = await userManager.FindByEmailAsync(fornecedorEmail);
            if (fornecedorUser == null)
            {
                fornecedorUser = new ApplicationUser
                {
                    UserName = fornecedorEmail,
                    Email = fornecedorEmail,
                    Nome = "Fornecedor",
                    Apelido = "Demo",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Estado = UserEstado.Activo
                };
                var create = await userManager.CreateAsync(fornecedorUser, "Aa.123456");
                if (create.Succeeded)
                    await userManager.AddToRoleAsync(fornecedorUser, "Fornecedor");
            }

            // Default Cliente
            var clienteEmail = "cliente@localhost.com";
            var clienteUser = await userManager.FindByEmailAsync(clienteEmail);
            if (clienteUser == null)
            {
                clienteUser = new ApplicationUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    Nome = "Cliente",
                    Apelido = "Demo",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    Estado = UserEstado.Activo
                };

                var create = await userManager.CreateAsync(clienteUser, "Aa.123456");
                if (create.Succeeded)
                    await userManager.AddToRoleAsync(clienteUser, "Cliente");
            }

            var categoriasDesejadas = new (string Nome, string? ParentNome, int Ordem)[]
            {
                ("Eletrónicos", null, 1),
                ("Computadores", "Eletrónicos", 1),
                ("Portáteis", "Computadores", 2),
                ("Telemóveis", "Eletrónicos", 2),
                ("Acessórios", "Eletrónicos", 3),
                ("Moda", null, 2),
                ("Moda Homem", "Moda", 1),
                ("Moda Mulher", "Moda", 2),
                ("Moda Calçado", "Moda", 3),
                ("Casa", null, 3)
            };

            var categoriasExistentes = await context.Categorias.ToListAsync();
            var categoriasPorNome = categoriasExistentes
                .GroupBy(c => c.Nome)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var (nome, parentNome, ordem) in categoriasDesejadas)
            {
                if (categoriasPorNome.ContainsKey(nome))
                    continue;

                if (parentNome is null)
                {
                    var categoria = new Categoria { Nome = nome, Ordem = ordem };
                    context.Categorias.Add(categoria);
                    categoriasPorNome[nome] = categoria;
                    continue;
                }

                if (!categoriasPorNome.TryGetValue(parentNome, out var parent))
                {
                    parent = new Categoria { Nome = parentNome, Ordem = 1 };
                    context.Categorias.Add(parent);
                    categoriasPorNome[parentNome] = parent;
                }

                var child = new Categoria { Nome = nome, Ordem = ordem, Parent = parent };
                context.Categorias.Add(child);
                categoriasPorNome[nome] = child;
            }

            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync();

            if (!await context.ModosEntrega.AnyAsync())
            {
                context.ModosEntrega.AddRange(
                    new ModoEntrega { Nome = "Entrega Local", Detalhe = "Entrega na mesma cidade em 24h." },
                    new ModoEntrega { Nome = "Envio Expresso", Detalhe = "Entrega nacional em 48h." },
                    new ModoEntrega { Nome = "Levantamento em Loja", Detalhe = "Disponível para recolha." });
                await context.SaveChangesAsync();
            }

            if (!await context.Produtos.AnyAsync())
            {
                var fornecedorId = fornecedorUser?.Id ?? throw new InvalidOperationException("Fornecedor não encontrado.");
                var categorias = await context.Categorias.AsNoTracking().ToListAsync();
                var modos = await context.ModosEntrega.AsNoTracking().ToListAsync();

                var categoriaPorNome = categorias.ToDictionary(c => c.Nome, c => c);
                var entregaLocal = modos.First(m => m.Nome == "Entrega Local");
                var envioExpresso = modos.First(m => m.Nome == "Envio Expresso");
                var levantamento = modos.First(m => m.Nome == "Levantamento em Loja");

                byte[] imagemPadrao = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=");

                Produto CriarProduto(
                    string nome,
                    string detalhe,
                    decimal precoBase,
                    decimal? margem,
                    ProdutoEstado estado,
                    int stock,
                    bool paraVenda,
                    Categoria categoria,
                    ModoEntrega modoEntrega,
                    bool promocao = false,
                    bool maisVendido = false,
                    string? origem = null)
                {
                    var precoFinal = margem.HasValue
                        ? Math.Round(precoBase * (1 + margem.Value / 100m), 2)
                        : precoBase;

                    return new Produto
                    {
                        Nome = nome,
                        Detalhe = detalhe,
                        PrecoBase = precoBase,
                        MargemPercentual = margem,
                        PrecoFinal = precoFinal,
                        Estado = estado,
                        FornecedorId = fornecedorId,
                        EmStock = stock,
                        ParaVenda = paraVenda,
                        CategoriaId = categoria.Id,
                        ModoEntregaId = modoEntrega.Id,
                        Promocao = promocao,
                        MaisVendido = maisVendido,
                        Origem = origem,
                        Imagem = imagemPadrao,
                        UrlImagem = $"{nome.Replace(' ', '_')}.png"
                    };
                }

                var produtos = new List<Produto>
                {
                    CriarProduto("Notebook Studio", "Portátil 14\" para trabalho móvel.", 899.90m, 12m, ProdutoEstado.Activo, 15, true, categoriaPorNome["Portáteis"], envioExpresso, maisVendido: true, origem: "Portugal"),
                    CriarProduto("Desktop Pro", "Torre para produtividade diária.", 699.00m, 10m, ProdutoEstado.Activo, 8, true, categoriaPorNome["Computadores"], levantamento, origem: "Alemanha"),
                    CriarProduto("Smartphone Aurora", "Ecrã OLED 6.5\" e 5G.", 599.00m, 15m, ProdutoEstado.Activo, 25, true, categoriaPorNome["Telemóveis"], entregaLocal, promocao: true, origem: "China"),
                    CriarProduto("Kit Cabos USB", "Conjunto de cabos rápidos.", 19.90m, 20m, ProdutoEstado.Activo, 80, true, categoriaPorNome["Eletrónicos"], entregaLocal, origem: "Portugal"),
                    CriarProduto("Capa Protect", "Capa resistente com proteção extra.", 24.90m, 18m, ProdutoEstado.Activo, 60, true, categoriaPorNome["Acessórios"], entregaLocal, origem: "Portugal"),
                    CriarProduto("Casaco Urbano", "Casaco impermeável para homem.", 79.90m, 18m, ProdutoEstado.Activo, 20, true, categoriaPorNome["Moda Homem"], envioExpresso, origem: "Itália"),
                    CriarProduto("Vestido Sol", "Vestido leve para verão.", 64.50m, 22m, ProdutoEstado.Activo, 12, true, categoriaPorNome["Moda Mulher"], envioExpresso, origem: "Espanha"),
                    CriarProduto("Sapatilhas City", "Calçado confortável para o dia a dia.", 89.00m, 18m, ProdutoEstado.Activo, 18, true, categoriaPorNome["Moda Calçado"], envioExpresso, origem: "Portugal"),
                    CriarProduto("Organizador Casa", "Caixas modulares para arrumação.", 29.90m, 15m, ProdutoEstado.Activo, 30, true, categoriaPorNome["Casa"], entregaLocal, origem: "Portugal"),
                    CriarProduto("Moda Lookbook", "Catálogo de tendências da estação.", 0m, null, ProdutoEstado.Activo, 999, false, categoriaPorNome["Moda"], levantamento, origem: "Portugal"),

                    CriarProduto("Tablet Sketch", "Tablet para desenho digital.", 449.00m, 14m, ProdutoEstado.Activo, 10, true, categoriaPorNome["Eletrónicos"], envioExpresso, origem: "Coreia"),
                    CriarProduto("Auriculares Pulse", "Som estéreo e cancelamento de ruído.", 129.90m, 16m, ProdutoEstado.Activo, 40, true, categoriaPorNome["Eletrónicos"], entregaLocal, promocao: true, origem: "China"),
                    CriarProduto("Monitor Prime", "Monitor 27\" para produtividade.", 229.00m, 13m, ProdutoEstado.Activo, 14, true, categoriaPorNome["Computadores"], envioExpresso, origem: "Taiwan"),
                    CriarProduto("Pré-venda Console", "Produto de exposição, sem vendas directas.", 399.00m, 8m, ProdutoEstado.Pendente, 0, false, categoriaPorNome["Eletrónicos"], levantamento, origem: "Japão")
                };

                context.Produtos.AddRange(produtos);
                await context.SaveChangesAsync();
            }

            if (!await context.Encomendas.AnyAsync())
            {
                var clienteId = clienteUser?.Id ?? throw new InvalidOperationException("Cliente não encontrado.");
                var produtos = await context.Produtos.AsNoTracking().ToListAsync();
                var produtosPorNome = produtos.ToDictionary(p => p.Nome ?? string.Empty, p => p);

                Encomenda CriarEncomenda(EncomendaEstado estado, params (Produto produto, int quantidade)[] linhas)
                {
                    var encomenda = new Encomenda
                    {
                        ClienteId = clienteId,
                        Estado = estado
                    };

                    foreach (var (produto, quantidade) in linhas)
                    {
                        var preco = produto.PrecoFinal ?? produto.PrecoBase;
                        var subtotal = preco * quantidade;

                        encomenda.Itens.Add(new EncomendaItem
                        {
                            ProdutoId = produto.Id,
                            Quantidade = quantidade,
                            PrecoUnitario = preco,
                            Subtotal = subtotal
                        });

                        encomenda.Total += subtotal;
                    }

                    return encomenda;
                }

                var encomendas = new List<Encomenda>
                {
                    CriarEncomenda(EncomendaEstado.Paga,
                        (produtosPorNome["Notebook Studio"], 1),
                        (produtosPorNome["Smartphone Aurora"], 2)),
                    CriarEncomenda(EncomendaEstado.Confirmada,
                        (produtosPorNome["Casaco Urbano"], 1),
                        (produtosPorNome["Organizador Casa"], 3)),
                    CriarEncomenda(EncomendaEstado.Expedida,
                        (produtosPorNome["Desktop Pro"], 1),
                        (produtosPorNome["Sapatilhas City"], 1),
                        (produtosPorNome["Auriculares Pulse"], 2))
                };

                context.Encomendas.AddRange(encomendas);
                await context.SaveChangesAsync();
            }
        }
    }
}
