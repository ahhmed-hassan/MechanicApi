
using ErrorOr;

namespace MechanicApplication.Common.Extensions;

public static class ErrorOrExtensions
{
    public static async Task<ErrorOr<TValue>> FailIfAsync<TValue>(
    this ErrorOr<TValue> errorOr,
    Func<TValue, Task<bool>> onValue,
    Error error)
    {
        if (errorOr.IsError)
        {
            return errorOr;
        }

        bool result = await onValue(errorOr.Value);

        return errorOr.FailIf(_ => result, error);
    }
    public static async Task<ErrorOr<TValue>> FailIfAsync<TValue>(
        this Task<ErrorOr<TValue>> errorOrTask,
        Func<TValue, Task<bool>> onValue,
        Error error)
    {

        var errorOr = await errorOrTask;
        if (errorOr.IsError)
        {
            return errorOr;
        }
        bool result = await onValue(errorOr.Value);
        return errorOr.FailIf(_ => result, error);
    }

    public static async Task<ErrorOr<TValue>> FailIf<TValue>(
        this Task<ErrorOr<TValue>> errorOrTask,
        Func<TValue, bool> onValue,
        Error error)
    {
        var errorOr = await errorOrTask;
        if (errorOr.IsError)
        {
            return errorOr;
        }
        bool result = onValue(errorOr.Value);
        return errorOr.FailIf(_ => result, error);


    }
}

   //public ErrorOr<TValue> FailIf(Func<TValue, bool> onValue, Error error)
   // {
   //     if (IsError)
   //     {
   //         return this;
   //     }

   //     if (!onValue(Value))
   //     {
   //         return this;
   //     }

   //     return error;
   // }
