using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.DTO.Auth;
using RESTfulAPIPWEB.Entity.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RESTfulAPIPWEB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilizadoresController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UtilizadoresController(
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _config = config;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // Keeps your old route working, registers a CLIENT by default
    [HttpPost("RegistarUser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistarUser([FromBody] RegisterRequest dto)
        => await RegistarComRole(dto, "Cliente");

    // Add this for enunciado (supplier registration)
    [HttpPost("RegistarFornecedor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistarFornecedor([FromBody] RegisterRequest dto)
        => await RegistarComRole(dto, "Fornecedor");

    private async Task<IActionResult> RegistarComRole(RegisterRequest dto, string role)
    {
        var utilizadorExiste = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (utilizadorExiste is not null)
            return BadRequest("Já existe um utilizador com este email.");

        var novoUtilizador = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nome = dto.Nome,
            Apelido = dto.Apelido,
            NIF = dto.NIF,
            Rua = dto.Rua,
            Localidade1 = dto.Localidade1,
            Localidade2 = dto.Localidade2,
            Pais = dto.Pais,
            Fotografia = dto.Fotografia,

            // Enunciado: Cliente/Fornecedor começa Pendente (Admin/Funcionário ativa depois)
            Estado = UserEstado.Pendente
        };

        var resultado = await _userManager.CreateAsync(novoUtilizador, dto.Password);
        if (!resultado.Succeeded)
            return BadRequest("Erro ao criar o utilizador: " +
                             string.Join(", ", resultado.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(novoUtilizador, role);

        var token = GerarToken(novoUtilizador, role);

        return StatusCode(StatusCodes.Status201Created, new
        {
            accesstoken = token,
            tokentype = "bearer",
            utilizadorid = novoUtilizador.Id,
            utilizadornome = novoUtilizador.Nome,
            role
        });
    }

    [HttpPost("LoginUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest dto)
    {
        var email = dto.Email.Trim();
        var utilizadorAtual = await _userManager.FindByEmailAsync(email);
        if (utilizadorAtual is null)
        {
            return Ok(new { errorMessage = "Utilizador não encontrado.", hasError = true });
        }

        var resultado = await _signInManager.PasswordSignInAsync(email, dto.Password, false, lockoutOnFailure: false);
        if (!resultado.Succeeded)
        {
            return Ok(new { errorMessage = "Erro: Login inválido!", hasError = true });
        }

        var userRoles = await _userManager.GetRolesAsync(utilizadorAtual);
        var role = userRoles.FirstOrDefault() ?? "Cliente";

        // Enunciado: Cliente/Fornecedor Pendente não deve autenticar para usar a app.
        if ((role == "Cliente" || role == "Fornecedor") && utilizadorAtual.Estado != UserEstado.Activo)
        {
            return Ok(new
            {
                errorMessage = "Conta pendente de ativação.",
                hasError = true
            });
        }

        var token = GerarToken(utilizadorAtual, role);

        return Ok(new
        {
            accesstoken = token,
            tokentype = "bearer",
            utilizadorid = utilizadorAtual.Id,
            utilizadornome = utilizadorAtual.Nome,
            role
        });
    }

    [HttpGet("userID")]
    [Authorize(Roles = "Cliente,Fornecedor,Admin,Gestor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInformation(string userID)
    {
        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userID);
        if (utilizador == null)
            return NotFound("Utilizador não encontrado.");

        return Ok(CreateUserInfoResponse(utilizador));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Cliente,Fornecedor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInformationById(string id)
    {
        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (utilizador == null)
            return NotFound("Utilizador não encontrado.");

        return Ok(CreateUserInfoResponse(utilizador));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Cliente,Fornecedor,Admin,Gestor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserInformation()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized("Token inválido.");

        var utilizador = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (utilizador == null)
            return NotFound("Utilizador não encontrado.");

        return Ok(CreateUserInfoResponse(utilizador));
    }

    [HttpPut("UpdateUser")]
    [Authorize(Roles = "Cliente,Fornecedor,Admin,Gestor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest dto)
    {
        var utilizadorAtual = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (utilizadorAtual == null)
            return BadRequest("Utilizador não encontrado.");

        utilizadorAtual.Nome = dto.Nome;
        utilizadorAtual.Apelido = dto.Apelido;
        utilizadorAtual.NIF = dto.NIF;
        utilizadorAtual.Rua = dto.Rua;
        utilizadorAtual.Localidade1 = dto.Localidade1;
        utilizadorAtual.Localidade2 = dto.Localidade2;
        utilizadorAtual.Pais = dto.Pais;
        utilizadorAtual.Fotografia = dto.Fotografia;

        if (!string.IsNullOrWhiteSpace(dto.Password) || !string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("As senhas não coincidem.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(utilizadorAtual);
            var resultadoReset = await _userManager.ResetPasswordAsync(utilizadorAtual, token, dto.Password!);
            if (!resultadoReset.Succeeded)
                return BadRequest("Erro ao atualizar a senha: " +
                                  string.Join(", ", resultadoReset.Errors.Select(e => e.Description)));
        }

        var resultado = await _userManager.UpdateAsync(utilizadorAtual);
        if (!resultado.Succeeded)
            return BadRequest("Erro ao atualizar o utilizador: " +
                             string.Join(", ", resultado.Errors.Select(e => e.Description)));

        return Ok("Utilizador atualizado com sucesso.");
    }

    private string GerarToken(ApplicationUser utilizador, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, utilizador.Email ?? ""),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, utilizador.Id)
        };

        var token = new JwtSecurityToken(
            issuer: _config["JWT:Issuer"],
            audience: _config["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static object CreateUserInfoResponse(ApplicationUser utilizador)
        => new
        {
            utilizador.Id,
            utilizador.Email,
            utilizador.Nome,
            utilizador.Apelido,
            utilizador.NIF,
            utilizador.Estado
        };
}
