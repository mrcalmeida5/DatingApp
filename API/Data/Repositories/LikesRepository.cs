

using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;

        public LikesRepository(DataContext context)
        {
            _context = context;
        }


        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            if (string.IsNullOrEmpty(likesParams.Predicate))
                return await PagedList<LikeDto>.CreateAsync(Enumerable.Empty<LikeDto>().AsQueryable(), 1, 0);

            var users = _context.Users.OrderBy(x => x.KnownAs).AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                users = _context.Likes
                            .AsQueryable()
                            .Where(x => x.SourceUserId == likesParams.UserId)
                            .Select(x => x.LikedUser);
            }
            else if (likesParams.Predicate == "likedBy")
            {
                users = _context.Likes
                            .AsQueryable()
                            .Where(x => x.LikedUserId == likesParams.UserId)
                            .Select(x => x.SourceUser);
            }

            var likedUsers = users.Select(user => new LikeDto
            {
                Age = user.DateOfBirth.CalculateAge(),
                City = user.City,
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.Where(p => p.IsMain).Select(p => p.Url).FirstOrDefault(),
                UserId = user.Id,
                Username = user.UserName
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}