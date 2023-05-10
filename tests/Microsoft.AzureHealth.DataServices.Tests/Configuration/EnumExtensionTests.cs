using Microsoft.AzureHealth.DataServices.Configuration;
using Microsoft.AzureHealth.DataServices.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.AzureHealth.DataServices.Tests.Configuration
{
    public enum TestEnum
    {
        Test
    }


    [TestClass]
    public class EnumExtensionsTests
    {
        [TestMethod]
        public void GetDescription_ReturnsDescriptionAttribute()
        {
            // Arrange
            var operationType = FhirOperationType.Reindex;

            // Act
            var description = operationType.GetDescription();

            // Assert
            Assert.AreEqual("reindex", description);
        }

        [TestMethod]
        public void GetDescription_ReturnsNullIfAttributeNotFound()
        {
            // Arrange
            var operationType = TestEnum.Test;

            // Act
            var category = operationType.GetDescription();

            // Assert
            Assert.IsNull(category);
        }

        [TestMethod]
        public void GetCategory_ReturnsCategoryAttribute()
        {
            // Arrange
            var operationType = FhirOperationType.Reindex;

            // Act
            var category = operationType.GetCategory();

            // Assert
            Assert.AreEqual("async", category);
        }

        [TestMethod]
        public void GetCategory_ReturnsNullIfAttributeNotFound()
        {
            // Arrange
            var operationType = FhirOperationType.ConvertData;

            // Act
            var category = operationType.GetCategory();

            // Assert
            Assert.IsNull(category);
        }

        [TestMethod]
        public void GetValueFromDescription_ReturnsEnumValueFromDescription()
        {
            // Arrange

            // Act
            var operationType = EnumExtensions.GetValueFromDescription<FhirOperationType>("import");

            // Assert
            Assert.AreEqual(FhirOperationType.Import, operationType);
        }

        [TestMethod]
        public void GetValueFromDescription_ReturnsEnumValueFromName()
        {
            // Arrange

            // Act
            var operationType = EnumExtensions.GetValueFromDescription<FhirOperationType>("convert-data");

            // Assert
            Assert.AreEqual(FhirOperationType.ConvertData, operationType);
        }

        [TestMethod]
        public void GetValueFromDescription_ThrowsArgumentExceptionIfNoMatchingValue()
        {
            // Arrange

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => EnumExtensions.GetValueFromDescription<FhirOperationType>("Unknown"));
        }

        [TestMethod]
        public void GetValuesByDescription_ReturnsEnumerableOfMatchingEnumValues()
        {
            // Arrange

            // Act
            var asyncOperationTypes = EnumExtensions.GetValuesByDescription<FhirOperationType>("reindex");

            // Assert
            CollectionAssert.AreEquivalent(new[] { FhirOperationType.Reindex }, asyncOperationTypes.ToList());
        }

        [TestMethod]
        public void GetValuesByDescription_ReturnsEmptyEnumerableIfNoMatchingValues()
        {
            // Arrange

            // Act
            var unknownOperationTypes = EnumExtensions.GetValuesByDescription<FhirOperationType>("unknown");

            // Assert
            CollectionAssert.AreEqual(new FhirOperationType[0], unknownOperationTypes.ToList());
        }
    }

}
