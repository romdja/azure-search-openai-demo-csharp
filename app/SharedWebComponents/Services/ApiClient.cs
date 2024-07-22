// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http.Headers;

namespace SharedWebComponents.Services;

public sealed class ApiClient(HttpClient httpClient)
{
    public async Task<ImageResponse?> RequestImageAsync(PromptRequest request)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/images", request, SerializerOptions.Default);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ImageResponse>();
    }

    public async Task<bool> ShowLogoutButtonAsync()
    {
        var response = await httpClient.GetAsync("api/enableLogout");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<UploadDocumentsResponse> UploadDocumentsAsync(
        IReadOnlyList<IBrowserFile> files,
        long maxAllowedSize,
        string cookie)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            foreach (var file in files)
            {
                // max allow size: 10mb
                var max_size = maxAllowedSize * 1024 * 1024;
#pragma warning disable CA2000 // Dispose objects before losing scope
                var fileContent = new StreamContent(file.OpenReadStream(max_size));
#pragma warning restore CA2000 // Dispose objects before losing scope
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, file.Name, file.Name);
            }

            // set cookie
            content.Headers.Add("X-CSRF-TOKEN-FORM", cookie);
            content.Headers.Add("X-CSRF-TOKEN-HEADER", cookie);

            var response = await httpClient.PostAsync("api/documents", content);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<UploadDocumentsResponse>();

            return result
                ?? UploadDocumentsResponse.FromError(
                    "Unable to upload files, unknown error.");
        }
        catch (Exception ex)
        {
            return UploadDocumentsResponse.FromError(ex.ToString());
        }
    }

    public async IAsyncEnumerable<DocumentResponse> GetDocumentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("api/documents", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var options = SerializerOptions.Default;

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            await foreach (var document in
                JsonSerializer.DeserializeAsyncEnumerable<DocumentResponse>(stream, options, cancellationToken))
            {
                if (document is null)
                {
                    continue;
                }

                yield return document;
            }
        }
    }

    public Task<AnswerResult<ChatRequest>> ChatConversationAsync(ChatRequest request) => PostRequestAsync(request, "api/chat");

    private async Task<AnswerResult<TRequest>> PostRequestAsync<TRequest>(
        TRequest request, string apiRoute) where TRequest : ApproachRequest
    {
        var result = new AnswerResult<TRequest>(
            IsSuccessful: false,
            Response: null,
            Approach: request.Approach,
            Request: request);

        var json = JsonSerializer.Serialize(
            request,
            SerializerOptions.Default);

        using var body = new StringContent(
            json, Encoding.UTF8, "application/json");


       HttpResponseMessage response = null;

        if(apiRoute == "api/chat")
        {
        string mock = "{\"Choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Rom is Romuald Djaroud, a certified Azure Cloud \\u0026 AI Architect with over 20 years of experience in innovation and leadership. He has worked on complex projects for clients like Airbus Defence \\u0026 Space and Marlink, and is fluent in English, Dutch, and French. [source] \\u003C\\u003CWhat specific projects has Rom worked on for Airbus Defence \\u0026 Space and Marlink?\\u003E\\u003E  \\u003C\\u003CWhat are some notable achievements or innovations Rom has made in his career?\\u003E\\u003E  \\u003C\\u003CHow does Rom\\u0027s experience as an Azure Cloud \\u0026 AI Architect contribute to his leadership abilities?\\u003E\\u003E \"},\"context\":{\"dataPointsContent\":[{\"Title\":\"CVRom-0.pdf\",\"Content\":\"Romuald Djaroud Certified Azure Cloud \\u0026 AI Architect | 20\\u002B Years of Innovation and Leadership The Hague, The Netherlands | Email: rom@yrdata.ai | LinkedIn: in/@romdja Professional Summary Innovative Microsoft Azure Cloud \\u0026 AI Solution Architect and Developer, leading complex projects for clients like Airbus Defence \\u0026 Space and Marlink. Recognized for successfully migrating critical systems to the cloud, integrating cutting- edge technologies and significantly enhancing system performance, security and delivery. Fluent in English, Dutch, and French with strong leadership, time management, and creative problem-solving skills. Education \\u00B7 Microsoft Certified: Azure AI Engineer Associate (AI-102), Microsoft, 2024 \\u00B7 Ongoing Professional Development: Tech Communities, Learning Platforms, Conferences \\u00B7 Time Management and Productivity, Online Brian Tracy Academy, 2021 \\u00B7 Effective Communication and Consultancy, Avanade, Seattle, USA, 2006 \\u00B7 University of Sciences \\u0026 Technologies, Lille, France, 1994 - 1998 Areas of Expertise \\u00B7 Cloud Architecture Azure Cloud, SaaS, Real-Time Event-Driven \"},{\"Title\":\"CVRom-1.pdf\",\"Content\":\" \\u00B7 Recognized for streamlining complex business processes, achieving high client satisfaction rates, and mentoring team members on adopting latest technologies. \\u00B7 Successfully transitioned from corporate consulting roles at Avanade and Atos to independent freelance consulting in 2006. \"},{\"Title\":\"CVRom-1.pdf\",\"Content\":\"Professional Experience Azure Cloud Solution Architect \\u0026 Developer, YrData.AI The Netherlands / Remote | 2014 - Present \\u00B7 Developed an Al-powered travel assistant for Canggu, Bali using .NET MAUI and Azure Al services. Launched on App Store and Google Play. Integrated Semantic Kernel and Azure AI Search for advanced RAG and Generative AI capabilities. Implemented Agentive AI principles, showcasing expertise in cloud services, mobile development, and cutting-edge AI technologies. \\u00B7 Successfully migrated HTM\\u0027s public transport monitoring system to Azure IOT Hub, enabling real-time telemetry, predictive maintenance, and two-way communication. This cloud-based transformation boosted efficiency, allowed on-demand scalability, reduced costs, and enhanced customer experience for Telexis Solutions\\u0027 platform. . Architected and implemented a scalable multi-tenant solution for Airbus Defence \\u0026 Space, featuring a white-labeled customer web portal. Leveraged cutting-edge web and API technologies, coupled with a custom-built code generator and framework, resulting in an 8x reduction in \"}],\"dataPointsImages\":null,\"followup_questions\":[\"What specific projects has Rom worked on for Airbus Defence \\u0026 Space and Marlink?\",\"What are some notable achievements or innovations Rom has made in his career?\",\"How does Rom\\u0027s experience as an Azure Cloud \\u0026 AI Architect contribute to his leadership abilities?\"],\"thoughts\":[{\"title\":\"Thoughts\",\"description\":\"I found this information in Romuald Djaroud\\u0027s professional summary and experience listed in CVRom-0.pdf and CVRom-1.pdf.\",\"props\":null}],\"data_points\":{\"text\":[\"CVRom-0.pdf: Romuald Djaroud Certified Azure Cloud \\u0026 AI Architect | 20\\u002B Years of Innovation and Leadership The Hague, The Netherlands | Email: rom@yrdata.ai | LinkedIn: in/@romdja Professional Summary Innovative Microsoft Azure Cloud \\u0026 AI Solution Architect and Developer, leading complex projects for clients like Airbus Defence \\u0026 Space and Marlink. Recognized for successfully migrating critical systems to the cloud, integrating cutting- edge technologies and significantly enhancing system performance, security and delivery. Fluent in English, Dutch, and French with strong leadership, time management, and creative problem-solving skills. Education \\u00B7 Microsoft Certified: Azure AI Engineer Associate (AI-102), Microsoft, 2024 \\u00B7 Ongoing Professional Development: Tech Communities, Learning Platforms, Conferences \\u00B7 Time Management and Productivity, Online Brian Tracy Academy, 2021 \\u00B7 Effective Communication and Consultancy, Avanade, Seattle, USA, 2006 \\u00B7 University of Sciences \\u0026 Technologies, Lille, France, 1994 - 1998 Areas of Expertise \\u00B7 Cloud Architecture Azure Cloud, SaaS, Real-Time Event-Driven \",\"CVRom-1.pdf:  \\u00B7 Recognized for streamlining complex business processes, achieving high client satisfaction rates, and mentoring team members on adopting latest technologies. \\u00B7 Successfully transitioned from corporate consulting roles at Avanade and Atos to independent freelance consulting in 2006. \",\"CVRom-1.pdf: Professional Experience Azure Cloud Solution Architect \\u0026 Developer, YrData.AI The Netherlands / Remote | 2014 - Present \\u00B7 Developed an Al-powered travel assistant for Canggu, Bali using .NET MAUI and Azure Al services. Launched on App Store and Google Play. Integrated Semantic Kernel and Azure AI Search for advanced RAG and Generative AI capabilities. Implemented Agentive AI principles, showcasing expertise in cloud services, mobile development, and cutting-edge AI technologies. \\u00B7 Successfully migrated HTM\\u0027s public transport monitoring system to Azure IOT Hub, enabling real-time telemetry, predictive maintenance, and two-way communication. This cloud-based transformation boosted efficiency, allowed on-demand scalability, reduced costs, and enhanced customer experience for Telexis Solutions\\u0027 platform. . Architected and implemented a scalable multi-tenant solution for Airbus Defence \\u0026 Space, featuring a white-labeled customer web portal. Leveraged cutting-edge web and API technologies, coupled with a custom-built code generator and framework, resulting in an 8x reduction in \"]},\"ThoughtsString\":\"Thoughts: I found this information in Romuald Djaroud\\u0027s professional summary and experience listed in CVRom-0.pdf and CVRom-1.pdf.\"},\"citationBaseUrl\":\"https://stgm7ggh3tkvxpc.blob.core.windows.net/content\",\"content_filter_results\":null}]}";

            response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(mock)
            };

        }
        else
        {
            response = await httpClient.PostAsync(apiRoute, body);
        }

        if (response.IsSuccessStatusCode)
        {
            var answer = await response.Content.ReadFromJsonAsync<ChatAppResponseOrError>();
            return result with
            {
                IsSuccessful = answer is not null,
                Response = answer,
            };
        }
        else
        {
            var errorTitle = $"HTTP {(int)response.StatusCode} : {response.ReasonPhrase ?? "☹️ Unknown error..."}";
            var answer = new ChatAppResponseOrError(
                Array.Empty<ResponseChoice>(),
                errorTitle);

            return result with
            {
                IsSuccessful = false,
                Response = answer
            };
        }
    }
}
