using QuickBite.Auth.DTOs;
using QuickBite.Auth.Entities;
using QuickBite.Auth.Interfaces;
using Serilog;

namespace QuickBite.Auth.Services
{
    public class AuthServiceImpl : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthServiceImpl> _logger;

        public AuthServiceImpl(
            IUserRepository userRepository,
            IJwtTokenProvider jwtTokenProvider,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<AuthServiceImpl> logger)
        {
            _userRepository = userRepository;
            _jwtTokenProvider = jwtTokenProvider;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // Check if email already exists
            if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already registered", request.Email);
                throw new InvalidOperationException($"Email '{request.Email}' is already registered");
            }

            // Create user entity
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                Role = request.Role.ToUpper(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            var createdUser = await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {Email}, Role: {Role}, UserId: {UserId}",
                createdUser.Email, createdUser.Role, createdUser.UserId);

            // Generate JWT token
            var token = _jwtTokenProvider.GenerateToken(createdUser);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToProfileResponse(createdUser)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // Find user by email
            var user = await _userRepository.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: Email {Email} not found", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: Account {Email} is deactivated", request.Email);
                throw new UnauthorizedAccessException("Account has been deactivated. Please contact support.");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            _logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}",
                user.Email, user.UserId);

            // Generate JWT token
            var token = _jwtTokenProvider.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToProfileResponse(user)
            };
        }

        public async Task<ProfileResponseDto> GetProfileAsync(int userId)
        {
            var user = await _userRepository.FindByUserIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            return MapToProfileResponse(user);
        }

        public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            var user = await _userRepository.FindByUserIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Track changes for logging
            var changes = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.FullName) && request.FullName != user.FullName)
            {
                _logger.LogInformation("Profile update: UserId {UserId}, FullName changed from '{OldName}' to '{NewName}'",
                    userId, user.FullName, request.FullName);
                changes.Add("FullName");
                user.FullName = request.FullName;
            }

            if (request.Phone != null && request.Phone != user.Phone)
            {
                _logger.LogInformation("Profile update: UserId {UserId}, Phone changed from '{OldPhone}' to '{NewPhone}'",
                    userId, user.Phone, request.Phone);
                changes.Add("Phone");
                user.Phone = request.Phone;
            }

            if (request.ProfilePicUrl != null && request.ProfilePicUrl != user.ProfilePicUrl)
            {
                _logger.LogInformation("Profile update: UserId {UserId}, ProfilePicUrl updated", userId);
                changes.Add("ProfilePicUrl");
                user.ProfilePicUrl = request.ProfilePicUrl;
            }

            if (changes.Count > 0)
            {
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Profile updated for UserId {UserId}. Changed fields: {Fields}",
                    userId, string.Join(", ", changes));
            }

            return MapToProfileResponse(user);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _userRepository.FindByUserIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed: Invalid current password for UserId {UserId}", userId);
                throw new UnauthorizedAccessException("Current password is incorrect");
            }

            // Hash and save new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for UserId {UserId}", userId);
        }

        public async Task DeactivateAccountAsync(int userId)
        {
            var user = await _userRepository.FindByUserIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            user.IsActive = false;
            await _userRepository.UpdateAsync(user);

            _logger.LogWarning("Account deactivated: UserId {UserId}, Email {Email}", userId, user.Email);

            // If the deactivated user is an AGENT, force their delivery profile offline
            // so the admin panel stops showing them as available.
            if (user.Role.Equals("AGENT", StringComparison.OrdinalIgnoreCase))
            {
                await ForceAgentOfflineAsync(userId);
            }
        }

        /// <summary>
        /// Makes an internal HTTP call to delivery-service to set the agent's IsAvailable = false.
        /// Protected by a shared secret header. Fails silently so deactivation always succeeds.
        /// </summary>
        private async Task ForceAgentOfflineAsync(int userId)
        {
            try
            {
                var deliveryBaseUrl = _config["ServiceUrls:Delivery"] ?? "http://localhost:5272";
                var secret = _config["InternalSecrets:ServiceKey"];

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Internal-Secret", secret);

                var response = await client.PutAsync(
                    $"{deliveryBaseUrl}/api/v1/agents/user/{userId}/forceOffline", null);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("Agent for UserId {UserId} forced offline after account deactivation", userId);
                else
                    _logger.LogWarning("Failed to force agent offline for UserId {UserId}. Status: {Status}", userId, response.StatusCode);
            }
            catch (Exception ex)
            {
                // Never block account deactivation due to delivery-service being unavailable
                _logger.LogError(ex, "Exception while forcing agent offline for UserId {UserId}", userId);
            }
        }

        /// <summary>
        /// Maps a User entity to a ProfileResponseDto.
        /// </summary>
        private static ProfileResponseDto MapToProfileResponse(User user)
        {
            return new ProfileResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                ProfilePicUrl = user.ProfilePicUrl,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
