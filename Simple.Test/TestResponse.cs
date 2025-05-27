namespace Simple.Test;

public class TestResponse
{
    public Args args { get; set; }
    public Headers headers { get; set; }
    public string origin { get; set; }
    public string url { get; set; }
    public TestData json { get; set; }
}
public class Args { }

public class Headers
{
    public string Accept { get; set; }
    public string AcceptEncoding { get; set; }
    public string AcceptLanguage { get; set; }
    public string ContentType { get; set; }
    public string Dnt { get; set; }
    public string Host { get; set; }
    public string Origin { get; set; }
    public string Referer { get; set; }
    public string UserAgent { get; set; }
}
