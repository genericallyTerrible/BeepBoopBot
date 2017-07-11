using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeepBoopBot.Extensions
{
    public static class IEmbedExtensions
    {
        /// <summary>
        /// Creates an EmbedBuilder with the same properties as the IEmbed.
        /// Only works on rich embeds, otherwise returns null.
        /// </summary>
        /// <param name="embed"> The embed to create a builder of. </param>
        /// <returns> An embed builder with the same properties as the passed embed. Null if the embed is not a rich embed. </returns>
        public static EmbedBuilder ToEmbedBuilder(this IEmbed embed)
        {
            EmbedBuilder eb = new EmbedBuilder();
            if (embed.Type == EmbedType.Rich)
            {
                if (embed.Author != null)
                {
                    EmbedAuthorBuilder eab = new EmbedAuthorBuilder
                    {
                        IconUrl = embed.Author.Value.IconUrl,
                        Name = embed.Author.Value.Name,
                        Url = embed.Author.Value.Url
                    };

                    eb.Author = eab;
                }

                eb.Color = embed.Color;
                eb.Description = embed.Description;

                foreach (EmbedField field in embed.Fields)
                {
                    EmbedFieldBuilder efb = new EmbedFieldBuilder
                    {
                        IsInline = field.Inline,
                        Name = field.Name,
                        Value = field.Value
                    };

                    eb.AddField(efb);
                }

                if (embed.Footer != null)
                {
                    EmbedFooterBuilder efb = new EmbedFooterBuilder
                    {
                        IconUrl = embed.Footer.Value.IconUrl,
                        Text = embed.Footer.Value.Text
                    };

                    eb.Footer = efb;
                }

                if (embed.Image != null)
                {
                    eb.ImageUrl = embed.Image.Value.Url;
                }

                if (embed.Thumbnail != null)
                {
                    eb.ThumbnailUrl = embed.Thumbnail.Value.Url;
                }

                if (embed.Timestamp != null)
                {
                    eb.Timestamp = embed.Timestamp.Value;
                }

                eb.Title = embed.Title;

                if (embed.Url != null)
                {
                    eb.Url = embed.Url;
                }

                return eb;
            }
            else
            {
                return null;
            }
        }
    }
}
