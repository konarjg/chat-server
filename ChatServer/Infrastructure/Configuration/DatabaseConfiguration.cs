namespace Infrastructure.Configuration;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User> {
  public void Configure(EntityTypeBuilder<User> builder) {
    builder.ToTable("Users");
    builder.HasKey(u => u.Id);
  }
}

public class ChatConfiguration : IEntityTypeConfiguration<Chat> {
  public void Configure(EntityTypeBuilder<Chat> builder) {
    builder.ToTable("Chats");
    builder.HasKey(c => c.Id);
    builder.HasOne(c => c.Sender).WithMany().HasForeignKey(c => c.SenderId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message> {
  public void Configure(EntityTypeBuilder<Message> builder) {
    builder.ToTable("Messages");
    builder.HasKey(m => m.Id);
    builder.HasOne(m => m.Chat).WithMany().HasForeignKey(m => m.ChatId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken> {
  public void Configure(EntityTypeBuilder<RefreshToken> builder) {
    builder.ToTable("RefreshTokens");
    builder.HasKey(r => r.Id);
    builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
  }
}
