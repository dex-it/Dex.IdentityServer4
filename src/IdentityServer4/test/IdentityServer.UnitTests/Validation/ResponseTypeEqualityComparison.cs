// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Validation;

/// <summary>
/// Tests for ResponseTypeEqualityComparer
/// </summary>
/// <remarks>
/// Some of these are pretty fundamental equality checks, but the purpose here is to ensure the
/// important property: that the order is insignificant when multiple values are
/// sent in a space-delimited string.  We want to ensure that property holds and at the same time
/// the basic equality function works as well.
/// </remarks>
public class ResponseTypeEqualityComparison
{
    /// <summary>
    /// These tests ensure that single-value strings compare with the
    /// same behavior as default string comparisons.
    /// </summary>
    public class SingleValueStringComparisons
    {
        [Fact]
        public void Both_null()
        {
            var comparer = new ResponseTypeEqualityComparer();
            string x = null;
            string y = null;
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }

        [Fact]
        public void Left_null_other_not()
        {
            var comparer = new ResponseTypeEqualityComparer();
            string x = null;
            var y = string.Empty;
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }

        [Fact]
        public void Right_null_other_not()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = string.Empty;
            string y = null;
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }

        [Fact]
        public void token_token()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "token";
            var y = "token";
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }

        [Fact]
        public void id_token_id_token()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "id_token";
            var y = "id_token";
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }

        [Fact]
        public void id_token_token()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "id_token";
            var y = "token";
            var result = comparer.Equals(x, y);
            var expected = (x == y);
            result.Should().Be(expected);
        }
    }

    /// <summary>
    /// These tests ensure the property demanded by the 
    /// <see href="https://tools.ietf.org/html/rfc6749#section-3.1.1">OAuth2 spec</see>
    /// where, in a space-delimited list of values, the order is not important.
    /// </summary>
    public class MultipleValueStringComparisons
    {
        [Fact]
        public void id_token_token_both_ways()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "id_token token";
            var y = "token id_token";
            var result = comparer.Equals(x, y);
            result.Should().BeTrue();
        }

        [Fact]
        public void code_id_token_both_ways()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token";
            var y = "id_token code";
            var result = comparer.Equals(x, y);
            result.Should().BeTrue();
        }

        [Fact]
        public void code_token_both_ways()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code token";
            var y = "token code";
            var result = comparer.Equals(x, y);
            result.Should().BeTrue();
        }

        [Fact]
        public void code_id_token_token_combo1()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token token";
            var y = "id_token code token";
            var result = comparer.Equals(x, y);
            result.Should().BeTrue();
        }

        [Fact]
        public void code_id_token_token_combo2()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token token";
            var y = "token id_token code";
            var result = comparer.Equals(x, y);
            result.Should().BeTrue();
        }

        [Fact]
        public void code_id_token_token_missing_code()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token token";
            var y = "id_token token";
            var result = comparer.Equals(x, y);
            result.Should().BeFalse();
        }

        [Fact]
        public void code_id_token_token_missing_code_and_token()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token token";
            var y = "id_token";
            var result = comparer.Equals(x, y);
            result.Should().BeFalse();
        }

        [Fact]
        public void Totally_different_words()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "blerg smoo";
            var y = "token code";
            var result = comparer.Equals(x, y);
            result.Should().BeFalse();
        }

        [Fact]
        public void Same_length_different_count()
        {
            var comparer = new ResponseTypeEqualityComparer();
            var x = "code id_token token";
            var y = "tokenizer bleegerfi";
            var result = comparer.Equals(x, y);
            result.Should().BeFalse();
        }
    }
}