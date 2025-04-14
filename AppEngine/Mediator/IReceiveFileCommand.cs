namespace AppEngine.Mediator;

public interface IReceiveFileCommand
{
    public FileUpload File { get; set; }
}

public record FileUpload(string ContentType, string Filename, Stream FileStream);