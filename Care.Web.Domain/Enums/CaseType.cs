namespace Care.Web.Domain.Enums;

public enum CaseType
{
    /// <summary>
    /// Incident: An unplanned event that disrupts or reduces the quality of a service and may require an immediate response
    /// </summary>
    Incident,
    /// <summary>
    /// Request: A formal user request for a new feature to be provided. Requests always has the severity None
    /// </summary>
    Request,
    /// <summary>
    /// Support: A user has questions about or needs help to use the service/product
    /// </summary>
    Support,
}