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
            Assert.True(StringComparer.InvariantCulture.Equals("root: ContactInfoRenamed(null,\"Sarel\");", output));
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

        [Fact]
        public void ShouldCallDefaultConstructorZeroArgsNoFieldMappingsGiven()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);
            var input = "root: person();";

            // Act
            nameAndTypeMappings.MapTermNameToType<Person>("person");
            var output = serializer.Deserialize<Person>(input);

            // Assert
            Assert.NotNull(output);
        }

        [Fact]
        public void ShouldCallNonDefaultConstructorNoFieldMappingsGiven()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);
            var input = "root: datetime(2020,10,10);";            

            // Act
            nameAndTypeMappings.MapTermNameToType<DateTime>("datetime");
            var output = serializer.Deserialize<DateTime>(input);

            // Assert
            Assert.Equal(2020, output.Year);
            Assert.Equal(10, output.Month);
            Assert.Equal(10, output.Day);
        }

        [Fact]
        public void ShouldGetBestDeconstructorFromExtensionMethods()
        {
            // Arrange
            NameAndTypeMappings mappings = new NameAndTypeMappings();

            // Act
            mappings.MapExtensionMethodDestructorsFromType(typeof(DateTimeExtensions));
            var deconstructor = mappings.GetBestDeconstructorMethodForAcTerm(typeof(DateTime));

            // Assert
            Assert.Equal(4, deconstructor.GetParameters().Length);
        }

        [Fact]
        public void ShouldThrowExceptionWhenLoadExtensionMethodDestructorsFromTypeNotExtensionClass()
        {
            // Arrange
            NameAndTypeMappings mappings = new NameAndTypeMappings();

            // Act & Assert
            Assert.Throws<Exception>(() => mappings.MapExtensionMethodDestructorsFromType(typeof(DateTime)));
        }

        [Fact]
        public void ShouldUseLongestDeconstructorToCreateNonAcTermForExtensionMethod()
        {
            // Arrange
            var mappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: mappings);
            mappings.MapExtensionMethodDestructorsFromType(typeof(DateTimeExtensions));
            mappings.MapTermNameToType<DateTime>("datetime");
            var input = new DateTime(2020,10,13);

            // Act
            string deserialized = serializer.Serialize<DateTime>(input);

            // Assert
            Assert.Equal("root: datetime(2020,10,13);", deserialized);
        }

        [Fact]
        public void ShouldUseLongestDeconstructorToCreateNonAcTermForNonExtensionMethod()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);
            var input = new PhoneInfo
            {
                Name = "Test T",
                PhoneNumber = "1234567890"
            };

            // Act
            nameAndTypeMappings.MapTermNameToType<PhoneInfo>("phone");
            var output = serializer.Serialize(input);

            // Assert
            Assert.Equal("root: phone(\"1234567890\",\"Test T\");", output);
        }


        [Fact]
        public void ShouldSupportDeconstructorWithZeroArgs()
        {
            // Arrange
            var nameAndTypeMappings = new NameAndTypeMappings();
            var serializer = new StringSerializer(nameAndTypeMappings: nameAndTypeMappings);
            var input = new RandomValue();

            // Act            
            var output = serializer.Serialize(input);

            // Assert
            Assert.Equal("root: RandomValue();", output);
        }
    }
}
