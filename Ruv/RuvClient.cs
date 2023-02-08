using RestSharp;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kafe.Ruv;

public class RuvClient : IDisposable
{
    public const string RuvBaseUrl = "https://www.iruv.cz";

    private readonly RestClient client = new RestClient(new RestClientOptions(RuvBaseUrl)
    {
        MaxTimeout = -1,
        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0",
    });

    public async Task LogIn(string username, string password)
    {
        var request = new RestRequest("/app/LoginProcess", Method.Post);
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("u_username", username);
        request.AddParameter("u_password", password);
        RestResponse response = await client.ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("The 'LogIn' request failed.");
        }
    }

    public async Task AddFilm()
    {

    }

    public async Task<RegisterArtworkAuthor?> SendCheckAuthor(string personalNumber)
    {
        var request = new RestRequest("/app/rest/user/check_author", Method.Post);
        request.AddHeader("Host", "www.iruv.cz");
        request.AddHeader("Accept", "application/json, text/javascript, */*; q=0.01");
        request.AddHeader("Accept-Language", "cs,en-US;q=0.7,en;q=0.3");
        request.AddHeader("Accept-Encoding", "gzip, deflate, br");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        request.AddHeader("X-Requested-With", "XMLHttpRequest");
        request.AddHeader("Content-Length", "25");
        request.AddHeader("Origin", "https://www.iruv.cz");
        request.AddHeader("Connection", "keep-alive");
        //request.AddHeader("Referer", "https://www.iruv.cz/app/user/Authors");
        request.AddHeader("Sec-Fetch-Dest", "empty");
        request.AddHeader("Sec-Fetch-Mode", "cors");
        request.AddHeader("Sec-Fetch-Site", "same-origin");
        request.AddHeader("Pragma", "no-cache");
        request.AddHeader("Cache-Control", "no-cache");
        //request.AddHeader("Cookie", "JSESSIONID=1F93E6D527194211EDD2BF39706249EF");
        request.AddParameter("personalNumber", personalNumber);
        RestResponse response = await client.ExecuteAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("A 'CheckAuthor' request was unsuccessful.");
        }

        var json = JsonNode.Parse(response.Content ?? string.Empty);
        if (json is null)
        {
            throw new InvalidOperationException("A 'CheckAuthor' response is not valid JSON.");
        }

        return new(
            Author: json["id"]?.GetValue<int>() ?? -1,
            FirstName: json["firstname"]?.GetValue<string>() ?? string.Empty,
            LastName: json["lastname"]?.GetValue<string>() ?? string.Empty,
            DegreeBeforeName: json["degreebeforename"]?.GetValue<string>() ?? string.Empty,
            DegreeAfterName: json["degreeaftername"]?.GetValue<string>() ?? string.Empty,
            Organization: string.Empty);
    }

    private async Task SendRegisterArtwork(RegisterArtworkParams data)
    {
        var request = new RestRequest("/app/artwork/RegisterArtwork", Method.Get);
        request.AddHeader("Accept-Language", "cs,en-US;q=0.7,en;q=0.3");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddHeader("Origin", "https://www.iruv.cz");
        request.AddHeader("Connection", "keep-alive");
        //request.AddHeader("Referer", "https://www.iruv.cz/app/artwork/RegisterArtwork");
        request.AddHeader("Upgrade-Insecure-Requests", "1");
        //request.AddHeader("Cookie", "JSESSIONID=1F93E6D527194211EDD2BF39706249EF");
        request.AddParameter("id", "");
        request.AddParameter("uuid", "7960b64f-8936-45cc-9e1d-b40e2ba64ce3");
        request.AddParameter("status", "0");
        request.AddParameter("acl", "");
        request.AddParameter("acl_status", "");
        request.AddParameter("originalImpactCategory", "");
        request.AddParameter("originalScopeCategory", "");
        request.AddParameter("finalImpactCategory", "");
        request.AddParameter("finalScopeCategory", "");
        request.AddParameter("reviewReason", "");
        request.AddParameter("year", data.Year);
        request.AddParameter("segment", data.Segment);
        request.AddParameter("artworkType", (int)data.ArtworkType);
        request.AddParameter("impact", data.Impact);
        request.AddParameter("scope", data.Scope);
        request.AddParameter("name_cs", data.NameCS);
        request.AddParameter("keywords_cs", string.Join('\n', data.KeywordsCS));
        request.AddParameter("annotation_cs", data.AnotationCS);
        request.AddParameter("name_en", data.NameEN);
        request.AddParameter("keywords_en", string.Join('\n', data.KeywordsEN));
        request.AddParameter("annotation_en", data.AnotationEN);
        for (int i = 0; i < data.Authors.Length; i++)
        {
            var author = data.Authors[i];
            var share = i == 0
                ? (int)Math.Ceiling(100.0 / data.Authors.Length)
                : (int)Math.Floor(100.0 / data.Authors.Length);
            request.AddParameter($"artworkAuthors[{i}].id", "");
            request.AddParameter($"artworkAuthors[{i}].uuid", Guid.NewGuid().ToString());
            request.AddParameter($"artworkAuthors[{i}].acl", "");
            request.AddParameter($"artworkAuthors[{i}].author", author.Author);
            request.AddParameter($"artworkAuthors[{i}].firstname", author.FirstName);
            request.AddParameter($"artworkAuthors[{i}].lastname", author.LastName);
            request.AddParameter($"artworkAuthors[{i}].degreeBeforeName", author.DegreeBeforeName);
            request.AddParameter($"artworkAuthors[{i}].degreeAfterName", author.DegreeAfterName);
            request.AddParameter($"artworkAuthors[{i}].organization", author.Organization);
            request.AddParameter($"artworkAuthors[{i}].share", share);
        }
        
        // FFFI MU
        request.AddParameter("artworkInstitutions[0].id", "");
        request.AddParameter("artworkInstitutions[0].uuid", "");
        request.AddParameter("artworkInstitutions[0].acl", "");
        request.AddParameter("artworkInstitutions[0].institution", "215950");
        request.AddParameter("artworkInstitutions[0].dateFrom", data.FestivalDate.ToString("dd.MM.yyyy"));
        request.AddParameter("artworkInstitutions[0].dateTo", data.FestivalDate.ToString("dd.MM.yyyy"));
        request.AddParameter("artworkInstitutions[0].year", "0");
        request.AddParameter("artworkInstitutions[0].specification", "");
        request.AddParameter("artworkInstitutions[0].contextFactor", "0");

        request.AddParameter("studyProgram", data.StudyProgram);
        request.AddParameter("studySubject", data.StudySubject);
        request.AddParameter("provider", "");

        if (data.CitationLink is not null)
        {
            request.AddParameter("artworkCitations[0].text", data.CitationLink);
        }

        for (int i = 0; i < data.Attachments.Length; i++)
        {
            request.AddParameter($"artworkAttachments[{i}].id", "");
            request.AddParameter($"artworkAttachments[{i}].uuid", "");
            request.AddParameter($"artworkAttachments[{i}].acl", "");
            request.AddParameter($"artworkAttachments[{i}].fileItemMetadata", data.Attachments[i].ToString());
        }

        request.AddParameter("file", "");
        request.AddParameter("save_ROZ", "");
        RestResponse response = await client.ExecuteAsync(request);
        Console.WriteLine(response.Content);
    }

    public void Dispose()
    {
        ((IDisposable)client).Dispose();
    }
}
