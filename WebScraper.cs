using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

class WebScraper
{
    static async Task Main(string[] args)
    {
        await getMovieInfo();
    }
    static async Task getMovieInfo()
    {
        var url = "https://www.rottentomatoes.com/m/the_super_mario_bros_movie";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);
        //En html está guardado el codigo completo tras la petición

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var MovieTitleElement = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='scoreboard__title']").InnerText;
        var MovieTitle = WebUtility.HtmlDecode(MovieTitleElement);

        Console.WriteLine("Titulo de la pelicula: " + MovieTitle + "\n");

        var ImageUrl = htmlDocument.DocumentNode.SelectSingleNode("//img[@alt='Watch trailer for " + MovieTitleElement + "' and @slot='image']")
                            .GetAttributeValue("src", "");
        Console.WriteLine("Imagen de la pelicula " +  ImageUrl+"\n");

        var scoreBoardElement = htmlDocument.DocumentNode.SelectSingleNode("//score-board");

        var tomatometerScore = scoreBoardElement.GetAttributeValue("tomatometerscore", "");

        var AudienceScore = scoreBoardElement.GetAttributeValue("audiencescore","");

        Console.WriteLine("Calificacion de la critica " + tomatometerScore + "\n");

        Console.WriteLine("Calificacion de la audiencia " + AudienceScore + "\n");

        var whereToWatchSection = htmlDocument.DocumentNode.SelectSingleNode("//section[@id='where-to-watch']");

        var platformElements = whereToWatchSection.SelectNodes(".//where-to-watch-meta");
        foreach (var platformElement in platformElements)
        {
            var platformUrl = platformElement.GetAttributeValue("href", "");
            //En el atributo image se encuentra el nombre de la plataforma donde se puede ver el Show
            var platform = platformElement.SelectSingleNode(".//where-to-watch-bubble").GetAttributeValue("image", "");

            Console.WriteLine("Plataforma disponible: " + platform);
            Console.WriteLine("URL de la plataforma: " + platformUrl);
            Console.WriteLine("");
        }
        //Se desciende sobre el nodo raiz hasta encontrar un nodo el cual tenga un atributo llamado "data-qa" cuyo valor sea "movie-info-synopsis" que es donde se encuentra la sinopsis
        var synopsisNode = htmlDocument.DocumentNode.Descendants()//Puede servir para bajar por todo el DOM y buscar los que cumplan con ciertas condiciones
            .FirstOrDefault(n => n.GetAttributeValue("data-qa", "") == "movie-info-synopsis");

        var synopsis = synopsisNode?.InnerText?.Trim();//El Trim solo es borrar los espacios vacios

        Console.WriteLine("Sinopsis:\n"+ synopsis);
        Console.WriteLine("");

        //Mucha de la información de la pelicula se encuentra en el elemento ul (lista desordenada de HTML)
        //con id=info, por lo tanto accederemos a cada elemento que necesitemos

        var infoList = htmlDocument.DocumentNode.SelectSingleNode("//ul[@id='info']");//Nodo de la lista con la info
        
        var ratingLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Rating:']");
        var ratingValue = ratingLabel?.ParentNode?.SelectSingleNode(".//span[contains(@data-qa, 'movie-info-item-value')]");
        var rating = ratingValue?.InnerText.Trim();
        Console.WriteLine("Clasificacion:\n" + rating);
        Console.WriteLine("");

        //var genreLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Genre:']");
        //var genreValue = genreLabel?.ParentNode?.SelectSingleNode(".//span[contains(@data-qa, 'movie-info-item-value')]");
        //var genre = genreValue?.InnerText.Trim();
        //genre = genre?.Replace(",", ", ");
        //Console.WriteLine("Genero:\n" + genre);
        //Console.WriteLine("");

        var languageLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Original Language:']");
        var languageValue = languageLabel?.ParentNode?.SelectSingleNode(".//span[contains(@data-qa, 'movie-info-item-value')]");
        var language = languageValue?.InnerText.Trim();
        Console.WriteLine("Lenguaje original:\n"+ language);
        Console.WriteLine("");

        var directorLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Director:']");
        var directorValue = directorLabel?.ParentNode?.SelectSingleNode(".//a[contains(@data-qa, 'movie-info-director')]");
        var director = directorValue?.InnerText.Trim();
        Console.WriteLine("Director:\n" + director);
        Console.WriteLine("");

        var releaseLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Release Date (Theaters):']");
        var releaseValue = releaseLabel?.ParentNode?.SelectSingleNode(".//time");
        var releaseDate = releaseValue?.Attributes["datetime"]?.Value.Trim();
        Console.WriteLine("Fecha de estreno:\n"+releaseDate);
        Console.WriteLine("");

        var runtimeLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Runtime:']");
        var runtimeValue = runtimeLabel?.ParentNode?.SelectSingleNode(".//time");
        var runtime = runtimeValue?.InnerText.Trim();
        Console.WriteLine("Duración:\n" + runtime);
        Console.WriteLine("");

        //Pasamos a la parte final de la pagina

        var castSection = htmlDocument.DocumentNode.SelectSingleNode("//div[@data-qa='cast-section']");
        if (castSection != null)
        {
            Console.WriteLine("Actores principales:");
            var castCrewItems = castSection.SelectNodes(".//div[@class='cast-and-crew-item ']");
            if (castCrewItems != null)
            {
                //moreCasts hide
                foreach (var item in castCrewItems)
                {
                    var actorImg = item.SelectSingleNode(".//img");
                    if (actorImg != null)
                    {
                        var actorName = actorImg.GetAttributeValue("alt", "");//El valor del atributo alt contiene el nombre del actor
                        Console.WriteLine(actorName);
                    }
                }
            }
        }
        Console.WriteLine("");

        //Faltan los comentarios de la critica y de la audiencia
    }
}