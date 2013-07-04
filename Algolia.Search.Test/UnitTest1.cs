using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Algolia.Search.Test
{
    [TestClass]
    public class AlgoliaClientTest
    {
        private static string _testApplicationKey = "";
        private static string _testApiKey = "";
        private static string[] _hosts = new string[] { "", "", "" };

        [TestMethod]
        public async Task TestMethod1()
        {
            AlgoliaClient client = new AlgoliaClient(_testApplicationKey, _testApiKey, _hosts );
            var result = await client.ListIndexes();
            Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        }
    }
}
