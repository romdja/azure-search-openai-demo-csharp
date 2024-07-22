// Copyright (c) Microsoft. All rights reserved.

namespace ClientApp.Options;

public class AppSettings
{
    [ConfigurationKeyName("BACKEND_URI")]
    
    //

    //public string BackendUri { get; set; } = "http://127.0.0.1:5001"!;
    public string BackendUri { get; set; } = "https://localhost:7181"!;
}
