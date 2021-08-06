using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Xunit;

using PassXYZLib;
using KeePassLib.Utility;

namespace PassXYZ.xunit
{
    public class PxItemTests
    {
        [Theory]
        [InlineData("http://github.com")]
        [InlineData("http://www.baidu.com")]
        [InlineData("https://www.bing.com/")]
        [InlineData("http://www.youdao.com")]
        [InlineData("https://www.dell.com")]
        [InlineData("http://www.cmbchina.com")]
        public void GetIconTest(string url)
        {
            var faviconUrl = PxItem.RetrieveFavicon(url);
            if (faviconUrl != null)
            {
                var imageFolder = "images";
                try
                {
                    DirectoryInfo di = new DirectoryInfo(imageFolder);
                    try
                    {
                        // Determine whether the directory exists.
                        if (!di.Exists)
                        {
                            di.Create();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("The process failed: {0}", e.ToString());
                    }

                    var uri = new Uri(faviconUrl);
                    WebClient myWebClient = new WebClient();
                    byte[] pb = myWebClient.DownloadData(faviconUrl);

                    if (faviconUrl.EndsWith(".ico") || faviconUrl.EndsWith(".png"))
                    {
                        GfxUtil.SaveImage(GfxUtil.ScaleImage(GfxUtil.LoadImage(pb), 128, 128), $"{imageFolder}/{uri.Host}.png");
                    }
                    else if (faviconUrl.EndsWith(".svg"))
                    {
                        GfxUtil.SaveImage(GfxUtil.LoadSvgImage(pb), $"{imageFolder}/{uri.Host}.png");
                    }
                    Debug.WriteLine($"{imageFolder}/{uri.Host}.png");
                }
                catch (System.Net.WebException ex)
                {
                    Debug.WriteLine($"{ex}");
                }
            }
            Assert.NotNull(faviconUrl);
        }

        [Theory]
        [InlineData("https://favicon.io/tutorials/what-is-a-favicon/")]
        public void NoFaviconTest(string url)
        {
            Assert.Null(PxItem.RetrieveFavicon(url));
        }
    }
}
