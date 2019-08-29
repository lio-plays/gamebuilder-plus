using GBWorldGen.Core.Algorithms.Generators;
using GBWorldGen.Core.Algorithms.Transformers;
using GBWorldGen.Core.Models;
using Xunit;

namespace GeneratorTests
{
    public class PerlinNoiseGeneratorTests
    {
        [Fact]
        public void PerlinNoiseGenerator_Creates_Valid_Map()
        {
            PerlinNoiseGenerator perlinNoiseGenerator = new PerlinNoiseGenerator(0, 0, 0, 20, 20);
            Block[] myMap = perlinNoiseGenerator.Generate();

            string serialized = Serializer.SerializeMap(myMap);
            Assert.True(Deserializer.DeserializeMap(serialized));
        }
    }
}
