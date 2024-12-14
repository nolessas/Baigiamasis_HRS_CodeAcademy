using Baigiamasis.DTOs.User;
using Baigiamasis.DTOs.Common;
using AutoMapper;
using Baigiamasis.Services.Repositories.Interfaces;
using Baigiamasis.Services.Services.Interfaces;

namespace Baigiamasis.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly ILogger<AdminService> _logger;
        private readonly IAdminRepository _adminRepository;
        private readonly IMapper _mapper;

        public AdminService(
            ILogger<AdminService> logger,
            IAdminRepository adminRepository,
            IMapper mapper)
        {
            _logger = logger;
            _adminRepository = adminRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _adminRepository.GetAllUsersAsync();
            if (!users.Any())
                throw new KeyNotFoundException("No users found");

            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _adminRepository.GetUserByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
        {
            try
            {
                var success = await _adminRepository.DeleteUserAsync(id);
                if (!success)
                    throw new KeyNotFoundException($"User with ID {id} not found");

                return ApiResponse<bool>.Success(true, "User deleted successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete last admin user");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                throw;
            }
        }
    }
}