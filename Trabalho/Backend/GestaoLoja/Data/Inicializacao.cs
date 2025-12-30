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
            if (!await ShouldSkipMigrationsAsync(context))
            {
                await context.Database.MigrateAsync();
            }
            var imagensRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "imgs"));

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

            var categoriaImagens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Eletrónicos"] = "ChipsdeBatataDoce.jpg",
                ["Computadores"] = "CrackersdeParmesaoeAlecrim.jpg",
                ["Portáteis"] = "CrackersdeSementes.jpg",
                ["Telemóveis"] = "BiscoitosIntegraisdeCacau.jpeg",
                ["Acessórios"] = "BiscoitosdeAmendoaseMel.jpg",
                ["Moda"] = "PetiscosGourmet.jpg",
                ["Moda Homem"] = "SnacksdeGraodeBicoAssado.jpg",
                ["Moda Mulher"] = "PetiscosDoces.jpg",
                ["Moda Calçado"] = "PetiscosSaudaveis.jpg",
                ["Casa"] = "Pipoca-gourmet.jpg"
            };

            var categoriasComImagem = await context.Categorias.ToListAsync();
            var categoriaAtualizada = false;
            foreach (var categoria in categoriasComImagem)
            {
                if (categoria.Imagem is { Length: > 0 })
                    continue;

                if (!categoriaImagens.TryGetValue(categoria.Nome, out var fileName))
                    continue;

                if (TryLoadImageBytes(imagensRoot, fileName) is { Length: > 0 } bytes)
                {
                    categoria.Imagem = bytes;
                    categoria.UrlImagem = fileName;
                    categoriaAtualizada = true;
                }
            }

            if (categoriaAtualizada)
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
                if (fornecedorUser is null)
                    return;

                var fornecedorId = fornecedorUser.Id;
                var categorias = await context.Categorias.AsNoTracking().ToListAsync();
                var modos = await context.ModosEntrega.AsNoTracking().ToListAsync();

                var categoriaPorNome = categorias.ToDictionary(c => c.Nome, c => c, StringComparer.OrdinalIgnoreCase);
                var entregaLocal = modos.FirstOrDefault(m => m.Nome == "Entrega Local");
                var envioExpresso = modos.FirstOrDefault(m => m.Nome == "Envio Expresso");
                var levantamento = modos.FirstOrDefault(m => m.Nome == "Levantamento em Loja");

                if (entregaLocal is null || envioExpresso is null || levantamento is null)
                    return;

                bool TryGetCategoria(string nome, out Categoria categoria)
                    => categoriaPorNome.TryGetValue(nome, out categoria!);

                var categoriasNecessarias = new[]
                {
                    "Portáteis",
                    "Computadores",
                    "Telemóveis",
                    "Eletrónicos",
                    "Acessórios",
                    "Moda Homem",
                    "Moda Mulher",
                    "Moda Calçado",
                    "Casa",
                    "Moda"
                };

                if (categoriasNecessarias.Any(nome => !TryGetCategoria(nome, out _)))
                    return;

                var produtoImagens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Notebook Studio"] = "BarradeProteinadeChocolateeCoco.jpg",
                    ["Desktop Pro"] = "BarrasdeGranola.png",
                    ["Smartphone Aurora"] = "BiscoitosIntegraisdeCacau.jpeg",
                    ["Kit Cabos USB"] = "ChipsdeBatataDoce.jpg",
                    ["Capa Protect"] = "CrackersdeParmesaoeAlecrim.jpg",
                    ["Casaco Urbano"] = "PetiscosSaudaveis.jpg",
                    ["Vestido Sol"] = "PetiscosDoces.jpg",
                    ["Sapatilhas City"] = "PetiscosGourmet.jpg",
                    ["Organizador Casa"] = "Pipoca-gourmet.jpg",
                    ["Moda Lookbook"] = "FrutasSecas.jpg",
                    ["Tablet Sketch"] = "MixNuts.jpeg",
                    ["Auriculares Pulse"] = "SnacksdeGraodeBicoAssado.jpg",
                    ["Monitor Prime"] = "QueijoBriecomGeleiadeFrutasVermelhas.jpg",
                    ["Pré-venda Console"] = "TamarasRecheadascomAmendoas.jpg"
                };

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
                        Imagem = GetProdutoImagem(nome, produtoImagens, imagemPadrao, imagensRoot),
                        UrlImagem = GetProdutoImagemUrl(nome, produtoImagens)
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
            else
            {
                var produtoImagens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Notebook Studio"] = "BarradeProteinadeChocolateeCoco.jpg",
                    ["Desktop Pro"] = "BarrasdeGranola.png",
                    ["Smartphone Aurora"] = "BiscoitosIntegraisdeCacau.jpeg",
                    ["Kit Cabos USB"] = "ChipsdeBatataDoce.jpg",
                    ["Capa Protect"] = "CrackersdeParmesaoeAlecrim.jpg",
                    ["Casaco Urbano"] = "PetiscosSaudaveis.jpg",
                    ["Vestido Sol"] = "PetiscosDoces.jpg",
                    ["Sapatilhas City"] = "PetiscosGourmet.jpg",
                    ["Organizador Casa"] = "Pipoca-gourmet.jpg",
                    ["Moda Lookbook"] = "FrutasSecas.jpg",
                    ["Tablet Sketch"] = "MixNuts.jpeg",
                    ["Auriculares Pulse"] = "SnacksdeGraodeBicoAssado.jpg",
                    ["Monitor Prime"] = "QueijoBriecomGeleiadeFrutasVermelhas.jpg",
                    ["Pré-venda Console"] = "TamarasRecheadascomAmendoas.jpg"
                };

                var produtosExistentes = await context.Produtos.ToListAsync();
                var produtosAtualizados = false;
                foreach (var produto in produtosExistentes)
                {
                    if (produto.Imagem is { Length: > 0 })
                        continue;

                    if (!produtoImagens.TryGetValue(produto.Nome, out var fileName))
                        continue;

                    if (TryLoadImageBytes(imagensRoot, fileName) is { Length: > 0 } bytes)
                    {
                        produto.Imagem = bytes;
                        produto.UrlImagem = fileName;
                        produtosAtualizados = true;
                    }
                }

                if (produtosAtualizados)
                    await context.SaveChangesAsync();
            }

            if (!await context.Encomendas.AnyAsync())
            {
                if (clienteUser is null)
                    return;

                var clienteId = clienteUser.Id;
                var produtos = await context.Produtos.AsNoTracking().ToListAsync();
                var produtosPorNome = produtos
                    .Where(p => !string.IsNullOrWhiteSpace(p.Nome))
                    .ToDictionary(p => p.Nome!, p => p, StringComparer.OrdinalIgnoreCase);

                bool TryGetProduto(string nome, out Produto produto)
                    => produtosPorNome.TryGetValue(nome, out produto!);

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

                var encomendas = new List<Encomenda>();

                if (TryGetProduto("Notebook Studio", out var notebook)
                    && TryGetProduto("Smartphone Aurora", out var smartphone))
                {
                    encomendas.Add(CriarEncomenda(EncomendaEstado.Paga,
                        (notebook, 1),
                        (smartphone, 2)));
                }

                if (TryGetProduto("Casaco Urbano", out var casaco)
                    && TryGetProduto("Organizador Casa", out var organizador))
                {
                    encomendas.Add(CriarEncomenda(EncomendaEstado.Confirmada,
                        (casaco, 1),
                        (organizador, 3)));
                }

                if (TryGetProduto("Desktop Pro", out var desktop)
                    && TryGetProduto("Sapatilhas City", out var sapatilhas)
                    && TryGetProduto("Auriculares Pulse", out var auriculares))
                {
                    encomendas.Add(CriarEncomenda(EncomendaEstado.Expedida,
                        (desktop, 1),
                        (sapatilhas, 1),
                        (auriculares, 2)));
                }

                if (encomendas.Count > 0)
                {
                    context.Encomendas.AddRange(encomendas);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static byte[]? TryLoadImageBytes(string imagensRoot, string fileName)
        {
            var path = Path.Combine(imagensRoot, fileName);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        private static async Task<bool> ShouldSkipMigrationsAsync(ApplicationDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var wasClosed = connection.State == System.Data.ConnectionState.Closed;
            if (wasClosed)
            {
                await connection.OpenAsync();
            }

            try
            {
                var aspNetRolesExists = await ExecuteScalarAsync(connection,
                    "SELECT COUNT(*) FROM sys.tables WHERE name = 'AspNetRoles'");
                var migrationsTableExists = await ExecuteScalarAsync(connection,
                    "SELECT COUNT(*) FROM sys.tables WHERE name = '__EFMigrationsHistory'");

                if (aspNetRolesExists == 0)
                {
                    return false;
                }

                if (migrationsTableExists == 0)
                {
                    return true;
                }

                var migrationsCount = await ExecuteScalarAsync(connection,
                    "SELECT COUNT(*) FROM [__EFMigrationsHistory]");

                return migrationsCount == 0;
            }
            finally
            {
                if (wasClosed)
                {
                    await connection.CloseAsync();
                }
            }
        }

        private static async Task<int> ExecuteScalarAsync(System.Data.Common.DbConnection connection, string sql)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        private static byte[] GetProdutoImagem(
            string nome,
            IReadOnlyDictionary<string, string> produtoImagens,
            byte[] imagemPadrao,
            string imagensRoot)
        {
            if (!produtoImagens.TryGetValue(nome, out var fileName))
                return imagemPadrao;

            return TryLoadImageBytes(imagensRoot, fileName) ?? imagemPadrao;
        }

        private static string? GetProdutoImagemUrl(
            string nome,
            IReadOnlyDictionary<string, string> produtoImagens)
        {
            return produtoImagens.TryGetValue(nome, out var fileName)
                ? fileName
                : $"{nome.Replace(' ', '_')}.png";
        }
    }
}
