using ServiceCenter.Data.Models;

namespace ServiceCenter.Data
{
    public static class DbSeed
    {
        public static void Ensure(AppDbContext db)
        {
            db.Database.EnsureCreated();
            if (db.Users.Any())
            {
                AssignDemoMaster(db);
                return;
            }

            db.TicketStatuses.AddRange(
                new TicketStatus { Code = "accepted", Name = "Принята" },
                new TicketStatus { Code = "in_progress", Name = "В работе" },
                new TicketStatus { Code = "ready", Name = "Готова" },
                new TicketStatus { Code = "closed", Name = "Закрыта" },
                new TicketStatus { Code = "cancelled", Name = "Отменена" });

            db.DeviceTypes.AddRange(
                new DeviceType { Name = "Смартфон" },
                new DeviceType { Name = "Ноутбук" },
                new DeviceType { Name = "Планшет" });

            db.Services.AddRange(
                new ServiceItem { Name = "Диагностика" },
                new ServiceItem { Name = "Замена экрана" },
                new ServiceItem { Name = "Чистка" });

            db.Users.AddRange(
                new User { Login = "admin", PasswordHash = PasswordHasher.Hash("admin123"), FullName = "Администратор", Role = "administrator", IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Login = "dispatcher", PasswordHash = PasswordHasher.Hash("dispatcher123"), FullName = "Диспетчер", Role = "dispatcher", IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Login = "master", PasswordHash = PasswordHasher.Hash("master123"), FullName = "Мастер", Role = "master", IsActive = true, CreatedAt = DateTime.UtcNow });

            db.SaveChanges();

            Client client = new Client { FullName = "Иванов Иван", Phone = "+79000000000", Note = "Постоянный клиент" };
            db.Clients.Add(client);
            db.SaveChanges();

            Ticket ticket = new Ticket
            {
                Client_Id = client.Id,
                DeviceType_Id = db.DeviceTypes.First().Id,
                Service_Id = db.Services.First().Id,
                Status_Id = db.TicketStatuses.First(s => s.Code == "accepted").Id,
                FaultText = "Не включается после падения",
                CreatedAt = DateTime.UtcNow
            };
            db.Tickets.Add(ticket);
            db.SaveChanges();
            AssignDemoMaster(db);
        }

        private static void AssignDemoMaster(AppDbContext db)
        {
            User? master = db.Users.FirstOrDefault(u => u.Login == "master");
            Ticket? ticket = db.Tickets.FirstOrDefault(t => t.FaultText == "Не включается после падения");
            if (master == null || ticket == null || ticket.Master_Id != null)
                return;
            ticket.Master_Id = master.Id;
            db.SaveChanges();
        }
    }
}
