// Copyright (c) Microsoft. All rights reserved.

namespace MinimalApi.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup("api");

        // Blazor 📎 Clippy streaming endpoint
        api.MapPost("openai/chat", OnPostChatPromptAsync);

        // Long-form chat w/ contextual history endpoint
        api.MapPost("chat", OnPostChatAsync);

        // Upload a document
        api.MapPost("documents", OnPostDocumentAsync);

        // Get all documents
        api.MapGet("documents", OnGetDocumentsAsync);

        // Get DALL-E image result from prompt
        api.MapPost("images", OnPostImagePromptAsync);

        api.MapGet("enableLogout", OnGetEnableLogout);

        return app;
    }

    private static IResult OnGetEnableLogout(HttpContext context)
    {
        var header = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"];
        var enableLogout = !string.IsNullOrEmpty(header);

        return TypedResults.Ok(enableLogout);
    }

    private static async IAsyncEnumerable<ChatChunkResponse> OnPostChatPromptAsync(
        PromptRequest prompt,
        OpenAIClient client,
        IConfiguration config,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var deploymentId = config["AZURE_OPENAI_CHATGPT_DEPLOYMENT"];
        var response = await client.GetChatCompletionsStreamingAsync(
            new ChatCompletionsOptions
            {
                DeploymentName = deploymentId,
                Messages =
                {
                    new ChatRequestSystemMessage("""
                        You're an AI assistant for developers, helping them write code more efficiently.
                        You're name is **Blazor 📎 Clippy** and you're an expert Blazor developer.
                        You're also an expert in ASP.NET Core, C#, TypeScript, and even JavaScript.
                        You will always reply with a Markdown formatted response.
                        """),
                    new ChatRequestUserMessage("What's your name?"),
                    new ChatRequestAssistantMessage("Hi, my name is **Blazor 📎 Clippy**! Nice to meet you."),
                    new ChatRequestUserMessage(prompt.Prompt)
                }
            }, cancellationToken);

        await foreach (var choice in response.WithCancellation(cancellationToken))
        {
            if (choice.ContentUpdate is { Length: > 0 })
            {
                yield return new ChatChunkResponse(choice.ContentUpdate.Length, choice.ContentUpdate);
            }
        }
    }

    private static async Task<IResult> OnPostChatAsync(
        ChatRequest request,
        ReadRetrieveReadChatService chatService,
        CancellationToken cancellationToken)
    {

        // string ser = "{\"Choices\":[{\"index\":0,\"message\":{\"role\":\"assistant\",\"content\":\"Rom is Romuald Djaroud, a certified Azure Cloud \\u0026 AI Architect with over 20 years of experience in innovation and leadership. He has worked on complex projects for clients like Airbus Defence \\u0026 Space and Marlink, and is fluent in English, Dutch, and French. [source] \\u003C\\u003CWhat specific projects has Rom worked on for Airbus Defence \\u0026 Space and Marlink?\\u003E\\u003E  \\u003C\\u003CWhat are some notable achievements or innovations Rom has made in his career?\\u003E\\u003E  \\u003C\\u003CHow does Rom\\u0027s experience as an Azure Cloud \\u0026 AI Architect contribute to his leadership abilities?\\u003E\\u003E \"},\"context\":{\"dataPointsContent\":[{\"Title\":\"CVRom-0.pdf\",\"Content\":\"Romuald Djaroud Certified Azure Cloud \\u0026 AI Architect | 20\\u002B Years of Innovation and Leadership The Hague, The Netherlands | Email: rom@yrdata.ai | LinkedIn: in/@romdja Professional Summary Innovative Microsoft Azure Cloud \\u0026 AI Solution Architect and Developer, leading complex projects for clients like Airbus Defence \\u0026 Space and Marlink. Recognized for successfully migrating critical systems to the cloud, integrating cutting- edge technologies and significantly enhancing system performance, security and delivery. Fluent in English, Dutch, and French with strong leadership, time management, and creative problem-solving skills. Education \\u00B7 Microsoft Certified: Azure AI Engineer Associate (AI-102), Microsoft, 2024 \\u00B7 Ongoing Professional Development: Tech Communities, Learning Platforms, Conferences \\u00B7 Time Management and Productivity, Online Brian Tracy Academy, 2021 \\u00B7 Effective Communication and Consultancy, Avanade, Seattle, USA, 2006 \\u00B7 University of Sciences \\u0026 Technologies, Lille, France, 1994 - 1998 Areas of Expertise \\u00B7 Cloud Architecture Azure Cloud, SaaS, Real-Time Event-Driven \"},{\"Title\":\"CVRom-1.pdf\",\"Content\":\" \\u00B7 Recognized for streamlining complex business processes, achieving high client satisfaction rates, and mentoring team members on adopting latest technologies. \\u00B7 Successfully transitioned from corporate consulting roles at Avanade and Atos to independent freelance consulting in 2006. \"},{\"Title\":\"CVRom-1.pdf\",\"Content\":\"Professional Experience Azure Cloud Solution Architect \\u0026 Developer, YrData.AI The Netherlands / Remote | 2014 - Present \\u00B7 Developed an Al-powered travel assistant for Canggu, Bali using .NET MAUI and Azure Al services. Launched on App Store and Google Play. Integrated Semantic Kernel and Azure AI Search for advanced RAG and Generative AI capabilities. Implemented Agentive AI principles, showcasing expertise in cloud services, mobile development, and cutting-edge AI technologies. \\u00B7 Successfully migrated HTM\\u0027s public transport monitoring system to Azure IOT Hub, enabling real-time telemetry, predictive maintenance, and two-way communication. This cloud-based transformation boosted efficiency, allowed on-demand scalability, reduced costs, and enhanced customer experience for Telexis Solutions\\u0027 platform. . Architected and implemented a scalable multi-tenant solution for Airbus Defence \\u0026 Space, featuring a white-labeled customer web portal. Leveraged cutting-edge web and API technologies, coupled with a custom-built code generator and framework, resulting in an 8x reduction in \"}],\"dataPointsImages\":null,\"followup_questions\":[\"What specific projects has Rom worked on for Airbus Defence \\u0026 Space and Marlink?\",\"What are some notable achievements or innovations Rom has made in his career?\",\"How does Rom\\u0027s experience as an Azure Cloud \\u0026 AI Architect contribute to his leadership abilities?\"],\"thoughts\":[{\"title\":\"Thoughts\",\"description\":\"I found this information in Romuald Djaroud\\u0027s professional summary and experience listed in CVRom-0.pdf and CVRom-1.pdf.\",\"props\":null}],\"data_points\":{\"text\":[\"CVRom-0.pdf: Romuald Djaroud Certified Azure Cloud \\u0026 AI Architect | 20\\u002B Years of Innovation and Leadership The Hague, The Netherlands | Email: rom@yrdata.ai | LinkedIn: in/@romdja Professional Summary Innovative Microsoft Azure Cloud \\u0026 AI Solution Architect and Developer, leading complex projects for clients like Airbus Defence \\u0026 Space and Marlink. Recognized for successfully migrating critical systems to the cloud, integrating cutting- edge technologies and significantly enhancing system performance, security and delivery. Fluent in English, Dutch, and French with strong leadership, time management, and creative problem-solving skills. Education \\u00B7 Microsoft Certified: Azure AI Engineer Associate (AI-102), Microsoft, 2024 \\u00B7 Ongoing Professional Development: Tech Communities, Learning Platforms, Conferences \\u00B7 Time Management and Productivity, Online Brian Tracy Academy, 2021 \\u00B7 Effective Communication and Consultancy, Avanade, Seattle, USA, 2006 \\u00B7 University of Sciences \\u0026 Technologies, Lille, France, 1994 - 1998 Areas of Expertise \\u00B7 Cloud Architecture Azure Cloud, SaaS, Real-Time Event-Driven \",\"CVRom-1.pdf:  \\u00B7 Recognized for streamlining complex business processes, achieving high client satisfaction rates, and mentoring team members on adopting latest technologies. \\u00B7 Successfully transitioned from corporate consulting roles at Avanade and Atos to independent freelance consulting in 2006. \",\"CVRom-1.pdf: Professional Experience Azure Cloud Solution Architect \\u0026 Developer, YrData.AI The Netherlands / Remote | 2014 - Present \\u00B7 Developed an Al-powered travel assistant for Canggu, Bali using .NET MAUI and Azure Al services. Launched on App Store and Google Play. Integrated Semantic Kernel and Azure AI Search for advanced RAG and Generative AI capabilities. Implemented Agentive AI principles, showcasing expertise in cloud services, mobile development, and cutting-edge AI technologies. \\u00B7 Successfully migrated HTM\\u0027s public transport monitoring system to Azure IOT Hub, enabling real-time telemetry, predictive maintenance, and two-way communication. This cloud-based transformation boosted efficiency, allowed on-demand scalability, reduced costs, and enhanced customer experience for Telexis Solutions\\u0027 platform. . Architected and implemented a scalable multi-tenant solution for Airbus Defence \\u0026 Space, featuring a white-labeled customer web portal. Leveraged cutting-edge web and API technologies, coupled with a custom-built code generator and framework, resulting in an 8x reduction in \"]},\"ThoughtsString\":\"Thoughts: I found this information in Romuald Djaroud\\u0027s professional summary and experience listed in CVRom-0.pdf and CVRom-1.pdf.\"},\"citationBaseUrl\":\"https://stgm7ggh3tkvxpc.blob.core.windows.net/content\",\"content_filter_results\":null}]}";


        // var mockresponse =  JsonSerializer.Deserialize<ChatAppResponse>(ser);

        // await Task.Delay(10, cancellationToken);

        // return TypedResults.Ok(mockresponse);


        if (request is { History.Length: > 0 })
        {
            var response = await chatService.ReplyAsync(
                request.History, request.Overrides, cancellationToken);

           // string jsonString = JsonSerializer.Serialize(response);


            return TypedResults.Ok(response);
        }

        return Results.BadRequest();
    }

    private static async Task<IResult> OnPostDocumentAsync(
        [FromForm] IFormFileCollection files,
        [FromServices] AzureBlobStorageService service,
        [FromServices] ILogger<AzureBlobStorageService> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Upload documents");

        var response = await service.UploadFilesAsync(files, cancellationToken);

        logger.LogInformation("Upload documents: {x}", response);

        return TypedResults.Ok(response);
    }

    private static async IAsyncEnumerable<DocumentResponse> OnGetDocumentsAsync(
        BlobContainerClient client,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var blob in client.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            if (blob is not null and { Deleted: false })
            {
                var props = blob.Properties;
                var baseUri = client.Uri;
                var builder = new UriBuilder(baseUri);
                builder.Path += $"/{blob.Name}";

                var metadata = blob.Metadata;
                var documentProcessingStatus = GetMetadataEnumOrDefault<DocumentProcessingStatus>(
                    metadata, nameof(DocumentProcessingStatus), DocumentProcessingStatus.NotProcessed);
                var embeddingType = GetMetadataEnumOrDefault<EmbeddingType>(
                    metadata, nameof(EmbeddingType), EmbeddingType.AzureSearch);

                yield return new(
                    blob.Name,
                    props.ContentType,
                    props.ContentLength ?? 0,
                    props.LastModified,
                    builder.Uri,
                    documentProcessingStatus,
                    embeddingType);

                static TEnum GetMetadataEnumOrDefault<TEnum>(
                    IDictionary<string, string> metadata,
                    string key,
                    TEnum @default) where TEnum : struct => metadata.TryGetValue(key, out var value)
                        && Enum.TryParse<TEnum>(value, out var status)
                            ? status
                            : @default;
            }
        }
    }

    private static async Task<IResult> OnPostImagePromptAsync(
        PromptRequest prompt,
        OpenAIClient client,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var result = await client.GetImageGenerationsAsync(new ImageGenerationOptions
        {
            Prompt = prompt.Prompt,
        },
        cancellationToken);

        var imageUrls = result.Value.Data.Select(i => i.Url).ToList();
        var response = new ImageResponse(result.Value.Created, imageUrls);

        return TypedResults.Ok(response);
    }
}
