using Application.ViewModels.Product;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<Item> CreateAsync(ProductRequest request);
        Task<Item> UpdateAsync(int id, ProductRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
