
## TaskRetryer

Suppose you have this method to test:
```csharp
private int _kidsCount = 0; // Number of kids I have
private const int DesiredKids = 3; // Number of kids I wish to

public bool CheckKidsCount() {
    if (_kidsCount < DesiredKids) {
        // Trying to make kids with my wife
        int newKid = (new Random().Next() % 2); // 0 to 1 kids being made
        _kidsCount += newKid;

        if (newKid == 0)
            throw new OperationCanceledException("Need to make more kids !\n");

        if (_kidsCount < DesiredKids) {
            TestContext.WriteLine("Not enough kids yet, wait for next opportunity.\n");
            return false;
        }
    }
    if (_kidsCount == DesiredKids) {
        TestContext.WriteLine("Success !\n");
        return true;
    }
    TestContext.WriteLine("Too many kids, give up !\n");
    return false;
}
```

First step is to prepare your retryer instance:
```csharp
var retryer = new TaskRetryer {
    RetryableOperation = CheckKidsCount,
    ExceptionLogger = (args) => TestContext.WriteLine("Exception message: " + args.Exception.Message),
    Config = new RetryConfig() {
        MaximumAttempts = 0, // Unlimited attempts
        RetryInterval = 900, // Representing 9 months
    },
};
```
Now you can call, just once, the `ProcessRetries` method instead of the `CheckKidsCount` method, as below:
```csharp
bool success = retryer.ProcessRetries();
```
In this example, all exceptions or false value returns will be ignored, and the re-attempts will continue untill your method returns true. Though we usually want to restrict the number of attempts:
```csharp
retryer.Config.MaximumAttempts = 5;
```
Now, if all 5 attempts fail and the last one throws an exception, that last exception won't be captured and you'll be able to capture it from your code without any interference from NRetry framework.

If still you want NRetry to handle a possible exception in the last attempt, then call this method instead:

```csharp
Exception error;
bool success = retryer.ProcessRetries(out error);
```

## CallRetryer

When your method to be re-attempted does not return any value, the failure is only detected through excpetions. In such case, you may use `CallRetryer`. First, you configure one instance (just like you'd do for `TaskRetryer`), but providing a void return method as the `RetryableOperation`. Then you just call it exactly like `TaskRetryer`.

```csharp
retryer.ProcessRetries(out error);
```

## ProcessRetryer&lt;T>

In case your method returns anything other than a boolean value, you should then use `ProcessRetryer<T>` class, were T is the type your method returns.

Unless otherwise specified, the default value of T will be interpreted as a failure. Such default is determined by `default(T)` and represents `false` for boolean, `0` for number and date/time types, `null` for class types, and default values for a struct object.

To provide a specific return value as failure indicator, define a `FailureDetectionMethod` as `ByReturnValue` and define the `FailureReturnValue` you wish:
```csharp
var retryer = new ProcessRetryer<int> {
    FailureDetectionMethod = FailureDetection.ByReturnValue,
    FailureReturnValue = -1,
};
```

Or, if there is a range of values which may indicate failure, or it is something in particular in the return value, you'll have to create your own failure detector method instead:
```csharp
var retryer = new ProcessRetryer<int> {
  FailureDetectionMethod = FailureDetection.ByFailureDetector,
  FailureDetector = (i) => i < 0,
};
```

---

Pending documentation for:
- recoverer delegate
- static contexts
- asynchronous calls
