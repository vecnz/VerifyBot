using Dapper.Contrib.Extensions;

namespace VerifyBot.Services.Storage.MySql.Models
{
    [Table(MySqlStorageService.PendingVerificationTable)]
    public class PendingVerification
    {
        [Key]
        public int id { get; set; }
        public ulong user_id { get; set; }
        public int username_record_id { get; set; }
        public string token { get; set; }
        public long creation_time { get; set; }
    }
}