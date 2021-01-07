using System.Text.RegularExpressions;

namespace chatserver.Models
{
    public class UserUpdate
    {
        public UserStatus? status { get; set; }

        public string nickname { get; set; }

        public string avatar { get; set; }

        public string description { get; set; }

        public void ParseInput()
        {
            if (!string.IsNullOrWhiteSpace(this.nickname))
            {
                this.nickname = this.nickname.Trim();
            }

            if (!string.IsNullOrWhiteSpace(this.avatar))
            {
                this.avatar = Regex.Replace(this.avatar, "[^0-9]", "");
                if (!Regex.IsMatch(this.avatar, @"^[1234]$"))
                {
                    this.avatar = "1";
                }
            }

            if (!string.IsNullOrWhiteSpace(this.description))
            {
                this.description = this.description.Trim();
            }
            else if (this.description != null)
            {
                this.description = string.Empty;
            }
        }
    }
}