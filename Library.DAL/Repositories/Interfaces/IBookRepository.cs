using Library.DAL.Entities;

namespace Library.DAL.Repositories.Interfaces
{
    public interface IBookRepository : IGenericRepository<Book>
    {
        // true لو فيه كتاب بنفس العنوان + المؤلف (ignoreId بنستخدمه في Edit)
        bool CheckBookExist(string title, string author, int ignoreId = 0);

        // true لو الكتاب عليه تاريخ استعارات (يعني ممنوع يتمسح)
        bool HasHistory(int bookId);
    }
}
