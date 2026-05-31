namespace FamilyPillsAPI.Models.Dto
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Token { get; set; } = string.Empty;
        public long TokenExpiry { get; set; }
    }

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public long TokenExpiry { get; set; }
    }

    public class UserProfileResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public int MedicineCount { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class MedicineListResponse
    {
        public List<Medicine> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ImageUploadResponse
    {
        public string ImagePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class BarcodeValidationResponse
    {
        public bool Exists { get; set; }
        public Medicine? Medicine { get; set; }
    }

    public class StatsResponse
    {
        public int TotalMedicines { get; set; }
        public int RunningLowCount { get; set; }
        public int ExpiredCount { get; set; }
        public int ExpiredSoon { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class DeleteResponse
    {
        public int DeletedMedicineId { get; set; }
    }
}
