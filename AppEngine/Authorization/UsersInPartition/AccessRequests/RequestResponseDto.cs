namespace AppEngine.Authorization.UsersInPartition.AccessRequests;

public class RequestResponseDto
{
    public RequestResponse Response { get; set; }
    public UserInPartitionRole Role { get; set; }
    public string Text { get; set; }
}