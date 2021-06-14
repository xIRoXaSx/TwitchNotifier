using System.Linq;
using TwitchNotifier.src.config;
using TwitchNotifier.src.Helper;

namespace TwitchNotifier.src.Validation {
    class EmbedValidation {
        /// <summary>
        /// Validate an embed and return truncated if field limits have exceeded
        /// </summary>
        /// <param name="embed">The embed to validate</param>
        /// <returns><c>DiscordEmbed</c> either truncated or original</returns>
        public EmbedValidationResult ValidateEmbed(DiscordEmbed embed) {
            var validationResult = new EmbedValidationResult();
            var isValid = false;

            isValid = embed.Content.Length < 2001 ? true : false;
            isValid &= embed.Username.Length < 81 ? true : false;
            isValid &= embed.Embed.Author.Name.Length < 257 ? true : false;
            isValid &= embed.Embed.Description.Length < 2049 ? true : false;
            isValid &= embed.Embed.Title.Length < 257 ? true : false;
            isValid &= embed.Embed.Fields.Count < 26 ? true : false;            
            isValid &= embed.Embed.Fields.All(x => x.Name.Length < 257) ? true : false;            
            isValid &= embed.Embed.Fields.All(x => x.Value.Length < 1025) ? true : false;            
            isValid &= embed.Embed.Footer.Text.Length < 2049 ? true : false;
            isValid &= embed.Content.Length + embed.Username.Length + embed.Embed.Author.Name.Length + embed.Embed.Description.Length + 
                embed.Embed.Title.Length + embed.Embed.Footer.Text.Length + embed.Embed.Footer.Text.Length +
                string.Join("", embed.Embed.Fields.Select(x => x.Name)).Length + string.Join("", embed.Embed.Fields.Select(x => x.Value)).Length < 6001 ? true : false;

            embed.Embed.Footer.Text = embed.Embed.Footer.Text.Truncate(2048);
            embed.Embed.Fields = embed.Embed.Fields.Where(x => x.Value.Length < 1025).ToList();
            embed.Embed.Fields = embed.Embed.Fields.Where(x => x.Name.Length < 257).ToList();
            embed.Embed.Fields = embed.Embed.Fields.Count < 26 ? embed.Embed.Fields : embed.Embed.Fields.GetRange(0, 24);
            embed.Embed.Title = embed.Embed.Title.Truncate(256);
            embed.Embed.Description = embed.Embed.Description.Truncate(2048);
            embed.Embed.Author.Name = embed.Embed.Author.Name.Truncate(256);
            embed.Content = embed.Content.Truncate(2000);
            embed.Username = embed.Username.Truncate(80);

            validationResult.Success = isValid;
            
            if (!isValid) {
                validationResult.ErrorMessage = ("Field limits reached, truncated the embed to be able to send it! Please make sure to not exceed the following limits: https://discord.com/developers/docs/resources/channel#embed-limits");
            }

            validationResult.Embed = embed;
            return validationResult;
        }
    }
}
