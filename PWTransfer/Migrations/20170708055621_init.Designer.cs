using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PWTransfer.Data;
using Shared;

namespace PWTransfer.Migrations
{
    [DbContext(typeof(AccountDbContext))]
    [Migration("20170708055621_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PWTransfer.Data.Account", b =>
                {
                    b.Property<long>("Id");

                    b.Property<decimal>("Value");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("PWTransfer.Data.AccountChange", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AccountId");

                    b.Property<long>("ActionLogId");

                    b.Property<decimal>("Value");

                    b.HasKey("Id");

                    b.HasIndex("ActionLogId");

                    b.ToTable("LastAccountChanges");
                });

            modelBuilder.Entity("PWTransfer.Data.ActionLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<DateTime>("DateUtc");

                    b.Property<long>("Receiver");

                    b.Property<long>("Sender");

                    b.HasKey("Id");

                    b.ToTable("ActionLogs");
                });

            modelBuilder.Entity("PWTransfer.Data.AccountChange", b =>
                {
                    b.HasOne("PWTransfer.Data.ActionLog", "ActionLog")
                        .WithMany()
                        .HasForeignKey("ActionLogId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
