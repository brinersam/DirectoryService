namespace DirectoryService.Shared.ErrorClasses;

public static class Errors
{
    public static class General
    {
        public static Error ValueIsEmptyOrNull<T>(T? value, string valueName)
        {
            string emptyOrNull = value == null ? "null" : "empty";

            return Error.Validation($"Value {valueName} of type {typeof(T).Name} cannot be {emptyOrNull}!");
        }

        public static Error ValueIsInvalid<T>(T value, string valueName) =>
            Error.Validation
            ($"Value {valueName} of type {typeof(T).Name} is invalid!");

        public static Error NotFound(Type type, Guid? id = null) =>
            Error.NotFound
            ($"Record with id {id} of type {type.Name} was not found!");

        public static Error ValueLengthMoreThan<T>(T? value, string valueName, int len)
            => ValueLength(value, "more", valueName, len);

        public static Error ValueLengthLessThan<T>(T? value, string valueName, int len)
            => ValueLength(value, "less", valueName, len);

        private static Error ValueLength<T>(T? value, string lessmore, string valueName, int len)
            => Error.Validation
                ($"Value {valueName} of type {typeof(T).Name} cannot be {lessmore} than {len} elements long");

    }

    public static class Database
    {
        public static Error DatabaseError() =>
            Error.Failure
            ($"Database operation error!");

        public static Error TransactionError() =>
            Error.Failure
            ($"Transaction error!");

        public static Error DBRowsAffectedError<T>(int affectedRows, int expectedRows) =>
            Error.Failure
            ($"Failure during DB operation with type {typeof(T)}. Rolls affected/expected: {affectedRows}/{expectedRows}");
    }
}
