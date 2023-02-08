using RestSharp;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kafe.Ruv;

public class RuvClient : IDisposable
{
    public const string RuvBaseUrl = "https://www.iruv.cz";

    private readonly RestClient client;
    private readonly CookieContainer cookieJar = new();

    public RuvClient()
    {
        client = new RestClient(new RestClientOptions(RuvBaseUrl)
        {
            MaxTimeout = -1,
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:108.0) Gecko/20100101 Firefox/108.0",
            CookieContainer = cookieJar
        });
    }

    public async Task LogIn(string username, string password)
    {
        var request = new RestRequest("/app/LoginProcess", Method.Post);
        //request.CookieContainer = cookieJar;
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request.AddParameter("u_username", username);
        request.AddParameter("u_password", password);
        RestResponse response = await client.ExecuteAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("The 'LogIn' request failed.");
        }
    }

    public async Task AddFilm(AddFilmParams data)
    {
        var registerAuthors = (await Task.WhenAll(data.Authors.Select(async a =>
        {
            var existing = await SendCheckAuthor(a.PersonalNumber);
            if (existing is not null)
            {
                return existing;
            }

            var newAuthor = await SendSaveAuthor(a);
            if (newAuthor is null)
            {
                throw new InvalidOperationException($"Author '{a.LastName}, {a.FirstName}' could not be registered.");
            }
            return newAuthor;
        }))).ToImmutableArray();

        var imageId = await SendUpload(data.ImagePath, "image/jpeg");
        if (imageId is null)
        {
            throw new InvalidOperationException("Could not upload a film's image.");
        }

        var impactOptions = await SendImpactQuery((int)data.ArtworkType);
        var impactCategory = data.IsFestivalWinner ? "C" : "D";
        var impact = impactOptions.SingleOrDefault(o => o.NameCS.StartsWith(impactCategory));
        if (impact is null)
        {
            throw new InvalidOperationException("A valid impact option could not be found.");
        }

        var scopeOptions = await SendScopeQuery((int)data.ArtworkType);
        var scope = scopeOptions.SingleOrDefault(o => o.NameCS.StartsWith("M"));
        if (scope is null)
        {
            throw new InvalidOperationException("A valid 'M' scope option could not be found.");
        }

        var registerArtworkParams = new RegisterArtworkParams(
            Year: data.Year,
            Segment: RuvConst.AudiovisionSegment,
            ArtworkType: data.ArtworkType,
            Impact: impact.Value,
            Scope: scope.Value,
            NameCS: data.NameCS,
            KeywordsCS: data.KeywordsCS,
            AnnotationCS: data.AnnotationCS,
            NameEN: data.NameEN,
            KeywordsEN: data.KeywordsEN,
            AnnotationEN: data.AnnotationEN,
            Authors: registerAuthors,
            FestivalDate: data.FestivalDate,
            Attachments: ImmutableArray.Create(imageId.Value),
            CitationLink: data.CitationLink,
            StudyProgram: data.StudyProgram,
            StudySubject: data.StudySubject);
        await SendRegisterArtwork(registerArtworkParams);
    }

    public async Task<Guid?> SendUpload(string filePath, string contentType)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        var fileStreamContent = new StreamContent(File.OpenRead(filePath));
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        multipartFormContent.Add(fileStreamContent, name: "file", fileName: "file");
            
        using var clientHandler = new HttpClientHandler() {  CookieContainer = cookieJar };
        using var client = new HttpClient(clientHandler);
        var response = await client.PostAsync($"{RuvBaseUrl}/app/rest/file/upload", multipartFormContent);
        response.EnsureSuccessStatusCode();
        var jsonString = await response.Content.ReadAsStringAsync();

        var json = JsonNode.Parse(jsonString);
        if (json is null)
        {
            throw new InvalidOperationException("An 'Upload' response is not valid JSON.");
        }

        return json["data"]?["uuid"]?.GetValue<Guid>();
    }

    public async Task<RegisterArtworkAuthor?> SendCheckAuthor(string personalNumber)
    {
        var request = new RestRequest("/app/rest/user/check_author", Method.Post);
        //request.CookieContainer = cookieJar;
        request.AddHeader("Host", "www.iruv.cz");
        request.AddHeader("Accept", "application/json, text/javascript, */*; q=0.01");
        request.AddHeader("Accept-Language", "cs,en-US;q=0.7,en;q=0.3");
        request.AddHeader("Accept-Encoding", "gzip, deflate, br");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        request.AddHeader("X-Requested-With", "XMLHttpRequest");
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

        return ParserRegisterArtworkAuthor(response.Content);
    }

    public async Task<RegisterArtworkAuthor?> SendSaveAuthor(SaveAuthorParams data)
    {
        var request = new RestRequest("/app/rest/user/save_author", Method.Post);
        request.AddHeader("Host", "www.iruv.cz");
        request.AddHeader("Accept", "application/json, text/javascript, */*; q=0.01");
        request.AddHeader("Accept-Language", "cs,en-US;q=0.7,en;q=0.3");
        request.AddHeader("Accept-Encoding", "gzip, deflate, br");
        request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
        request.AddHeader("X-Requested-With", "XMLHttpRequest");
        request.AddHeader("Origin", "https://www.iruv.cz");
        request.AddHeader("Connection", "keep-alive");
        //request.AddHeader("Referer", "https://www.iruv.cz/app/user/Authors");
        request.AddHeader("Sec-Fetch-Dest", "empty");
        request.AddHeader("Sec-Fetch-Mode", "cors");
        request.AddHeader("Sec-Fetch-Site", "same-origin");
        request.AddHeader("Pragma", "no-cache");
        request.AddHeader("Cache-Control", "no-cache");
        request.AddParameter("id", "");
        request.AddParameter("uuid", "");
        request.AddParameter("orgUnit", data.OrgUnit);
        request.AddParameter("firstname", data.FirstName);
        request.AddParameter("lastname", data.LastName);
        request.AddParameter("degreeBeforeName", data.DegreeBeforeName);
        request.AddParameter("degreeAfterName", data.DegreeAfterName);
        request.AddParameter("personalNumber", data.PersonalNumber);
        request.AddParameter("authorType", "");
        RestResponse response = await client.ExecuteAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("A 'SaveAuthor' request was unsuccessful.");
        }


        return ParserRegisterArtworkAuthor(response.Content);
    }

    public async Task SendRegisterArtwork(RegisterArtworkParams data)
    {
        var request = new RestRequest("/app/artwork/RegisterArtwork", Method.Get);
        //request.CookieContainer = cookieJar;
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
        request.AddParameter("annotation_cs", data.AnnotationCS);
        request.AddParameter("name_en", data.NameEN);
        request.AddParameter("keywords_en", string.Join('\n', data.KeywordsEN));
        request.AddParameter("annotation_en", data.AnnotationEN);
        for (int i = 0; i < data.Authors.Length; i++)
        {
            var author = data.Authors[i];
            var share = i == 0
                ? (int)Math.Ceiling(100.0 / data.Authors.Length)
                : (int)Math.Floor(100.0 / data.Authors.Length);
            request.AddParameter($"artworkAuthors[{i}].id", "");
            request.AddParameter($"artworkAuthors[{i}].uuid", "");
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

    public record ImpactOption(string NameCS, int Value);

    public async Task<ImmutableArray<ImpactOption>> SendImpactQuery(int artworkType)
    {
        var request = new RestRequest($"/app/rest/api/impact?artwork_type={artworkType}", Method.Get);
        RestResponse response = await client.ExecuteAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("An 'Impact' query was unsuccessful.");
        }

        var json = JsonNode.Parse(response.Content ?? string.Empty);
        if (json is null)
        {
            throw new InvalidOperationException("An 'Impact' query response is not valid JSON.");
        }

        var options = json.AsArray()
            .Where(o => o is not null)
            .Select(o => new ImpactOption(
                NameCS: o!["name_cs"]?.GetValue<string>() ?? string.Empty,
                Value: o!["id"]?.GetValue<int>() ?? -1))
            .Where(o => !string.IsNullOrEmpty(o.NameCS) && o.Value != -1)
            .ToImmutableArray();
        if (options.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException($"No impact options were found for the '{artworkType}' artwork type.");
        }

        return options;
    }

    public record ScopeOption(string NameCS, int Value);

    public async Task<ImmutableArray<ScopeOption>> SendScopeQuery(int artworkType)
    {
        var request = new RestRequest($"/app/rest/api/scope?artwork_type={artworkType}", Method.Get);
        RestResponse response = await client.ExecuteAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("An 'Scope' query was unsuccessful.");
        }

        var json = JsonNode.Parse(response.Content ?? string.Empty);
        if (json is null)
        {
            throw new InvalidOperationException("An 'Scope' query response is not valid JSON.");
        }

        var options = json.AsArray()
            .Where(o => o is not null)
            .Select(o => new ScopeOption(
                NameCS: o!["name_cs"]?.GetValue<string>() ?? string.Empty,
                Value: o!["id"]?.GetValue<int>() ?? -1))
            .Where(o => !string.IsNullOrEmpty(o.NameCS) && o.Value != -1)
            .ToImmutableArray();
        if (options.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException($"No impact options were found for the '{artworkType}' artwork type.");
        }

        return options;
    }

    public void Dispose()
    {
        ((IDisposable)client).Dispose();
    }

    private RegisterArtworkAuthor? ParserRegisterArtworkAuthor(string? jsonString)
    {
        var json = JsonNode.Parse(jsonString ?? string.Empty);
        if (json is null)
        {
            throw new InvalidOperationException("Could not parse an author. Maybe the string is not valid JSON.");
        }

        return new(
            Author: json["id"]?.GetValue<int>() ?? -1,
            FirstName: json["firstname"]?.GetValue<string>() ?? string.Empty,
            LastName: json["lastname"]?.GetValue<string>() ?? string.Empty,
            DegreeBeforeName: json["degreeBeforeName"]?.GetValue<string>() ?? string.Empty,
            DegreeAfterName: json["degreeAfterName"]?.GetValue<string>() ?? string.Empty,
            Organization: string.Empty);
    }
}
