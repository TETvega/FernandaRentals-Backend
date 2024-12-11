using AutoMapper;
using FernandaRentals.Constants;
using FernandaRentals.Database;
using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.Auth;
using FernandaRentals.Dtos.ClientType;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FernandaRentals.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly FernandaRentalsContext _context;
        private readonly ILogger<UserEntity> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            FernandaRentalsContext context,
            ILogger<UserEntity> logger,
            IConfiguration configuration,
            IMapper mapper
            )
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._context = context;
            this._logger = logger;
            this._configuration = configuration;
            this._mapper = mapper;
        }

        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var result = await _signInManager.
                PasswordSignInAsync(dto.Email,
                                    dto.Password,
                                    isPersistent: false,
                                    lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Generación del Token
                var userEntity = await _userManager.FindByEmailAsync(dto.Email);

                var clientTypeEntity = await _context.Clients
                    .Where(c => c.UserId == userEntity.Id) 
                    .Select(c => _context.TypesOfClient
                        .Where(tc => tc.Id == c.ClientTypeId) 
                        .FirstOrDefault()) 
                    .FirstOrDefaultAsync();

                if (clientTypeEntity != null)
                {
                    // Aquí puedes usar el nombre del tipo de cliente
                    Console.WriteLine(clientTypeEntity);
                }

                var clientTypeDto = _mapper.Map<ClientTypeDto>(clientTypeEntity);

                // ClaimList creation
                List<Claim> authClaims = await GetClaims(userEntity);

                var jwtToken = GetToken(authClaims);

                var refreshToken = GenerateRefreshTokenString();

                userEntity.RefreshToken = refreshToken;
                userEntity.RefreshTokenExpire = DateTime.Now
                    .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

                _context.Entry(userEntity);
                await _context.SaveChangesAsync();


                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Inicio de sesion satisfactorio",
                    Data = new LoginResponseDto
                    {
                        Name = userEntity.Name,
                        Email = userEntity.Email,
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken), // convertir token en string
                        TokenExpiration = jwtToken.ValidTo,
                        RefreshToken = refreshToken,
                        ClientType = clientTypeDto
                    }
                };
            }

            return new ResponseDto<LoginResponseDto>
            {
                Status = false,
                StatusCode = 401,
                Message = "Falló el inicio de sesión"
            };
        }

        public async Task<ResponseDto<LoginResponseDto>> RegisterClientAsync(RegisterClientDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new UserEntity
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    Name = dto.ClientName
                };

                // Intento de creación del usuario
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    List<IdentityError> errorList = result.Errors.ToList();  // Listamos los errores
                    string errors = "";

                    foreach (var error in errorList)
                    {
                        errors += error.Description;
                        if (error.Code == "DuplicateUserName") // si el error trata de DuplicateUserName, personalizar ErrorMessage
                        {
                            return new ResponseDto<LoginResponseDto>
                            {
                                StatusCode = 400,
                                Status = false,
                                Message = "El email ya está registrado en el sistema."
                            };
                        }
                    }

                    return new ResponseDto<LoginResponseDto>
                    {
                        StatusCode = 400,
                        Status = false,
                        Message = errors
                    };
                }

                // Usuario creado exitosamente
                var userEntity = await _userManager.FindByEmailAsync(dto.Email);
                await _userManager.AddToRoleAsync(userEntity, RolesConstants.CLIENT);


                //var clientTypeEntity = await _context.Clients
                //    .Where(c => c.UserId == userEntity.Id)
                //    .Select(c => _context.TypesOfClient
                //        .Where(tc => tc.Id == c.ClientTypeId)
                //        .FirstOrDefault())
                //    .FirstOrDefaultAsync();





                // verificacin de tipo de cliente
                var clientTypeEntity = await _context.TypesOfClient.FindAsync(dto.ClientTypeId);
                if (clientTypeEntity == null)
                {
                    await transaction.RollbackAsync(); // Rollback si no se encuentra el tipo de cliente
                    return new ResponseDto<LoginResponseDto>
                    {
                        StatusCode = 404,
                        Status = false,
                        Message = "No se ha encontrado el tipo de cliente especificado."
                    };
                }

                var clientTypeDto = _mapper.Map<ClientTypeDto>(clientTypeEntity);


                // Creación de ClientEntity
                var clientEntity = new ClientEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = userEntity.Id,
                    ClientTypeId = clientTypeEntity.Id
                };

                _context.Clients.Add(clientEntity);
                await _context.SaveChangesAsync(); // Guarda el cliente

                // Confirmamos la transacción después de que todos los pasos fueron exitosos
                await transaction.CommitAsync();

                // Configuración de Claims para el JWT
                var authClaims = new List<Claim>
         {
             new Claim(ClaimTypes.Email, userEntity.Email),
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             new Claim("UserId", userEntity.Id),
             new Claim(ClaimTypes.Role, RolesConstants.CLIENT)
         };

                var jwtToken = GetToken(authClaims);
                var refreshToken = GenerateRefreshTokenString();

                userEntity.RefreshToken = refreshToken;
                userEntity.RefreshTokenExpire = DateTime.Now
                    .AddMinutes(int.Parse(_configuration["JWT:RefreshTokenExpire"] ?? "30"));

                _context.Entry(userEntity);
                await _context.SaveChangesAsync();


                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 200,
                    Status = true,
                    Message = "Registro de usuario realizado satisfactoriamente.",
                    Data = new LoginResponseDto
                    {
                        Name = userEntity.Name,
                        Email = userEntity.Email,
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtToken), // convertir token en string
                        TokenExpiration = jwtToken.ValidTo,
                        RefreshToken = refreshToken,
                        ClientType = clientTypeDto
                    }
                };
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(); // Rollback si ocurre una excepción
                _logger.LogError(e, "Ocurrió un error inesperado al registrar el usuario.");
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = 500,
                    Status = false,
                    Message = "Ocurrió un error inesperado al registrar el usuario."
                };
            }
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(10),
                    claims: authClaims, signingCredentials: new SigningCredentials(
                                                                authSigninKey,
                                                                SecurityAlgorithms.HmacSha256)
             );

            var secret = _configuration["JWT:Secret"];
            var iss = _configuration["JWT:ValidIssuer"];

            Console.WriteLine("Estoy cansado jefe");
            return token;
        }

        private async Task<List<Claim>> GetClaims(UserEntity userEntity)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, userEntity.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("UserId", userEntity.Id),
                };

            var userRoles = await _userManager.GetRolesAsync(userEntity);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }
    }
}
