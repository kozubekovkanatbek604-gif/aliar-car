namespace Aliyar.Web.Migrations;

internal static class DemoClientsSeedData
{
    internal sealed record DemoClient(string UserId, string Login, string FullName);

    internal static readonly DemoClient[] Clients =
    [
        new("a1000001-0001-4001-8001-000000000001", "shul@gmail.co", "Шульман Екатерина"),
        new("a1000001-0001-4001-8001-000000000002", "ivan@gmail.co", "Иванов Пётр"),
        new("a1000001-0001-4001-8001-000000000003", "petr@gmail.co", "Петрова Анна"),
        new("a1000001-0001-4001-8001-000000000004", "sido@gmail.co", "Сидоров Алексей"),
        new("a1000001-0001-4001-8001-000000000005", "kozl@gmail.co", "Козлова Мария"),
        new("a1000001-0001-4001-8001-000000000006", "niko@gmail.co", "Николаев Дмитрий"),
        new("a1000001-0001-4001-8001-000000000007", "smir@gmail.co", "Смирнова Ольга"),
        new("a1000001-0001-4001-8001-000000000008", "volk@gmail.co", "Волков Игорь"),
        new("a1000001-0001-4001-8001-000000000009", "novi@gmail.co", "Новикова Елена"),
        new("a1000001-0001-4001-8001-000000000010", "moro@gmail.co", "Морозов Сергей"),
    ];

    internal const string ClientRoleId = "b2000002-0002-4002-8002-000000000002";
    internal const string Password = "123456";
}
