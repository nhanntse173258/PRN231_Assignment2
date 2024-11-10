using BOs.Dtos;
using BOs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Repository;
using System.Reflection.Emit;

namespace PRN_PE_Fall23.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class JewelryController : ODataController
    {
        public static readonly Random random = new Random();
        private readonly IJewelryRepo _jewelryRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly ICategoryRepo _categoryRepo;

        public JewelryController(
            IJewelryRepo jewelryRepo,
            IAccountRepo accountRepo,
            ICategoryRepo categoryRepo)
        {
            _jewelryRepo = jewelryRepo;
            _accountRepo = accountRepo;
            _categoryRepo = categoryRepo;
        }

        [Authorize(Roles = "Admin")]
        [EnableQuery]
        [HttpGet("odata/Jewelry")]
        public ActionResult<List<SilverJewelry>> GetAllJewelry()
        {
            var jewelryList = _jewelryRepo.GetJewelrys();

            //var result = jewelryList.Select(j => new SilverJewelryDto
            //{
            //    SilverJewelryId = j.SilverJewelryId,
            //    SilverJewelryName = j.SilverJewelryName,
            //    MetalWeight = j.MetalWeight,
            //    SilverJewelryDescription = j.SilverJewelryDescription,
            //    CreatedDate = j.CreatedDate,
            //    ProductionYear = j.ProductionYear.Value,
            //    Price = j.Price.Value,
            //    CategoryName = _categoryRepo.GetCategory(j.CategoryId)?.CategoryName ?? "Unknown"
            //}).ToList();
            return Ok(jewelryList);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("api/Jewelry/{id}")]
        public ActionResult<SilverJewelry> GetById([FromRoute] string id)
        {
            var result = _jewelryRepo.GetJewelry(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("api/Jewelry/")]
        public async Task<IActionResult> Create([FromBody] SilverJewelry newJewelry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _jewelryRepo.AddJewelry(newJewelry);
                return CreatedAtAction(nameof(GetById), new { id = newJewelry.SilverJewelryId }, newJewelry);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the jewelry.", details = ex.Message });
            }
        }
        [Authorize(Roles = "Admin, Staff")]
        [HttpGet("api/Jewelry/search")]
        public async Task<ActionResult<List<SilverJewelry>>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term cannot be empty.");
            }

            // Use a case-insensitive search for both conditions
            var results = await Task.Run(() =>
                _jewelryRepo.GetJewelrys()
                    .Where(j => j.SilverJewelryName.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || j.MetalWeight.ToString().Contains(term))
                    .ToList()
            );

            if (results.Count == 0)
            {
                return NotFound("No jewelry found matching the search criteria.");
            }

            return Ok(results);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("api/Jewelry/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SilverJewelry updatedJewelry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the jewelry item exists
            var existingJewelry = _jewelryRepo.GetJewelry(id);
            if (existingJewelry == null)
            {
                return NotFound();
            }

            // Update the existing jewelry item's properties
            existingJewelry.SilverJewelryName = updatedJewelry.SilverJewelryName;
            existingJewelry.MetalWeight = updatedJewelry.MetalWeight;
            existingJewelry.SilverJewelryDescription = updatedJewelry.SilverJewelryDescription;
            existingJewelry.CreatedDate = updatedJewelry.CreatedDate;
            existingJewelry.ProductionYear = updatedJewelry.ProductionYear;
            existingJewelry.Price = updatedJewelry.Price;
            existingJewelry.CategoryId = updatedJewelry.CategoryId;

            try
            {
                _jewelryRepo.UpdateJewlry(existingJewelry);
                return NoContent(); // Return 204 No Content if the update is successful
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the jewelry.", details = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("api/Jewelry/{id}")]
        public async Task<IActionResult> DeleteSilverJewelry(string id)
        {
            var jewelry = _jewelryRepo.GetJewelry(id);
            if (jewelry == null)
            {
                return NotFound();
            }
            try
            {
                var result = _jewelryRepo.RemoveJewelry(id);
                if (result) return NoContent();
            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the jewelry.", details = ex.Message });
            }
            return BadRequest();
        }
    }
}
