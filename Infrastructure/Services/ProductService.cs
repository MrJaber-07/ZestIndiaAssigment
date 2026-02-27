using Application.Repositories;
using Application.ViewModels.Product;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IRepository<Product> productRepository, IMapper mapper, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductResponse>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse> CreateAsync(ProductRequest request)
        {
            var entity = _mapper.Map<Product>(request);

            await _productRepository.AddAsync(entity);
            _logger.LogInformation($"Created new product {entity.Id}.");

            return _mapper.Map<ProductResponse>(entity);
        }

        public async Task<ProductResponse> UpdateAsync(int id, ProductRequest request)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Product with id {id} not found.");
            }

            _mapper.Map(request, existing);
            await _productRepository.UpdateAsync(existing);
            _logger.LogInformation($"Updated product {id}.");

            return _mapper.Map<ProductResponse>(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _productRepository.DeleteAsync(id);
            _logger.LogInformation($"Deleted product {id}.");
            return true;
        }

        public async Task<int> CommitAsync()
        {

            return await _productRepository.CountAsync();
        }
    }
}