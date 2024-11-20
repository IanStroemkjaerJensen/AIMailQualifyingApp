namespace Care.Web.Domain.Enums;

public enum Severity
{
    /// <summary>
    /// Critical: The system is down or unavailable for all users
    /// </summary>
    Critical,
    /// <summary>
    /// Significantly: Essential operationally critical business processes cannot be performed as intended or multiple users do not have access to the system
    /// </summary>
    Significantly,
    /// <summary>
    /// Important: Important business processes cannot be performed as intended and cannot be handled with reasonable manual effort
    /// </summary>
    Important,
    /// <summary>
    /// Regular: Errors in the system that do not have a significant impact on daily operations and cannot be handled with a simple manual effort
    /// </summary>
    Regular,
    /// <summary>
    /// None: Cosmetic errors
    /// </summary>
    None
}