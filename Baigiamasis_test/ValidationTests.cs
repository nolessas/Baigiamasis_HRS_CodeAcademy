using Xunit;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Baigiamasis.DTOs.HumanInformation;
using Baigiamasis.DTOs.User;
using Baigiamasis.DTOs.Auth;

namespace Baigiamasis_test
{
    public class ValidationTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void CreateHumanInformationDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new CreateHumanInformationDto
            {
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "39001010000",
                PhoneNumber = "+37061234567",
                Email = "test@gmail.com",
                Street = "Gedimino",
                City = "Vilnius",
                HouseNumber = "1",
                ApartmentNumber = "1"
            };

            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

            // Assert
            if (!isValid)
            {
                // Detailed error logging
                foreach (var validationResult in results)
                {
                    Console.WriteLine($"Property: {string.Join(", ", validationResult.MemberNames)}");
                    Console.WriteLine($"Error: {validationResult.ErrorMessage}");
                    Console.WriteLine("---");
                }
            }

            Assert.True(isValid, $"Validation failed with {results.Count} errors: {string.Join(", ", results.Select(r => r.ErrorMessage))}");
            Assert.Empty(results);
        }

        [Theory]
        [InlineData("", "LastName", "39001010000", "+37061234567", "test@gmail.com", "City", "Street", "1", "The FirstName field is required.")]
        [InlineData("FirstName", "", "39001010000", "+37061234567", "test@gmail.com", "City", "Street", "1", "The LastName field is required.")]
        [InlineData("FirstName", "LastName", "", "+37061234567", "test@gmail.com", "City", "Street", "1", "The PersonalCode field is required.")]
        [InlineData("FirstName", "LastName", "39001010000", "", "test@gmail.com", "City", "Street", "1", "The PhoneNumber field is required.")]
        [InlineData("FirstName", "LastName", "39001010000", "+37061234567", "", "City", "Street", "1", "The Email field is required.")]
        [InlineData("FirstName", "LastName", "39001010000", "+37061234567", "test@gmail.com", "", "Street", "1", "The City field is required.")]
        [InlineData("FirstName", "LastName", "39001010000", "+37061234567", "test@gmail.com", "City", "", "1", "The Street field is required.")]
        [InlineData("FirstName", "LastName", "39001010000", "+37061234567", "test@gmail.com", "City", "Street", "", "The HouseNumber field is required.")]
        public void CreateHumanInformationDto_InvalidData_FailsValidation(
            string firstName, string lastName, string personalCode, string phoneNumber,
            string email, string city, string street, string houseNumber, string expectedError)
        {
            // Arrange
            var dto = new CreateHumanInformationDto
            {
                FirstName = firstName,
                LastName = lastName,
                PersonalCode = personalCode,
                PhoneNumber = phoneNumber,
                Email = email,
                City = city,
                Street = street,
                HouseNumber = houseNumber
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            var errors = validationResults.Select(r => r.ErrorMessage).ToList();
            Assert.Contains(expectedError, errors);
        }

        [Theory]
        [InlineData("", false, "The Email field is required.")]
        [InlineData("invalid-email", false, "Invalid email format (only gmail.com and yahoo.com domains are allowed)")]
        [InlineData("test@gmail.com", true, null)]
        [InlineData("test@hotmail.com", false, "Invalid email format (only gmail.com and yahoo.com domains are allowed)")]
        [InlineData("test@yahoo.com", true, null)]
        public void Email_Validation_ChecksFormatAndDomain(string email, bool shouldBeValid, string expectedError)
        {
            // Arrange
            var model = new CreateHumanInformationDto 
            { 
                Email = email,
                // Add other required fields to make the model valid
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "39001010000",
                PhoneNumber = "+37061234567",
                Street = "Main Street",
                City = "Vilnius",
                HouseNumber = "1"
            };
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateProperty(
                model.Email,
                new ValidationContext(model) { MemberName = "Email" },
                results
            );

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid && expectedError != null)
            {
                Assert.Contains(results, r => r.ErrorMessage.Contains(expectedError));
            }
        }

        [Theory]
        [InlineData("", false, "The PhoneNumber field is required.")]
        [InlineData("+37061", false, "Phone number must be in format: +370xxxxxxxx")]
        [InlineData("+37061234567", true, null)]
        [InlineData("86123456789", false, "Phone number must be in format: +370xxxxxxxx")]
        public void PhoneNumber_Validation_ChecksLithuanianFormat(string phoneNumber, bool shouldBeValid, string expectedError)
        {
            // Arrange
            var model = new CreateHumanInformationDto 
            { 
                PhoneNumber = phoneNumber,
                // Add other required fields
                FirstName = "John",
                LastName = "Doe",
                Email = "test@gmail.com",
                PersonalCode = "39001010000",
                Street = "Main Street",
                City = "Vilnius",
                HouseNumber = "1"
            };
            
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateProperty(
                model.PhoneNumber,
                new ValidationContext(model) { MemberName = "PhoneNumber" },
                results
            );

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid && expectedError != null)
            {
                Assert.Contains(results, r => r.ErrorMessage.Contains(expectedError));
            }
        }

        [Theory]
        [InlineData("", false, "The PersonalCode field is required.")]
        [InlineData("390010100", false, "Invalid Lithuanian personal code format (11 digits starting with 3-6)")]
        [InlineData("39001010000", true, null)]
        [InlineData("3900101000012", false, "Invalid Lithuanian personal code format (11 digits starting with 3-6)")]
        [InlineData("ABCDEFGHIJK", false, "Invalid Lithuanian personal code format (11 digits starting with 3-6)")]
        public void PersonalCode_Validation_ChecksFormat(string personalCode, bool shouldBeValid, string expectedError)
        {
            // Arrange
            var model = new CreateHumanInformationDto 
            { 
                PersonalCode = personalCode,
                // Add other required fields
                FirstName = "John",
                LastName = "Doe",
                Email = "test@gmail.com",
                PhoneNumber = "+37061234567",
                Street = "Main Street",
                City = "Vilnius",
                HouseNumber = "1"
            };
            
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateProperty(
                model.PersonalCode,
                new ValidationContext(model) { MemberName = "PersonalCode" },
                results
            );

            // Assert
            Assert.Equal(shouldBeValid, isValid);
            if (!shouldBeValid && expectedError != null)
            {
                Assert.Contains(results, r => r.ErrorMessage.Contains(expectedError));
            }
        }

        [Theory]
        [InlineData("user", "Password123", true)]
        [InlineData("us", "Password123", false)]  // Username too short
        [InlineData("user", "pass", false)]       // Password too short
        [InlineData("", "Password123", false)]    // Empty username
        [InlineData("user", "", false)]           // Empty password
        public void UserRegistration_Validation(string username, string password, bool shouldBeValid)
        {
            // Arrange
            var dto = new UserRegistrationDto
            {
                Username = username,
                Password = password
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(validationResults);
            }
            else
            {
                Assert.NotEmpty(validationResults);
            }
        }
    }
} 