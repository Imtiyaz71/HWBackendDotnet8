using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Som_Models.DTOs;
using Som_Service.Interface;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Som_Service.Service
{
    public class OpenAuthService : IOpenAuthService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;

        public OpenAuthService(IDbConnectionFactory connectionFactory, IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        public async Task<AuthUserResponse> RegisterStudentAsync(StudentRegisterDto dto)
        {
            using var db = _connectionFactory.CreateConnection();

            // Check if phone already registered
            var existing = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM RegisteredUsers WHERE Phone = @Phone",
                new { Phone = dto.GuardianPhone });

            if (existing > 0)
            {
                return new AuthUserResponse
                {
                    Success = false,
                    Message = "A user with this guardian phone number is already registered."
                };
            }

            var passwordToHash = string.IsNullOrWhiteSpace(dto.Password) ? "123" : dto.Password;
            var passwordHash = ComputeSha256Hash(passwordToHash);

            var sql = @"
                INSERT INTO RegisteredUsers (
                    Name, Phone, Email, PasswordHash, Role, 
                    SchoolName, SchoolAddress, Address, Gender, 
                    FatherName, MotherName, CurriculumType, ClassName, CreatedAt
                ) 
                VALUES (
                    @Name, @Phone, @Email, @PasswordHash, 'Student',
                    @SchoolName, @SchoolAddress, @Address, @Gender,
                    @FatherName, @MotherName, @CurriculumType, @ClassName, GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var userId = await db.ExecuteScalarAsync<int>(sql, new
            {
                Name = dto.StudentName,
                Phone = dto.GuardianPhone,
                Email = dto.GuardianEmail,
                PasswordHash = passwordHash,
                SchoolName = dto.SchoolName,
                SchoolAddress = dto.SchoolAddress,
                Address = dto.Address,
                Gender = dto.Gender,
                FatherName = dto.FatherName,
                MotherName = dto.MotherName,
                CurriculumType = dto.CurriculumType,
                ClassName = dto.ClassName
            });

            var token = GenerateJwtToken(userId, dto.StudentName, dto.GuardianPhone, "Student");

            return new AuthUserResponse
            {
                Success = true,
                Message = "Student registration successful.",
                Token = token,
                Role = "Student",
                UserId = userId,
                Name = dto.StudentName,
                Phone = dto.GuardianPhone,
                Email = dto.GuardianEmail,
                SchoolName = dto.SchoolName,
                ClassName = dto.ClassName,
                CurriculumType = dto.CurriculumType
            };
        }

        public async Task<AuthUserResponse> RegisterTeacherAsync(TeacherRegisterDto dto)
        {
            using var db = _connectionFactory.CreateConnection();

            var existing = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM RegisteredUsers WHERE Phone = @Phone",
                new { Phone = dto.Phone });

            if (existing > 0)
            {
                return new AuthUserResponse
                {
                    Success = false,
                    Message = "A teacher with this phone number is already registered."
                };
            }

            var passwordToHash = string.IsNullOrWhiteSpace(dto.Password) ? "123" : dto.Password;
            var passwordHash = ComputeSha256Hash(passwordToHash);
            var specialistJson = JsonSerializer.Serialize(dto.SpecialistSubjects ?? new List<string>());

            var sql = @"
                INSERT INTO RegisteredUsers (
                    Name, Phone, Email, PasswordHash, Role, 
                    SchoolName, SchoolAddress, Address, SpecialistSubjects, CreatedAt
                ) 
                VALUES (
                    @Name, @Phone, @Email, @PasswordHash, 'Teacher',
                    @SchoolName, @SchoolAddress, @Address, @SpecialistSubjects, GETUTCDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var userId = await db.ExecuteScalarAsync<int>(sql, new
            {
                Name = dto.TeacherName,
                Phone = dto.Phone,
                Email = dto.Email,
                PasswordHash = passwordHash,
                SchoolName = dto.SchoolName,
                SchoolAddress = dto.SchoolAddress,
                Address = dto.Address,
                SpecialistSubjects = specialistJson
            });

            var token = GenerateJwtToken(userId, dto.TeacherName, dto.Phone, "Teacher");

            return new AuthUserResponse
            {
                Success = true,
                Message = "Teacher registration successful.",
                Token = token,
                Role = "Teacher",
                UserId = userId,
                Name = dto.TeacherName,
                Phone = dto.Phone,
                Email = dto.Email,
                SchoolName = dto.SchoolName,
                SpecialistSubjects = dto.SpecialistSubjects
            };
        }

        public async Task<AuthUserResponse> LoginUserAsync(UserLoginDto dto)
        {
            using var db = _connectionFactory.CreateConnection();

            var passwordHash = ComputeSha256Hash(dto.Password);

            var sql = @"
                SELECT * FROM RegisteredUsers 
                WHERE (Phone = @Identifier OR (Email IS NOT NULL AND Email = @Identifier))
                  AND PasswordHash = @PasswordHash";

            var user = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new
            {
                Identifier = dto.Identifier.Trim(),
                PasswordHash = passwordHash
            });

            if (user == null)
            {
                return new AuthUserResponse
                {
                    Success = false,
                    Message = "Invalid phone/email or password."
                };
            }

            int userId = user.Id;
            string name = user.Name;
            string phone = user.Phone;
            string? email = user.Email;
            string role = user.Role;
            string? schoolName = user.SchoolName;
            string? className = user.ClassName;
            string? curriculumType = user.CurriculumType;
            string? specJson = user.SpecialistSubjects;

            List<string>? specialistSubjects = null;
            if (!string.IsNullOrEmpty(specJson))
            {
                try { specialistSubjects = JsonSerializer.Deserialize<List<string>>(specJson); } catch { }
            }

            // Update DeviceToken if provided
            if (!string.IsNullOrWhiteSpace(dto.DeviceToken))
            {
                await db.ExecuteAsync(
                    "UPDATE RegisteredUsers SET DeviceToken = @Token WHERE Id = @Id",
                    new { Token = dto.DeviceToken, Id = userId });
            }

            var token = GenerateJwtToken(userId, name, phone, role);

            return new AuthUserResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                Role = role,
                UserId = userId,
                Name = name,
                Phone = phone,
                Email = email,
                SchoolName = schoolName,
                ClassName = className,
                CurriculumType = curriculumType,
                SpecialistSubjects = specialistSubjects
            };
        }

        private string GenerateJwtToken(int userId, string name, string phone, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? "ThisIsASuperSecretKeyForJWT1234567890776656777!";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.MobilePhone, phone),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "SomApi",
                audience: jwtSettings["Audience"] ?? "SomApiUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"] ?? "120")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 3)
            {
                return (false, "New password must be at least 3 characters.");
            }

            using var db = _connectionFactory.CreateConnection();
            var oldHash = ComputeSha256Hash(oldPassword);

            var currentHash = await db.ExecuteScalarAsync<string>(
                "SELECT PasswordHash FROM RegisteredUsers WHERE Id = @Id",
                new { Id = userId });

            if (currentHash == null)
            {
                return (false, "User not found.");
            }

            if (currentHash != oldHash)
            {
                return (false, "Current password does not match.");
            }

            var newHash = ComputeSha256Hash(newPassword);
            await db.ExecuteAsync(
                "UPDATE RegisteredUsers SET PasswordHash = @PasswordHash WHERE Id = @Id",
                new { PasswordHash = newHash, Id = userId });

            return (true, "Password changed successfully.");
        }
    }
}
