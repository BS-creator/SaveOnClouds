namespace SaveOnClouds.Web.Services.DataAccess
{
    public class OwnedAccountModel
    {
        public long Id { get; set; }
        public string AccountName { get; set; }
        public int AccountType { get; set; }
        public string CreatorUserId { get; set; }
    }
}