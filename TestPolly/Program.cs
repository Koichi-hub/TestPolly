using TestPolly.Snippets;

var httpRequestSnippet = new HttpRequestSnippet();

for (var i = 0; i < 1000; i++)
{
    var word = await httpRequestSnippet.GetRandomEnglishWord();
    Console.WriteLine(word);
}
