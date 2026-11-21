using Library.DAL.Contexts;
using Library.DAL.Entities;
using Library.DAL.Repositories.Interfaces;

namespace Library.DAL.Repositories.Classes
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext _context) : base(_context)
        {
        }

        public bool CheckNameExist(string name)
        {
            return context.Categories.Any(c => c.Name == name);
        }
    }
}
