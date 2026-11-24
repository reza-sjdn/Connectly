namespace Connectly.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
}
