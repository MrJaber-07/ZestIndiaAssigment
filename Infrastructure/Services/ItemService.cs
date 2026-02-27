namespace Infrastructure.Services
{

    using Application.Repositories;
    using Application.Services;
    using Application.ViewModels.Product;
    using AutoMapper;
    using Domain.Entities;
    using Microsoft.Extensions.Logging;

    namespace Infrastructure.Services
    {
        public class ItemService : IItemService
        {
            private readonly IRepository<Item> _itemRepository;
            private readonly IMapper _mapper;
            private readonly ILogger<ItemService> _logger;

            public ItemService(
                IRepository<Item> itemRepository,
                IMapper mapper,
                ILogger<ItemService> logger)
            {
                _itemRepository = itemRepository;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<IEnumerable<Item>> GetAllAsync()
            {
                _logger.LogInformation("Fetching all items");
                return await _itemRepository.GetAllAsync();
            }

            public async Task<Item?> GetByIdAsync(int id)
            {
                _logger.LogInformation($"Fetching item with ID {id}");
                return await _itemRepository.GetByIdAsync(id);
            }

            public async Task<Item> CreateAsync(ProductRequest request)
            {
                _logger.LogInformation("Creating new item");
                var item = _mapper.Map<Item>(request);
                await _itemRepository.AddAsync(item);
                return item;
            }

            public async Task<Item> UpdateAsync(int id, ProductRequest request)
            {
                _logger.LogInformation($"Updating item with ID {id}");
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null)
                    throw new Exception($"Item with ID {id} not found.");

                _mapper.Map(request, item);
                await _itemRepository.UpdateAsync(item);
                return item;
            }

            public async Task<bool> DeleteAsync(int id)
            {
                _logger.LogInformation($"Deleting item with ID {id}");
                var item = await _itemRepository.GetByIdAsync(id);
                if (item == null) return false;

                await _itemRepository.DeleteAsync(item.Id);
                return true;
            }
        }
    }

}
