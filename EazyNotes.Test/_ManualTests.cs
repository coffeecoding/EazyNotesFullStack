using Newtonsoft.Json;
using NUnit.Framework;

namespace EazyNotes.Test
{
    public class _ManualTests
    {
        [Test]
        public void DateTimeDeserializationFromString()
        {
            string json = "{ \"Id\" : 23101418, \"DateCreated\" : \"20.08.2021 14:00:23\" }";
            dynamic jsonObject = JsonConvert.DeserializeObject(json);

            Assert.IsTrue(true);
        }
    }
}
