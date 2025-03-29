// Copyright (c) 2025 - TillW
// Licensed to you under the MIT License

using Mapster;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UsersApi.ManageUsers;

[ApiController]
[Route("v2/Users")]
public sealed class UsersController : ControllerBase
{
	private readonly IIdFactory _idFactory;

	public UsersController(IIdFactory idFactory)
	{
		_idFactory = idFactory;
	}

    [HttpPost]
	public IActionResult Create([FromBody] CreateUserRequest payload)
	{
		var user = payload.Adapt<CreateUserResponse>();
		user.Id = _idFactory.Create();
		return Created($"/v2/Users/{user.Id}", user);
    }

    public sealed record CreateUserRequest
    {
        [EmailAddress]
        public required string UserName { get; set; }
        public required string Password { get; set; }
		[RegularExpression("^(New|Migrated|Unknown)$")]
		public string State { get; set; } = "New";
    }

	private sealed record CreateUserResponse
	{
		public required Guid Id { get; set; }
		public required string UserName { get; set; }
		public required string Password { get; set; }
		public required string State { get; set; }
	}
}
