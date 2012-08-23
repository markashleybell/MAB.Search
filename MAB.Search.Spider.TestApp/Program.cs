using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Spider.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ISpider spider = new Spider();

            spider.OnUrlRetrieved += OnUrlRetrieved;

            spider.Begin("http://en.wikipedia.org/wiki/Battle_of_Bosworth_Field");

            Console.ReadLine();
        }

        private static void OnUrlRetrieved(object sender, UrlRetrievedEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
