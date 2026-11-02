namespace Connectly.Application.Common.Exceptions;

public class NotFoundException(string name, object key)
    : Exception($"{name} ({key}) was not found.");

public class ConflictException(string message) : Exception(message);

public class AppUnauthorizedException(string message = "Unauthorized.") : Exception(message);

public class ForbiddenException(string message = "Forbidden.") : Exception(message);
