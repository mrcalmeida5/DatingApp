
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository,
        IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }



        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userId = User.GetUserId();

            if (userId == createMessageDto.RecipientId)
                return BadRequest("You cannot send messages to yourself");

            var sender = await _userRepository.GetUserById(userId);
            var recepient = await _userRepository.GetUserById(createMessageDto.RecipientId);

            if (recepient == null)
                return NotFound();

            var message = _mapper.Map<Message>(createMessageDto);
            message.Sender = sender;
            message.SenderKnownAs = sender.KnownAs;
            message.Recipient = recepient;
            message.RecipientKnownAs = recepient.KnownAs;
            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAll())
                return CreatedAtRoute("GetMessage", new { messageId = message.Id },
                _mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }


        [HttpGet("{messageId:int}", Name = "GetMessage")]
        public async Task<ActionResult<MessageDto>> GetMessage(int messageId)
        {
            var message = await _messageRepository.GetMessage(messageId);

            return Ok(_mapper.Map<MessageDto>(message));
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessages([FromQuery] MessagesParams messagesParams)
        {
            messagesParams.UserId = User.GetUserId();
            var messages = await _messageRepository.GetMessages(messagesParams);

            Response.AddPaginationHeader(messages.CurrrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{recipientId:int}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(int recipientId)
        {
            var currentUserId = User.GetUserId();
            return Ok(await _messageRepository.GetMessageThread(currentUserId, recipientId));
        }

        [HttpDelete("{messageId}")]
        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            var message = await _messageRepository.GetMessage(messageId);

            if (message == null) return NotFound();

            var userId = User.GetUserId();
            if (message.SenderId != userId && message.RecipientId != userId)
                return Unauthorized();

            if (message.SenderId == userId) message.SenderDeleted = true;
            if (message.RecipientId == userId) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAll())
                return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}