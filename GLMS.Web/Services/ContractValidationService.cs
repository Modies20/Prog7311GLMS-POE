using GLMS.Data.Entities;

namespace GLMS.Web.Services;

public class ContractValidationService : IContractValidationService
{
    public bool CanCreateServiceRequest(Contract? contract)
    {
        if (contract == null)
            return false;

        return contract.Status == ContractStatus.Active &&
               contract.StartDate <= DateTime.Today &&
               contract.EndDate >= DateTime.Today;
    }

    public bool IsContractActive(Contract? contract)
    {
        if (contract == null)
            return false;

        return contract.Status == ContractStatus.Active;
    }

    public string GetValidationErrorMessage(Contract? contract)
    {
        if (contract == null)
            return "Contract does not exist in the system.";

        if (contract.Status != ContractStatus.Active)
            return $"Contract '{contract.ContractNumber}' has status '{contract.Status}'. " +
                   "Service requests can only be created against Active contracts.";

        if (contract.StartDate > DateTime.Today)
            return $"Contract '{contract.ContractNumber}' starts on {contract.StartDate:yyyy-MM-dd}. " +
                   "Service requests cannot be created before the contract start date.";

        if (contract.EndDate < DateTime.Today)
            return $"Contract '{contract.ContractNumber}' expired on {contract.EndDate:yyyy-MM-dd}. " +
                   "Please contact the client to renew the contract.";

        return "Contract is valid for service requests.";
    }
}
