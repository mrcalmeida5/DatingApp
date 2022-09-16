
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }


        

        public async Task<AppUser> GetUserById(int id)
        {
            //return await _context.Users.FindAsync(id);
            //The "Find" method returns the object from local tracked store if it exists, avoiding a round trip to database.
            return await _context.Users
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<AppUser> GetUserByUsername(string username)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            return await _context.Users
            .Include(p => p.Photos)
            .ToListAsync();
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }


        public async Task<PagedList<MemberDto>> GetMembers(UserParams userParams)
        {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            var usersQuery = _context.Users.AsQueryable();
            usersQuery = usersQuery.Where(u => u.Id != userParams.UserId);
            usersQuery = usersQuery.Where(u => u.Gender == userParams.Gender);
            usersQuery = usersQuery.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            usersQuery = userParams.OrderBy switch
            {
                "created" => usersQuery.OrderByDescending(x => x.Created),
                _ => usersQuery.OrderByDescending(x => x.LastActive)
            };

            var query = usersQuery
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<MemberDto> GetMemberById(int id)
        {
            return await _context.Users
            .Where(x => x.Id == id)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }
        public async Task<MemberDto> GetMemberByUsername(string username)
        {
            return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
        }


        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}