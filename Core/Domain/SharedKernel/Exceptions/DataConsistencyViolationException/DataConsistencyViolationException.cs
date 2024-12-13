using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;

[ExcludeFromCodeCoverage]
public class DataConsistencyViolationException(string message = "Data consistency violation") : Exception(message);