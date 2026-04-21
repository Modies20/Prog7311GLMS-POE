using GLMS.Data.Entities;

namespace GLMS.Web.Services;

public interface IContractValidationService
{
    bool CanCreateServiceRequest(Contract? contract);
    string GetValidationErrorMessage(Contract? contract);
    bool IsContractActive(Contract? contract);
}