# ChatServer

This project is a gRPC-based chat server built with ASP.NET Core, following the principles of Hexagonal Architecture (also known as Ports and Adapters). This architectural style promotes a clear separation of concerns, making the application more modular, testable, and maintainable.

## Architecture

The application is divided into three main layers:

*   **Domain:** This is the core of the application, containing the business logic and entities. It has no dependencies on any other layer.
*   **Application:** This layer orchestrates the application's use cases, acting as a bridge between the outside world and the domain. In this project, the gRPC services in the `ChatServer` project serve as this layer.
*   **Infrastructure:** This layer contains the concrete implementations of the interfaces (ports) defined in the domain layer. This includes things like database repositories, authentication services, and real-time messaging.

This architecture is visualized in the diagram below:

```mermaid
graph TD
    subgraph " "
        direction TB
        subgraph "Application Layer (ChatServer)"
            direction LR
            A[gRPC Services] --> B{Domain Ports};
        end
        subgraph "Domain Layer"
            direction LR
            B --> C[Domain Logic];
        end
        subgraph "Infrastructure Layer"
            direction LR
            D[Infrastructure Adapters] --> B;
        end
    end

    subgraph "External"
        direction LR
        E[Clients] --> A;
        D --> F[Database];
    end

    style B fill:#f9f,stroke:#333,stroke-width:2px
```

### Project Structure

*   `ChatServer/`: The main project, containing the gRPC services and the application's entry point.
    *   `Protos/`: The gRPC service definitions (`.proto` files).
    *   `Services/`: The gRPC service implementations.
*   `Domain/`: The core of the application.
    *   `Entities/`: The domain entities.
    *   `Interfaces/`: The domain service interfaces.
    *   `Ports/`: The interfaces for the infrastructure layer (repositories, etc.).
*   `Infrastructure/`: The implementation of the domain's ports.
    *   `Repositories/`: The database repository implementations.
    *   `Auth/`: The authentication service implementations.
    *   `Realtime/`: The real-time messaging implementation.

## Running the Server

To run the server, you will need the .NET 9 SDK installed.

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/konarjg/chat-server.git
    cd chat-server
    ```

2.  **Restore dependencies:**

    ```bash
    dotnet restore
    ```

3.  **Apply database migrations:**

    The application uses Entity Framework Core for database management. To create and seed the database, run the following command from the `ChatServer` directory:

    ```bash
    dotnet ef database update -p Infrastructure -s ChatServer
    ```

4.  **Run the application:**

    ```bash
    dotnet run --project ChatServer/ChatServer
    ```

    The server will start and listen for gRPC connections on the configured port.

## Client Code Examples

Here are some examples of how to use the gRPC client in different languages. These examples assume that you have already generated the client code from the `.proto` file.

**Note:** For connections on the same network, replace `localhost` with the server's IP address.

### C#

```csharp
using Grpc.Core;
using Grpc.Net.Client;
using Chat;
using Google.Protobuf;

// --- 1. Setup ---
using var channel = GrpcChannel.ForAddress("http://localhost:5241");
var authClient = new AuthService.AuthServiceClient(channel);
var userClient = new UserService.UserServiceClient(channel);
var chatClient = new ChatService.ChatServiceClient(channel);

// --- 2. Register and Login ---
await authClient.RegisterAsync(new RegisterRequest { Name = "testuser", Password = "password", PublicKey = "key" });
await authClient.RegisterAsync(new RegisterRequest { Name = "anotheruser", Password = "password", PublicKey = "key2" });
var loginResponse = await authClient.LoginAsync(new LoginRequest { Name = "testuser", Password = "password" });
string accessToken = loginResponse.AccessToken;
string refreshToken = loginResponse.RefreshToken;
Console.WriteLine("Logged in successfully!");

// --- 3. Making Authenticated Calls ---
var headers = new Metadata { { "Authorization", $"Bearer {accessToken}" } };
var usersResponse = await userClient.GetUsersAsync(new GetUsersRequest { PageSize = 10 }, headers);
var otherUser = usersResponse.Users.FirstOrDefault(u => u.Name == "anotheruser");
Console.WriteLine($"Found {usersResponse.Users.Count} users.");

// --- 4. Chat Operations ---
if (otherUser != null)
{
    // Create a chat
    var createChatRequest = new CreateChatRequest
    {
        ReceiverId = otherUser.Id,
        SenderEncryptedAesKey = ByteString.CopyFrom(new byte[32]),
        ReceiverEncryptedAesKey = ByteString.CopyFrom(new byte[32])
    };
    var chat = await chatClient.CreateChatAsync(createChatRequest, headers);
    Console.WriteLine($"Created chat with ID: {chat.Id}");

    // Get message history (will be empty)
    var historyRequest = new GetMessageHistoryRequest { ChatId = chat.Id, PageSize = 20 };
    var historyResponse = await chatClient.GetMessageHistoryAsync(historyRequest, headers);
    Console.WriteLine($"Initial message count: {historyResponse.Messages.Count}");

    // --- 5. Real-time Chat Stream ---
    using var call = chatClient.ChatStream(headers);

    // Task to receive messages from the server
    var readTask = Task.Run(async () =>
    {
        await foreach (var message in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"Received message: {message.NewMessage.AesEncryptedContent.ToStringUtf8()}");
        }
    });

    // Send a message
    var sendMessageRequest = new SendMessageRequest { ChatId = chat.Id, AesEncryptedContent = ByteString.CopyFromUtf8("Hello, world!") };
    await call.RequestStream.WriteAsync(new ClientToServerMessage { SendMessage = sendMessageRequest });

    await Task.Delay(1000); // Wait for message to be received

    await call.RequestStream.CompleteAsync();
    await readTask;
}

// --- 6. Refreshing the Access Token ---
var refreshResponse = await authClient.RefreshAsync(new RefreshRequest { RefreshToken = refreshToken });
accessToken = refreshResponse.AccessToken;
Console.WriteLine("Token refreshed!");

// --- 7. Logging Out ---
await authClient.LogoutAsync(new LogoutRequest { RefreshToken = refreshToken });
Console.WriteLine("Logged out.");
```

### Python

```python
import grpc
import chat_pb2
import chat_pb2_grpc
import threading
import time

# --- 1. Setup ---
channel = grpc.insecure_channel('localhost:5241')
auth_stub = chat_pb2_grpc.AuthServiceStub(channel)
user_stub = chat_pb2_grpc.UserServiceStub(channel)
chat_stub = chat_pb2_grpc.ChatServiceStub(channel)

# --- 2. Register and Login ---
auth_stub.Register(chat_pb2.RegisterRequest(name='testuser', password='password', public_key='key'))
auth_stub.Register(chat_pb2.RegisterRequest(name='anotheruser', password='password', public_key='key2'))
login_response = auth_stub.Login(chat_pb2.LoginRequest(name='testuser', password='password'))
access_token = login_response.access_token
refresh_token = login_response.refresh_token
print("Logged in successfully!")

# --- 3. Making Authenticated Calls ---
auth_metadata = [('authorization', f'Bearer {access_token}')]
users_response = user_stub.GetUsers(chat_pb2.GetUsersRequest(page_size=10), metadata=auth_metadata)
other_user = next((u for u in users_response.users if u.name == "anotheruser"), None)
print(f"Found {len(users_response.users)} users.")

# --- 4. Chat Operations ---
if other_user:
    # Create a chat
    create_chat_request = chat_pb2.CreateChatRequest(receiver_id=other_user.id, sender_encrypted_aes_key=b'x'*32, receiver_encrypted_aes_key=b'y'*32)
    chat = chat_stub.CreateChat(create_chat_request, metadata=auth_metadata)
    print(f"Created chat with ID: {chat.id}")

    # Get message history
    history_request = chat_pb2.GetMessageHistoryRequest(chat_id=chat.id, page_size=20)
    history_response = chat_stub.GetMessageHistory(history_request, metadata=auth_metadata)
    print(f"Initial message count: {len(history_response.messages)}")

    # --- 5. Real-time Chat Stream ---
    def send_messages():
        time.sleep(1) # wait for receiver to be ready
        message = chat_pb2.SendMessageRequest(chat_id=chat.id, aes_encrypted_content=b"Hello from Python!")
        yield chat_pb2.ClientToServerMessage(send_message=message)

    responses = chat_stub.ChatStream(send_messages(), metadata=auth_metadata)
    for response in responses:
        print(f"Received message: {response.new_message.aes_encrypted_content.decode('utf-8')}")

# --- 6. Refreshing the Access Token ---
refresh_response = auth_stub.Refresh(chat_pb2.RefreshRequest(refresh_token=refresh_token))
access_token = refresh_response.access_token
print("Token refreshed!")

# --- 7. Logging Out ---
auth_stub.Logout(chat_pb2.LogoutRequest(refresh_token=refresh_token))
print("Logged out.")
```

### Java

```java
import io.grpc.*;
import io.grpc.stub.StreamObserver;
import chat.Chat.*;
import chat.AuthServiceGrpc;
import chat.ChatServiceGrpc;
import chat.UserServiceGrpc;
import com.google.protobuf.ByteString;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

// --- 1. Setup ---
ManagedChannel channel = ManagedChannelBuilder.forAddress("localhost", 5241).usePlaintext().build();
AuthServiceGrpc.AuthServiceBlockingStub authStub = AuthServiceGrpc.newBlockingStub(channel);
UserServiceGrpc.UserServiceBlockingStub userStub = UserServiceGrpc.newBlockingStub(channel);
ChatServiceGrpc.ChatServiceBlockingStub chatStub = ChatServiceGrpc.newBlockingStub(channel);
ChatServiceGrpc.ChatServiceStub asyncChatStub = ChatServiceGrpc.newStub(channel);

// --- 2. Register and Login ---
authStub.register(RegisterRequest.newBuilder().setName("testuser").setPassword("password").setPublicKey("key").build());
authStub.register(RegisterRequest.newBuilder().setName("anotheruser").setPassword("password").setPublicKey("key2").build());
AuthResponse loginResponse = authStub.login(LoginRequest.newBuilder().setName("testuser").setPassword("password").build());
final String accessToken = loginResponse.getAccessToken();
String refreshToken = loginResponse.getRefreshToken();
System.out.println("Logged in successfully!");

// --- 3. Authenticated Stubs ---
ClientInterceptor authInterceptor = new ClientInterceptor() {
    public <ReqT, RespT> ClientCall<ReqT, RespT> interceptCall(MethodDescriptor<ReqT, RespT> method, CallOptions callOptions, Channel next) {
        return new ForwardingClientCall.SimpleForwardingClientCall<ReqT, RespT>(next.newCall(method, callOptions)) {
            public void start(Listener<RespT> responseListener, Metadata headers) {
                headers.put(Metadata.Key.of("Authorization", Metadata.ASCII_STRING_MARSHALLER), "Bearer " + accessToken);
                super.start(responseListener, headers);
            }
        };
    }
};
UserServiceGrpc.UserServiceBlockingStub authedUserStub = userStub.withInterceptors(authInterceptor);
ChatServiceGrpc.ChatServiceBlockingStub authedChatStub = chatStub.withInterceptors(authInterceptor);
ChatServiceGrpc.ChatServiceStub authedAsyncChatStub = asyncChatStub.withInterceptors(authInterceptor);

// --- 4. Chat Operations ---
GetUsersResponse usersResponse = authedUserStub.getUsers(GetUsersRequest.newBuilder().setPageSize(10).build());
User otherUser = usersResponse.getUsersList().stream().filter(u -> u.getName().equals("anotheruser")).findFirst().orElse(null);

if (otherUser != null) {
    // Create a chat
    CreateChatRequest createChatRequest = CreateChatRequest.newBuilder()
        .setReceiverId(otherUser.getId())
        .setSenderEncryptedAesKey(ByteString.copyFrom(new byte[32]))
        .setReceiverEncryptedAesKey(ByteString.copyFrom(new byte[32]))
        .build();
    chat.Chat newChat = authedChatStub.createChat(createChatRequest);
    System.out.println("Created chat with ID: " + newChat.getId());

    // --- 5. Real-time Chat Stream ---
    CountDownLatch latch = new CountDownLatch(1);
    StreamObserver<ServerToClientMessage> responseObserver = new StreamObserver<>() {
        public void onNext(ServerToClientMessage value) {
            System.out.println("Received message: " + value.getNewMessage().getAesEncryptedContent().toStringUtf8());
        }
        public void onError(Throwable t) { latch.countDown(); }
        public void onCompleted() { latch.countDown(); }
    };
    StreamObserver<ClientToServerMessage> requestObserver = authedAsyncChatStub.chatStream(responseObserver);
    SendMessageRequest message = SendMessageRequest.newBuilder()
        .setChatId(newChat.getId())
        .setAesEncryptedContent(ByteString.copyFromUtf8("Hello from Java!"))
        .build();
    requestObserver.onNext(ClientToServerMessage.newBuilder().setSendMessage(message).build());
    requestObserver.onCompleted();
    latch.await(1, TimeUnit.MINUTES);
}

// --- 6. Refreshing and Logging out ---
refreshToken = authStub.refresh(RefreshRequest.newBuilder().setRefreshToken(refreshToken).build()).getRefreshToken();
System.out.println("Token refreshed!");
authStub.logout(LogoutRequest.newBuilder().setRefreshToken(refreshToken).build());
System.out.println("Logged out.");
```
