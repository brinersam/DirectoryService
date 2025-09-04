using DirectoryService.Shared.ErrorClasses;
using System.Collections;
using System.Text.RegularExpressions;

namespace DirectoryService.Shared.Validator;
public class ModelValidator
{
    private readonly Queue<IValidationUnit> _validationOptions = new();

    public ModelValidator()
    {}

    public ModelValidationUnit<T> Validate<T>(T value, string valueName)
        where T : class
    {
        var unit = new ModelValidationUnit<T>(value, valueName);
        _validationOptions.Enqueue(unit);
        return unit;
    }

    public List<List<Error>> ValidateAll(out bool IsError)
    {
        IsError = true;
        var errors = new List<List<Error>>();

        foreach(var unit in _validationOptions)
        {
            var valueErrors = unit.Validate();
            if (valueErrors.Count > 0)
                errors.Add(valueErrors);
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        IsError = false;
        return errors;
    }
}


internal interface IValidationUnit
{
    public List<Error> Validate();
}

public class ModelValidationUnit<T> : IValidationUnit
    where T: class
{
    private readonly T _value;
    private readonly string _paramName;
    private readonly Queue<Func<T, Error?>> _checks = new();

    internal ModelValidationUnit(
        T value,
        string paramName = null!)
    {
        _value = value;
        _paramName = paramName;
    }

    public List<Error> Validate()
    {
        var errors = new List<Error>();

        foreach (var check in _checks)
        {
            var result = check.Invoke(_value);
            if (result != null)
                errors.Add(result);
        }

        return errors;
    }

    private string ErrStringNotNullOrEmpty()
        => $"Value should not be null or empty!";

    public ModelValidationUnit<T> NotNullOrEmpty()
    {
        _checks.Enqueue((x) =>
        {
            if (x is null || x == default)
                return ValidationErr(ErrStringNotNullOrEmpty());

            if (x is string xString)
            {
                if (String.IsNullOrWhiteSpace(xString))
                    return ValidationErr(ErrStringNotNullOrEmpty());
            }

            if (x is ICollection xCollection)
            {
                if (xCollection.Count <= 0)
                    return ValidationErr(ErrStringNotNullOrEmpty());
            }

            return null;
        });

        return this;
    }

    private string ErrStringMinLength(int length)
        => $"Value should have minimal length of {length}!";
    public ModelValidationUnit<T> MinLength(int minLengthInclusive) // x or more is ok
    {
        _checks.Enqueue((x) =>
        {
            if (x is string xString)
            {
                if (xString.Length < minLengthInclusive)
                    return ValidationErr(ErrStringMinLength(minLengthInclusive));
            }

            if (x is ICollection xCollection)
            {
                if (xCollection.Count < minLengthInclusive)
                    return ValidationErr(ErrStringMinLength(minLengthInclusive));
            }

            //if (typeof(T).IsArray)
            //{
            //    var xArray = x as Array;
            //    if (xArray!.Length < minLengthInclusive)
            //        return ValidationErr(x, ErrStringMinLength(minLengthInclusive));
            //}

            return null;
        });

        return this;
    }

    private string ErrStringMaxLength(int length)
        => $"Value should have maximum length of {length}!";
    public ModelValidationUnit<T> MaxLength(int maxLengthInclusive) // x or less is ok
    {
        _checks.Enqueue((x) =>
        {
            if (x is string xString)
            {
                if (xString.Length > maxLengthInclusive)
                    return ValidationErr(ErrStringMaxLength(maxLengthInclusive));
            }

            if (x is ICollection xCollection)
            {
                if (xCollection.Count > maxLengthInclusive)
                    return ValidationErr(ErrStringMaxLength(maxLengthInclusive));
            }

            return null;
        });

        return this;
    }

    public ModelValidationUnit<T> HasFormat(FormatRulesEnum rules)
    {
        _checks.Enqueue((x) =>
        {
            if (x is string xString)
            {
                if (rules == FormatRulesEnum.Latin)
                {
                    if (!Regex.IsMatch(xString, @"^[\p{IsBasicLatin}]+$"))
                        return ValidationErr($"{_paramName}: String must be latin characters only!");
                }
            }

            return null;
        });

        return this;
    }

    private string ErrStringContainsNone<TParam>(IEnumerable<TParam> objects)
        => $"Value should not have any of: {String.Join(":", objects)}!";
    public ModelValidationUnit<T> ContainsNone<TParam>(params TParam[] objects)
    {
        _checks.Enqueue((x) =>
        {
            if (x is string xString && objects.Length > 0 && objects[0] is char)
            {
                var hashSet = objects.Cast<char>().ToHashSet();
                foreach (var ch in xString)
                {
                    if (hashSet.Contains(ch))
                        return ValidationErr(ErrStringContainsNone(hashSet));
                }
            }

            if (x is IList xCollection && typeof(TParam) == typeof(object))
            {
                foreach (var obj in objects)
                {
                    if (xCollection.Contains(obj))
                        return ValidationErr(ErrStringContainsNone(objects));
                }
            }

            return null;
        });

        return this;
    }

    private Error ValidationErr(string message)
    {
        return Error.Validation($"Value [{_paramName}]: {message}");
    }
}

public enum FormatRulesEnum
{
    Latin,
}
