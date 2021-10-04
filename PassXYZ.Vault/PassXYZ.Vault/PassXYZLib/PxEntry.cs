using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

using PureOtp;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Security;
using KeePassLib.Utility;
using Image = SkiaSharp.SKBitmap;

using PassXYZ.Vault.Resx;

namespace PassXYZLib
{
    /// <summary>
    /// A class representing a field in PwEntry. A field is stored in ProtectedStringDictionary.
    /// We convert key value pair into a field so that it can be used by the user interface.
    /// </summary>
    public class Field : INotifyPropertyChanged
    {
        private string _key;
        /// <summary>
        /// This is the key used by Field. This Key should be decoded for PxEntry.
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        /// <summary>
        /// The EncodeKey is used by PxEntry. For PwEntry, this is an empty string.
        /// </summary>
        public string EncodedKey = string.Empty;

        public bool IsEncoded => !string.IsNullOrEmpty(EncodedKey);

        private string _value;
        private string _shadowValue = string.Empty;
        public string Value
        {
            get => _value;
            set
            {
                if(IsProtected)
                {
                    _shadowValue = value;
                    _value = new string('*', _shadowValue.Length);
                }
                else
                {
                    _value = value;
                }
                OnPropertyChanged("Value");
            }
        }

        private bool _isProtected = false;
        public bool IsProtected
        {
            get => _isProtected;
            set
            {
                _isProtected = value;
                OnPropertyChanged("IsProtected");
            }
        }

        private bool _isBinaries = false;
        /// <summary>
        /// Whether this field is an attachment
        /// </summary>
        public bool IsBinaries
        {
            get => _isBinaries;
            set
            {
                _isBinaries = value;
                OnPropertyChanged("IsProtected");
            }
        }

        private ProtectedBinary _binary = null;
        /// <summary>
        /// Binary data in the attachment
        /// </summary>
        public ProtectedBinary Binary
        {
            get => _binary;
            set
            {
                _binary = value;
                OnPropertyChanged("Binary");
            }
        }

        public bool IsHide { get; private set; } = true;

        public ImageSource ImgSource { get; set; }

        public Field(string key, string value, bool isProtected, string encodedKey = "")
        {
            Key = key;
            EncodedKey = encodedKey;
            IsProtected = isProtected;
            Value = value;

            string lastWord = key.Split(' ').Last();
            ImgSource = FieldIcons.GetImage(lastWord.ToLower());
        }

        public void ShowPassword()
        {
            if (IsProtected && !string.IsNullOrEmpty(_shadowValue))
            {
                _value = _shadowValue;
                IsHide = false;
                OnPropertyChanged("Value");
            }
        }

        public void HidePassword()
        {
            if (IsProtected && !string.IsNullOrEmpty(_shadowValue))
            {
                _value = new string('*', _shadowValue.Length);
                IsHide = true;
                OnPropertyChanged("Value");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class PxEntry : PwEntry
    {
        public PxEntry(bool bCreateNewUuid, bool bSetTimes) : base(bCreateNewUuid, bSetTimes) { }
        public PxEntry() : base() { }
    }

    public static class PwEntryEx
    {
        public static bool IsNotes(this PwEntry entry)
        {
            string subType = entry.CustomData.Get(PxDefs.PxCustomDataItemSubType);
            return !string.IsNullOrEmpty(subType) && subType.Equals(ItemSubType.Notes.ToString());
        }

        /// <summary>
        /// This is an extension method of PwEntry.
        /// This method is used to set the sub-type of a PwEntry.
        /// </summary>
        /// <param name="entry">an instance of PwEntry</param>
        /// <param name="itemSubType">sub-type of PwEntry</param>
		/// <returns>A list of fields</returns>
        public static void SetType(this PwEntry entry, ItemSubType itemSubType)
        {
            if (itemSubType != ItemSubType.None && itemSubType != ItemSubType.Group)
            {
                entry.CustomData.Set(PxDefs.PxCustomDataItemSubType, itemSubType.ToString());
            }
        }

        public static bool IsPxEntry(this PwEntry entry)
        {
            return PxDefs.IsPxEntry(entry);
        }

        public static string EncodeKey(this PwEntry entry, string key)
        {
            if(PxDefs.IsPxEntry(entry))
            {
                string lastKey = string.Empty;
                foreach (var pstr in entry.Strings)
                {
                    if (!pstr.Key.Equals(PwDefs.TitleField) && !pstr.Key.Equals(PwDefs.NotesField))
                    {
                        lastKey = pstr.Key;
                    }
                }

                if(string.IsNullOrEmpty(lastKey))
                {
                    return "000" + key;
                }
                else
                {
                    uint index = uint.Parse(lastKey.Substring(0, PxDefs.PxEntryKeyDigits));
                    return PxDefs.EncodeKey(key, index + 1);
                }
            }
            else
            {
                return key;
            }
        }

        public static string GetNotes(this PwEntry entry)
        {
            return entry.Strings.ReadSafe(PwDefs.NotesField);
        }

        /// <summary>
        /// This is an extension method of PwEntry.
        /// Convert ProtectedStringDictionary into a list of fields. TitleField and NotesField
        /// are not included in the list.
        /// TitleField will be used to display the title in UI and NotesField will be displayed at the
        /// bottom of a page with Markdown support.
        /// </summary>
        /// <param name="entry">an instance of PwEntry</param>
		/// <returns>A list of fields</returns>
        public static List<Field> GetFields(this PwEntry entry)
        {
            List<Field> fields = new List<Field>();
            bool isPxEntry = PxDefs.IsPxEntry(entry);

            if (isPxEntry)
            {
                // If this is an instance of PxEntry, we handle it here. We need to convert ProtectedString to Field.
                foreach (var pstr in entry.Strings)
                {
                    if (!pstr.Key.Equals(PwDefs.TitleField) && !pstr.Key.Equals(PwDefs.NotesField))
                    {
                        fields.Add(new Field(PxDefs.DecodeKey(pstr.Key), entry.Strings.ReadSafe(pstr.Key), entry.Strings.GetSafe(pstr.Key).IsProtected, pstr.Key));
                    }
                }
            }
            else
            {
                // If this is an instance of PwEntry, we handle it here.
                if(entry.Strings.Exists(PwDefs.UserNameField))
                {
                    fields.Add(new Field(PwDefs.UserNameField, entry.Strings.ReadSafe(PwDefs.UserNameField), entry.Strings.GetSafe(PwDefs.UserNameField).IsProtected));
                }

                if (entry.Strings.Exists(PwDefs.PasswordField))
                {
                    fields.Add(new Field(PwDefs.PasswordField, entry.Strings.ReadSafe(PwDefs.PasswordField), entry.Strings.GetSafe(PwDefs.PasswordField).IsProtected));
                }

                if (entry.Strings.Exists(PwDefs.UrlField))
                {
                    fields.Add(new Field(PwDefs.UrlField, entry.Strings.ReadSafe(PwDefs.UrlField), entry.Strings.GetSafe(PwDefs.UrlField).IsProtected));
                }

                foreach (var field in entry.Strings)
                {
                    if (!PwDefs.IsStandardField(field.Key))
                    {
                        fields.Add(new Field(field.Key, entry.Strings.ReadSafe(field.Key), entry.Strings.GetSafe(field.Key).IsProtected));
                    }
                }
            }

            foreach (var field in entry.Binaries)
            {
                fields.Add(new Field(field.Key, $"{AppResources.label_id_attachment} {entry.Binaries.UCount}", false)
                {
                    IsBinaries = true,
                    Binary = entry.Binaries.Get(field.Key),
                    ImgSource = new FontAwesome.Solid.IconSource
                    {
                        Icon = FontAwesome.Solid.Icon.Paperclip
                    }
                });
            }

            return fields;
        }
    
        public static string GetUrlField(this PwEntry entry)
        {
            foreach (KeyValuePair<string, ProtectedString> pstr in entry.Strings)
            {
                if (pstr.Key.EndsWith(PwDefs.UrlField))
                {
                    return entry.Strings.ReadSafe(pstr.Key);
                }
            }
            return string.Empty;
        }
    }

    public static class FieldIcons
    {
        public static Dictionary<string, FontAwesome.Regular.Icon> RegularIcons = new Dictionary<string, FontAwesome.Regular.Icon>()
        {
            { "calendar", FontAwesome.Regular.Icon.CalendarAlt }
        };

        public static Dictionary<string, FontAwesome.Solid.Icon> SolidIcons = new Dictionary<string, FontAwesome.Solid.Icon>()
        {
            { "address", FontAwesome.Solid.Icon.MapMarkerAlt },
            { "地址", FontAwesome.Solid.Icon.MapMarkerAlt },
            { "地点", FontAwesome.Solid.Icon.MapMarkerAlt },
            { "card", FontAwesome.Solid.Icon.IdCard },
            { "账号", FontAwesome.Solid.Icon.IdCard },
            { "身份证号", FontAwesome.Solid.Icon.IdCard },
            { "date", FontAwesome.Solid.Icon.CalendarAlt },
            { "日期", FontAwesome.Solid.Icon.CalendarAlt },
            { "签发日期", FontAwesome.Solid.Icon.CalendarAlt },
            { "有效期至", FontAwesome.Solid.Icon.CalendarAlt },
            { "email", FontAwesome.Solid.Icon.Envelope },
            { "邮件地址", FontAwesome.Solid.Icon.Envelope },
            { "邮件", FontAwesome.Solid.Icon.Envelope },
            { "mobile", FontAwesome.Solid.Icon.Phone },
            { "手机号码", FontAwesome.Solid.Icon.Phone },
            { "name", FontAwesome.Solid.Icon.User },
            { "姓名", FontAwesome.Solid.Icon.User },
            { "password", FontAwesome.Solid.Icon.Key },
            { "密码", FontAwesome.Solid.Icon.Key },
            { "支付密码", FontAwesome.Solid.Icon.Key },
            { "交易密码", FontAwesome.Solid.Icon.Key },
            { "网银密码", FontAwesome.Solid.Icon.Key },
            { "取款密码", FontAwesome.Solid.Icon.Key },
            { "U盾密码", FontAwesome.Solid.Icon.Key },
            { "phone", FontAwesome.Solid.Icon.Phone },
            { "pin", FontAwesome.Solid.Icon.Key },
            { "url", FontAwesome.Solid.Icon.Link },
            { "链接地址", FontAwesome.Solid.Icon.Link },
            { "username", FontAwesome.Solid.Icon.User }
        };

        public static Dictionary<string, FontAwesome.Brand.Icon> BrandIcons = new Dictionary<string, FontAwesome.Brand.Icon>()
        {
            { "alipay", FontAwesome.Brand.Icon.Alipay },
            { "qq", FontAwesome.Brand.Icon.Qq },
            { "wechat", FontAwesome.Brand.Icon.Weixin }
        };

        public static ImageSource GetImage(string key) 
        { 
            if (BrandIcons.ContainsKey(key))
            {
                var brandIconSource = new FontAwesome.Brand.IconSource
                {
                    Icon = BrandIcons[key]
                };
                return brandIconSource;
            }
            else if (RegularIcons.ContainsKey(key)) 
            {
                var regularIconSource = new FontAwesome.Regular.IconSource
                {
                    Icon = RegularIcons[key]
                };
                return regularIconSource;
            }
            else 
            {
                var solidIconSource = new FontAwesome.Solid.IconSource
                {
                    Icon = FontAwesome.Solid.Icon.File
                };

                if (SolidIcons.ContainsKey(key))
                {
                    solidIconSource.Icon = SolidIcons[key];
                }

                return solidIconSource;
            }
        }
    }

    public enum BinaryDataClass
    {
        Unknown = 0,
        Text,
        RichText,
        Excel,
        Image,
        PDF,
        WebDocument
    }

    public static class BinaryDataClassifier
    {
        private static readonly string[] m_vTextExtensions = new string[] {
            "txt", "csv", "c", "cpp", "h", "hpp", "css", "js", "bat"
        };

        private static readonly string[] m_vRichTextExtensions = new string[] {
            "rtf", "doc", "docx"
        };

        private static readonly string[] m_vExcelExtensions = new string[] {
            "xls", "xlsx"
        };

        private static readonly string[] m_vPdfExtensions = new string[] {
            "pdf"
        };

        private static readonly string[] m_vImageExtensions = new string[] {
            "bmp", "emf", "exif", "gif", "ico", "jpeg", "jpe", "jpg",
            "png", "tiff", "tif", "wmf"
        };

        private static readonly string[] m_vWebExtensions = new string[] {
            "htm", "html"
        };

        public static BinaryDataClass ClassifyUrl(string strUrl)
        {
            Debug.Assert(strUrl != null);
            if (strUrl == null) throw new ArgumentNullException("strUrl");

            string str = strUrl.Trim().ToLower();

            foreach (string strPdfExt in m_vPdfExtensions)
            {
                if (str.EndsWith("." + strPdfExt))
                    return BinaryDataClass.PDF;
            }

            foreach (string strTextExt in m_vTextExtensions)
            {
                if (str.EndsWith("." + strTextExt))
                    return BinaryDataClass.Text;
            }

            foreach (string strRichTextExt in m_vRichTextExtensions)
            {
                if (str.EndsWith("." + strRichTextExt))
                    return BinaryDataClass.RichText;
            }

            foreach (string strImageExt in m_vImageExtensions)
            {
                if (str.EndsWith("." + strImageExt))
                    return BinaryDataClass.Image;
            }

            foreach (string strWebExt in m_vWebExtensions)
            {
                if (str.EndsWith("." + strWebExt))
                    return BinaryDataClass.WebDocument;
            }

            foreach (string strExcelExt in m_vExcelExtensions)
            {
                if (str.EndsWith("." + strExcelExt))
                    return BinaryDataClass.Excel;
            }

            return BinaryDataClass.Unknown;
        }

        public static BinaryDataClass ClassifyData(byte[] pbData)
        {
            Debug.Assert(pbData != null);
            if (pbData == null) throw new ArgumentNullException("pbData");

            try
            {
                Image img = GfxUtil.LoadImage(pbData);
                if (img != null)
                {
                    img.Dispose();
                    return BinaryDataClass.Image;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"BinaryDataClass: Exception={e.ToString()}");
            }

            return BinaryDataClass.Unknown;
        }

        public static BinaryDataClass Classify(string strUrl, byte[] pbData)
        {
            BinaryDataClass bdc = ClassifyUrl(strUrl);
            if (bdc != BinaryDataClass.Unknown) return bdc;

            // We don't have classify data by content.
            // return ClassifyData(pbData);
            return BinaryDataClass.Unknown;
        }

        public static StrEncodingInfo GetStringEncoding(byte[] pbData,
            out uint uStartOffset)
        {
            Debug.Assert(pbData != null);
            if (pbData == null) throw new ArgumentNullException("pbData");

            uStartOffset = 0;

            List<StrEncodingInfo> lEncs = new List<StrEncodingInfo>(StrUtil.Encodings);
            lEncs.Sort(BinaryDataClassifier.CompareBySigLengthRev);

            foreach (StrEncodingInfo sei in lEncs)
            {
                byte[] pbSig = sei.StartSignature;
                if ((pbSig == null) || (pbSig.Length == 0)) continue;
                if (pbSig.Length > pbData.Length) continue;

                byte[] pbStart = MemUtil.Mid<byte>(pbData, 0, pbSig.Length);
                if (MemUtil.ArraysEqual(pbStart, pbSig))
                {
                    uStartOffset = (uint)pbSig.Length;
                    return sei;
                }
            }

            if ((pbData.Length % 4) == 0)
            {
                byte[] z3 = new byte[] { 0, 0, 0 };
                int i = MemUtil.IndexOf<byte>(pbData, z3);
                if ((i >= 0) && (i < (pbData.Length - 4))) // Ignore last zero char
                {
                    if ((i % 4) == 0) return StrUtil.GetEncoding(StrEncodingType.Utf32BE);
                    if ((i % 4) == 1) return StrUtil.GetEncoding(StrEncodingType.Utf32LE);
                    // Don't assume UTF-32 for other offsets
                }
            }

            if ((pbData.Length % 2) == 0)
            {
                int i = Array.IndexOf<byte>(pbData, 0);
                if ((i >= 0) && (i < (pbData.Length - 2))) // Ignore last zero char
                {
                    if ((i % 2) == 0) return StrUtil.GetEncoding(StrEncodingType.Utf16BE);
                    return StrUtil.GetEncoding(StrEncodingType.Utf16LE);
                }
            }

            try
            {
                UTF8Encoding utf8Throw = new UTF8Encoding(false, true);
                utf8Throw.GetString(pbData);
                return StrUtil.GetEncoding(StrEncodingType.Utf8);
            }
            catch (Exception) { }

            return StrUtil.GetEncoding(StrEncodingType.Default);
        }

        private static int CompareBySigLengthRev(StrEncodingInfo a, StrEncodingInfo b)
        {
            Debug.Assert((a != null) && (b != null));

            int na = 0, nb = 0;
            if ((a != null) && (a.StartSignature != null))
                na = a.StartSignature.Length;
            if ((b != null) && (b.StartSignature != null))
                nb = b.StartSignature.Length;

            return -(na.CompareTo(nb));
        }
    }

}
