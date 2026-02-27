using API.Filters;
using Application.Abstractions;
using Application.DTOs.Item;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ServiceFilter(typeof(LogActionFilter))]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ItemController(ILogger<ItemController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {

            var items = await _unitOfWork.Repository<Item>().GetAllAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _unitOfWork.Repository<Item>().GetByIdAsync(id);
            if (item == null) return NotFound();

            return Ok(item);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ItemRequest dto)
        {
            if (dto == null) return BadRequest();

            var productExists = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);

            if (productExists == null)
                return NotFound($"No Product found with ID {dto.ProductId}");

            var item = new Item
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };

            await _unitOfWork.Repository<Item>().AddAsync(item);

            await _unitOfWork.Commit();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemRequest dto)
        {
            var itemRepo = _unitOfWork.Repository<Item>();
            var item = await itemRepo.GetByIdAsync(id);

            if (item == null) return NotFound();

            item.ProductId = dto.ProductId;
            item.Quantity = dto.Quantity;

            await itemRepo.UpdateAsync(item);
            await _unitOfWork.Commit();

            return Ok(item);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var itemRepo = _unitOfWork.Repository<Item>();
            var existing = await itemRepo.GetByIdAsync(id);

            if (existing == null) return NotFound();

            await itemRepo.DeleteAsync(id);
            await _unitOfWork.Commit();

            return Ok(new { Message = $"Item with ID {id} deleted successfully." });
        }
    }
}