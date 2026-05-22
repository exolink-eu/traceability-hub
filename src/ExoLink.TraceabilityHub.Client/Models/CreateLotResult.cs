namespace ExoLink.TraceabilityHub.Client.Models;

public enum CreateLotResult
{
    Created,
    InvalidInput,
    DuplicateLotId,
    InvalidRecipientFormat,
    RecipientNotFound,
}
