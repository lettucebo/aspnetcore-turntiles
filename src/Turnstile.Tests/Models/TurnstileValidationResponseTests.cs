﻿using AspNetCore.Turnstile.Core;
using NUnit.Framework;

namespace Turnstile.Tests.Models
{
    [TestFixture]
    public class TurnstileValidationResponseTests
    {
        [TestCase("missing-input-secret", ValidationError.MissingInputSecret)]
        [TestCase("invalid-input-secret", ValidationError.InvalidInputSecret)]
        [TestCase("missing-input-response", ValidationError.MissingInputResponse)]
        [TestCase("invalid-input-response", ValidationError.InvalidInputResponse)]
        [TestCase("bad-request", ValidationError.BadRequest)]
        [TestCase("timeout-or-duplicate", ValidationError.TimeoutOrDuplicate)]
        [TestCase("request-failed", ValidationError.HttpRequestFailed)]
        [TestCase("anything", ValidationError.Undefined)]
        public void Errors_ShouldReturnCorrectEnum_ForErrorMessage(string errorMessage, ValidationError expected)
        {
            // Arrange
            var response = new ValidationResponse()
            {
                ErrorMessages = new List<string>() { errorMessage }
            };

            // Act


            // Assert
            Assert.AreEqual(1, response.Errors.Count());
            Assert.AreEqual(expected, response.Errors.First());
        }
    }
}
