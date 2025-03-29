// Copyright (c) 2025 - TillW
// Licensed to you under the MIT License

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UsersApi.ManageUsers;

namespace UsersApi.Tests;

public class IntegrationTests
{
	private readonly IIdFactory _idFactory;
	private readonly MyWebApplicationFactory _appFactory;
	private readonly Guid _userId = Guid.CreateVersion7();

	public IntegrationTests()
	{
		_idFactory = Substitute.For<IIdFactory>();
		_idFactory.Create().Returns(_userId);
		_appFactory = new MyWebApplicationFactory(_idFactory);
	}

	[Fact]
	public async Task CreateUser()
	{
		const string username = "alice@example.com";
		const string password = "wonderland123!";
		using var client = _appFactory.CreateClient();

		var response = await client.PostAsJsonAsync("/v2/Users", CreatePayload(username, password));

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
		var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
		Assert.NotNull(responseBody);
		Assert.Equal(_userId.ToString(), responseBody["id"].GetString());
		Assert.Equal(username, responseBody["userName"].GetString());
		Assert.Equal(password, responseBody["password"].GetString());
		Assert.Equal("New", responseBody["state"].GetString());
	}

	[Fact]
	public async Task CreateUser_SetState()
	{
		const string username = "alice@example.com";
		const string password = "wonderland123!";
		const string state = "Migrated";
		using var client = _appFactory.CreateClient();

		var response = await client.PostAsJsonAsync("/v2/Users", CreatePayload(username, password, state));

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
		var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
		Assert.NotNull(responseBody);
		Assert.Equal(_userId.ToString(), responseBody["id"].GetString());
		Assert.Equal(username, responseBody["userName"].GetString());
		Assert.Equal(password, responseBody["password"].GetString());
		Assert.Equal(state, responseBody["state"].GetString());
	}

	[Fact]
	public async Task CreateUser_InvalidEmail()
	{
		using var client = _appFactory.CreateClient();
		const string username = "no email";
		const string password = "wonderland123!";

		var response = await client.PostAsJsonAsync("/v2/Users", CreatePayload(username, password));

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
		var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
		Assert.NotNull(responseBody);
		Assert.Equal("urn:UsersApi/bad-request", responseBody["type"].GetString());
	}

	[Fact]
	public async Task CreateUser_InvalidState()
	{
		using var client = _appFactory.CreateClient();
		var response = await client.PostAsJsonAsync("/v2/Users", CreatePayload("bob@example.com", "metamorphosator!", "deleted"));

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
		var responseBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
		Assert.NotNull(responseBody);
		Assert.Equal("urn:UsersApi/bad-request", responseBody["type"].GetString());
	}

	private static Object CreatePayload(string username, string password)
		=> new Dictionary<string, string>
		{
			{ "userName", username },
			{ "password", password },
		};

	private static Object CreatePayload(string username, string password, string state)
		=> new Dictionary<string, string>
		{
			{ "userName", username },
			{ "password", password },
			{ "state", state },
		};

	private class MyWebApplicationFactory(IIdFactory idFactory) : WebApplicationFactory<TestingApplication>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IIdFactory));
				if (descriptor != null)
				{
					services.Remove(descriptor);
				}

				services.AddSingleton(idFactory);
			});
		}
	}
}
