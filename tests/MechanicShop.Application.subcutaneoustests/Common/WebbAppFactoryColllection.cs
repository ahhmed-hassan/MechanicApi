

using Xunit;

namespace MechanicApi.Application.subcutaneoustests.Common;


/// <summary>
/// Defines an xUnit collection used to share a single <see cref="WebAppFactory"/> instance
/// among all tests that declare the same collection name.
/// </summary>
/// <remarks>
/// xUnit collections group tests so they share fixtures and are not run in parallel with
/// other tests in the same collection. By implementing <see cref="ICollectionFixture{T}"/>,
/// this collection ensures one <see cref="WebAppFactory"/> is created and reused for
/// all tests that reference <see cref="CollectionName"/>. Use this collection by decorating
/// test classes with: <c>[Collection(WebbAppFactoryColllection.CollectionName)]</c>.
/// </remarks>

[CollectionDefinition(CollectionName)]
public class WebbAppFactoryColllection : ICollectionFixture<WebAppFactory>
{
    /// <summary>
    /// Logical name of the collection to be used on test classes.
    /// Example: <c>[Collection(WebbAppFactoryColllection.CollectionName)]</c>
    /// </summary>
    public const string CollectionName = "WebAppFactoryCollection";
}
