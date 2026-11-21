using Library.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.DAL.Configurations
{
    public class MemberConfig : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MembershipNumber).IsRequired().HasMaxLength(20);
            builder.HasIndex(m => m.MembershipNumber).IsUnique();   // ممنوع تكرار رقم العضوية

            builder.Property(m => m.FullName).IsRequired().HasMaxLength(100);
            builder.Property(m => m.PhoneNumber).HasMaxLength(20);
            builder.Property(m => m.Address).HasMaxLength(250);

            // Member (1) <---> (1) ApplicationUser
            // Restrict: حذف الحساب بالغلط مايمسحش سجل العضوية وتاريخه
            builder.HasOne(m => m.ApplicationUser)
                .WithOne(u => u.Member)
                .HasForeignKey<Member>(m => m.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
