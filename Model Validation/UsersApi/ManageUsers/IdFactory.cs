// Copyright (c) 2025 - TillW
// Licensed to you under the MIT License

namespace UsersApi.ManageUsers;

public sealed class IdFactory : IIdFactory
{
	public Guid Create() => Guid.CreateVersion7();
}
