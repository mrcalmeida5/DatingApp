using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository repository,
             IMapper mapper,
             IPhotoService photoService)
        {
            _userRepository = repository;
            _mapper = mapper;
            _photoService = photoService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            // var users = await _userRepository.GetUsers();
            // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);

            return Ok(await _userRepository.GetMembers());
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemeberByUsername(username);
        }

        // [HttpGet("{id:int}")]
        // public async Task<ActionResult<MemberDto>> GetUser(int id)
        // {
        //     var user = await _userRepository.GetUserById(id);
        //     return _mapper.Map<MemberDto>(user);
        //  }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdate)
        {
            var user = await _userRepository.GetUserByUsername(User.GetUsername());
            if (user != null)
            {
                _mapper.Map(memberUpdate, user);
                _userRepository.Update(user);

                if (await _userRepository.SaveAll())
                    return NoContent();
            }

            return BadRequest("Failed to update user");
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsername(User.GetUsername());
            var uploadResult = await _photoService.UploadPhoto(file);
            if (uploadResult.Error != null)
                return BadRequest(uploadResult.Error.Message);

            var photo = new Entities.Photo()
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId
            };

            if (user.Photos.Count == 0)
                photo.IsMain = true;
            user.Photos.Add(photo);

            if (await _userRepository.SaveAll())
                return CreatedAtRoute("GetUser",
                new { username = user.UserName },
                _mapper.Map<PhotoDto>(photo));

            return BadRequest("Problem adding photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsername(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo.IsMain)
                return BadRequest("This photo is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
                currentMain.IsMain = false;

            photo.IsMain = true;

            if (await _userRepository.SaveAll())
                return NoContent();

            return BadRequest("Failed to set main photo");
        }


        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsername(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
                return NotFound();

            if (photo.IsMain)
                return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var delResult = await _photoService.DeletePhoto(photo.PublicId);
                if (delResult.Error != null)
                    return BadRequest(delResult.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAll())
                return Ok();

            return BadRequest("Fail to delete the photo");
        }
    }
}