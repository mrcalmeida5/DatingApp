using API.DTOs;
using API.Extensions;
using API.Helpers;
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
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await _userRepository.GetUserById(User.GetUserId());
            userParams.UserId = user.Id;
            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "male" ? "female" : "male";

            var users = await _userRepository.GetMembers(userParams);

            Response.AddPaginationHeader(users.CurrrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }



        [HttpGet("{id:int}", Name ="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(int id)
        {
            return await _userRepository.GetMemberById(id);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdate)
        {
            var user = await _userRepository.GetUserById(User.GetUserId());
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
            var user = await _userRepository.GetUserById(User.GetUserId());
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
            var user = await _userRepository.GetUserById(User.GetUserId());

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
            var user = await _userRepository.GetUserById(User.GetUserId());
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