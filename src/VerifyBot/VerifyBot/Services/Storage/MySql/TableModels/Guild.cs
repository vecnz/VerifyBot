using Dapper.Contrib.Extensions;

namespace VerifyBot.Services.Storage.MySql.TableModels
{
    [Table(MySqlStorageService.GuildTable)]
    public class Guild
    {
        public ulong id { get; set; }
        public ulong? verified_role_id { get; set; }
        public long creation_time { get; set; }
    }
}