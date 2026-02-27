using API.Filters;
using Application.Abstractions;
using Application.ViewModels.Product;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [ServiceFilter(typeof(LogActionFilter))]
    public class ProductController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductRequest input)
        {
            if (input == null) return BadRequest();

            var product = _mapper.Map<Product>(input);

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.Commit();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequest input)
        {
            var repo = _unitOfWork.Repository<Product>();
            var existingProduct = await repo.GetByIdAsync(id);

            if (existingProduct == null) return NotFound();

            _mapper.Map(input, existingProduct);

            await repo.UpdateAsync(existingProduct);
            await _unitOfWork.Commit();

            return Ok(existingProduct);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var repo = _unitOfWork.Repository<Product>();
            var existing = await repo.GetByIdAsync(id);

            if (existing == null) return NotFound();

            await repo.DeleteAsync(id);
            await _unitOfWork.Commit();

            return NoContent();
        }
    }
}