namespace DirectoryService.Shared.ErrorClasses;

public class Error
{
    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        if (type == ErrorType.Not_set)
            throw new ArgumentException("Error was created with no ErrorType set!");

        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Validation(string message) =>
         new Error("validation.failed", message, ErrorType.Validation);

    public static Error NotFound(string message) =>
         new Error("value.not.found", message, ErrorType.NotFound);

    public static Error Failure(string message, string code = "failure") =>
         new Error(code, message, ErrorType.Failure);
}

public enum ErrorType
{
    Not_set,
    Validation = 422,
    NotFound = 404,
    Failure = 500,
    Conflict,
}
