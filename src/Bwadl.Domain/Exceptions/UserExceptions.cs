namespace Bwadl.Domain.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID '{userId}' was not found.")
    {
    }

    public UserNotFoundException(string email) 
        : base($"User with email '{email}' was not found.")
    {
    }
}

public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email) 
        : base($"A user with email '{email}' already exists.")
    {
    }
}
