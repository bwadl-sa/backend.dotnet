using Bwadl.Application.Common.DTOs;
using Bwadl.API.Models.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Xunit;

namespace Bwadl.Tests.Integration;

public class ApiVersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiVersioningTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUsers_V1_Should_Return_Simple_List()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<List<UserDto>>("/api/v1/users");

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<List<UserDto>>();
    }

    [Fact]
    public async Task GetUsers_V2_Should_Return_Paged_Response()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<PagedResponse<UserDto>>("/api/v2/users?page=1&pageSize=5");

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<PagedResponse<UserDto>>();
        response!.Page.Should().Be(1);
        response.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetUsers_Default_Should_Use_V1()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - No version specified, should default to V1
        var response = await client.GetFromJsonAsync<List<UserDto>>("/api/users");

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<List<UserDto>>();
    }

    [Fact]
    public async Task GetUsers_QueryString_Versioning_Should_Work()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Version via query string
        var response = await client.GetFromJsonAsync<PagedResponse<UserDto>>("/api/users?version=2.0&page=1&pageSize=3");

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<PagedResponse<UserDto>>();
        response!.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task GetUsers_Header_Versioning_Should_Work()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Version", "2.0");

        // Act - Version via header
        var response = await client.GetFromJsonAsync<PagedResponse<UserDto>>("/api/users?page=1&pageSize=2");

        // Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<PagedResponse<UserDto>>();
        response!.PageSize.Should().Be(2);
    }
}
