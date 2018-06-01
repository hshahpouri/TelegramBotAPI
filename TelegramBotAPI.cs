using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

public class TelegramBotAPI
{
    public const string baseUrl = "https://api.telegram.org/bot";
    public const string fileUrl = "https://api.telegram.org/file/bot";
    public const string token = "PLACE_YOUR_API_TOKEN_HERE";

    public interface IMarkup { }

    public class Result
    {
        public bool ok { get; set; }
        public string description { get; set; }
        public int error_code { get; set; }
        public string json { get; set; }
    }
    public class MessageResult : Result
    {
        public Message result { get; set; }
    }
    public class FileResult : Result
    {
        public File result { get; set; }
    }
    public class ChatResult : Result
    {
        public Chat result { get; set; }
    }
    public class ChatMemberResult : Result
    {
        public ChatMember result { get; set; }
    }
    public class ChatMembersResult : Result
    {
        public ChatMember[] result { get; set; }
    }
    public class ChatMembersCountResult : Result
    {
        public int result { get; set; }
    }
    public class BooleanResult : Result
    {
        public bool result { get; set; }
    }
    public class StringResult : Result
    {
        public string result { get; set; }
    }
    public class WebhookInfoResult : Result
    {
        public WebhookInfo result { get; set; }
    }
    public class UserProfilePhotosResult : Result
    {
        public UserProfilePhotos result { get; set; }
    }

    private class tgWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            request.Timeout = 60000;
            ((HttpWebRequest)request).ReadWriteTimeout = 60000;
            return request;
        }
    }
    private class WebStream : Stream
    {
        private Stream _stream;
        public WebStream(Stream Stream)
        {
            _stream = Stream;
        }

        public override bool CanRead { get { return _stream.CanRead; } }
        public override bool CanSeek { get { return _stream.CanSeek; } }
        public override bool CanWrite { get { return _stream.CanWrite; } }
        public override long Length { get { return _stream.Length; } }
        public override long Position { get { return _stream.Position; } set { _stream.Position = value; } }

        public override void Flush()
        {
            _stream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
        public void Write(byte[] buffer)
        {
            _stream.Write(buffer, 0, buffer.Length);
        }
        public void WriteLine(string Text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Text + "\r\n");
            _stream.Write(buffer, 0, buffer.Length);
        }
        public void WriteLine()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("\r\n");
            _stream.Write(buffer, 0, buffer.Length);
        }
    }

    /// <summary>
    /// This object represents an incoming update
    /// </summary>
    public class Update
    {
        /// <summary>
        /// The update‘s unique identifier. Update identifiers start from a certain positive number and increase sequentially
        /// </summary>
        public int update_id { get; set; }
        /// <summary>
        /// Optional. New incoming message of any kind — text, photo, sticker, etc
        /// </summary>
        public Message message { get; set; }
        /// <summary>
        /// Optional. New version of a message that is known to the bot and was edited
        /// </summary>
        public Message edited_message { get; set; }
        /// <summary>
        /// Optional. New incoming channel post of any kind — text, photo, sticker, etc.
        /// </summary>
        public Message channel_post { get; set; }
        /// <summary>
        /// Optional. New version of a channel post that is known to the bot and was edited
        /// </summary>
        public Message edited_channel_post { get; set; }
        /// <summary>
        /// Optional. New incoming inline query
        /// </summary>
        public InlineQuery inline_query { get; set; }
        /// <summary>
        /// Optional. The result of an inline query that was chosen by a user and sent to their chat partner
        /// </summary>
        public ChosenInlineResult chosen_inline_result { get; set; }
        /// <summary>
        /// Optional. New incoming callback query
        /// </summary>
        public CallbackQuery callback_query { get; set; }
        /// <summary>
        /// Optional. New incoming shipping query. Only for invoices with flexible price
        /// </summary>
        public CallbackQuery shipping_query { get; set; }
        /// <summary>
        /// Optional. New incoming pre-checkout query. Contains full information about checkout
        /// </summary>
        public PreCheckoutQuery pre_checkout_query { get; set; }
    }

    /// <summary>
    /// This object represents a Telegram user or bot
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for this user or bot
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// User‘s or bot’s first name
        /// </summary>
        public string first_name { get; set; }
        /// <summary>
        /// Optional. User‘s or bot’s last name
        /// </summary>
        public string last_name { get; set; }
        /// <summary>
        /// Optional. User‘s or bot’s username
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// Optional. IETF language tag of the user's language
        /// </summary>
        public string language_code { get; set; }
    }

    /// <summary>
    /// This object represents a chat
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Unique identifier for this chat.
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// Type of chat, can be either “private”, “group”, “supergroup” or “channel”
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Optional. Title, for channels and group chats
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Optional. Username, for private chats, supergroups and channels if available
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// Optional. First name of the other party in a private chat
        /// </summary>
        public string first_name { get; set; }
        /// <summary>
        /// Optional. Last name of the other party in a private chat
        /// </summary>
        public string last_name { get; set; }
        /// <summary>
        /// Optional. True if a group has ‘All Members Are Admins’ enabled
        /// </summary>
        public bool all_members_are_administrators { get; set; }
        /// <summary>
        /// Optional. Chat photo. Returned only in getChat
        /// </summary>
        public ChatPhoto photo { get; set; }
        /// <summary>
        /// Optional. Description, for supergroups and channel chats. Returned only in getChat
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Optional. Chat invite link, for supergroups and channel chats. Returned only in getChat
        /// </summary>
        public string invite_link { get; set; }
    }

    /// <summary>
    /// This object represents a message
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Unique message identifier
        /// </summary>
        public int message_id { get; set; }
        /// <summary>
        /// Optional. Sender, can be empty for messages sent to channels
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Date the message was sent in Unix time
        /// </summary>
        public int date { get; set; }
        /// <summary>
        /// Conversation the message belongs to
        /// </summary>
        public Chat chat { get; set; }
        /// <summary>
        /// Optional. For forwarded messages, sender of the original message
        /// </summary>
        public User forward_from { get; set; }
        /// <summary>
        /// Optional. For messages forwarded from a channel, information about the original channel
        /// </summary>
        public Chat forward_from_chat { get; set; }
        /// <summary>
        /// Optional. For forwarded channel posts, identifier of the original message in the channel
        /// </summary>
        public int forward_from_message_id { get; set; }
        /// <summary>
        /// Optional. For forwarded messages, date the original message was sent in Unix time
        /// </summary>
        public int forward_date { get; set; }
        /// <summary>
        /// Optional. For replies, the original message
        /// </summary>
        public Message reply_to_message { get; set; }
        /// <summary>
        /// Optional. Date the message was last edited in Unix time
        /// </summary>
        public int edit_date { get; set; }
        /// <summary>
        /// Optional. For text messages, the actual UTF-8 text of the message, 0-4096 characters
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Optional. For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text
        /// </summary>
        public MessageEntity[] entities { get; set; }
        /// <summary>
        /// Optional. Message is an audio file, information about the file
        /// </summary>
        public Audio audio { get; set; }
        /// <summary>
        /// Optional. Message is a general file, information about the file
        /// </summary>
        public Document document { get; set; }
        /// <summary>
        /// Optional. Message is a photo, available sizes of the photo
        /// </summary>
        public PhotoSize[] photo { get; set; }
        /// <summary>
        /// Optional. Message is a sticker, information about the sticker
        /// </summary>
        public Sticker sticker { get; set; }
        /// <summary>
        /// Optional. Message is a video, information about the video
        /// </summary>
        public Video video { get; set; }
        /// <summary>
        /// Optional. Message is a video note, information about the video message
        /// </summary>
        public VideoNote video_note { get; set; }
        /// <summary>
        /// Optional. Message is a voice message, information about the file
        /// </summary>
        public Voice voice { get; set; }
        /// <summary>
        /// Optional. New members that were added to the group or supergroup and information about them (the bot itself may be one of these members)
        /// </summary>
        public User[] new_chat_members { get; set; }
        /// <summary>
        /// Optional. Caption for the document, photo or video, 0-200 characters
        /// </summary>
        public string caption { get; set; }
        /// <summary>
        /// Optional. Message is a shared contact, information about the contact
        /// </summary>
        public Contact contact { get; set; }
        /// <summary>
        /// Optional. Message is a shared location, information about the location
        /// </summary>
        public Location location { get; set; }
        /// <summary>
        /// Optional. Message is a venue, information about the venue
        /// </summary>
        public Venue venue { get; set; }
        /// <summary>
        /// Optional. A new member was added to the group, information about them (this member may be the bot itself)
        /// </summary>
        public User new_chat_member { get; set; }
        /// <summary>
        /// Optional. A member was removed from the group, information about them (this member may be the bot itself)
        /// </summary>
        public User left_chat_member { get; set; }
        /// <summary>
        /// Optional. A chat title was changed to this value
        /// </summary>
        public string new_chat_title { get; set; }
        /// <summary>
        /// Optional. A chat photo was change to this value
        /// </summary>
        public PhotoSize[] new_chat_photo { get; set; }
        /// <summary>
        /// Optional. Service message: the chat photo was deleted
        /// </summary>
        public bool delete_chat_photo { get; set; }
        /// <summary>
        /// Optional. Service message: the group has been created
        /// </summary>
        public bool group_chat_created { get; set; }
        /// <summary>
        /// Optional. Service message: the supergroup has been created
        /// </summary>
        public bool supergroup_chat_created { get; set; }
        /// <summary>
        /// Optional. Service message: the channel has been created
        /// </summary>
        public bool channel_chat_created { get; set; }
        /// <summary>
        /// Optional. The group has been migrated to a supergroup with the specified identifier
        /// </summary>
        public long migrate_to_chat_id { get; set; }
        /// <summary>
        /// Optional. The supergroup has been migrated from a group with the specified identifier
        /// </summary>
        public long migrate_from_chat_id { get; set; }
        /// <summary>
        /// Optional. Specified message was pinned
        /// </summary>
        public Message pinned_message { get; set; }
        /// <summary>
        /// Optional. Message is an invoice for a payment, information about the invoice.
        /// </summary>
        public Invoice invoice { get; set; }
        /// <summary>
        /// Optional. Message is a service message about a successful payment, information about the payment.
        /// </summary>
        public SuccessfulPayment successful_payment { get; set; }
    }

    /// <summary>
    /// This object represents one special entity in a text message. For example, hashtags, usernames, URLs, etc
    /// </summary>
    public class MessageEntity
    {
        /// <summary>
        /// Type of the entity. Can be mention (@username), hashtag, bot_command, url, email, bold (bold text), italic (italic text), code (monowidth string), pre (monowidth block), text_link (for clickable text URLs), text_mention (for users without usernames)
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Offset in UTF-16 code units to the start of the entity
        /// </summary>
        public int offset { get; set; }
        /// <summary>
        /// Length of the entity in UTF-16 code units
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// Optional. For “text_link” only, url that will be opened after user taps on the text
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// Optional. For “text_mention” only, the mentioned user
        /// </summary>
        public User user { get; set; }
    }

    /// <summary>
    /// This object represents a chat photo
    /// </summary>
    public class ChatPhoto
    {
        /// <summary>
        /// Unique file identifier of small (160x160) chat photo. This file_id can be used only for photo download
        /// </summary>
        public string small_file_id { get; set; }
        /// <summary>
        /// Unique file identifier of big (640x640) chat photo. This file_id can be used only for photo download
        /// </summary>
        public string big_file_id { get; set; }
    }
    /// <summary>
    /// This object represents one size of a photo or a file / sticker thumbnail
    /// </summary>
    public class PhotoSize
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Photo width
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Photo height
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents an audio file to be treated as music by the Telegram clients
    /// </summary>
    public class Audio
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Duration of the audio in seconds as defined by sender
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Optional. Performer of the audio as defined by sender or by audio tags
        /// </summary>
        public string performer { get; set; }
        /// <summary>
        /// Optional. Title of the audio as defined by sender or by audio tags
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Optional. MIME type of the file as defined by sender
        /// </summary>
        public string mime_type { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a general file
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Unique file identifier
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Optional. Document thumbnail as defined by sender
        /// </summary>
        public PhotoSize thumb { get; set; }
        /// <summary>
        /// Optional. Original filename as defined by sender
        /// </summary>
        public string file_name { get; set; }
        /// <summary>
        /// Optional. MIME type of the file as defined by sender
        /// </summary>
        public string mime_type { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a sticker
    /// </summary>
    public class Sticker
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Sticker width
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Sticker height
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// Optional. Sticker thumbnail in .webp or .jpg format
        /// </summary>
        public PhotoSize thumb { get; set; }
        /// <summary>
        /// Optional. Emoji associated with the sticker
        /// </summary>
        public string emoji { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a video file
    /// </summary>
    public class Video
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Video width as defined by sender
        /// </summary>
        public int width { get; set; }
        /// <summary>
        /// Video height as defined by sender
        /// </summary>
        public int height { get; set; }
        /// <summary>
        /// Duration of the video in seconds as defined by sender
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Optional. Video thumbnail
        /// </summary>
        public PhotoSize thumb { get; set; }
        /// <summary>
        /// Optional. Mime type of a file as defined by sender
        /// </summary>
        public string mime_type { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a voice note
    /// </summary>
    public class Voice
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Duration of the audio in seconds as defined by sender
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Optional. MIME type of the file as defined by sender
        /// </summary>
        public string mime_type { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a video message (available in Telegram apps as of v.4.0).
    /// </summary>
    public class VideoNote
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Video width and height as defined by sender
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// Duration of the video in seconds as defined by sender
        /// </summary>
        public int duration { get; set; }
        /// <summary>
        /// Optional. Video thumbnail
        /// </summary>
        public PhotoSize thumb { get; set; }
        /// <summary>
        /// Optional. File size
        /// </summary>
        public int file_size { get; set; }
    }

    /// <summary>
    /// This object represents a phone contact
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Contact's phone number
        /// </summary>
        public string phone_number { get; set; }
        /// <summary>
        /// Contact's first name
        /// </summary>
        public string first_name { get; set; }
        /// <summary>
        /// Optional. Contact's last name
        /// </summary>
        public string last_name { get; set; }
        /// <summary>
        /// Optional. Contact's user identifier in Telegram
        /// </summary>
        public string user_id { get; set; }
    }

    /// <summary>
    /// This object represents a point on the map
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Longitude as defined by sender
        /// </summary>
        public float longitude { get; set; }
        /// <summary>
        /// Latitude as defined by sender
        /// </summary>
        public float latitude { get; set; }
    }

    /// <summary>
    /// This object represents a venue
    /// </summary>
    public class Venue
    {
        /// <summary>
        /// Venue location
        /// </summary>
        public Location location { get; set; }
        /// <summary>
        /// Name of the venue
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Address of the venue
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// Optional. Foursquare identifier of the venue
        /// </summary>
        public string foursquare_id { get; set; }
    }

    /// <summary>
    /// This object represent a user's profile pictures
    /// </summary>
    public class UserProfilePhotos
    {
        /// <summary>
        /// Total number of profile pictures the target user has
        /// </summary>
        public int total_count { get; set; }
        /// <summary>
        /// Requested profile pictures (in up to 4 sizes each)
        /// </summary>
        public PhotoSize[][] photos { get; set; }
    }

    /// <summary>
    /// This object represents a file ready to be downloaded. Maximum file size to download is 20 MB
    /// </summary>
    public class File
    {
        /// <summary>
        /// Unique identifier for this file
        /// </summary>
        public string file_id { get; set; }
        /// <summary>
        /// Optional. File size, if known
        /// </summary>
        public int file_size { get; set; }
        /// <summary>
        /// Optional. File path. Use https://api.telegram.org/file/bot<token>/<file_path> to get the file
        /// </summary>
        public string file_path { get; set; }
    }

    /// <summary>
    /// This object represents a custom keyboard with reply options
    /// </summary>
    public class ReplyKeyboardMarkup : IMarkup
    {
        /// <summary>
        /// Array of button rows, each represented by an Array of KeyboardButton objects
        /// </summary>
        public KeyboardButton[][] keyboard { get; set; }
        /// <summary>
        /// Optional. Requests clients to resize the keyboard vertically for optimal fit
        /// </summary>
        public bool resize_keyboard { get; set; }
        /// <summary>
        /// Optional. Requests clients to hide the keyboard as soon as it's been used
        /// </summary>
        public bool one_time_keyboard { get; set; }
        /// <summary>
        /// Optional. Use this parameter if you want to show the keyboard to specific users only
        /// </summary>
        public bool selective { get; set; }
    }

    /// <summary>
    /// This object represents one button of the reply keyboard
    /// </summary>
    public class KeyboardButton
    {
        /// <summary>
        /// Text of the button. If none of the optional fields are used, it will be sent to the bot as a message when the button is pressed
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Optional. If True, the user's phone number will be sent as a contact when the button is pressed. Available in private chats only
        /// </summary>
        public bool request_contact { get; set; }
        /// <summary>
        /// Optional. If True, the user's current location will be sent when the button is pressed. Available in private chats only
        /// </summary>
        public bool request_location { get; set; }
    }

    /// <summary>
    /// Upon receiving a message with this object, Telegram clients will hide the current custom keyboard and display the default letter-keyboard
    /// </summary>
    public class ReplyKeyboardRemove : IMarkup
    {
        /// <summary>
        /// Requests clients to hide the custom keyboard
        /// </summary>
        public bool remove_keyboard { get; set; }
        /// <summary>
        /// Optional. Use this parameter if you want to hide keyboard for specific users only
        /// </summary>
        public bool selective { get; set; }
    }

    /// <summary>
    /// This object represents an inline keyboard that appears right next to the message it belongs to
    /// </summary>
    public class InlineKeyboardMarkup : IMarkup
    {
        /// <summary>
        /// Array of button rows, each represented by an Array of InlineKeyboardButton objects
        /// </summary>
        public InlineKeyboardButton[][] inline_keyboard { get; set; }
    }

    /// <summary>
    /// This object represents one button of an inline keyboard. Must use exactly one of the optional fields
    /// </summary>
    public class InlineKeyboardButton
    {
        /// <summary>
        /// Label text on the button
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Optional. HTTP url to be opened when button is pressed
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// Optional. Data to be sent in a callback query to the bot when button is pressed, 1-64 bytes
        /// </summary>
        public string callback_data { get; set; }
        /// <summary>
        /// Optional. If set, pressing the button will prompt the user to select one of their chats, open that chat and insert the bot‘s username and the specified inline query in the input field
        /// </summary>
        public string switch_inline_query { get; set; }
        /// <summary>
        /// Optional. If set, pressing the button will insert the bot‘s username and the specified inline query in the current chat's input field. Can be empty, in which case only the bot’s username will be inserted
        /// </summary>
        public string switch_inline_query_current_chat { get; set; }
        /// <summary>
        /// Optional. Specify True, to send a Pay button. NOTE: This type of button must always be the first button in the first row.
        /// </summary>
        public bool pay { get; set; }
    }

    /// <summary>
    /// This object represents an incoming callback query from a callback button in an inline keyboard
    /// </summary>
    public class CallbackQuery
    {
        /// <summary>
        /// Unique identifier for this query
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Sender
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Optional. Message with the callback button that originated the query. Note that message content and message date will not be available if the message is too old
        /// </summary>
        public Message message { get; set; }
        /// <summary>
        /// Optional. Identifier of the message sent via the bot in inline mode, that originated the query
        /// </summary>
        public string inline_message_id { get; set; }
        /// <summary>
        /// Global identifier, uniquely corresponding to the chat to which the message with the callback button was sent. Useful for high scores in games
        /// </summary>
        public string chat_instance { get; set; }
        /// <summary>
        /// Data associated with the callback button
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// Optional. Short name of a Game to be returned, serves as the unique identifier for the game
        /// </summary>
        public string game_short_name { get; set; }
    }

    /// <summary>
    /// This object contains basic information about an invoice
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Product name
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Product description
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Unique bot deep-linking parameter that can be used to generate this invoice
        /// </summary>
        public string start_parameter { get; set; }
        /// <summary>
        /// Three-letter ISO 4217 currency code
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// Total price in the smallest units of the currency (integer, not float/double). For example, for a price of US$ 1.45 pass amount = 145. See the exp parameter in currencies.json, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).
        /// </summary>
        public int total_amount { get; set; }
    }

    /// <summary>
    /// This object contains basic information about a successful payment
    /// </summary>
    public class SuccessfulPayment
    {
        /// <summary>
        /// Three-letter ISO 4217 currency code
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// Total price in the smallest units of the currency (integer, not float/double). For example, for a price of US$ 1.45 pass amount = 145. See the exp parameter in currencies.json, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).
        /// </summary>
        public int total_amount { get; set; }
        /// <summary>
        /// Bot specified invoice payload
        /// </summary>
        public string invoice_payload { get; set; }
        /// <summary>
        /// Optional. Identifier of the shipping option chosen by the user
        /// </summary>
        public string shipping_option_id { get; set; }
        /// <summary>
        /// Optional. Order info provided by the user
        /// </summary>
        public OrderInfo order_info { get; set; }
        /// <summary>
        /// Telegram payment identifier
        /// </summary>
        public string telegram_payment_charge_id { get; set; }
        /// <summary>
        /// Provider payment identifier
        /// </summary>
        public string provider_payment_charge_id { get; set; }
    }

    /// <summary>
    /// This object contains information about an incoming shipping query.
    /// </summary>
    public class ShippingQuery
    {
        /// <summary>
        /// Unique query identifier
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// User who sent the query
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Bot specified invoice payload
        /// </summary>
        public string invoice_payload { get; set; }
        /// <summary>
        /// User specified shipping address
        /// </summary>
        public ShippingAddress shipping_address { get; set; }
    }

    /// <summary>
    /// This object represents a shipping address.
    /// </summary>
    public class ShippingAddress
    {
        /// <summary>
        /// ISO 3166-1 alpha-2 country code
        /// </summary>
        public string country_code { get; set; }
        /// <summary>
        /// State, if applicable
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// City
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// First line for the address
        /// </summary>
        public string street_line1 { get; set; }
        /// <summary>
        /// Second line for the address
        /// </summary>
        public string street_line2 { get; set; }
        /// <summary>
        /// Address post code
        /// </summary>
        public string post_code { get; set; }
    }

    /// <summary>
    /// This object contains information about an incoming pre-checkout query.
    /// </summary>
    public class PreCheckoutQuery
    {
        /// <summary>
        /// Unique query identifier
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// User who sent the query
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Three-letter ISO 4217 currency code
        /// </summary>
        public string currency { get; set; }
        /// <summary>
        /// Total price in the smallest units of the currency (integer, not float/double). For example, for a price of US$ 1.45 pass amount = 145. See the exp parameter in currencies.json, it shows the number of digits past the decimal point for each currency (2 for the majority of currencies).
        /// </summary>
        public int total_amount { get; set; }
        /// <summary>
        /// Bot specified invoice payload
        /// </summary>
        public string invoice_payload { get; set; }
        /// <summary>
        /// Optional. Identifier of the shipping option chosen by the user
        /// </summary>
        public string shipping_option_id { get; set; }
        /// <summary>
        /// Optional. Order info provided by the user
        /// </summary>
        public OrderInfo order_info { get; set; }
    }

    /// <summary>
    /// This object represents information about an order.
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// Optional. User name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Optional. User's phone number
        /// </summary>
        public string phone_number { get; set; }
        /// <summary>
        /// Optional. User email
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Optional. User shipping address
        /// </summary>
        public ShippingAddress shipping_address { get; set; }
    }

    /// <summary>
    /// Upon receiving a message with this object, Telegram clients will display a reply interface to the user
    /// </summary>
    public class ForceReply : IMarkup
    {
        /// <summary>
        /// Shows reply interface to the user, as if they manually selected the bot‘s message and tapped ’Reply'
        /// </summary>
        public bool force_reply { get; set; }
        /// <summary>
        /// Optional. Use this parameter if you want to force reply from specific users only
        /// </summary>
        public bool selective { get; set; }
    }

    /// <summary>
    /// This object contains information about one member of the chat
    /// </summary>
    public class ChatMember
    {
        /// <summary>
        /// Information about the user
        /// </summary>
        public User user { get; set; }
        /// <summary>
        /// The member's status in the chat. Can be “creator”, “administrator”, “member”, “restricted”, “left” or “kicked”
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// Optional. Restictred and kicked only. Date when restrictions will be lifted for this user, unix time
        /// </summary>
        public int until_date { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the bot is allowed to edit administrator privileges of that user
        /// </summary>
        public bool can_be_edited { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can change the chat title, photo and other settings
        /// </summary>
        public bool can_change_info { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can post in the channel, channels only
        /// </summary>
        public bool can_post_messages { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can edit messages of other users, channels only
        /// </summary>
        public bool can_edit_messages { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can delete messages of other users
        /// </summary>
        public bool can_delete_messages { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can invite new users to the chat
        /// </summary>
        public bool can_invite_users { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can restrict, ban or unban chat members
        /// </summary>
        public bool can_restrict_members { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can pin messages, supergroups only
        /// </summary>
        public bool can_pin_messages { get; set; }
        /// <summary>
        /// Optional. Administrators only. True, if the administrator can add new administrators with a subset of his own privileges or demote administrators that he has promoted, directly or indirectly (promoted by administrators that were appointed by the user)
        /// </summary>
        public bool can_promote_members { get; set; }
        /// <summary>
        /// Optional. Restricted only. True, if the user can send text messages, contacts, locations and venues
        /// </summary>
        public bool can_send_messages { get; set; }
        /// <summary>
        /// Optional. Restricted only. True, if the user can send audios, documents, photos, videos, video notes and voice notes, implies can_send_messages
        /// </summary>
        public bool can_send_media_messages { get; set; }
        /// <summary>
        /// Optional. Restricted only. True, if the user can send animations, games, stickers and use inline bots, implies can_send_media_messages
        /// </summary>
        public bool can_send_other_messages { get; set; }
        /// <summary>
        /// Optional. Restricted only. True, if user may add web page previews to his messages, implies can_send_media_messages
        /// </summary>
        public bool can_add_web_page_previews { get; set; }
    }

    /// <summary>
    /// This object represents an incoming inline query
    /// </summary>
    public class InlineQuery
    {
        /// <summary>
        /// Unique identifier for this query
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Sender
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Optional. Sender location, only for bots that request user location
        /// </summary>
        public Location location { get; set; }
        /// <summary>
        /// Text of the query (up to 512 characters)
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// Offset of the results to be returned, can be controlled by the bot
        /// </summary>
        public string offset { get; set; }
    }

    /// <summary>
    /// Represents a result of an inline query that was chosen by the user and sent to their chat partner
    /// </summary>
    public class ChosenInlineResult
    {
        /// <summary>
        /// The unique identifier for the result that was chosen
        /// </summary>
        public string result_id { get; set; }
        /// <summary>
        /// The user that chose the result
        /// </summary>
        public User from { get; set; }
        /// <summary>
        /// Optional. Sender location, only for bots that require user location
        /// </summary>
        public Location location { get; set; }
        /// <summary>
        /// Optional. Identifier of the sent inline message. Available only if there is an inline keyboard attached to the message. Will be also received in callback queries and can be used to edit the message
        /// </summary>
        public string inline_message_id { get; set; }
        /// <summary>
        /// The query that was used to obtain the result
        /// </summary>
        public string query { get; set; }
    }

    /// <summary>
    /// Contains information about the current status of a webhook
    /// </summary>
    public class WebhookInfo
    {
        /// <summary>
        /// Webhook URL, may be empty if webhook is not set up
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// True, if a custom certificate was provided for webhook certificate checks
        /// </summary>
        public bool has_custom_certificate { get; set; }
        /// <summary>
        /// Number of updates awaiting delivery
        /// </summary>
        public int pending_update_count { get; set; }
        /// <summary>
        /// Optional. Unix time for the most recent error that happened when trying to deliver an update via webhook
        /// </summary>
        public int last_error_date { get; set; }
        /// <summary>
        /// Optional. Error message in human-readable format for the most recent error that happened when trying to deliver an update via webhook
        /// </summary>
        public string last_error_message { get; set; }
        /// <summary>
        /// Optional. Maximum allowed number of simultaneous HTTPS connections to the webhook for update delivery
        /// </summary>
        public int max_connections { get; set; }
        /// <summary>
        /// Optional. A list of update types the bot is subscribed to. Defaults to all update types
        /// </summary>
        public string[] allowed_updates { get; set; }
    }

    private static object sendToTelegram(string command, NameValueCollection postData, Type ResultType)
    {
        using (tgWebClient c = new tgWebClient())
        {
            c.Encoding = Encoding.UTF8;
            object sentResult = null;
            try
            {
                string result = Encoding.UTF8.GetString(c.UploadValues(baseUrl + token + "/" + command, postData));
                sentResult = new JavaScriptSerializer().Deserialize(result, ResultType);
                ((Result)sentResult).json = result;
            }
            catch (WebException ex)
            {
                try
                {
                    if (ex.Response == null || ex.Response.GetResponseStream() == null)
                    {
                        sentResult = new Result()
                        {
                            ok = false,
                            error_code = 0,
                            description = "|Response=null| " + ex.Message + (ex.InnerException == null ? "" : " -> " + ex.InnerException.Message + (ex.InnerException.InnerException == null ? "" : " -> " + ex.InnerException.InnerException.Message))
                        };
                    }
                    else
                    {
                        StreamReader responseReader = new StreamReader(ex.Response.GetResponseStream());
                        sentResult = new JavaScriptSerializer().Deserialize<Result>(responseReader.ReadToEnd());
                    }
                }
                catch (Exception err)
                {
                    Log("|Exception| " + err.Message + (err.InnerException == null ? "" : " -> " + err.InnerException.Message + (err.InnerException.InnerException == null ? "" : " -> " + err.InnerException.InnerException.Message)));
                }
            }
            catch (Exception ex)
            {
                sentResult = new Result()
                {
                    ok = false,
                    error_code = 0,
                    description = ex.Message + (ex.InnerException == null ? "" : " -> " + ex.InnerException.Message + (ex.InnerException.InnerException == null ? "" : " -> " + ex.InnerException.InnerException.Message))
                };
            }

            Result checkError = (Result)sentResult;
            if (!checkError.ok)
                Log("~/Error-" + command + ".txt", "(" + checkError.error_code + ") " + checkError.description + " -> " + postData["chat_id"]);

            if (sentResult.GetType().Name == "Result")
            {
                object t = Activator.CreateInstance(ResultType);
                ((Result)t).ok = ((Result)sentResult).ok;
                ((Result)t).description = ((Result)sentResult).description;
                ((Result)t).error_code = ((Result)sentResult).error_code;
                ((Result)t).json = ((Result)sentResult).json;
                return t;
            }

            return sentResult;
        }
    }

    private static MessageResult uploadToTelegram(string command, string fileUrl, string uploadType, string contentType, NameValueCollection postData)
    {
        MessageResult sentResult = null;
        try
        {
            string boundary = "-----TelegramBotAPI_" + DateTime.Now.Ticks.ToString();
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(baseUrl + token + "/" + command);
            r.ServicePoint.Expect100Continue = false;
            r.Method = "POST";
            r.ContentType = "multipart/form-data; boundary=" + boundary.Substring(2);
            r.Timeout = 60000;
            using (WebStream s = new WebStream(r.GetRequestStream()))
            {
                for (int i = 0; i < postData.Count; i++)
                {
                    s.WriteLine(boundary);
                    s.WriteLine("Content-Disposition: form-data; name=\"" + postData.Keys[i] + "\"");
                    s.WriteLine();
                    s.WriteLine(postData[i]);
                }
                s.WriteLine(boundary);
                s.WriteLine("Content-Disposition: form-data; name=\"" + uploadType + "\"; filename=\"" + Path.GetFileName(fileUrl) + "\"");
                s.WriteLine("Content-Type: " + contentType);
                s.WriteLine();
                s.Write(System.IO.File.ReadAllBytes(fileUrl));
                s.WriteLine("\r\n" + boundary + "--");
            }
            using (HttpWebResponse res = (HttpWebResponse)r.GetResponse())
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string result = sr.ReadToEnd();
                    sentResult = new JavaScriptSerializer().Deserialize<MessageResult>(result);
                    sentResult.json = result;
                }
            }
        }
        catch (Exception ex)
        {
            Log("Exception");

            sentResult = new MessageResult()
            {
                ok = false,
                error_code = 0,
                description = ex.Message + (ex.InnerException == null ? "" : " -> " + ex.InnerException.Message + (ex.InnerException.InnerException == null ? "" : " -> " + ex.InnerException.InnerException.Message))
            };
        }

        if (!sentResult.ok)
            Log("~/Error-" + command + ".txt", "(" + sentResult.error_code + ") " + sentResult.description + " -> " + postData["chat_id"]);

        return sentResult;
    }

    private static BooleanResult uploadToTelegramBoolean(string command, string fileUrl, string uploadType, string contentType, NameValueCollection postData)
    {
        BooleanResult sentResult = null;
        try
        {
            string boundary = "-----TelegramBotV7_" + DateTime.Now.Ticks.ToString();
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(baseUrl + token + "/" + command);
            r.ServicePoint.Expect100Continue = false;
            r.Method = "POST";
            r.ContentType = "multipart/form-data; boundary=" + boundary.Substring(2);
            r.Timeout = 60000;
            using (WebStream s = new WebStream(r.GetRequestStream()))
            {
                for (int i = 0; i < postData.Count; i++)
                {
                    s.WriteLine(boundary);
                    s.WriteLine("Content-Disposition: form-data; name=\"" + postData.Keys[i] + "\"");
                    s.WriteLine();
                    s.WriteLine(postData[i]);
                }
                s.WriteLine(boundary);
                s.WriteLine("Content-Disposition: form-data; name=\"" + uploadType + "\"; filename=\"" + Path.GetFileName(fileUrl) + "\"");
                s.WriteLine("Content-Type: " + contentType);
                s.WriteLine();
                s.Write(System.IO.File.ReadAllBytes(fileUrl));
                s.WriteLine("\r\n" + boundary + "--");
            }
            using (HttpWebResponse res = (HttpWebResponse)r.GetResponse())
            {
                using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                {
                    string result = sr.ReadToEnd();
                    sentResult = new JavaScriptSerializer().Deserialize<BooleanResult>(result);
                    sentResult.json = result;
                }
            }
        }
        catch (Exception ex)
        {
            Log("Exception");

            sentResult = new BooleanResult()
            {
                result = false,
                ok = false,
                error_code = 0,
                description = ex.Message + (ex.InnerException == null ? "" : " -> " + ex.InnerException.Message + (ex.InnerException.InnerException == null ? "" : " -> " + ex.InnerException.InnerException.Message))
            };
        }

        if (!sentResult.ok)
            Log("~/Error-" + command + ".txt", "(" + sentResult.error_code + ") " + sentResult.description + " -> " + postData["chat_id"]);

        return sentResult;
    }

    private static void Log(string Message)
    {
        Log("~/ErrorLog.txt", DateTime.Now.ToLocalTime().ToString("yyyy/MM/dd - HH:mm:ss ► ") + Message);
    }

    private static void Log(string Filename, string Message)
    {
        System.IO.File.AppendAllText(HttpContext.Current.Server.MapPath(Filename), Message + Environment.NewLine, Encoding.UTF8);
    }

    /// <summary>
    /// A simple method for testing your bot's auth token
    /// </summary>
    public static string getMe()
    {
        using (tgWebClient c = new tgWebClient())
        {
            c.Encoding = Encoding.UTF8;
            c.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            return Regex.Unescape(c.UploadString(baseUrl + token + "/getme", ""));
        }
    }

    /// <summary>
    /// Use this method to specify a url and receive incoming updates via an outgoing webhook
    /// </summary>
    /// <param name="url">HTTPS url to send updates to. Use an empty string to remove webhook integration</param>
    /// <returns>returns the result of setting webhook url operation</returns>
    public static Result setWebhook(string url)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("url", url);
        return sendToTelegram("setWebhook", postData, typeof(Result)) as Result;
    }

    /// <summary>
    /// Use this method to get current webhook status
    /// </summary>
    /// <returns>On success, returns a WebhookInfo object</returns>
    public static WebhookInfoResult getWebhookInfo()
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        return sendToTelegram("getWebhookInfo", postData, typeof(WebhookInfoResult)) as WebhookInfoResult;
    }

    /// <summary>
    /// Check user has joined in channel
    /// </summary>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <param name="channelUsername">username of the target channel or supergroup</param>
    /// <returns>True if user has joined in channel, otherwise False</returns>
    public static bool IsUserInChannel(long user_id, string channelUsername)
    {
        string[] In = new string[] { "creator", "administrator", "member" };
        ChatMemberResult result = getChatMember(channelUsername, user_id);
        return result.ok && In.Contains(result.result.status);
    }

    public enum ParseMode { Markdown, HTML, None }
    /// <summary>
    /// Use this method to send text messages
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="text">Text of the message to be sent</param>
    /// <param name="parse_mode">Send Markdown or HTML, if you want Telegram apps to show bold, italic, fixed-width text or inline URLs in your bot's message</param>
    /// <param name="disable_web_page_preview">Disables link previews for links in this message</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendMessage(string chat_id, string text, ParseMode parse_mode, bool? disable_web_page_preview, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("text", text);
        if (disable_web_page_preview.HasValue)
            postData.Add("disable_web_page_preview", disable_web_page_preview.ToString());
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (parse_mode != ParseMode.None)
            postData.Add("parse_mode", parse_mode.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("sendMessage", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to forward messages of any kind
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel</param>
    /// <param name="from_chat_id">Unique identifier for the chat where the original message was sent</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="message_id">Unique message identifier</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult forwardMessage(string chat_id, string from_chat_id, bool? disable_notification, int message_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("from_chat_id", from_chat_id);
        postData.Add("message_id", message_id.ToString());
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());

        return sendToTelegram("forwardMessage", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to send photos
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="photo">Photo to send. You can either pass a file_id as String to resend a photo that is already on the Telegram servers, or upload a new photo</param>
    /// <param name="is_file_id">If the photo parameter is a file_id, send True otherwise False</param>
    /// <param name="caption">Photo caption (may also be used when resending photos by file_id), 0-200 characters</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendPhoto(string chat_id, string photo, bool is_file_id, string caption, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (caption != null)
            postData.Add("caption", caption);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("photo", photo);
            return sendToTelegram("sendPhoto", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendPhoto", photo, "photo", "image/jpeg", postData);
    }

    /// <summary>
    /// Use this method to send audio files, if you want Telegram clients to display them in the music player up to 50 MB in size
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="audio">Audio file to send. You can either pass a file_id as String to resend an audio that is already on the Telegram servers, or upload a new audio</param>
    /// <param name="caption">Audio caption, 0-200 characters</param>
    /// <param name="is_file_id">Define that audio parameter is a file_id or filepath</param>
    /// <param name="duration">Duration of the audio in seconds</param>
    /// <param name="performer">Performer</param>
    /// <param name="title">Track name</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendAudio(string chat_id, string audio, bool is_file_id, string caption, int? duration, string performer, string title, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (duration.HasValue)
            postData.Add("duration", duration.ToString());
        if (caption != null)
            postData.Add("caption", caption);
        if (performer != null)
            postData.Add("performer", performer);
        if (title != null)
            postData.Add("title", title);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("audio", audio);
            return sendToTelegram("sendAudio", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendAudio", audio, "audio", "audio/mpeg", postData);
    }

    /// <summary>
    /// Use this method to send general files up to 50 MB in size
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="document">File to send. You can either pass a file_id as String to resend a file that is already on the Telegram servers, or upload a new file</param>
    /// <param name="caption">Document caption (may also be used when resending documents by file_id), 0-200 characters</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendDocument(string chat_id, string document, bool is_file_id, string caption, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (caption != null)
            postData.Add("caption", caption);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("document", document);
            return sendToTelegram("sendDocument", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendDocument", document, "document", "application/octet-stream", postData);
    }

    /// <summary>
    /// Use this method to send video files, Telegram clients support mp4 videos up to 50 MB in size (other formats may be sent as Document)
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="video">Video to send. You can either pass a file_id as String to resend a video that is already on the Telegram servers, or upload a new video</param>
    /// <param name="duration">Duration of sent video in seconds</param>
    /// <param name="width">Video width</param>
    /// <param name="height">Video height</param>
    /// <param name="caption">Video caption (may also be used when resending videos by file_id), 0-200 characters</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendVideo(string chat_id, string video, bool is_file_id, string caption, int? duration, int? width, int? height, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (duration.HasValue)
            postData.Add("duration", duration.ToString());
        if (width.HasValue)
            postData.Add("width", width.ToString());
        if (height.HasValue)
            postData.Add("height", height.ToString());
        if (caption != null)
            postData.Add("caption", caption);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("video", video);
            return sendToTelegram("sendVideo", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendVideo", video, "video", "video/mp4", postData);
    }

    /// <summary>
    /// As of v.4.0, Telegram clients support rounded square mp4 videos of up to 1 minute long. Use this method to send video messages. On success, the sent Message is returned.
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="video_note">Video note to send. Pass a file_id as String to send a video note that exists on the Telegram servers (recommended) or upload a new video using multipart/form-data. More info on Sending Files ».</param>
    /// <param name="duration">Duration of sent video in seconds</param>
    /// <param name="length">Video width and height</param>
    /// <param name="caption">Video caption (may also be used when resending videos by file_id), 0-200 characters</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns></returns>
    public static MessageResult sendVideoNote(string chat_id, string video_note, bool is_file_id, int? duration, int? length, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (duration.HasValue)
            postData.Add("duration", duration.ToString());
        if (length.HasValue)
            postData.Add("length", length.ToString());
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("video_note", video_note);
            return sendToTelegram("sendVideoNote", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendVideoNote", video_note, "video_note", "video/mp4", postData);
    }

    /// <summary>
    /// Use this method to send audio files, if you want Telegram clients to display the file as a playable voice message. For this to work, your audio must be in an .ogg file encoded with OPUS up to 50 MB in size (other formats may be sent as Audio or Document)
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="voice">Audio file to send. You can either pass a file_id as String to resend an audio that is already on the Telegram servers, or upload a new audio</param>
    /// <param name="caption">Voice message caption, 0-200 characters</param>
    /// <param name="duration">Duration of sent audio in seconds</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendVoice(string chat_id, string voice, bool is_file_id, string caption, int? duration, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (duration.HasValue)
            postData.Add("duration", duration.ToString());
        if (caption != null)
            postData.Add("caption", caption);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("voice", voice);
            return sendToTelegram("sendVoice", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendVoice", voice, "voice", "audio/ogg", postData);
    }

    /// <summary>
    /// Use this method to send .webp stickers
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="sticker">Sticker to send. You can either pass a file_id as String to resend a sticker that is already on the Telegram servers, or upload a new sticker</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendSticker(string chat_id, string sticker, bool is_file_id, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        if (is_file_id)
        {
            postData.Add("sticker", sticker);
            return sendToTelegram("sendSticker", postData, typeof(MessageResult)) as MessageResult;
        }
        else
            return uploadToTelegram("sendSticker", sticker, "sticker", "image/webp", postData);
    }

    /// <summary>
    /// Use this method to send point on the map
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="latitude">Latitude of location</param>
    /// <param name="longitude">Longitude of location</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendLocation(string chat_id, float latitude, float longitude, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("latitude", latitude.ToString());
        postData.Add("longitude", longitude.ToString());
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("sendLocation", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to send information about a venue
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="latitude">Latitude of location</param>
    /// <param name="longitude">Longitude of location</param>
    /// <param name="title">Name of the venue</param>
    /// <param name="address">Address of the venue</param>
    /// <param name="foursquare_id">Foursquare identifier of the venue</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendVenue(string chat_id, float latitude, float longitude, string title, string address, string foursquare_id, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("latitude", latitude.ToString());
        postData.Add("longitude", longitude.ToString());
        postData.Add("title", title);
        postData.Add("address", address);
        if (!string.IsNullOrEmpty(foursquare_id))
            postData.Add("foursquare_id", foursquare_id);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("sendVenue", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to send phone contacts
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="phone_number">Contact's phone number</param>
    /// <param name="first_name">Contact's first name</param>
    /// <param name="last_name">Contact's last name</param>
    /// <param name="disable_notification">Sends the message silently. iOS users will not receive a notification, Android users will receive a notification with no sound</param>
    /// <param name="reply_to_message_id">If the message is a reply, ID of the original message</param>
    /// <param name="reply_markup">Additional interface options. A JSON-serialized object for an inline keyboard, custom reply keyboard, instructions to hide reply keyboard or to force a reply from the user</param>
    /// <returns>On success, the sent Message is returned</returns>
    public static MessageResult sendContact(string chat_id, string phone_number, string first_name, string last_name, bool? disable_notification, int? reply_to_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("phone_number", phone_number);
        postData.Add("first_name", first_name);
        if (last_name != null)
            postData.Add("last_name", last_name);
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());
        if (reply_to_message_id.HasValue)
            postData.Add("reply_to_message_id", reply_to_message_id.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("sendContact", postData, typeof(MessageResult)) as MessageResult;
    }

    public enum ChatAction { typing, upload_photo, record_video, upload_video, record_audio, upload_audio, record_video_note, upload_video_note, upload_document, find_location }
    /// <summary>
    /// Use this method when you need to tell the user that something is happening on the bot's side. The status is set for 5 seconds or less
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="phone_number">Type of action to broadcast</param>
    public static void sendChatAction(string chat_id, ChatAction action)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("action", action.ToString());

        sendToTelegram("sendChatAction", postData, typeof(Result));
    }

    /// <summary>
    /// Use this method to get a list of profile pictures for a user
    /// </summary>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <param name="offset">Sequential number of the first photo to be returned. By default, all photos are returned</param>
    /// <param name="limit">Limits the number of photos to be retrieved. Values between 1—100 are accepted. Defaults to 100</param>
    /// <returns>Returns a UserProfilePhotos object</returns>
    public static UserProfilePhotosResult getUserProfilePhotos(long user_id, int? offset, int? limit)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("user_id", user_id.ToString());
        if (offset.HasValue)
            postData.Add("offset", offset.ToString());
        if (limit.HasValue)
            postData.Add("limit", limit.ToString());

        return sendToTelegram("getUserProfilePhotos", postData, typeof(UserProfilePhotosResult)) as UserProfilePhotosResult;
    }

    /// <summary>
    /// Use this method to get basic info about a file and prepare it for downloading
    /// </summary>
    /// <param name="file_id">File identifier to get info about</param>
    /// <returns>On success, a File object is returned</returns>
    public static FileResult getFile(string file_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("file_id", file_id);

        return sendToTelegram("getFile", postData, typeof(FileResult)) as FileResult;
    }

    /// <summary>
    /// Use this method to kick a user from a group or a supergroup
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target group or username of the target supergroup</param>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <param name="until_date">Date when the user will be unbanned, unix time. If user is banned for more than 366 days or less than 30 seconds from the current time they are considered to be banned forever</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult kickChatMember(string chat_id, long user_id, int? until_date)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("user_id", user_id.ToString());
        if (until_date.HasValue)
            postData.Add("until_date", until_date.ToString());

        return sendToTelegram("kickChatMember", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to unban a previously kicked user in a supergroup. The user will not return to the group automatically, but will be able to join via link, etc
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target group or username of the target supergroup</param>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult unbanChatMember(string chat_id, long user_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("user_id", user_id.ToString());

        return sendToTelegram("unbanChatMember", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to restrict a user in a supergroup. The bot must be an administrator in the supergroup for this to work and must have the appropriate admin rights. Pass True for all boolean parameters to lift restrictions from a user
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)</param>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <param name="until_date">Date when the user will be unbanned, unix time. If user is banned for more than 366 days or less than 30 seconds from the current time they are considered to be banned forever</param>
    /// <param name="can_send_messages">Pass True, if the user can send text messages, contacts, locations and venues</param>
    /// <param name="can_send_media_messages">Pass True, if the user can send audios, documents, photos, videos, video notes and voice notes, implies can_send_messages</param>
    /// <param name="can_send_other_messages">Pass True, if the user can send animations, games, stickers and use inline bots, implies can_send_media_messages</param>
    /// <param name="can_add_web_page_previews">Pass True, if the user may add web page previews to their messages, implies can_send_media_messages</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult restrictChatMember(string chat_id, long user_id, int? until_date, bool? can_send_messages, bool? can_send_media_messages, bool? can_send_other_messages, bool? can_add_web_page_previews)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("user_id", user_id.ToString());
        if (until_date.HasValue)
            postData.Add("until_date", until_date.ToString());
        if (can_send_messages.HasValue)
            postData.Add("can_send_messages", can_send_messages.ToString());
        if (can_send_media_messages.HasValue)
            postData.Add("can_send_media_messages", can_send_media_messages.ToString());
        if (can_send_other_messages.HasValue)
            postData.Add("can_send_other_messages", can_send_other_messages.ToString());
        if (can_add_web_page_previews.HasValue)
            postData.Add("can_add_web_page_previews", can_add_web_page_previews.ToString());

        return sendToTelegram("restrictChatMember", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to promote or demote a user in a supergroup or a channel. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights. Pass False for all boolean parameters to demote a user
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup (in the format @supergroupusername)</param>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <param name="can_change_info">Pass True, if the administrator can change chat title, photo and other settings</param>
    /// <param name="can_post_messages">Pass True, if the administrator can create channel posts, channels only</param>
    /// <param name="can_edit_messages">Pass True, if the administrator can edit messages of other users, channels only</param>
    /// <param name="can_delete_messages">Pass True, if the administrator can delete messages of other users</param>
    /// <param name="can_invite_users">Pass True, if the administrator can invite new users to the chat</param>
    /// <param name="can_restrict_members">Pass True, if the administrator can restrict, ban or unban chat members</param>
    /// <param name="can_pin_messages">Pass True, if the administrator can pin messages, supergroups only</param>
    /// <param name="can_promote_members">Pass True, if the administrator can add new administrators with a subset of his own privileges or demote administrators that he has promoted, directly or indirectly (promoted by administrators that were appointed by him)</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult promoteChatMember(string chat_id, long user_id, bool? can_change_info, bool? can_post_messages, bool? can_edit_messages, bool? can_delete_messages, bool? can_invite_users, bool? can_restrict_members, bool? can_pin_messages, bool? can_promote_members)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("user_id", user_id.ToString());
        if (can_change_info.HasValue)
            postData.Add("can_change_info", can_change_info.ToString());
        if (can_post_messages.HasValue)
            postData.Add("can_post_messages", can_post_messages.ToString());
        if (can_edit_messages.HasValue)
            postData.Add("can_edit_messages", can_edit_messages.ToString());
        if (can_delete_messages.HasValue)
            postData.Add("can_delete_messages", can_delete_messages.ToString());
        if (can_invite_users.HasValue)
            postData.Add("can_invite_users", can_invite_users.ToString());
        if (can_restrict_members.HasValue)
            postData.Add("can_restrict_members", can_restrict_members.ToString());
        if (can_pin_messages.HasValue)
            postData.Add("can_pin_messages", can_pin_messages.ToString());
        if (can_promote_members.HasValue)
            postData.Add("can_promote_members", can_promote_members.ToString());

        return sendToTelegram("promoteChatMember", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to export an invite link to a supergroup or a channel. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <returns>Returns exported invite link as String on success</returns>
    public static StringResult exportChatInviteLink(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("exportChatInviteLink", postData, typeof(StringResult)) as StringResult;
    }

    /// <summary>
    /// Use this method to set a new profile photo for the chat. Photos can't be changed for private chats. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="photo">New chat photo, uploaded using multipart/form-data</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult setChatPhoto(string chat_id, string photo)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return uploadToTelegramBoolean("setChatPhoto", photo, "photo", "image/jpeg", postData);
    }

    /// <summary>
    /// Use this method to delete a chat photo. Photos can't be changed for private chats. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult deleteChatPhoto(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("deleteChatPhoto", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to change the title of a chat. Titles can't be changed for private chats. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="title">New chat title, 1-255 characters</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult setChatTitle(string chat_id, string title)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("title", title);

        return sendToTelegram("setChatTitle", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to change the description of a supergroup or a channel. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="description">New chat description, 0-255 characters</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult setChatDescription(string chat_id, string description)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("description", description);

        return sendToTelegram("setChatDescription", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to pin a message in a supergroup. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="description">Identifier of a message to pin</param>
    /// <param name="disable_notification">Pass True, if it is not necessary to send a notification to all group members about the new pinned message</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult pinChatMessage(string chat_id, int message_id, bool? disable_notification)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("message_id", message_id.ToString());
        if (disable_notification.HasValue)
            postData.Add("disable_notification", disable_notification.ToString());

        return sendToTelegram("pinChatMessage", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to unpin a message in a supergroup chat. The bot must be an administrator in the chat for this to work and must have the appropriate admin rights
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult unpinChatMessage(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("unpinChatMessage", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method for your bot to leave a group, supergroup or channel
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target group or username of the target supergroup</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult leaveChat(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("leaveChat", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to get up to date information about the chat
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup or channel (in the format @channelusername)</param>
    /// <returns>Returns a Chat object on success</returns>
    public static ChatResult getChat(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("getChat", postData, typeof(ChatResult)) as ChatResult;
    }

    /// <summary>
    /// Use this method to get a list of administrators in a chat. If the chat is a group or a supergroup and no administrators were appointed, only the creator will be returned
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup or channel (in the format @channelusername)</param>
    /// <returns>On success, returns an Array of ChatMember objects that contains information about all chat administrators except other bots</returns>
    public static ChatMembersResult getChatAdministrators(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("getChatAdministrators", postData, typeof(ChatMembersResult)) as ChatMembersResult;
    }

    /// <summary>
    /// Use this method to get the number of members in a chat
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup or channel (in the format @channelusername)</param>
    /// <returns>Returns Int on success</returns>
    public static ChatMembersCountResult getChatMembersCount(string chat_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);

        return sendToTelegram("getChatMembersCount", postData, typeof(ChatMembersCountResult)) as ChatMembersCountResult;
    }

    /// <summary>
    /// Use this method to get information about a member of a chat
    /// </summary>
    /// <param name="chat_id">Unique identifier for the target chat or username of the target supergroup or channel (in the format @channelusername)</param>
    /// <param name="user_id">Unique identifier of the target user</param>
    /// <returns>Returns a ChatMember object on success</returns>
    public static ChatMemberResult getChatMember(string chat_id, long user_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("user_id", user_id.ToString());

        return sendToTelegram("getChatMember", postData, typeof(ChatMemberResult)) as ChatMemberResult;
    }

    /// <summary>
    /// Use this method to send answers to callback queries sent from inline keyboards
    /// </summary>
    /// <param name="callback_query_id">Unique identifier for the query to be answered</param>
    /// <param name="text">Text of the notification. If not specified, nothing will be shown to the user, 0-200 characters</param>
    /// <param name="show_alert">If true, an alert will be shown by the client instead of a notification at the top of the chat screen. Defaults to false</param>
    /// <param name="cache_time">The maximum amount of time in seconds that the result of the callback query may be cached client-side. Telegram apps will support caching starting in version 3.14. Defaults to 0.</param>
    /// <returns>On success, True is returned</returns>
    public static BooleanResult answerCallbackQuery(string callback_query_id, string text, bool? show_alert, int? cache_time)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("callback_query_id", callback_query_id);
        if (text != null)
            postData.Add("text", text);
        if (show_alert.HasValue)
            postData.Add("show_alert", show_alert.ToString());
        if (cache_time.HasValue)
            postData.Add("cache_time", cache_time.ToString());

        return sendToTelegram("answerCallbackQuery", postData, typeof(BooleanResult)) as BooleanResult;
    }

    /// <summary>
    /// Use this method to edit text messages sent by the bot or via the bot (for inline bots)
    /// </summary>
    /// <param name="chat_id">Required if inline_message_id is not specified. Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="message_id">Required if inline_message_id is not specified. Unique identifier of the sent message</param>
    /// <param name="inline_message_id">Required if chat_id and message_id are not specified. Identifier of the inline message</param>
    /// <param name="text">New text of the message</param>
    /// <param name="parse_mode">Send Markdown or HTML, if you want Telegram apps to show bold, italic, fixed-width text or inline URLs in your bot's message</param>
    /// <param name="disable_web_page_preview">Disables link previews for links in this message</param>
    /// <param name="reply_markup">A JSON-serialized object for an inline keyboard</param>
    /// <returns>On success, if edited message is sent by the bot, the edited Message is returned, otherwise True is returned</returns>
    public static MessageResult editMessageText(string chat_id, int? message_id, string inline_message_id, string text, ParseMode parse_mode, bool? disable_web_page_preview, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        if (chat_id != null)
            postData.Add("chat_id", chat_id);
        if (message_id.HasValue)
            postData.Add("message_id", message_id.ToString());
        if (inline_message_id != null)
            postData.Add("inline_message_id", inline_message_id);
        postData.Add("text", text);
        if (parse_mode != ParseMode.None)
            postData.Add("parse_mode", parse_mode.ToString());
        if (disable_web_page_preview.HasValue)
            postData.Add("disable_web_page_preview", disable_web_page_preview.ToString());
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("editMessageText", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to edit captions of messages sent by the bot or via the bot (for inline bots)
    /// </summary>
    /// <param name="chat_id">Required if inline_message_id is not specified. Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="message_id">Required if inline_message_id is not specified. Unique identifier of the sent message</param>
    /// <param name="inline_message_id">Required if chat_id and message_id are not specified. Identifier of the inline message</param>
    /// <param name="caption">New caption of the message</param>
    /// <param name="reply_markup">A JSON-serialized object for an inline keyboard</param>
    /// <returns>On success, if edited message is sent by the bot, the edited Message is returned, otherwise True is returned</returns>
    public static MessageResult editMessageCaption(string chat_id, int? message_id, string inline_message_id, string caption, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        if (chat_id != null)
            postData.Add("chat_id", chat_id);
        if (message_id.HasValue)
            postData.Add("message_id", message_id.ToString());
        if (inline_message_id != null)
            postData.Add("inline_message_id", inline_message_id);
        postData.Add("caption", caption);
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("editMessageCaption", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to edit only the reply markup of messages sent by the bot or via the bot (for inline bots)
    /// </summary>
    /// <param name="chat_id">Required if inline_message_id is not specified. Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="message_id">Required if inline_message_id is not specified. Unique identifier of the sent message</param>
    /// <param name="inline_message_id">Required if chat_id and message_id are not specified. Identifier of the inline message</param>
    /// <param name="caption">New caption of the message</param>
    /// <param name="reply_markup">A JSON-serialized object for an inline keyboard</param>
    /// <returns>On success, if edited message is sent by the bot, the edited Message is returned, otherwise True is returned</returns>
    public static MessageResult editMessageReplyMarkup(string chat_id, int? message_id, string inline_message_id, IMarkup reply_markup)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        if (chat_id != null)
            postData.Add("chat_id", chat_id);
        if (message_id.HasValue)
            postData.Add("message_id", message_id.ToString());
        if (inline_message_id != null)
            postData.Add("inline_message_id", inline_message_id);
        if (reply_markup != null)
            postData.Add("reply_markup", new JavaScriptSerializer().Serialize(reply_markup));

        return sendToTelegram("editMessageReplyMarkup", postData, typeof(MessageResult)) as MessageResult;
    }

    /// <summary>
    /// Use this method to delete a message, including service messages
    /// </summary>
    /// <param name="chat_id">Required if inline_message_id is not specified. Unique identifier for the target chat or username of the target channel (in the format @channelusername)</param>
    /// <param name="message_id">Required if inline_message_id is not specified. Unique identifier of the sent message</param>
    /// <returns>Returns True on success</returns>
    public static BooleanResult deleteMessage(string chat_id, int message_id)
    {
        NameValueCollection postData = HttpUtility.ParseQueryString(string.Empty);
        postData.Add("chat_id", chat_id);
        postData.Add("message_id", message_id.ToString());

        return sendToTelegram("deleteMessage", postData, typeof(BooleanResult)) as BooleanResult;
    }

}