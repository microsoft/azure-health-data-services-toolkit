using Capl;
using Capl.Matching;
using Capl.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Claims;

namespace CaplTests
{
    [TestClass]
    public class CaplUnitTests
    {
        [TestMethod]
        public void Match_Literal_True_Test()
        {
            string type = "#Literal";
            string claimType = "http://example.org/claim/name";
            string claimValue = "foo";
            bool required = true;
            LiteralMatch literal = new(claimType, claimValue, true);
            string json = JsonConvert.SerializeObject(literal);
            LiteralMatch expected = JsonConvert.DeserializeObject<LiteralMatch>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(claimType, expected.ClaimType);
            Assert.AreEqual(claimValue, expected.Value);
            Assert.AreEqual(required, expected.Required);

        }

        [TestMethod]
        public void Match_Literal_False_ClaimType_Test()
        {
            string type = "#Literal";
            string claimType = "http://example.org/claim/name";
            string claimValue = "foo";
            string actualClaimType = "http://example.org/claim/name1";
            bool required = true;
            LiteralMatch literal = new(claimType, claimValue, true);
            string json = JsonConvert.SerializeObject(literal);
            LiteralMatch expected = JsonConvert.DeserializeObject<LiteralMatch>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(required, expected.Required);
            Assert.AreNotEqual(actualClaimType, expected.ClaimType);
        }

        [TestMethod]
        public void Match_Literal_False_ClaimValue_Test()
        {
            string type = "#Literal";
            string claimType = "http://example.org/claim/name";
            string claimValue = "foo";
            string actualClaimValue = "foo1";
            bool required = true;
            LiteralMatch literal = new(claimType, claimValue, true);
            string json = JsonConvert.SerializeObject(literal);
            LiteralMatch expected = JsonConvert.DeserializeObject<LiteralMatch>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(claimType, expected.ClaimType);
            Assert.AreEqual(required, expected.Required);
            Assert.AreNotEqual(actualClaimValue, expected.Value);
        }

        [TestMethod]
        public void Match_AbstractMatch_Test()
        {
            string type = "#Literal";
            string claimType = "http://example.org/claim/name";
            string claimValue = "foo";
            bool required = true;
            LiteralMatch literal = new(claimType, claimValue, true);
            Match literalMatch = literal;
            string json = JsonConvert.SerializeObject(literalMatch);
            Match expected = JsonConvert.DeserializeObject<Match>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(claimType, expected.ClaimType);
            Assert.AreEqual(claimValue, expected.Value);
            Assert.AreEqual(required, expected.Required);
        }

        [TestMethod]
        public void Operations_Contains_Test()
        {
            string type = "#Contains";
            string value = "bar";
            ContainsOperation contains = new(value);
            string json = JsonConvert.SerializeObject(contains);
            ContainsOperation expected = JsonConvert.DeserializeObject<ContainsOperation>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(value, expected.Value);
        }

        [TestMethod]
        public void Operations_AbstractOperation_Test()
        {
            string type = "#Contains";
            string value = "bar";
            ContainsOperation contains = new(value);
            Operation op = contains;
            string json = JsonConvert.SerializeObject(op);
            Operation expected = JsonConvert.DeserializeObject<Operation>(json);
            Assert.AreEqual(type, expected.Type);
            Assert.AreEqual(value, expected.Value);
        }


        [TestMethod]
        public void Rule_Test()
        {

            string claimType = "http://example.org/claim/name";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, claimValue, true);

            string value = "bar";
            ContainsOperation contains = new(value);
            Operation op = contains;

            Rule rule = new(true, literal, contains);
            string json = JsonConvert.SerializeObject(rule);
            Rule expect = JsonConvert.DeserializeObject<Rule>(json);
            Assert.IsTrue(expect.Eval);
            Assert.AreEqual(op.Type, expect.OperationExp.Type);
            Assert.AreEqual(op.Value, expect.OperationExp.Value);
            Assert.AreEqual(literal.Type, expect.MatchExp.Type);
            Assert.AreEqual(literal.Required, expect.MatchExp.Required);
            Assert.AreEqual(literal.ClaimType, expect.MatchExp.ClaimType);
            Assert.AreEqual(literal.Value, expect.MatchExp.Value);
        }

        [TestMethod]
        public void LogicalAnd_True_Test()
        {
            string claimType1 = "http://example.org/claim/role1";
            string claimValue1 = "foo1";
            LiteralMatch literal1 = new(claimType1, null, true);

            ContainsOperation contains1 = new(claimValue1);

            Rule rule1 = new(true, literal1, contains1);

            string claimType2 = "http://example.org/claim/role2";
            string claimValue2 = "foo2";
            LiteralMatch literal2 = new(claimType2, null, true);

            ContainsOperation contains2 = new(claimValue2);

            Rule rule2 = new(true, literal2, contains2);

            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });


            string json = JsonConvert.SerializeObject(logicalAnd);
            LogicalAnd actual = JsonConvert.DeserializeObject<LogicalAnd>(json);
            Claim[] claims = new Claim[] { new Claim(claimType1, claimValue1), new Claim(claimType2, claimValue2) };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_True_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, true);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, claimValue) };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Deserialize_SimpleDefault_Test()
        {
            string filename = $"../../../Assets/simpleDefault.json";
            string content = File.ReadAllText(filename);
            Policy policy = JsonConvert.DeserializeObject<Policy>(content);
            Assert.AreEqual("ABC", policy.Id);
            Assert.AreEqual(true, policy.EvaluationExp.Eval);
            Assert.AreEqual("#Rule", policy.EvaluationExp.Type);
            Rule rule = (Rule)policy.EvaluationExp;
            Assert.AreEqual("#EqualCaseSensitive", rule.OperationExp.Type);
            Assert.AreEqual("reader", rule.OperationExp.Value);
            Assert.AreEqual(true, rule.MatchExp.Required);
            Assert.AreEqual("roles", rule.MatchExp.ClaimType);

            _ = JsonConvert.SerializeObject(policy);

        }

        [TestMethod]
        public void Policy_Rule_True_MultiClaimType_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, true);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, "hi"), new Claim(claimType, "there"), new Claim(claimType, claimValue) };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_True_MultiClaimType_Negation_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, true);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(false, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, "hi"), new Claim(claimType, "there"), new Claim(claimType, "boo") };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_True_NotRequired_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, false);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, claimValue) };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_False_NotRequired_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, false);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, "boom") };
            Assert.IsFalse(actual.Evaluate(claims));
        }


        [TestMethod]
        public void Policy_Rule_True_NotRequired_Negation_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, false);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(false, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, "boom") };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_False_NotRequired_Negation_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, false);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(false, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, claimValue) };
            Assert.IsFalse(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_True_NoRequiredEmpty_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo";
            LiteralMatch literal = new(claimType, null, false);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim($"{claimType}/blah", claimValue) };
            Assert.IsTrue(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_Rule_False_Test()
        {
            string claimType = "http://example.org/claim/role1";
            string claimValue = "foo1";
            LiteralMatch literal = new(claimType, null, true);
            EqualCaseSensitiveOperation op = new(claimValue);
            Rule rule = new(true, literal, op);
            Policy policy = new("MyPolicy", rule);
            string json = JsonConvert.SerializeObject(policy);
            Policy actual = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType, "boom") };
            Assert.IsFalse(actual.Evaluate(claims));
        }

        [TestMethod]
        public void Policy_WithLogicalAnd_Test()
        {
            string claimType1 = "http://example.org/claim/role1";
            LiteralMatch literal1 = new(claimType1, null, true);

            string value1 = "bar1";
            ContainsOperation contains1 = new(value1);

            Rule rule1 = new(true, literal1, contains1);

            string claimType2 = "http://example.org/claim/role2";
            LiteralMatch literal2 = new(claimType2, null, true);

            string value2 = "bar2";
            ContainsOperation contains2 = new(value2);

            Rule rule2 = new(true, literal2, contains2);

            LogicalOr logicalAnd = new(true, new Term[] { rule1, rule2 });

            Policy policy = new("MyPolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy);
            _ = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType1, "boom"), new Claim(claimType2, value2) };
            Assert.IsNotNull(json);
            Assert.IsTrue(policy.Evaluate(claims));

        }

        [TestMethod]
        public void Policy_WithLogicalAnd_Negation_Test()
        {
            string claimType1 = "http://example.org/claim/role1";
            LiteralMatch literal1 = new(claimType1, null, true);

            string value1 = "bar1";
            ContainsOperation contains1 = new(value1);

            Rule rule1 = new(true, literal1, contains1);

            string claimType2 = "http://example.org/claim/role2";
            LiteralMatch literal2 = new(claimType2, null, true);

            string value2 = "bar2";
            ContainsOperation contains2 = new(value2);

            Rule rule2 = new(true, literal2, contains2);

            LogicalOr logicalAnd = new(false, new Term[] { rule1, rule2 });

            Policy policy = new("MyPolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy);
            _ = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType1, "boom"), new Claim(claimType2, "boom") };
            Assert.IsNotNull(json);
            Assert.IsTrue(policy.Evaluate(claims));

        }

        [TestMethod]
        public void Policy_WithLogicalOr_Test()
        {
            string claimType1 = "http://example.org/claim/role1";
            LiteralMatch literal1 = new(claimType1, null, true);

            string value1 = "bar1";
            ContainsOperation contains1 = new(value1);

            Rule rule1 = new(true, literal1, contains1);

            string claimType2 = "http://example.org/claim/role2";
            LiteralMatch literal2 = new(claimType2, null, true);

            string value2 = "bar2";
            ContainsOperation contains2 = new(value2);

            Rule rule2 = new(true, literal2, contains2);

            LogicalAnd logicalOr = new(true, new Term[] { rule1, rule2 });

            Policy policy = new("MyPolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy);
            _ = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType1, value1), new Claim(claimType2, value2) };
            Assert.IsNotNull(json);
            Assert.IsTrue(policy.Evaluate(claims));

        }

        [TestMethod]
        public void Policy_WithLogicalOr_Negation_Test()
        {
            string claimType1 = "http://example.org/claim/role1";
            LiteralMatch literal1 = new(claimType1, null, true);

            string value1 = "bar1";
            ContainsOperation contains1 = new(value1);

            Rule rule1 = new(true, literal1, contains1);

            string claimType2 = "http://example.org/claim/role2";
            LiteralMatch literal2 = new(claimType2, null, true);

            string value2 = "bar2";
            ContainsOperation contains2 = new(value2);

            Rule rule2 = new(true, literal2, contains2);

            LogicalAnd logicalOr = new(false, new Term[] { rule1, rule2 });

            Policy policy = new("MyPolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy);
            _ = JsonConvert.DeserializeObject<Policy>(json);
            Claim[] claims = new Claim[] { new Claim(claimType1, "boom"), new Claim(claimType2, "kaboom") };
            Assert.IsNotNull(json);
            Assert.IsTrue(policy.Evaluate(claims));
        }

        #region LogicalOr serialzation tests

        [TestMethod]
        public void Serialize_LogicalOr_CaseInsensitive_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "StateAll" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "$State" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_CaseSensitive_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "StateAll" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "$State" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_CaseBetweenDateTime_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("limitTime", true), new BetweenDateTimeOperation() { Value = DateTime.Now.ToLongDateString() });
            Rule rule2 = new(true, new LiteralMatch("limitTime", true), new BetweenDateTimeOperation() { Value = DateTime.Now.ToLongDateString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_BetweenExclusiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("exc", true), new BetweenDateTimeOperation() { Value = "1 3" });
            Rule rule2 = new(true, new LiteralMatch("exc", true), new BetweenDateTimeOperation() { Value = "5 7" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_BetweenInclusiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("exc", true), new BetweenInclusiveOperation() { Value = "1 3" });
            Rule rule2 = new(true, new LiteralMatch("exc", true), new BetweenInclusiveOperation() { Value = "5 7" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_ContainsOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("con", true), new ContainsOperation() { Value = "x" });
            Rule rule2 = new(true, new LiteralMatch("con", true), new ContainsOperation() { Value = "y" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_EqualCaseInsensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "bar" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_EqualCaseSensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "bar" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_EqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_EqualNumericOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("level", true), new EqualNumericOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("level", true), new EqualNumericOperation() { Value = "2" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_ExistsOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("level", true), new ExistsOperation() { Value = "x" });
            Rule rule2 = new(true, new LiteralMatch("level", true), new ExistsOperation() { Value = "y" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_GreaterThanDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("time", true), new GreaterThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("time", true), new GreaterThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_GreaterThanOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new GreaterThanOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new GreaterThanOperation() { Value = "2" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_GreaterThanOrEqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_GreaterThanOrEqualOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualOperation() { Value = "2" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_LessThanDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new LessThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new LessThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_LessThanOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new LessThanOperation() { Value = "3" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new LessThanOperation() { Value = "4" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_LessThanOrEqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new LessThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new LessThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_LessThanOrEqualOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new LessThanOrEqualOperation() { Value = "3" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new LessThanOrEqualOperation() { Value = "4" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_NotEqualCaseInsensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("val", true), new NotEqualCaseInsensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("val", true), new NotEqualCaseInsensitiveOperation() { Value = "bar" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalOr_NotEqualCaseSensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("val", true), new NotEqualCaseSensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("val", true), new NotEqualCaseSensitiveOperation() { Value = "bar" });
            LogicalOr logicalOr = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalOr);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        #endregion

        #region logicaland serialization tests

        [TestMethod]
        public void Serialize_LogicalAnd_CaseInsensitive_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "StateAll" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "$State" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_CaseSensitive_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "StateAll" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "$State" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_CaseBetweenDateTime_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("limitTime", true), new BetweenDateTimeOperation() { Value = DateTime.Now.ToLongDateString() });
            Rule rule2 = new(true, new LiteralMatch("limitTime", true), new BetweenDateTimeOperation() { Value = DateTime.Now.ToLongDateString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_BetweenExclusiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("exc", true), new BetweenDateTimeOperation() { Value = "1 3" });
            Rule rule2 = new(true, new LiteralMatch("exc", true), new BetweenDateTimeOperation() { Value = "5 7" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_BetweenInclusiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("exc", true), new BetweenInclusiveOperation() { Value = "1 3" });
            Rule rule2 = new(true, new LiteralMatch("exc", true), new BetweenInclusiveOperation() { Value = "5 7" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_ContainsOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("con", true), new ContainsOperation() { Value = "x" });
            Rule rule2 = new(true, new LiteralMatch("con", true), new ContainsOperation() { Value = "y" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_EqualCaseInsensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseInsensitiveOperation() { Value = "bar" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_EqualCaseSensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualCaseSensitiveOperation() { Value = "bar" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_EqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("roles", true), new EqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("roles", true), new EqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_EqualNumericOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("level", true), new EqualNumericOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("level", true), new EqualNumericOperation() { Value = "2" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_ExistsOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("level", true), new ExistsOperation() { Value = "x" });
            Rule rule2 = new(true, new LiteralMatch("level", true), new ExistsOperation() { Value = "y" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_GreaterThanDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("time", true), new GreaterThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("time", true), new GreaterThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_GreaterThanOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new GreaterThanOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new GreaterThanOperation() { Value = "2" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_GreaterThanOrEqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_GreaterThanOrEqualOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualOperation() { Value = "1" });
            Rule rule2 = new(true, new LiteralMatch("date", true), new GreaterThanOrEqualOperation() { Value = "2" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_LessThanDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new LessThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new LessThanDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_LessThanOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new LessThanOperation() { Value = "3" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new LessThanOperation() { Value = "4" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_LessThanOrEqualDateTimeOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("date", true), new LessThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            Rule rule2 = new(true, new LiteralMatch("date", true), new LessThanOrEqualDateTimeOperation() { Value = DateTime.Now.ToString() });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_LessThanOrEqualOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("num", true), new LessThanOrEqualOperation() { Value = "3" });
            Rule rule2 = new(true, new LiteralMatch("num", true), new LessThanOrEqualOperation() { Value = "4" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_NotEqualCaseInsensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("val", true), new NotEqualCaseInsensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("val", true), new NotEqualCaseInsensitiveOperation() { Value = "bar" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialize_LogicalAnd_NotEqualCaseSensitiveOperation_Test()
        {
            Rule rule1 = new(true, new LiteralMatch("val", true), new NotEqualCaseSensitiveOperation() { Value = "foo" });
            Rule rule2 = new(true, new LiteralMatch("val", true), new NotEqualCaseSensitiveOperation() { Value = "bar" });
            LogicalAnd logicalAnd = new(true, new Term[] { rule1, rule2 });
            Policy policy = new("testpolicy", logicalAnd);

            string json = JsonConvert.SerializeObject(policy, Formatting.None);
            Assert.IsNotNull(json);
        }

        #endregion
    }
}
