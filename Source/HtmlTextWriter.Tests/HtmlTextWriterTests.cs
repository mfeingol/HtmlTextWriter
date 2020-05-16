// (c) 2019 Max Feingold

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.UI.Tests
{
    [TestClass]
    public class HtmlTextWriterTests
    {
        [TestMethod]
        public void EnsureEnumerations()
        {
            WriteDictionaryEntries<HtmlTextWriterTag>();
            WriteDictionaryEntries<HtmlTextWriterAttribute>();

            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            foreach (FieldInfo tagInfo in typeof(HtmlTextWriterTag).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                HtmlTextWriterTag tag = (HtmlTextWriterTag)tagInfo.GetRawConstantValue();
                if (tag == HtmlTextWriterTag.Unknown)
                    continue;

                writer.RenderBeginTag(tag);

                foreach (FieldInfo attrInfo in typeof(HtmlTextWriterAttribute).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    HtmlTextWriterAttribute attr = (HtmlTextWriterAttribute)attrInfo.GetRawConstantValue();
                    writer.AddAttribute(attr, "1");
                }

                writer.RenderEndTag();
            }

            // If we make it here without any exceptions, we're good

            static void WriteDictionaryEntries<T>() where T : Enum
            {
                foreach (FieldInfo tagInfo in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    T tag = (T)tagInfo.GetRawConstantValue();
                    Debug.WriteLine($"{{ {typeof(T).Name}.{tag}, \"{tag.ToString().ToLower()}\" }},");
                }
            }
        }

        [TestMethod]
        public void TestMicrosoftDocs()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Adapted from https://docs.microsoft.com/en-us/dotnet/api/system.web.ui.htmltextwriter?view=netframework-4.8
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "alert('Hello');");
            writer.AddAttribute("CustomAttribute", "CustomAttributeValue");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            writer.WriteLine();
            writer.Indent++;
            writer.Write("Hello");
            writer.WriteLine();

            writer.AddAttribute(HtmlTextWriterAttribute.Alt, "Encoding, \"Required\"", true);
            writer.AddAttribute("myattribute", "No &quot;encoding &quot; required", false);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.WriteLine();

            writer.RenderBeginTag("MyTag");
            writer.Write("Contents of MyTag");
            writer.RenderEndTag();
            writer.WriteLine();

            writer.WriteBeginTag("img");
            writer.WriteAttribute("alt", "A custom image.");
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteEndTag("img");
            writer.WriteLine();

            writer.Indent--;
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<span onclick=\"alert(&#39;Hello&#39;);\" CustomAttribute=\"CustomAttributeValue\">\r\n\t\tHello\r\n\t\t<img alt=\"Encoding, &quot;Required&quot;\" myattribute=\"No &quot;encoding &quot; required\" />\r\n\r\n\t\t<MyTag>\r\n\t\t\tContents of MyTag\r\n\t\t</MyTag>\r\n\r\n\t\t<img alt=\"A custom image.\"></img>\r\n</span>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario1()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Custom scenario from private code
            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Center);

            foreach (int i in new[] { 1, 2, 3, 4, 5 })
            {
                writer.RenderBeginTag(HtmlTextWriterTag.P);

                writer.RenderBeginTag(HtmlTextWriterTag.H2);
                writer.Write(i);
                writer.RenderEndTag();

                foreach (string u in new[] { "/a", "/b", "/c" })
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, u);
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "640");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "480");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
                writer.WriteLine();
            }
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\t<center>\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t1\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t2\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t3\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t4\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t\t<p>\r\n\t\t\t<h2>\r\n\t\t\t\t5\r\n\t\t\t</h2>\r\n\t\t\t<img src=\"/a\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/b\" width=\"640\" height=\"480\" />\r\n\t\t\t<img src=\"/c\" width=\"640\" height=\"480\" />\r\n\t\t</p>\r\n\r\n\t</center>\r\n</html>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void TestScenario2()
        {
            using MemoryStream mem = new MemoryStream();
            using StreamWriter sw = new StreamWriter(mem);
            using HtmlTextWriter writer = new HtmlTextWriter(sw);

            // Custom scenario from private code
            writer.RenderBeginTag(HtmlTextWriterTag.Html);

            // <h2>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write("TestScenario2");
            writer.RenderEndTag();
            // </h2>

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // <h3>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.RenderBeginTag(HtmlTextWriterTag.H3);

            writer.Write("Hello {0}", "World");

            writer.RenderEndTag();
            writer.RenderEndTag();

            // <p>
            writer.RenderBeginTag(HtmlTextWriterTag.P);

            // <table>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "1");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "1024");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "3");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            // <font>
            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("A nice summary of things");
            writer.RenderEndTag();
            // </font>

            writer.RenderEndTag();
            // </th>

            writer.RenderEndTag();
            // </tr>

            // Begin, end times
            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Began");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("12:00pm");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Ended");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.Write("1:00pm");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "arial");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Elapsed");
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write(TimeSpan.FromHours(1).ToString());

            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("Result");
            writer.RenderEndTag();

            // </th>
            writer.RenderEndTag();

            // <th>
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("Great Success!");

            // </th>
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // <tr>
            writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.AddAttribute("face", "Times New Roman");
            writer.RenderBeginTag(HtmlTextWriterTag.Font);
            writer.Write("A wonderful description");
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Th);

            writer.Write("Some text");
            writer.RenderEndTag();

            // </tr>
            writer.RenderEndTag();

            // Download rows
            for (int i = 0; i < 10; i++)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Bgcolor, "#FFFF00");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("/a/b/c");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("26 files");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Align, "right");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write("200GB");
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
            // </table></p>

            // </html>
            writer.RenderEndTag();

            writer.Flush();
            mem.Position = 0;

            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html>\r\n\t<h2 align=\"center\">\r\n\t\tTestScenario2\r\n\t</h2>\r\n\t<p>\r\n\t\t<h3 align=\"center\">\r\n\t\t\tHello World\r\n\t\t</h3>\r\n\t</p>\r\n\t<p>\r\n\t\t<table align=\"center\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\" width=\"1024\">\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th colspan=\"3\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tA nice summary of things\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tBegan\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\t12:00pm\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tEnded\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\t1:00pm\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"arial\">\r\n\t\t\t\t\t\tElapsed\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th align=\"right\" colspan=\"2\">\r\n\t\t\t\t\t01:00:00\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tResult\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\tGreat Success!\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<th align=\"left\">\r\n\t\t\t\t\t<font face=\"Times New Roman\">\r\n\t\t\t\t\t\tA wonderful description\r\n\t\t\t\t\t</font>\r\n\t\t\t\t</th>\r\n\t\t\t\t<th colspan=\"2\" align=\"right\">\r\n\t\t\t\t\tSome text\r\n\t\t\t\t</th>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t\t<tr bgcolor=\"#FFFF00\">\r\n\t\t\t\t<td>\r\n\t\t\t\t\t/a/b/c\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t26 files\r\n\t\t\t\t</td>\r\n\t\t\t\t<td align=\"right\">\r\n\t\t\t\t\t200GB\r\n\t\t\t\t</td>\r\n\t\t\t</tr>\r\n\t\t</table>\r\n\t</p>\r\n</html>\r\n";
            Assert.AreEqual(test, html);
        }

        [TestMethod]
        public void CheckNotDisposed()
        {
            MemoryStream mem = new MemoryStream();
            StreamWriter sw = new StreamWriter(mem);

            using (HtmlTextWriter writer = new HtmlTextWriter(sw))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html);
                writer.RenderEndTag();
                writer.Flush();
            }

            mem.Position = 0;
            string html = Encoding.UTF8.GetString(mem.ToArray());

            const string test = "<html />\r\n";
            Assert.AreEqual(test, html);
        }
    }
}
