using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Laso.Identity.Core.IntegrationEvents;
using Laso.Identity.Core.Mediator;
using Laso.Identity.Core.Persistence;
using Laso.Identity.Domain.Entities;
using Laso.IntegrationEvents;

namespace Laso.Identity.Core.Partners.Commands
{
    public class CreatePartnerCommand : ICommand<string>, IInputValidator
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string PublicKey { get; set; }
        public string NormalizedName { get; set; }

        public ValidationResult ValidateInput()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(Name))
            {
                result.AddFailure(nameof(Name), "Field is required");
            }

            return result;
        }
    }

    public class CreatePartnerHandler : CommandHandler<CreatePartnerCommand, string>
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly IEventPublisher _eventPublisher;

        public CreatePartnerHandler(ITableStorageService tableStorageService, IEventPublisher eventPublisher)
        {
            _tableStorageService = tableStorageService;
            _eventPublisher = eventPublisher;
        }

        public override async Task<Response<string>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
        {
            var normalizedName = new string((request.NormalizedName ?? request.Name).ToLower()
                .Where(char.IsLetterOrDigit)
                .SkipWhile(char.IsDigit)
                .ToArray());

            var existingPartner = await _tableStorageService.FindAllAsync<Partner>(x => x.NormalizedName == normalizedName, 1);

            if (existingPartner.Any())
            {
                return Failed(nameof(Partner.NormalizedName), "A partner with the same normalized name already exists");
            }
            
            var partner = new Partner
            {
                Name = request.Name,
                ContactName = request.ContactName,
                ContactPhone = request.ContactPhone,
                ContactEmail = request.ContactEmail,
                PublicKey = request.PublicKey,
                NormalizedName = normalizedName
            };
            
            await _tableStorageService.InsertAsync(partner);
            
            await _eventPublisher.Publish(new PartnerCreatedEventV1
            {
                Id = partner.Id,
                Name = partner.Name,
                NormalizedName = partner.NormalizedName
            });
            
            return Succeeded(partner.Id);
        }
    }
}