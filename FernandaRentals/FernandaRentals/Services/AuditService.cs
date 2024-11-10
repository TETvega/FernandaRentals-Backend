using FernandaRentals.Constants;
using FernandaRentals.Services.Interfaces;
using System.Security.Claims;

namespace FernandaRentals.Services
{
    public class AuditService:IAuditService
    {
        /// <summary>
        /// Aqui se da parte de la Autentificacion del Usuario 
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuditService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            // Posibles Errores
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Exception("HttpContext no está disponible.");
            }

            var user = _httpContextAccessor.HttpContext.User;
            // Verificacion de Autentificacion del Usuario
            if (!user.Identity.IsAuthenticated)
            {
                throw new Exception($"{MessagesConstant.UNAUTHENTICATED_USER_ERROR}");
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Verificacion de las Claims
            if (userId == null)
            {
                throw new Exception($"{MessagesConstant.MISSING_CLAIMS_ERROR}");
            }

            return userId;
        }

    }
}
