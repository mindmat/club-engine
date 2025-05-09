﻿using AppEngine.Authorization.UsersInPartition;
using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppEngine.Authentication.Users;

public class User : Entity
{
    public ICollection<UserInPartition>? Partitions { get; set; }

    public required IdentityProvider IdentityProvider { get; set; }
    public required string IdentityProviderUserIdentifier { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}

public class UserMap : EntityMap<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(usr => usr.IdentityProviderUserIdentifier)
               .HasMaxLength(200);

        builder.Property(usr => usr.FirstName)
               .HasMaxLength(200);

        builder.Property(usr => usr.LastName)
               .HasMaxLength(200);

        builder.Property(usr => usr.Email)
               .HasMaxLength(200);

        builder.Property(usr => usr.AvatarUrl)
               .HasMaxLength(500);
    }
}