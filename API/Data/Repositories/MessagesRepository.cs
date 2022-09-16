

using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class MessagesRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessagesRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessages(MessagesParams messagesParams)
        {
            var query = _context.Messages
            .OrderByDescending(m => m.MessageSent)
            .AsQueryable();

            query = messagesParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientId == messagesParams.UserId
                                        && !m.RecipientDeleted),
                "Outbox" => query.Where(m => m.SenderId == messagesParams.UserId
                                        && !m.SenderDeleted),
                _ => query.Where(m => m.RecipientId == messagesParams.UserId
                                        && !m.RecipientDeleted  && m.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messagesParams.PageNumber, messagesParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(int currentUserId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender).ThenInclude(u => u.Photos)
                .Where(m => (m.SenderId == currentUserId && !m.SenderDeleted && m.RecipientId == recipientId)
                         || (m.RecipientId == currentUserId && !m.RecipientDeleted && m.SenderId == recipientId))
                .OrderBy(m => m.MessageSent)
                .ToListAsync();

            var unreadMessages = messages
                .Where(m => m.DateRead == null && m.RecipientId == currentUserId)
                .ToList();
            if (unreadMessages.Any())
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.Now);
                await SaveAll();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}