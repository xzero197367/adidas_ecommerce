using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adidas.Context.Migrations
{
    /// <inheritdoc />
    public partial class _add_cutomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-aaaa-bbbb-cccc-111111111111",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2cd08120-af28-4185-b601-bf96bad7e15e", new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3541), "AQAAAAIAAYagAAAAEI4HhUm/TeqB80FAYj1hj3ed4n2ap4+4B3SmAhWSCtpyRWwtHFy180HSbSi8E/DoXw==", "4439ce47-3819-41b9-a9b4-336f12138e35" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-aaaa-bbbb-cccc-222222222222",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d340f116-a3f1-41f8-98b0-31e83ba82fd2", new DateTime(2025, 8, 9, 11, 41, 2, 879, DateTimeKind.Utc).AddTicks(1288), "AQAAAAIAAYagAAAAEImfrPP+tJ5TLq4c5Ty+DLhNG5Q167nPl7TD4fu/ACd/inyIYGO5UGs+Q4ieBKVlEg==", "dc90c219-461f-4962-88ee-004263c830cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-aaaa-bbbb-cccc-333333333333",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2ab0d7c8-3e8e-4700-9ef0-3a4b43ae7afc", new DateTime(2025, 8, 9, 11, 41, 2, 945, DateTimeKind.Utc).AddTicks(4850), "AQAAAAIAAYagAAAAEMFi9QRs5m/BiiuEGydWssypASelAQ1jBqxkKCGgdiji3S7KJeynLEU1PEMCfis3sw==", "88b0eb93-e34f-484e-9373-4b3eaca2b61c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2a940a38-368d-4de4-ae4b-4d18662231de", new DateTime(2025, 8, 9, 11, 41, 2, 738, DateTimeKind.Utc).AddTicks(6046), "AQAAAAIAAYagAAAAEN7cQwrm+/TfxqDa4ay3aO+ahw0eF01p6GLCZOfXCis+jtNqENrbC1dD8sYa76vv4g==", "0ec8e85b-2695-414d-b8ab-3b8a024c4a32" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3098), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3103) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3144), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3145) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3148), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3149) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3206), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3206) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3215), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3215) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3434), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3434) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3439), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3440) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3376), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3376) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3397), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3397) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3297), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3289) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3306), new DateTime(2025, 8, 9, 11, 41, 2, 807, DateTimeKind.Utc).AddTicks(3300) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000c"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 41, 3, 9, DateTimeKind.Utc).AddTicks(3026));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000d"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 41, 3, 9, DateTimeKind.Utc).AddTicks(3075));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000e"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 41, 3, 9, DateTimeKind.Utc).AddTicks(3092));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "11111111-aaaa-bbbb-cccc-111111111111",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "60a011d6-6d9f-4b76-a0ec-3fe7c9effc09", new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2902), "AQAAAAIAAYagAAAAEPyA83GIlnAbj4jFQImLpbxwN4+9DAB/Wsat4xRhoIghrGU7g5hhgHPPDl4OdNb9qA==", "4751c609-5fe8-4d88-893d-25e3bb531a9e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "22222222-aaaa-bbbb-cccc-222222222222",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9bfb22dd-1140-43be-aff5-45d421acd491", new DateTime(2025, 8, 9, 11, 39, 12, 92, DateTimeKind.Utc).AddTicks(3661), "AQAAAAIAAYagAAAAEHbcffpTWdZNM6VzqC/Ntp0rw5Dcq24Idru39oXBXlFpHQQmJOplCrN532J6anKu/w==", "fdec43cd-aa51-476b-ae0b-1a07ac9b7aa3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "33333333-aaaa-bbbb-cccc-333333333333",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "48a37b94-38b6-4a86-b072-640bcab27171", new DateTime(2025, 8, 9, 11, 39, 12, 150, DateTimeKind.Utc).AddTicks(8291), "AQAAAAIAAYagAAAAEFQ/0Oz2kFNZo0YaJw9eb7W5PlpxmQv8/OpjBk6mc3MOSt5uWKi9O4vTx3HqiFePxA==", "557c3247-31c1-4f07-8319-bef4e561da0d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "98abe2c6-01a8-4128-86e0-0307f98deee8", new DateTime(2025, 8, 9, 11, 39, 11, 970, DateTimeKind.Utc).AddTicks(7264), "AQAAAAIAAYagAAAAEDzPs1wTOZ9Rz2fKhBts92gbTBropWLVj/Ble2VRlR/eTAhENIMWennUAPAhECJkXg==", "53d8c7eb-d783-4f67-84fa-03e53fb1a7cc" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2539), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2544) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2587), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2588) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2591), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2591) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2624), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2624) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2628), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2629) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2775), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2775) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2782), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2783) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2732), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2732) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2741), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2741) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2680), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2670) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2696), new DateTime(2025, 8, 9, 11, 39, 12, 33, DateTimeKind.Utc).AddTicks(2682) });

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000c"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 39, 12, 211, DateTimeKind.Utc).AddTicks(2356));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000d"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 39, 12, 211, DateTimeKind.Utc).AddTicks(2433));

            migrationBuilder.UpdateData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000e"),
                column: "UpdatedAt",
                value: new DateTime(2025, 8, 9, 11, 39, 12, 211, DateTimeKind.Utc).AddTicks(2439));
        }
    }
}
