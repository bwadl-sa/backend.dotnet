using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bwadl.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Starting validation for {RequestName}", requestName);

        if (_validators.Any())
        {
            _logger.LogInformation("Found {ValidatorCount} validators for {RequestName}", _validators.Count(), requestName);
            
            var context = new ValidationContext<TRequest>(request);
            
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                _logger.LogWarning("Validation failed for {RequestName} with {ErrorCount} errors: {Errors}", 
                    requestName, failures.Count, string.Join(", ", failures.Select(f => f.ErrorMessage)));
                throw new ValidationException(failures);
            }

            _logger.LogInformation("Validation passed for {RequestName}", requestName);
        }
        else
        {
            _logger.LogInformation("No validators found for {RequestName}", requestName);
        }

        _logger.LogInformation("Proceeding to next handler for {RequestName}", requestName);
        return await next();
    }
}
