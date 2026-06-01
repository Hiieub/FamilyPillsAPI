using System.Security.Claims;
using FamilyPillsAPI.Data;
using FamilyPillsAPI.Models;
using FamilyPillsAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyPillsAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : BaseController
    {
        private readonly FamilyPillsDbContext _context;

        public MedicinesController(FamilyPillsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<MedicineListResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<MedicineListResponse>>> GetMedicines(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? filter = "all",
            [FromQuery] string? search = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<MedicineListResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            skip = Math.Max(0, skip);
            take = take <= 0 ? 10 : Math.Min(take, 100);
            filter = string.IsNullOrWhiteSpace(filter) ? "all" : filter.Trim();

            var query = _context.Medicines
                .AsNoTracking()
                .Where(m => m.UserId == userId.Value);

            if (filter.Equals("runningLow", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.IsRunningLow);
            }
            else if (filter.Equals("expired", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.IsExpired);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(m =>
                    m.Name.Contains(keyword) ||
                    (m.Barcode != null && m.Barcode.Contains(keyword)));
            }

            List<Medicine> allFilteredMedicines;
            if (filter.Equals("expiringSoon", StringComparison.OrdinalIgnoreCase))
            {
                var allMeds = await query.ToListAsync();
                var formats = new[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss" };
                var now = DateTime.Now;

                allFilteredMedicines = allMeds.Where(m =>
                {
                    if (string.IsNullOrWhiteSpace(m.ExpiryDate) || m.IsExpired)
                        return false;

                    if (DateTime.TryParseExact(m.ExpiryDate.Trim(), formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var expiryDate))
                    {
                        return expiryDate.Date > now.Date && expiryDate.Date <= now.AddDays(30).Date;
                    }
                    return false;
                }).ToList();
            }
            else
            {
                allFilteredMedicines = await query.OrderByDescending(m => m.Id).ToListAsync();
            }

            var totalCount = allFilteredMedicines.Count;
            var medicines = allFilteredMedicines
                .Skip(skip)
                .Take(take)
                .ToList();

            var response = new MedicineListResponse
            {
                Items = medicines,
                TotalCount = totalCount,
                PageNumber = (skip / take) + 1,
                PageSize = take,
                TotalPages = (int)Math.Ceiling(totalCount / (double)take)
            };

            return SuccessResponse(response, "Lay danh sach thuoc thanh cong");
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Medicine>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Medicine>>> GetMedicine(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<Medicine>("Token khong hop le", "INVALID_TOKEN");
            }

            var medicine = await _context.Medicines
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId.Value);

            if (medicine == null)
            {
                return NotFoundResponse<Medicine>($"Khong tim thay thuoc voi Id {id}");
            }

            return SuccessResponse(medicine, "Lay thong tin thuoc thanh cong");
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Medicine>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Medicine>>> PostMedicine(Medicine medicine)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<Medicine>("Token khong hop le", "INVALID_TOKEN");
            }

            var validation = ValidateMedicine(medicine);
            if (validation != null)
            {
                return validation;
            }

            if (!string.IsNullOrWhiteSpace(medicine.Barcode))
            {
                var barcode = medicine.Barcode.Trim();
                var exists = await _context.Medicines.AnyAsync(m => m.UserId == userId.Value && m.Barcode == barcode);
                if (exists)
                {
                    return BadRequestResponse<Medicine>("Ma vach da ton tai", "DUPLICATE_BARCODE", "barcode");
                }

                medicine.Barcode = barcode;
            }

            medicine.Id = 0;
            medicine.UserId = userId.Value;
            medicine.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CalculateMedicineStatus(medicine);

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            return CreatedResponse(medicine, "Them thuoc thanh cong");
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Medicine>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Medicine>>> PutMedicine(int id, Medicine medicine)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<Medicine>("Token khong hop le", "INVALID_TOKEN");
            }

            var existingMedicine = await _context.Medicines
                .SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId.Value);
            if (existingMedicine == null)
            {
                return NotFoundResponse<Medicine>($"Khong tim thay thuoc voi Id {id}");
            }

            var validation = ValidateMedicine(medicine);
            if (validation != null)
            {
                return validation;
            }

            if (!string.IsNullOrWhiteSpace(medicine.Barcode))
            {
                var barcode = medicine.Barcode.Trim();
                var exists = await _context.Medicines.AnyAsync(m =>
                    m.UserId == userId.Value &&
                    m.Id != id &&
                    m.Barcode == barcode);
                if (exists)
                {
                    return BadRequestResponse<Medicine>("Ma vach da ton tai", "DUPLICATE_BARCODE", "barcode");
                }

                existingMedicine.Barcode = barcode;
            }
            else
            {
                existingMedicine.Barcode = null;
            }

            existingMedicine.Name = medicine.Name.Trim();
            existingMedicine.TotalQuantity = medicine.TotalQuantity;
            existingMedicine.Unit = medicine.Unit;
            existingMedicine.ExpiryDate = medicine.ExpiryDate;
            existingMedicine.ImagePath = medicine.ImagePath;
            existingMedicine.Quantity = medicine.Quantity;
            CalculateMedicineStatus(existingMedicine);
            existingMedicine.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();

            return SuccessResponse(existingMedicine, "Cap nhat thuoc thanh cong");
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DeleteResponse>>> DeleteMedicine(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<DeleteResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            var medicine = await _context.Medicines
                .SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId.Value);
            if (medicine == null)
            {
                return NotFoundResponse<DeleteResponse>($"Khong tim thay thuoc voi Id {id}");
            }

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();

            return SuccessResponse(new DeleteResponse { DeletedMedicineId = id }, "Xoa thuoc thanh cong");
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<StatsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<StatsResponse>>> GetStats()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<StatsResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            var medicines = await _context.Medicines
                .AsNoTracking()
                .Where(m => m.UserId == userId.Value)
                .ToListAsync();

            var totalMedicines = medicines.Count;
            var runningLowCount = medicines.Count(m => m.IsRunningLow);
            var expiredCount = medicines.Count(m => m.IsExpired);

            var formats = new[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };
            var now = DateTime.Now;
            var expiringSoonCount = 0;

            foreach (var medicine in medicines)
            {
                if (string.IsNullOrWhiteSpace(medicine.ExpiryDate) || medicine.IsExpired)
                    continue;

                if (DateTime.TryParseExact(medicine.ExpiryDate.Trim(), formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var expiryDate))
                {
                    if (expiryDate > now && expiryDate <= now.AddDays(30))
                    {
                        expiringSoonCount++;
                    }
                }
            }

            var response = new StatsResponse
            {
                TotalMedicines = totalMedicines,
                RunningLowCount = runningLowCount,
                ExpiredCount = expiredCount,
                ExpiredSoon = expiringSoonCount,
                LastUpdated = DateTime.Now
            };

            return SuccessResponse(response, "Lay thong ke thanh cong");
        }

        [HttpGet("validate-barcode/{barcode}")]
        [ProducesResponseType(typeof(ApiResponse<BarcodeValidationResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<BarcodeValidationResponse>>> ValidateBarcode(string barcode)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<BarcodeValidationResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            var medicine = await _context.Medicines
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId.Value && m.Barcode == barcode);

            var response = new BarcodeValidationResponse
            {
                Exists = medicine != null,
                Medicine = medicine
            };

            return SuccessResponse(response, "Kiem tra ma vach thanh cong");
        }

        [HttpPost("upload-image")]
        [ProducesResponseType(typeof(ApiResponse<ImageUploadResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ImageUploadResponse>>> UploadImage(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return UnauthorizedResponse<ImageUploadResponse>("Token khong hop le", "INVALID_TOKEN");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequestResponse<ImageUploadResponse>("File khong duoc de trong", "VALIDATION_ERROR", "file");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequestResponse<ImageUploadResponse>("File khong duoc vuot qua 5MB", "VALIDATION_ERROR", "file");
            }

            if (file.ContentType == null || !file.ContentType.StartsWith("image/"))
            {
                return BadRequestResponse<ImageUploadResponse>("Chi chap nhan file hinh anh", "VALIDATION_ERROR", "file");
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medicines");

            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var response = new ImageUploadResponse
            {
                ImagePath = $"/uploads/medicines/{fileName}",
                FileName = fileName,
                FileSize = file.Length,
                UploadedAt = DateTime.Now
            };

            return CreatedResponse(response, "Upload hinh anh thanh cong");
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }

        private ActionResult<ApiResponse<Medicine>>? ValidateMedicine(Medicine medicine)
        {
            if (string.IsNullOrWhiteSpace(medicine.Name))
            {
                return BadRequestResponse<Medicine>("Ten thuoc khong duoc de trong", "VALIDATION_ERROR", "name");
            }

            if (medicine.TotalQuantity < 0)
            {
                return BadRequestResponse<Medicine>("So luong thuoc khong duoc am", "VALIDATION_ERROR", "totalQuantity");
            }

            medicine.Name = medicine.Name.Trim();
            return null;
        }

        private void CalculateMedicineStatus(Medicine medicine)
        {
            // Tự động set Sắp hết nếu số lượng <= 5 và > 0 (nếu bằng 0 thì là Hết)
            medicine.IsRunningLow = medicine.TotalQuantity <= 5 && medicine.TotalQuantity > 0;

            medicine.IsExpired = false;
            if (!string.IsNullOrWhiteSpace(medicine.ExpiryDate))
            {
                var formats = new[] { "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss" };
                if (DateTime.TryParseExact(medicine.ExpiryDate.Trim(), formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var expiryDate))
                {
                    if (expiryDate.Date < DateTime.Now.Date)
                    {
                        medicine.IsExpired = true;
                    }
                }
            }
        }
    }
}
