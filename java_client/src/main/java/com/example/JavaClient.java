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
        Chat.Chat createChatResponse = chatStub.withOption(io.grpc.CallOptions.DEFAULT.withCallCredentials(new BearerToken(loginResponse.getAccessToken())))
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
