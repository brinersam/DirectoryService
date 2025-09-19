using CSharpFunctionalExtensions;
using DirectoryService.Shared.ErrorClasses;
using FluentValidation;
using FluentValidation.Results;

namespace DirectoryService.Shared.Validator;
public static class FluentValidationExtensions
{
    public static IEnumerable<Error> ToAppErrors(this IEnumerable<ValidationFailure> errors)
        => errors.Select(error =>
                    Error.Validation($"Value [{error.PropertyName}]: {error.ErrorMessage}"));

    public static IRuleBuilderOptionsConditions<T, TBuilder> ValidateValueObj<T, TBuilder, TValueObject, TError>(
        this IRuleBuilder<T, TBuilder> ruleBuilder,
        Func<TBuilder, Result<TValueObject, TError>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, TError> result = factoryMethod(value);

            if (result.IsSuccess)
                return;

            var errors = result.Error as IEnumerable<Error> ?? new[] { result.Error as Error }!;

            if (errors is null)
                throw new NullReferenceException($"Error of type {result.Error!.GetType().Name} is not supported");

            foreach (var err in errors)
            {
                ValidationFailure failure = new()
                {
                    ErrorMessage = err.Message,
                    PropertyName = context.PropertyPath,
                    ErrorCode = err.Code,
                };

                context.AddFailure(failure);
            }
        });
    }
}
