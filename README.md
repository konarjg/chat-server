# chat-server

This repository contains a gRPC-based chat server built with C# and .NET. It follows a clean architecture pattern, separating the application into three distinct projects: `ChatServer`, `Domain`, and `Infrastructure`.

## Architecture

The application is divided into the following projects:

*   **`ChatServer`**: This is the main project that exposes the gRPC services. It handles the presentation layer, receiving requests and sending responses. It also contains the protobuf definition file (`chat.proto`) that defines the service contract.
*   **`Domain`**: This project contains the core business logic of the application. It includes entities, services, and interfaces that define the application's behavior. It is completely independent of the presentation and infrastructure layers.
*   **`Infrastructure`**: This project implements the interfaces defined in the `Domain` project. It handles data access using Entity Framework Core with a SQLite database, as well as other concerns like authentication and real-time communication.

This separation of concerns makes the application more maintainable, scalable, and testable.

## Running the Server

To run the server, follow these steps:

1.  **Install the .NET SDK**: If you don't have it already, install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or a later version.
2.  **Clone the repository**:
    ```bash
    git clone https://github.com/your-username/chat-server.git
    cd chat-server
    ```
3.  **Run the server**:
    ```bash
    dotnet run --project ChatServer/ChatServer/ChatServer.csproj
    ```
The server will start and listen for gRPC requests on the configured port.

## Client Examples

Here are some examples of how to connect to the server using different programming languages.

### Python

```python
import grpc
import chat_pb2
import chat_pb2_grpc

def run():
    channel = grpc.insecure_channel('localhost:5000')
    auth_stub = chat_pb2_grpc.AuthServiceStub(channel)
    chat_stub = chat_pb2_grpc.ChatServiceStub(channel)

    # Register a new user
    register_response = auth_stub.Register(chat_pb2.RegisterRequest(name='testuser', password='password', public_key='key'))
    print(f'Registered user with access token: {register_response.access_token}')

    # Login
    login_response = auth_stub.Login(chat_pb2.LoginRequest(name='testuser', password='password'))
    print(f'Logged in with access token: {login_response.access_token}')

    # Create a chat
    create_chat_response = chat_stub.CreateChat(chat_pb2.CreateChatRequest(receiver_id=2, sender_encrypted_aes_key=b'key', receiver_encrypted_aes_key=b'key'), metadata=[('authorization', f'Bearer {login_response.access_token}')])
    print(f'Created chat with id: {create_chat_response.id}')

    # Chat stream
    def client_stream():
        yield chat_pb2.ClientToServerMessage(send_message=chat_pb2.SendMessageRequest(chat_id=create_chat_response.id, aes_encrypted_content=b'hello'))

    server_stream = chat_stub.ChatStream(client_stream(), metadata=[('authorization', f'Bearer {login_response.access_token}')])

    for message in server_stream:
        print(f'Received message: {message.new_message.aes_encrypted_content}')

if __name__ == '__main__':
    run()
```

### Java

```java
package com.example;

import com.google.protobuf.ByteString;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.stub.StreamObserver;
import chat.Chat;
import chat.AuthServiceGrpc;
import chat.ChatServiceGrpc;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

public class JavaClient {

    public static void main(String[] args) throws InterruptedException {
        ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 5000)
                .usePlaintext()
                .build();

        AuthServiceGrpc.AuthServiceBlockingStub authStub = AuthServiceGrpc.newBlockingStub(channel);
        ChatServiceGrpc.ChatServiceStub chatStub = ChatServiceGrpc.newStub(channel);

        // Register a new user
        Chat.AuthResponse registerResponse = authStub.register(Chat.RegisterRequest.newBuilder()
                .setName("testuserjava")
                .setPassword("password")
                .setPublicKey("key")
                .build());
        System.out.println("Registered user with access token: " + registerResponse.getAccessToken());

        // Login
        Chat.AuthResponse loginResponse = authStub.login(Chat.LoginRequest.newBuilder()
                .setName("testuserjava")
                .setPassword("password")
                .build());
        System.out.println("Logged in with access token: " + loginResponse.getAccessToken());

        // Create a chat
        Chat.Chat createChatResponse = authStub.withOption(io.grpc.CallOptions.DEFAULT.withCallCredentials(new BearerToken(loginResponse.getAccessToken())))
                .createChat(Chat.CreateChatRequest.newBuilder()
                        .setReceiverId(1)
                        .setSenderEncryptedAesKey(ByteString.copyFromUtf8("key"))
                        .setReceiverEncryptedAesKey(ByteString.copyFromUtf8("key"))
                        .build());
        System.out.println("Created chat with id: " + createChatResponse.getId());

        // Chat stream
        CountDownLatch latch = new CountDownLatch(1);
        StreamObserver<Chat.ClientToServerMessage> requestObserver = chatStub.withOption(io.grpc.CallOptions.DEFAULT.withCallCredentials(new BearerToken(loginResponse.getAccessToken())))
                .chatStream(new StreamObserver<Chat.ServerToClientMessage>() {
                    @Override
                    public void onNext(Chat.ServerToClientMessage value) {
                        System.out.println("Received message: " + value.getNewMessage().getAesEncryptedContent().toStringUtf8());
                    }

                    @Override
                    public void onError(Throwable t) {
                        t.printStackTrace();
                        latch.countDown();
                    }

                    @Override
                    public void onCompleted() {
                        latch.countDown();
                    }
                });

        requestObserver.onNext(Chat.ClientToServerMessage.newBuilder()
                .setSendMessage(Chat.SendMessageRequest.newBuilder()
                        .setChatId(createChatResponse.getId())
                        .setAesEncryptedContent(ByteString.copyFromUtf8("hello"))
                        .build())
                .build());

        requestObserver.onCompleted();
        latch.await(1, TimeUnit.MINUTES);

        channel.shutdownNow().awaitTermination(5, TimeUnit.SECONDS);
    }
}
```

### C#

```csharp
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
```
