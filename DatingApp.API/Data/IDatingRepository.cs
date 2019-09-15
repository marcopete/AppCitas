using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         // Tipo Task requiere using System.Threading.Tasks;
         Task<bool> SaveAll();
         // IEnumerable requiere using System.Collections.Generic;
         // User requiere using DatingApp.API.Models;
         Task<PagedList<User>> GetUsers(UserParams userParams);
         Task<User> GetUser(int id);
         Task<Photo> GetPhoto(int id);

         Task<Photo> GetMainPhotoForUser(int userId);
    }
}