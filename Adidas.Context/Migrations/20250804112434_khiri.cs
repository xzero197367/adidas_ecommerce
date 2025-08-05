using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Adidas.Context.Migrations
{
    /// <inheritdoc />
    public partial class khiri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReviewText",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4b6c119a-eef5-4e34-9a0a-ca175e4fb0b6", new DateTime(2025, 8, 4, 11, 24, 25, 228, DateTimeKind.Utc).AddTicks(8241), "AQAAAAIAAYagAAAAEM1vN5MLgvDoohxfNULJIB3hNxQzBgYHSfi3KdPD5z6BzEyuXT+coCcxUb7/y0Cskg==", "d4cc0471-bbdb-4b0d-87a1-e15645e7bdd0" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(34), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(40) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(100), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(101) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(109), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(110) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(230), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(230) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(244), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(245) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(642), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(643) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(655), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(656) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(523), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(524) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(567), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(568) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(374), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(356) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(390), new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(380) });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "AddedById", "CreatedAt", "IsActive", "IsApproved", "IsVerifiedPurchase", "ProductId", "Rating", "ReviewText", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-44665544000c"), null, new DateTime(2025, 8, 4, 11, 0, 0, 0, DateTimeKind.Utc), true, false, true, new Guid("66666666-6666-6666-6666-666666666666"), 4, "Amazing shoes, waiting for approval!", "Fresh Review - Pending", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(714), "user1@example.com" },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000d"), null, new DateTime(2025, 8, 4, 10, 30, 0, 0, DateTimeKind.Utc), true, false, false, new Guid("77777777-7777-7777-7777-777777777777"), 3, "Good style, need to see more.", "New Review - Pending", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(751), "user2@example.com" },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000e"), null, new DateTime(2025, 8, 4, 10, 0, 0, 0, DateTimeKind.Utc), true, true, true, new Guid("66666666-6666-6666-6666-666666666666"), 5, "", "Approved Feedback", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(762), "user3@example.com" },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000f"), null, new DateTime(2025, 8, 4, 9, 30, 0, 0, DateTimeKind.Utc), true, true, false, new Guid("77777777-7777-7777-7777-777777777777"), 4, "", "Approved Comment", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(772), "user4@example.com" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440010"), null, new DateTime(2025, 8, 4, 11, 15, 0, 0, DateTimeKind.Utc), true, false, true, new Guid("66666666-6666-6666-6666-666666666666"), 2, "Rejected due to bad words.", "Rejected Feedback", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(782), "user5@example.com" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440011"), null, new DateTime(2025, 8, 4, 11, 30, 0, 0, DateTimeKind.Utc), true, false, false, new Guid("77777777-7777-7777-7777-777777777777"), 1, "Rejected for spam content.", "Rejected Comment", new DateTime(2025, 8, 4, 11, 24, 25, 508, DateTimeKind.Utc).AddTicks(799), "user6@example.com" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000c"));

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000d"));

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000e"));

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000f"));

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440010"));

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440011"));

            migrationBuilder.AlterColumn<string>(
                name: "ReviewText",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ba754000-29e4-4b47-a7cb-4bd0012b729d", new DateTime(2025, 7, 29, 12, 36, 58, 52, DateTimeKind.Utc).AddTicks(4939), "AQAAAAIAAYagAAAAEEMD4hvo00ky0bZXEYBkwrYiaMj4iUZRdSi61JjWSFzlp201UyL4A5IucHH0cGhXdg==", "197786cc-5e55-42ea-94e5-be9f08dcf389" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5651), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5657) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5727), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5727) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5742), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5742) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5798), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5798) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5803), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5804) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5980), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5980) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5989), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5989) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5939), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5948), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5948) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5883), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5872) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5894), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5886) });
        }
    }
}
