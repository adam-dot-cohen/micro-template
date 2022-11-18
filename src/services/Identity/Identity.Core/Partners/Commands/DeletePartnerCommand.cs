using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Command;
using Infrastructure.Mediation.Validation;
using Laso.Identity.Domain.Entities;
using Laso.TableStorage;

namespace Laso.Identity.Core.Partners.Commands
{
    public class DeletePartnerCommand : ICommand, IInputValidator
    {
        public string PartnerId { get; set; }

        public ValidationResult ValidateInput()
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(PartnerId))
            {
                result.AddFailure(nameof(PartnerId), $"{nameof(PartnerId)} must supply valid Guid string");
            }
            else
            {
                if (!Guid.TryParse(PartnerId, out var whoCares))
                    result.AddFailure(nameof(PartnerId), $"{nameof(PartnerId)} must be a valid Guid.");
            }

            return result;
        }
    }

    public class DeletePartnerHandler : CommandHandler<DeletePartnerCommand>
    {
        private readonly ITableStorageService _tableStorageService;

        public DeletePartnerHandler(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public override async Task<CommandResponse> Handle(DeletePartnerCommand request, CancellationToken cancellationToken)
        {
            var partner = await _tableStorageService.GetAsync<Partner>(request.PartnerId);
            if (partner == null)
                return Succeeded();

            await _tableStorageService.DeleteAsync(partner);
            return Succeeded();
        }
    }
}
