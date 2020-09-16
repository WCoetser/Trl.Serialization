using System;
using System.IO;
using Trl.Serialization.Tests.TestObjects;
using Xunit;

namespace Trl.Serialization.Tests
{
    public class NameAndTypeMappingsTests
    {
        [Fact]
        public void ShouldTestTypeAssignability()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            nameAndTypeMappings.MapTermNameToType<FileInfo>("term1");

            // Act & Assert
            Assert.Throws<Exception>(() =>
            {
                _ = nameAndTypeMappings.GetTypeForTermName("term1", typeof(DateTime));
            });
        }

        [Fact]
        public void ShouldNotMapSameTermNameMoreThanOnce()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            nameAndTypeMappings.MapTermNameToType<FileInfo>("term1");

            // Act & Assert
            Assert.Throws<Exception>(() =>
            {
                nameAndTypeMappings.MapTermNameToType<DateTime>("term1");
            });
        }

        [Fact]
        public void ShouldSubstituteMappedTypeOnDeserialize()
        {
            // Arrange            
            var nameAndTypeMappings = new NameAndTypeMappings();            
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);

            // Act
            var input = "root: Person<ContactInfo>(ContactInfo<Name>(\"Sarel\"));";
            nameAndTypeMappings.MapTermNameToType<PhoneInfo>("ContactInfo");
            var output = serializer.Deserialize<Person>(input);

            // Assert
            Assert.IsType<PhoneInfo>(output.ContactInfo);
            Assert.True(StringComparer.InvariantCulture.Equals(output.ContactInfo.Name, "Sarel"));
        }
    }
}
