using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Studying_With_Future.Data;
using Studying_With_Future.DTOs.Auth;

namespace Studying_With_Future.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO loginDTO)
        {
            Console.WriteLine($"=== TENTATIVA DE LOGIN ===");
            Console.WriteLine($"Email: {loginDTO.Email}");
            Console.WriteLine($"Senha: {loginDTO.Senha}");

            // Buscar usuário por email (incluindo telas permitidas)
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioTelas)
                .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            Console.WriteLine($"Usuário encontrado: {usuario != null}");

            if (usuario == null)
            {
                Console.WriteLine("❌ Usuário não encontrado no banco");
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            Console.WriteLine($"ID do usuário: {usuario.Id}");
            Console.WriteLine($"Nome: {usuario.Nome}");
            Console.WriteLine($"Email no banco: {usuario.Email}");
            Console.WriteLine($"Senha no banco: {usuario.Senha}");
            Console.WriteLine($"Ativo: {usuario.Ativo}");
            Console.WriteLine($"TipoUsuario: {usuario.GetType().Name}");

            // Verificação detalhada da senha
            Console.WriteLine($"=== VERIFICAÇÃO DE SENHA ===");
            Console.WriteLine($"Senha recebida: {loginDTO.Senha}");
            Console.WriteLine($"Hash no banco: {usuario.Senha}");

            var passwordValid = VerifyPassword(loginDTO.Senha, usuario.Senha);
            Console.WriteLine($"Resultado da verificação: {passwordValid}");

            if (!passwordValid)
            {
                Console.WriteLine("❌ Senha inválida");
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            if (!usuario.Ativo)
            {
                Console.WriteLine("❌ Usuário inativo");
                return Unauthorized(new { message = "Usuário inativo" });
            }

            Console.WriteLine("✅ Login bem-sucedido!");
            // Resto do código...

            // Gerar tokens
            var token = GenerateJwtToken(usuario);
            var refreshToken = GenerateRefreshToken();

            // Salvar refresh token no usuário (você pode criar uma tabela para refresh tokens se quiser)
            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token válido por 7 dias

            await _context.SaveChangesAsync();

            var response = new LoginResponseDTO
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiraEm = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                Usuario = new UserInfoDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    TipoUsuario = usuario.GetType().Name,
                    Funcao = usuario.ObterFuncao(),
                    TelasPermitidasIds = usuario.UsuarioTelas?.Select(ut => ut.TelaId).ToList() ?? new List<int>()
                }
            };

            return Ok(response);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                Console.WriteLine($"=== MÉTODO VerifyPassword ===");
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"HashedPassword: {hashedPassword}");

                // Verificar se o hash parece ser um BCrypt válido
                if (!hashedPassword.StartsWith("$2a$") && !hashedPassword.StartsWith("$2b$") && !hashedPassword.StartsWith("$2y$"))
                {
                    Console.WriteLine("❌ Hash não parece ser BCrypt válido");
                    return false;
                }

                var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                Console.WriteLine($"BCrypt.Verify retornou: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO na verificação: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDTO>> RefreshToken(RefreshTokenDTO refreshTokenDTO)
        {
            var principal = GetPrincipalFromExpiredToken(refreshTokenDTO.RefreshToken);
            if (principal == null)
            {
                return Unauthorized(new { message = "Refresh token inválido" });
            }

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioTelas)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null || usuario.RefreshToken != refreshTokenDTO.RefreshToken || usuario.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Refresh token inválido ou expirado" });
            }

            var newToken = GenerateJwtToken(usuario);
            var newRefreshToken = GenerateRefreshToken();

            usuario.RefreshToken = newRefreshToken;
            usuario.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            var response = new LoginResponseDTO
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiraEm = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                Usuario = new UserInfoDTO
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    TipoUsuario = usuario.GetType().Name,
                    Funcao = usuario.ObterFuncao(),
                    TelasPermitidasIds = usuario.UsuarioTelas?.Select(ut => ut.TelaId).ToList() ?? new List<int>()
                }
            };

            return Ok(response);
        }

        private string GenerateJwtToken(Studying_With_Future.Models.Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.GetType().Name),
                new Claim("TipoUsuario", usuario.GetType().Name),
                new Claim("Funcao", usuario.ObterFuncao())
            };

            // Adicionar claims das telas permitidas
            var telasIds = usuario.UsuarioTelas?.Select(ut => ut.TelaId.ToString()).ToList() ?? new List<string>();
            claims = claims.Concat(telasIds.Select(t => new Claim("Tela", t))).ToArray();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido");

            return principal;
        }
    }
}