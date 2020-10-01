using CascadeWorker.Shared;
using NUnit.Framework;

namespace CascadeWorker.Tests
{
    [TestFixture]
    public class IsValidSnapchatUsernameTest
    {
        [Test]
        public void IsValidSnapchatUsername_ReturnTrue(string username)
        {
            Assert.IsTrue(Utilities.IsValidSnapchatUsername(username), $"Username '{username}' should return true for IsValidSnapchatUsername.");
        }
    }
}