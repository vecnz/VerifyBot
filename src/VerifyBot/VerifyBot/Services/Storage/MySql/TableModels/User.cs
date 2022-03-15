using Dapper.Contrib.Extensions;

namespace VerifyBot.Services.Storage.MySql.TableModels
{
    [Table(MySqlStorageService.UserTable)]
    public class User
    {
        public ulong id { get; set; }
        public int? username_record_id { get; set; }
    }
}