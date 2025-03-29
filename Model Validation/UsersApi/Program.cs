// Copyright (c) 2025 - TillW
// Licensed to you under the MIT License

using Microsoft.AspNetCore.Mvc;
using UsersApi.ManageUsers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IIdFactory, IdFactory>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
	options.InvalidModelStateResponseFactory = actionContext =>
	new ObjectResult(new Dictionary<string, string> { { "type", "urn:UsersApi/bad-request" } })
	{
		StatusCode = 400,
		ContentTypes = { "application/problem+json" }
	}
);

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
