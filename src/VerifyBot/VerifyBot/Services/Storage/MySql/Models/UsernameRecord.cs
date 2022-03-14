using Dapper.Contrib.Extensions;

namespace VerifyBot.Services.Storage.MySql.Models
{
    [Table(MySqlStorageService.UsernameRecordTable)]
    public class UsernameRecord
    {
        [Key]
        public int id { get; set; }
        public byte[] encrypted_username { get; set; }

        public byte[] username_salt { get; set; }

        public byte[] username_hash { get; set; }
        public long creation_time { get; set; }
    }
}