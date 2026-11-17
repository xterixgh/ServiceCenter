namespace ServiceCenter.Data
{
    public static class TicketRules
    {
        public static bool CanMove(string role, string fromCode, string toCode)
        {
            if (string.IsNullOrWhiteSpace(role) || fromCode == toCode)
                return false;

            bool allowed = (fromCode, toCode) switch
            {
                ("accepted", "in_progress") => true,
                ("accepted", "cancelled") => true,
                ("in_progress", "ready") => true,
                ("in_progress", "cancelled") => true,
                ("ready", "closed") => role is "dispatcher" or "administrator",
                ("closed", "in_progress") => role == "administrator",
                _ => false
            };

            if (!allowed)
                return false;

            if (role == "master" && toCode == "closed")
                return false;

            return role is "administrator" or "dispatcher" or "master";
        }

        public static string RoleTitle(string role)
        {
            if (role == "administrator")
                return "Администратор";
            if (role == "dispatcher")
                return "Диспетчер";
            if (role == "master")
                return "Мастер";
            return role;
        }
    }
}
