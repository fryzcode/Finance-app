using FinanceApp.Api.Application.Auth;
using FluentAssertions;
using Xunit;

namespace FinanceApp.Api.Tests.Auth;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_Works()
    {
        var hash = PasswordHasher.HashPassword("secret123");
        PasswordHasher.VerifyPassword("secret123", hash).Should().BeTrue();
        PasswordHasher.VerifyPassword("wrong", hash).Should().BeFalse();
    }
}


