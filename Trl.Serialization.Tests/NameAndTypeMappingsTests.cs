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
        public void ShouldSubstituteMappedTypeOnSerialize()
        {
            // Arrange            
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);

            // Act
            var input = new PhoneInfo
            {
                Name = "Sarel"
            };
            nameAndTypeMappings.MapTermNameToType<PhoneInfo>("ContactInfoRenamed");
            var output = serializer.Serialize(input);

            // Assert            
            Assert.True(StringComparer.InvariantCulture.Equals("root: ContactInfoRenamed<Name>(\"Sarel\");", output));
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

        [Fact]
        public void ShouldNotMapSameConstantIdentifierMultipleTimes()
        {
            // Arrange            
            var nameAndTypeMappings = new NameAndTypeMappings();

            // Act & Assert
            Assert.Throws<Exception>(() =>
            {
                nameAndTypeMappings.MapIdentifierNameToConstant("Pi", Math.PI);
                nameAndTypeMappings.MapIdentifierNameToConstant("Pi", Math.PI);
            });
        }

        [InlineData(null, "null")]
        [InlineData(Math.PI, "Pi")]
        [InlineData("Point Nemo", "Location")]
        [Theory]
        public void ShouldSerializeConstant(object value, string identifier)
        {
            // Arrange            
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);

            // Act
            nameAndTypeMappings.MapIdentifierNameToConstant(identifier, value);
            var output = serializer.Serialize(value);

            // Assert
            Assert.True(StringComparer.InvariantCulture.Equals($"root: {identifier};", output));
        }

        [Fact]
        public void ShouldReturnNullIfConstantNotKnown()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();

            // Act
            var value = nameAndTypeMappings.GetIdentifierForConstantValue(123);

            // Assert
            Assert.Null(value);
        }

        [InlineData(null, "null")]
        [InlineData(Math.PI, "Pi")]
        [InlineData("Point Nemo", "Location")]
        [Theory]
        public void ShouldDeserializeConstant(object value, string identifier)
        {
            // Arrange            
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);
            var input = $"root: {identifier};";

            // Act
            nameAndTypeMappings.MapIdentifierNameToConstant(identifier, value);
            var output = serializer.Deserialize<object>(input);

            // Assert
            Assert.Equal(value, output);

        }
    }
}
