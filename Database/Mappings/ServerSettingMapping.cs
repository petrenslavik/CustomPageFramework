//using CustomPage.Database.DbModels;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace CustomPage.Database.Mappings
//{
//    public class ServerSettingMapping : IEntityTypeConfiguration<ServerSetting>
//    {
//        public void Configure(EntityTypeBuilder<ServerSetting> builder)
//        {
//            builder.HasKey(x => x.ServerType);
//            builder.Property(x => x.ServerType).HasMaxLength(64);
//            builder.Property(x => x.ServerUrl);
//            builder.Property(x => x.Username).HasMaxLength(128);
//            builder.Property(x => x.Password).HasMaxLength(128);
//            builder.Property(x => x.LastSavedOn).IsRequired();
//        }
//    }
//}
