using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

class WebScraper
{
    static async Task Main(string[] args)
    {
        //await getMovieInfo();
        await getSeriesInfo();
        //await getTop10();
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
        var MovieTitle = WebUtility.HtmlDecode(MovieTitleElement); //utilizamos WebUtility.HtmlDecode() porque hay peliculas que
                                                                   //tienen carecteres especiales en sus titulos, asi los devolvemos a como son

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

        var genreLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'movie-info-item-label') and text()='Genre:']");
        var genreValue = genreLabel?.ParentNode?.SelectSingleNode(".//span[contains(@data-qa, 'movie-info-item-value')]");
        var genre = genreValue?.InnerText.Trim();
        //genre = genre?.Replace(",", ", ");
        genre = Regex.Replace(genre, @"\s+", " ");
        genre = genre?.Replace(",", ", ");
        genre = WebUtility.HtmlDecode(genre);
        Console.WriteLine("Generos:\n" + genre);
        Console.WriteLine("");

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
                    var actorRolNode = item.SelectSingleNode(".//p[@class='p--small']");
                    if (actorRolNode != null)
                    {
                        var actorRolEspaciado = actorRolNode.InnerText.Trim();
                        //regex para eliminar espacios innecesarios entre palabras y dejar solo uno
                        var actorRol = Regex.Replace(actorRolEspaciado, @"\s+", " ");
                        Console.WriteLine(actorRol + "\n");
                    }
                }
            }
        }
        Console.WriteLine("");
        var criticReview = htmlDocument.DocumentNode.SelectNodes("//review-speech-balloon[@data-qa='critic-review' and @istopcritic= 'true']");

        if (criticReview != null)
        {
            Console.WriteLine("Comentarios de la critica:");

            for (int i = 0; i < 3 && i < criticReview.Count; i++)
            {
                var reviewQuote = criticReview[i]?.GetAttributeValue("reviewquote", "");
                reviewQuote = WebUtility.HtmlDecode(reviewQuote);
                Console.WriteLine("- " + reviewQuote?.Trim()+"\n");
            }
        }
        //solo cambia el atributo istopcritic a false
        var audienceReview = htmlDocument.DocumentNode.SelectNodes("//review-speech-balloon[@data-qa='critic-review' and @istopcritic= 'false']");
        if (audienceReview != null)
        {
            Console.WriteLine("Comentarios de la audiencia:");
            for (int i = 0; i < 3 && i < audienceReview.Count; i++)
            {
                var audienceQuote = audienceReview[i]?.GetAttributeValue("reviewquote", "");
                audienceQuote = WebUtility.HtmlDecode(audienceQuote);
                Console.WriteLine("- " + audienceQuote?.Trim() + "\n");
            }
        }
    }

    static async Task getSeriesInfo()
    {
        var url = "https://www.rottentomatoes.com/tv/the_mandalorian";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);


        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        var SerieTitleElement = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='scoreboard__title']").InnerText;
        var SerieTitle = WebUtility.HtmlDecode(SerieTitleElement);
        Console.WriteLine("Titulo de la serie:\n"+SerieTitle);
        Console.WriteLine("");

        var ImageUrl = htmlDocument.DocumentNode.SelectSingleNode("//img[@alt='Watch trailer for " + SerieTitle + "' and @slot='image']")
                            .GetAttributeValue("src", "");
        Console.WriteLine("Portada de la serie: " + ImageUrl);

        var scoreBoardElement = htmlDocument.DocumentNode.SelectSingleNode("//score-board");

        var tomatometerScore = scoreBoardElement.GetAttributeValue("tomatometerscore", "");

        var AudienceScore = scoreBoardElement.GetAttributeValue("audiencescore", "");

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
        var synopsisNode = htmlDocument.DocumentNode.Descendants()
            .FirstOrDefault(n => n.GetAttributeValue("data-qa", "") == "series-info-description");

        var synopsis = synopsisNode?.InnerText?.Trim();
        Console.WriteLine("Sinopsis:\n" + synopsis);
        Console.WriteLine("");

        //var infoList = htmlDocument.DocumentNode.SelectSingleNode("//ul");
        //var creatorLabel = infoList.SelectSingleNode(".//b[contains(@data-qa, 'series-info-creators')]");
        var creatorValue = htmlDocument.DocumentNode.SelectSingleNode("//li[b[@data-qa='series-info-creators']]//span[@class='info-item-value']/a/span");
        var creator = creatorValue?.InnerText.Trim();
        Console.WriteLine("Creador: " + creator);
        Console.WriteLine("");

        var starringNode = htmlDocument.DocumentNode.SelectSingleNode("//li[contains(., 'Starring: ')]");
        var starringLinks = starringNode.SelectNodes(".//a");
        Console.WriteLine("Actores principales:");
        // Iterar sobre los elementos <a> y extraer el texto de los elementos <span>
        foreach (HtmlNode link in starringLinks)
        {
            var span = link.SelectSingleNode(".//span");
            string actorName = span.InnerText.Trim();
            Console.WriteLine(actorName);
        }


        Console.WriteLine("");

        var tvNetworkValue = htmlDocument.DocumentNode.SelectSingleNode("//li[contains(., 'TV Network: ')]");
        var tvNetwork = tvNetworkValue?.InnerText.Replace("TV Network: ", "").Trim(); //con el replace hacemos que no se guarde el "TV network" y borramos espacios en blanco
        Console.WriteLine("TV Network: "+tvNetwork);
        Console.WriteLine("");

        var premiereDateValue = htmlDocument.DocumentNode.SelectSingleNode("//li[contains(., 'Premiere Date: ')]");
        var premiereDate = premiereDateValue?.InnerText.Replace("Premiere Date: ", "").Trim();
        Console.WriteLine("Fecha de lanzamiento: "+premiereDate);
        Console.WriteLine("");

        var genreValue = htmlDocument.DocumentNode.SelectSingleNode("//li[contains(., 'Genre: ')]");
        var genre = genreValue?.InnerText.Replace("Genre: ", "").Trim();
        Console.WriteLine("Genero: " + genre);
        

    }
    static async Task getTop10()
    {
        var url = "https://www.rottentomatoes.com";
        var httpClient = new HttpClient();
        var html = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);

        Console.WriteLine("\nTop peliculas de la semana:\n");
        for (int i = 1; i <= 10; i++)
        {
            //Iteramos sobre cada nodo que cumple con la condicion de ese value de class
            //Solamente iteraremos 10 veces debido a que en estos 10 primeros nodos está el top de peliculas
            var topValue = htmlDocument.DocumentNode.SelectSingleNode($"(//span[@class='dynamic-text-list__item-title clamp clamp-1'])[{i}]");
            var topMovie = topValue?.InnerText.Trim();
            topMovie = WebUtility.HtmlDecode(topMovie);
            Console.WriteLine("Titulo: "+topMovie);
            var linkNode = htmlDocument.DocumentNode.SelectSingleNode($"(//a[@class='dynamic-text-list__tomatometer-group'])[{i}]");
            var movieLink = linkNode?.GetAttributeValue("href", "");//aqui solamente se almacena "/m/{nombre_pelicula}", asi que falta completar el link
            var fullLink = "https://www.rottentomatoes.com" + movieLink;
            Console.WriteLine("Link de la pelicula: " + fullLink+"\n");
        }
        Console.WriteLine("");

        Console.WriteLine("Top series de la semana:\n");
        for (int i = 11; i <= 20; i++)
        {

            var serieValue = htmlDocument.DocumentNode.SelectSingleNode($"(//span[@class='dynamic-text-list__item-title clamp clamp-1'])[{i}]");
            var topSerie = serieValue?.InnerText.Trim();
            topSerie = WebUtility.HtmlDecode(topSerie);
            Console.WriteLine("Titulo: "+ topSerie);
            var linkNode = htmlDocument.DocumentNode.SelectSingleNode($"(//a[@class='dynamic-text-list__tomatometer-group'])[{i}]");
            var serieLink = linkNode?.GetAttributeValue("href", "");
            var fullLink = "https://www.rottentomatoes.com" + serieLink;
            Console.WriteLine("Link de la serie: " + fullLink + "\n");

        }
    }
}