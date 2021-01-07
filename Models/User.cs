using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace chatserver.Models
{
    public enum UserStatus
    {
        offline,
        online,
        away
    }
    public class User
    {
        [Key]
        public string id { get; set; }

        [ReadOnly(true)]
        public DateTime created { get; set; }

        [ReadOnly(true)]
        public DateTime updated { get; set; }

        [ReadOnly(true)]
        public DateTime last_status_change { get; set; }

        [DefaultValue(UserStatus.offline)]
        public UserStatus status { get; set; }

        [Required]
        public string nickname { get; set; }

        [DefaultValue("1")]
        public string avatar { get; set; }

        [DefaultValue("")]
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
            else
            {
                this.avatar = "1";
            }

            if (!string.IsNullOrWhiteSpace(this.description))
            {
                this.description = this.description.Trim();
            }
            else
            {
                this.description = string.Empty;
            }
        }
    }
}