using System.IO;

namespace PageManagment
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Super Gestor de Páginas HC 1000");
            Console.Write("Digite o nome do arquivo .txt: ");
            string articleName = Console.ReadLine();

            List<ContentBlock> contentBlockList = ArticleParser.ParseAsync(articleName).Result;
            
            foreach (var content in contentBlockList)
            {
                Console.WriteLine(content);
            }

            var (html,slug) = HtmlGenerator.GenerateHTML(contentBlockList);

            //Console.WriteLine(html);

            await File.WriteAllTextAsync($"../articles/{slug}.html", html, System.Text.Encoding.UTF8);
        }
    }
}

