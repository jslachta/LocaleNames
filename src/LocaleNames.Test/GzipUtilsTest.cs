using LocaleNames.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocaleNames.Test
{
    [TestClass]
    public class GzipUtilsTest
    {
        static string InputString =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris placerat" +
            " augue non neque scelerisque rutrum. Curabitur condimentum dui sodales, " +
            "sagittis turpis id, molestie quam. Pellentesque euismod, elit quis tempo" +
            "r luctus, lacus massa dignissim erat, vel ornare nunc metus quis nunc. A" +
            "enean vehicula faucibus dui, ac lobortis nisi faucibus et. Fusce convall" +
            "is, velit a blandit pellentesque, felis elit commodo ligula, sed commodo" +
            " lorem augue non est. Curabitur eu diam eget ipsum feugiat malesuada non" +
            " et ligula. Cras pellentesque tortor a porttitor cursus. Nulla laoreet e" +
            "t quam non placerat. Sed convallis, justo in ultricies molestie, est mi " +
            "ultrices lacus, ac placerat lorem nisi nec risus. Nunc fringilla quis do" +
            "lor ut consectetur. Integer non maximus orci. Duis in augue sagittis, sa" +
            "gittis lorem vitae, porta erat. Sed vulputate, lectus accumsan porttitor" +
            " sagittis, urna turpis tincidunt nunc, et posuere diam dui sed ex. Cras " +
            "tincidunt volutpat dolor sit amet tristique. ";

        public string LongString = new string(string.Join(" ", Enumerable.Repeat(InputString, 100)));

        [TestMethod]
        public void CompressionDecompressionTest()
        {
            var compressedResult = GzipUtils.Compress(InputString);
            var decompressedResult = GzipUtils.Decompress(compressedResult);

            Assert.AreEqual(InputString, decompressedResult);
        }

        /// <summary>
        /// Check whether Gzip compresses the input string.
        /// </summary>
        [TestMethod]
        public void IsCompressedTest()
        {
            string longString = LongString;
            int longStringLength = longString.Length;

            var result = GzipUtils.Compress(longString);

            int resultLength = result.Length;

            Assert.IsTrue(resultLength < longStringLength);
        }
    }
}
