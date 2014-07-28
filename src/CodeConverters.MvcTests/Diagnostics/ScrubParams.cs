using System;
using System.Collections.Generic;
using System.Linq;
using CodeConverters.Core.Diagnostics;
using Xunit;

namespace CodeConverters.MvcTests.Diagnostics
{
    public class ScrubParams
    {
        [Fact]
        public void CanUpdateDefaultScrubParams()
        {
            var newParam = Guid.NewGuid().ToString();
            var originalParams = LoggingConfig.DefaultScrubParams;
            LoggingConfig.DefaultScrubParams = LoggingConfig.DefaultScrubParams.Concat(new[] { newParam }).ToArray();
            var newParams = LoggingConfig.DefaultScrubParams;
            Assert.NotEqual(originalParams, newParams);
            Assert.Contains(newParam, newParams);
        }

        [Fact]
        public void DoesNotUpdateTheOriginalDictionary()
        {
            var original = new Dictionary<string, string> {{"password", "abc123"}};

            var scrubbed = original.Scrub(LoggingConfig.DefaultScrubParams);
            Assert.Equal("******", scrubbed["password"]);
            Assert.Equal("abc123", original["password"]);

        }
    }
}