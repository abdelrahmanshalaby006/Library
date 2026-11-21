using Library.DAL.Entities;

namespace Library.DAL.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        bool CheckNameExist(string name);
    }
}
