using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyPillsAPI.Data;
using FamilyPillsAPI.Models;

namespace FamilyPillsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicinesController : ControllerBase
    {
        private readonly FamilyPillsDbContext _context;

        public MedicinesController(FamilyPillsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả các loại thuốc
        /// </summary>
        /// <returns>Danh sách thuốc</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Medicine>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Medicine>>> GetMedicines()
        {
            try
            {
                var medicines = await _context.Medicines.ToListAsync();
                return Ok(medicines);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Lỗi khi lấy danh sách thuốc", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một loại thuốc theo Id
        /// </summary>
        /// <param name="id">Id của thuốc</param>
        /// <returns>Chi tiết thuốc</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Medicine), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Medicine>> GetMedicine(int id)
        {
            try
            {
                var medicine = await _context.Medicines.FindAsync(id);

                if (medicine == null)
                {
                    return NotFound(new { message = $"Không tìm thấy thuốc với Id {id}" });
                }

                return Ok(medicine);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi lấy thông tin thuốc", error = ex.Message });
            }
        }

        /// <summary>
        /// Thêm một loại thuốc mới
        /// </summary>
        /// <param name="medicine">Thông tin thuốc cần thêm</param>
        /// <returns>Thuốc vừa được tạo</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Medicine), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Medicine>> PostMedicine(Medicine medicine)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(medicine.Name))
                {
                    return BadRequest(new { message = "Tên thuốc không được để trống" });
                }

                if (medicine.TotalQuantity < 0)
                {
                    return BadRequest(new { message = "Số lượng thuốc không được âm" });
                }

                // Set LastUpdated to current timestamp
                medicine.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                _context.Medicines.Add(medicine);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetMedicine), new { id = medicine.Id }, medicine);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi thêm thuốc mới", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin một loại thuốc
        /// </summary>
        /// <param name="id">Id của thuốc cần cập nhật</param>
        /// <param name="medicine">Thông tin cập nhật</param>
        /// <returns>Thuốc đã cập nhật</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Medicine), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutMedicine(int id, Medicine medicine)
        {
            try
            {
                // Check if the medicine exists
                var existingMedicine = await _context.Medicines.FindAsync(id);
                if (existingMedicine == null)
                {
                    return NotFound(new { message = $"Không tìm thấy thuốc với Id {id}" });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(medicine.Name))
                {
                    return BadRequest(new { message = "Tên thuốc không được để trống" });
                }

                if (medicine.TotalQuantity < 0)
                {
                    return BadRequest(new { message = "Số lượng thuốc không được âm" });
                }

                // Update properties
                existingMedicine.Name = medicine.Name;
                existingMedicine.Barcode = medicine.Barcode;
                existingMedicine.TotalQuantity = medicine.TotalQuantity;
                existingMedicine.Unit = medicine.Unit;
                existingMedicine.ExpiryDate = medicine.ExpiryDate;
                existingMedicine.ImagePath = medicine.ImagePath;
                existingMedicine.Quantity = medicine.Quantity;
                existingMedicine.IsRunningLow = medicine.IsRunningLow;
                existingMedicine.IsExpired = medicine.IsExpired;
                existingMedicine.LastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                _context.Medicines.Update(existingMedicine);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thuốc thành công", data = existingMedicine });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi cập nhật thuốc", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa một loại thuốc theo Id
        /// </summary>
        /// <param name="id">Id của thuốc cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMedicine(int id)
        {
            try
            {
                var medicine = await _context.Medicines.FindAsync(id);
                if (medicine == null)
                {
                    return NotFound(new { message = $"Không tìm thấy thuốc với Id {id}" });
                }

                _context.Medicines.Remove(medicine);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa thuốc thành công", deletedMedicineId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi xóa thuốc", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra xem thuốc có tồn tại không
        /// </summary>
        /// <param name="id">Id của thuốc</param>
        /// <returns>true nếu tồn tại, false nếu không</returns>
        private bool MedicineExists(int id)
        {
            return _context.Medicines.Any(e => e.Id == id);
        }
    }
}
