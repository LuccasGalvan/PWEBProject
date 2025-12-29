using Microsoft.AspNetCore.Identity;
using GestaoLoja.Entity.Enums;

namespace GestaoLoja.Data
{
    public class Inicializacao
    {
        public static async Task CriaDadosIniciais(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
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
        }
    }
}
