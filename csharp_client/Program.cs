using Grpc.Core;
using Grpc.Net.Client;
using Chat;

var channel = GrpcChannel.ForAddress("http://localhost:5000");
var authClient = new AuthService.AuthServiceClient(channel);
var chatClient = new ChatService.ChatServiceClient(channel);

// Register a new user
var registerResponse = await authClient.RegisterAsync(new RegisterRequest
{
    Name = "testusercsharp",
    Password = "password",
    PublicKey = "key"
});
Console.WriteLine($"Registered user with access token: {registerResponse.AccessToken}");

// Login
var loginResponse = await authClient.LoginAsync(new LoginRequest
{
    Name = "testusercsharp",
    Password = "password"
});
Console.WriteLine($"Logged in with access token: {loginResponse.AccessToken}");

var headers = new Metadata
{
    { "Authorization", $"Bearer {loginResponse.AccessToken}" }
};

// Create a chat
var createChatResponse = await chatClient.CreateChatAsync(new CreateChatRequest
{
    ReceiverId = 1,
    SenderEncryptedAesKey = Google.Protobuf.ByteString.CopyFromUtf8("key"),
    ReceiverEncryptedAesKey = Google.Protobuf.ByteString.CopyFromUtf8("key")
}, headers);
Console.WriteLine($"Created chat with id: {createChatResponse.Id}");

// Chat stream
using var call = chatClient.ChatStream(headers);

var readTask = Task.Run(async () =>
{
    await foreach (var message in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Received message: {message.NewMessage.AesEncryptedContent.ToStringUtf8()}");
    }
});

await call.RequestStream.WriteAsync(new ClientToServerMessage
{
    SendMessage = new SendMessageRequest
    {
        ChatId = createChatResponse.Id,
        AesEncryptedContent = Google.Protobuf.ByteString.CopyFromUtf8("hello")
    }
});

await call.RequestStream.CompleteAsync();
await readTask;
