
using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<AppUser> GetUserById(int id);
        Task<AppUser> GetUserByUsername(string username);
        Task<bool> SaveAll();


        Task<IEnumerable<MemberDto>> GetMembers();
        Task<MemberDto> GetMemeberByUsername(string username);
    }
}