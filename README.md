## Synopsis

NRetry Framework, an abstraction layer for retryable operations, is meant to simplify and standardize the requests to operations which may need, if they fail, to be re-attempted a number of times. By allowing NRetry to take care of the looping, failure detection, optional recovery action and optional exception handling, your calling code is greatly simplified. You can even call your retrieble operation asynchronously.

<!---This description should match descriptions added for package managers (Gemspec, package.json, etc.)-->

## Code Example

In the  most trivial example below, :
```csharp
var retryer = new TaskRetryer {
  RetryableOperation = MyOperation,
  ExceptionLogger = (args) =>
    Console.WriteLine("Exception:\n" + args.Exception),
};

// Maximum of 3 attempts and one minute wait between attempts
retryer.Config.MaximumAttempts = 3;
retryer.Config.RetryInterval = 60*1000;

// Trying out attempts. The last attempt may throw an exception, as it won't be wrapped with a try-catch.
bool success = retryer.ProcessRetries();
```
For further examples, check the [Examples Page](docs/Examples.md) or check to the code in the unit test project.
<!---Show what the library does as concisely as possible, developers should be able to figure out **how** your project solves their problem by looking at the code example. Make sure the API you are showing off is obvious, and that your code is short and concise.-->

## Motivation

This project was motivated by my experience with a project following [Component-based software engineering](https://en.wikipedia.org/wiki/Component-based_software_engineering#Software_component), were each component was called over a timeline. And further usage of it was found in a data-intensive  project following [SOA architecture](https://en.wikipedia.org/wiki/Service-oriented_architecture), were a few different internal services were exposed in multiple redundant servers. There it is used in almost 100 different places and the framework came in handy to standardize the way other developers write calls to those services, assuring correct usage of the server redundancy in order to sustain the system's robustness.

<!---
## Installation

Provide code examples and explanations of how to get the project.

## API Reference

Depending on the size of the project, if it is small and simple enough the reference docs can be added to the README. For medium size to larger projects it is important to at least provide a link to where the API reference docs live.

## Tests

Describe and show how to run the tests with code examples.

## Contributors

Let people know how they can dive into the project, include important links to things like issue trackers, irc, twitter accounts if applicable.
-->

## License

The MIT License allows you to anything what you need with this code, as long as you provide attribution back to me and don't hold me liable.
