using FernandaRentals.Dtos.Auth;
using FernandaRentals.Dtos.Common;

namespace FernandaRentals.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ResponseDto<LoginResponseDto>> RegisterClientAsync(RegisterClientDto dto);
    }
}
