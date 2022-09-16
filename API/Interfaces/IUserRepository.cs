
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser> GetUserById(int id);
        Task<AppUser> GetUserByUsername(string username);
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<bool> SaveAll();


        Task<PagedList<MemberDto>> GetMembers(UserParams paginationParams);

        Task<MemberDto> GetMemberById(int id);
        Task<MemberDto> GetMemberByUsername(string username);

    }
}