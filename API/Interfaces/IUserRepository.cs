
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<AppUser> GetUserById(int id);
        Task<AppUser> GetUserByUsername(string username);
        Task<bool> SaveAll();


        Task<PagedList<MemberDto>> GetMembers(UserParams paginationParams);
        Task<MemberDto> GetMemberByUsername(string username);
    }
}