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
