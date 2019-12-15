using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => 
                u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams) // DEVUELVE LISTA DE USUARIOS
        {
            var users = _context.Users.OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            // Determina lista de genero diferente de quien se loguea
             users = users.Where(u => u.Gender == userParams.Gender);

             if (userParams.teDieronLike)
             {
                 var userLikers = await GetUserLikes(userParams.UserId, userParams.teDieronLike);
                 users = users.Where(u => userLikers.Contains(u.Id));
             }

             if (userParams.disteLike)
             {
                 var userLikees = await GetUserLikes(userParams.UserId, userParams.teDieronLike);
                 users = users.Where(u => userLikees.Contains(u.Id));
             }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minFecNac = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxFecNac = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minFecNac && u.DateOfBirth <= maxFecNac);
            }

            if (!string.IsNullOrEmpty(userParams.OrdenadoPor))
            {
                switch (userParams.OrdenadoPor)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }


            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool teDieronLike)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (teDieronLike)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }    
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var mensajes = _context.Messages.AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    mensajes = mensajes.Where(u => u.RecipientId == messageParams.UserId
                        && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    mensajes = mensajes.Where(u=> u.SenderId == messageParams.UserId 
                        && u.SenderDeleted == false);
                    break;
                default:
                    mensajes = mensajes.Where(u => u.RecipientId == messageParams.UserId
                        && u.RecipientDeleted == false && u.IsRead == false);    
                    break;
            }

            mensajes = mensajes.OrderByDescending(d => d.MessageSent);

            return await PagedList<Message>.CreateAsync(mensajes,
                messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var mensajes = await _context.Messages
                .Where(m => m.RecipientId == userId && m.RecipientDeleted == false 
                    && m.SenderId == recipientId
                    || m.RecipientId == recipientId && m.SenderId == userId 
                    && m.SenderDeleted == false)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            return mensajes;        
        }
    }
}