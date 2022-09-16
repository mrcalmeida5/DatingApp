
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository _likesRepository;
        private readonly IUserRepository _userRepository;

        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _userRepository = userRepository;
            _likesRepository = likesRepository;
        }

        [HttpPost("{likedUserId}")]
        public async Task<ActionResult> AddLike(int likedUserId)
        {
            var userId = User.GetUserId();
            var sourceUser = await _likesRepository.GetUserWithLikes(userId);
            var likedUser = await _userRepository.GetUserById(likedUserId);

            if (likedUser == null) return NotFound();

            var userLike = await _likesRepository.GetUserLike(sourceUser.Id, likedUser.Id);
            if (userLike != null) return BadRequest("You already like this user");

            if (sourceUser.Id == likedUserId) return BadRequest("You cannot like yourself");

            userLike = new UserLike
            {
                LikedUserId = likedUser.Id,
                SourceUserId = sourceUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SaveAll())
                return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrrentPage, users.PageSize, 
                users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}