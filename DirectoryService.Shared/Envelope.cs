namespace DirectoryService.Shared;

public record Envelope
{
    //public static ObjectResult ToResponse(IEnumerable<ValidationFailure> errors)
    //=> EnvelopeErrors<FieldError>.ToValidationResponse(errors);

    public static EnvelopeErrors<T> Error<T>(IEnumerable<T> errors)
    => EnvelopeErrors<T>.Create(errors);
    public static Envelope<T> Ok<T>(T? result)
    => new Envelope<T>(result);
}

public record Envelope<T>
{
    public T? Result { get; }
    public DateTime? TimeGenerated { get; }
    public object? Errors { get; } = null;
    internal Envelope(T? result)
    {
        Result = result;
        TimeGenerated = DateTime.Now;
    }
}

public record EnvelopeErrors<TErr> : Envelope<TErr>
{
    new public List<TErr> Errors { get; }

    private EnvelopeErrors(TErr? result, IEnumerable<TErr> errors) : base(result)
    {
        Errors = errors.ToList();
    }

    public static EnvelopeErrors<TErr> Create(IEnumerable<TErr> errors)
        => new EnvelopeErrors<TErr>(default, errors);

    //internal static ObjectResult ToValidationResponse(IEnumerable<ValidationFailure> errors)
    //{
    //    List<FieldError> fieldErrors = [];

    //    foreach (var error in errors)
    //        fieldErrors.Add(new FieldError(error.ErrorCode ?? "value.is.invalid", error.ErrorMessage, error.PropertyName));

    //    var envelope = new EnvelopeErrors<FieldError>(null, fieldErrors);

    //    return new ObjectResult(envelope) { StatusCode = StatusCodes.Status400BadRequest };
    //}
}

public record FieldError(
    string? ErrorCode,
    string? ErrorMessage,
    string? InvalidField);
