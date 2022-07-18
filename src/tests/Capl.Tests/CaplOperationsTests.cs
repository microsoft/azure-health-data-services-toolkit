using Capl.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

namespace CaplTests
{
    [TestClass]
    public class CaplOperationsTests
    {
        public CaplOperationsTests()
        {

        }

        [TestMethod]
        public void Operations_EqualCaseSensitive_True_Test()
        {
            string rhs = "Foo";
            string lhs = "Foo";
            EqualCaseSensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not equal");
        }

        [TestMethod]
        public void Operations_EqualCaseSensitive_False_Test()
        {
            string rhs = "Foo";
            string lhs = "foo";
            EqualCaseSensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "equal");
        }

        [TestMethod]
        public void Operations_EqualCaseInsensitive_True_Test()
        {
            string rhs = "Foo";
            string lhs = "foo";
            EqualCaseInsensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not equal");
        }

        [TestMethod]
        public void Operations_EqualCaseInsensitive_False_Test()
        {
            string rhs = "Foo";
            string lhs = "bar";
            EqualCaseInsensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "equal");
        }


        [TestMethod]
        public void Operations_EqualNumeric_True_Test()
        {
            string rhs = "1.1";
            string lhs = "1.10";
            EqualNumericOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not equal");

        }

        [TestMethod]
        public void Operations_EqualNumeric_False_Test()
        {
            string rhs = "1.1";
            string lhs = "1.01";
            EqualNumericOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "equal");
        }

        [TestMethod]
        public void Operations_GreaterThan_True_Test()
        {
            string rhs = "1.1";
            string lhs = "2.2";
            GreaterThanOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not greater than");
        }

        [TestMethod]
        public void Operations_GreaterThan_False_Test()
        {
            string rhs = "1.1";
            string lhs = "1.01";
            GreaterThanOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "greater than");
        }

        [TestMethod]
        public void Operations_LessThan_True_Test()
        {
            string rhs = "1.1";
            string lhs = "1.01";
            LessThanOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not less than");
        }

        [TestMethod]
        public void Operations_LessThan_False_Test()
        {
            string rhs = "1.1";
            string lhs = "2.01";
            LessThanOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "less than");
        }

        [TestMethod]
        public void Operations_GreaterThanOrEqual_True_GreateThan_Test()
        {
            string rhs = "1.1";
            string lhs = "2.01";
            GreaterThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not greater than or equal");
        }

        [TestMethod]
        public void Operations_GreaterThanOrEqual_True_Equal_Test()
        {
            string rhs = "1.1";
            string lhs = "1.1";
            GreaterThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not greater than or equal");
        }

        [TestMethod]
        public void Operations_GreaterThanOrEqual_False__Test()
        {
            string rhs = "1.1";
            string lhs = "1.01";
            GreaterThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "greater than or equal");
        }



        [TestMethod]
        public void Operations_LessThanOrEqual_True_GreateThan_Test()
        {
            string rhs = "1.1";
            string lhs = "0.91";
            LessThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not greater than or equal");
        }

        [TestMethod]
        public void Operations_LessThanOrEqual_True_Equal_Test()
        {
            string rhs = "1.1";
            string lhs = "1.1";
            LessThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not greater than or equal");
        }

        [TestMethod]
        public void Operations_LessThanOrEqual_False__Test()
        {
            string rhs = "1.1";
            string lhs = "1.2";
            LessThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "greater than or equal");
        }


        [TestMethod]
        public void Operations_BetweenInclusive_True_Equal_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.15";
            BetweenInclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not inclusive");
        }

        [TestMethod]
        public void Operations_BetweenInclusive_WithEqual_True_Equal_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.1";
            BetweenInclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not inclusive");
        }

        [TestMethod]
        public void Operations__BetweenInclusive_False_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.05";
            BetweenInclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "inclusive");
        }

        [TestMethod]
        public void Operations_LessThanOrEqual_False_Test()
        {
            string rhs = "1.1";
            string lhs = "1.2";
            LessThanOrEqualOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "greater than or equal");
        }


        [TestMethod]
        public void Operations_BetweenExclusive_True_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.15";
            BetweenExclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs), "not between");
        }

        [TestMethod]
        public void Operations_BetweenInclusive_WithEqual_False_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.1";
            BetweenExclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "between");
        }

        [TestMethod]
        public void Operations_BetweenExclusive_False_Test()
        {
            string rhs = "1.1 1.2";
            string lhs = "1.05";
            BetweenExclusiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs), "between");
        }

        [TestMethod]
        public void Operations_Contains_True_Test()
        {
            string lhs = "brown fox";
            ContainsOperation op = new("quick brown fox");
            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_Contains_False_Test()
        {
            string lhs = "green fox";
            ContainsOperation op = new("quick brown fox");
            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_Exists_True_Test()
        {
            string lhs = "foo";
            ExistsOperation op = new();
            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_Exists_False_Test()
        {
            string lhs = null;
            ExistsOperation op = new();
            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanDateTime_True_Test()
        {
            string lhs = DateTime.Now.AddMinutes(10).ToString();
            GreaterThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanDateTime_WithJson_True_Test()
        {
            string lhs = JsonConvert.SerializeObject(DateTime.Now.AddMinutes(10));
            GreaterThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanDateTime_False_Test()
        {
            string lhs = DateTime.Now.ToString();
            GreaterThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.AddMinutes(10).ToString()
            };

            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanDateTime_WithJson_False_Test()
        {

            string lhs = JsonConvert.SerializeObject(DateTime.Now);
            GreaterThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.AddMinutes(10).ToString()
            };

            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_LessThanDateTime_True_Test()
        {
            string lhs = DateTime.Now.AddMinutes(-10).ToString();
            LessThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_LessThanDateTime_False_Test()
        {
            string lhs = DateTime.Now.AddMinutes(10).ToString();
            LessThanDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsFalse(op.Execute(lhs));
        }


        [TestMethod]
        public void Operations_GreaterThanOrEqualDateTime_True_Test()
        {
            string lhs = DateTime.Now.AddMinutes(10).ToString();
            GreaterThanOrEqualDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanOrEqualDateTime_False_Test()
        {
            string lhs = DateTime.Now.ToString();
            GreaterThanOrEqualDateTimeOperation op = new()
            {
                Value = DateTime.Now.AddMinutes(10).ToString()
            };

            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_GreaterThanOrEqualDateTime_WithEqual_True_Test()
        {
            string dateTime = DateTime.Now.ToString();
            string lhs = dateTime;
            GreaterThanOrEqualDateTimeOperation op = new()
            {
                Value = dateTime
            };

            Assert.IsTrue(op.Execute(lhs));
        }


        [TestMethod]
        public void Operations_LessThanOrEqualDateTime_True_Test()
        {
            string lhs = DateTime.Now.AddMinutes(-10).ToString();
            LessThanOrEqualDateTimeOperation op = new()
            {
                Value = DateTime.Now.ToString()
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_LessThanOrEqualDateTime_False_Test()
        {
            string lhs = DateTime.Now.ToString();
            LessThanOrEqualDateTimeOperation op = new()
            {
                Value = DateTime.Now.AddMinutes(-10).ToString()
            };

            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_LessThanOrEqualDateTime_WithEqual_True_Test()
        {
            string dateTime = DateTime.Now.ToString();
            string lhs = dateTime;
            LessThanOrEqualDateTimeOperation op = new()
            {
                Value = dateTime
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_NotEqualCaseSensative_True_Test()
        {
            string lhs = "Foo";
            string rhs = "foo";

            NotEqualCaseSensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_NotEqualCaseSensative_False_Test()
        {
            string lhs = "foo";
            string rhs = "foo";

            NotEqualCaseSensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_NotEqualCaseInsensative_True_Test()
        {
            string lhs = "Foo1";
            string rhs = "foo";

            NotEqualCaseInsensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsTrue(op.Execute(lhs));
        }

        [TestMethod]
        public void Operations_NotEqualCaseInsensative_False_Test()
        {
            string lhs = "Foo";
            string rhs = "foo";

            NotEqualCaseInsensitiveOperation op = new()
            {
                Value = rhs
            };

            Assert.IsFalse(op.Execute(lhs));
        }
    }
}
