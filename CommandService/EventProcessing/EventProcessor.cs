using System.Text.Json;
using AutoMapper;
using CommandService.Dtos;
using CommandService.Data;
using CommandService.Models;

namespace CommandService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventProcessor> _logger;
        private readonly IMapper _mapper;

        public EventProcessor(
            IServiceScopeFactory scopeFactory,
            IMapper mapper,
            ILogger<EventProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message, _logger);

            switch(eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message, _logger);
                    break;
                default:
                    break;
            }
        }

        private static EventType DetermineEvent(string notificationMessage, ILogger<EventProcessor> logger)
        {
            logger.LogInformation("--> Determining Event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch(eventType.Event) 
            {
                case "Platform_Published":
                    logger.LogInformation("--> Platform Published Event Detected");
                    return EventType.PlatformPublished;
                default:
                    logger.LogInformation("--> Could not determine event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishedmessage, ILogger<EventProcessor> logger)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

            var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedmessage);

            try
            {
                var platform = _mapper.Map<Platform>(platformPublishedDto);

                if(!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                    repository.SaveChanges();

                    logger.LogInformation("--> Platform Added!");
                }
                else
                {
                    logger.LogInformation("--> Platform already exists...");
                }
            }
            catch(Exception ex)
            {
                logger.LogError("--> Could not add Platform to DB: {ex.Message}", ex.Message);
            }
        }
    }

    enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}